using BitDisplayDaemo.SSNS_ENV;
using SkiaSharp;
using System.Collections;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BitDisplayDaemo;

public class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;
	private const int Port = 3000;
	private readonly IConfiguration _configuration;
	private HttpListener? _httpListener;
	private readonly SSENV2Manager _ssenv2Manager;

	public Worker(ILogger<Worker> logger, IConfiguration configuration, SSENV2Manager ssenv2Manager)
	{
		_logger = logger;
		_configuration = configuration;
		_ssenv2Manager = ssenv2Manager;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_httpListener = new HttpListener();
		_httpListener.Prefixes.Add($"http://*:{Port}/ws/");
		_httpListener.Start();
		_logger.LogInformation("C# Display Daemon started on port {Port}. Listening for React websocket frame packages...", Port);
		try
		{
			var comPortName = _configuration["HardwareSettings:ComPortName"];
			var baudRate = int.TryParse(_configuration["HardwareSettings:BaudRate"], out var rate) ? rate : 921600;
			_ssenv2Manager.ConnectProc(comPortName, baudRate);
			_logger.LogInformation("Serial port {ComPortName} opened successfully for e-ink display communication.", comPortName);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to open serial port {ComPortName} for e-ink display communication.", _configuration["HardwareSettings:ComPortName"]);
		}
		


		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				HttpListenerContext context = await _httpListener.GetContextAsync();
				if (context.Request.IsWebSocketRequest)
				{
					// Process each connection asynchronously so the daemon stays responsive
					_ = HandleWebSocketConnectionAsync(context, stoppingToken);
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					context.Response.Close();
				}
			}
			catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogError(ex, "Error accepting incoming connection request.");
			}
		}
	}

	private async Task HandleWebSocketConnectionAsync(HttpListenerContext context, CancellationToken stoppingToken)
	{
		HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
		WebSocket webSocket = wsContext.WebSocket;
		_logger.LogInformation("React application kiosk successfully connected over WebSocket.");

		// Large 2MB buffer to seamlessly hold QHD base64 frame data packets
		byte[] buffer = new byte[2 * 1024 * 1024];
		var jsonBuilder = new StringBuilder();

		try
		{
			while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
			{
				jsonBuilder.Clear();
				WebSocketReceiveResult result;

				do
				{
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

					if (result.MessageType == WebSocketMessageType.Close)
					{
						await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
						_logger.LogInformation("WebSocket connection closed by React client.");
						return;
					}

					string textChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
					jsonBuilder.Append(textChunk);

				} while (!result.EndOfMessage);

				// Parse the complete message payload package
				ProcessIncomingFrame(jsonBuilder.ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception encountered in WebSocket data streaming loop.");
		}
		finally
		{
			webSocket.Dispose();
		}
	}

	private void ProcessIncomingFrame(string rawJson)
	{
		try
		{
			using var doc = JsonDocument.Parse(rawJson);
			var root = doc.RootElement;

			if (root.TryGetProperty("station_id", out var stationProp) &&
				root.TryGetProperty("image_base64", out var base64Prop))
			{
				var stationId = stationProp.GetString() ?? "";
				var base64Data = base64Prop.GetString() ?? "";

				if (!string.IsNullOrEmpty(base64Data))
				{
					//// 💥 SUCCESS: Convert the string back into raw byte array data!
					//byte[] imageBytes = Convert.FromBase64String(base64Data);

					//_logger.LogInformation("Successfully received and converted frame matrix for Station {StationId} ({Bytes} bytes).", stationId, imageBytes.Length);

					//// TODO: Pass 'imageBytes' into an e-ink display driver, 
					//// write it to a physical file, or render it to a hardware frame buffer.
					//// e.g., File.WriteAllBytes($"/tmp/station_{stationId}.png", imageBytes);

					//SSENV2Manager.TCON이미지쓰기3(imageBytes, 0, 0, 1440, 2560);
					if (!string.IsNullOrEmpty(base64Data))
					{
						byte[] pngCompressedBytes = Convert.FromBase64String(base64Data);

						// 1. Decode PNG bytes using SkiaSharp (Commercially safe - MIT License)
						using (var stream = new MemoryStream(pngCompressedBytes))
						using (var managedStream = new SKManagedStream(stream))
						using (var fullBitmap = SKBitmap.Decode(managedStream))
						{
							if (fullBitmap == null)
							{
								_logger.LogError("Failed decoding incoming stream via Skia engine.");
								return;
							}

							// Define your cropping region boundaries
							var region = new System.Drawing.Rectangle(0, 0, fullBitmap.Width, fullBitmap.Height);
							bool isHorizontal = false; // toggle based on layout orientation setup

							ProcessAndSendToEpdSkia(fullBitmap, region, isHorizontal, stationId);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to parse or convert incoming text frame payload package.");
		}
	}

	private void ProcessAndSendToEpdSkia(SKBitmap fullBitmap, System.Drawing.Rectangle region, bool isHorizontal, string stationId)
	{
		int x = region.X;
		int y = region.Y;
		int w = region.Width;
		int h = region.Height;

		if (!isHorizontal)
		{
			w = (w + 3) / 4 * 4; // Multiple of 4 alignment

			// 2. Crop bitmap subset out
			using (var croppedBitmap = new SKBitmap(w, h))
			{
				using (var canvas = new SKCanvas(croppedBitmap))
				{
					var srcRect = new SKRect(x, y, x + w, y + h);
					var destRect = new SKRect(0, 0, w, h);
					canvas.DrawBitmap(fullBitmap, srcRect, destRect);
				}

				// 3. Mirror/Flip the image context (Legacy Rotate180FlipY equivalent)
				using (var flippedBitmap = new SKBitmap(w, h))
				{
					using (var canvas = new SKCanvas(flippedBitmap))
					{
						// Scale Y by -1 to flip vertically, translate to keep inside layout space
						canvas.Translate(w / 2f, h / 2f);
						canvas.RotateDegrees(180);
						canvas.Scale(1, -1);
						
						canvas.Translate(-w / 2f, -h / 2f);
						canvas.DrawBitmap(croppedBitmap, 0, 0);
					}

					// 4. Transform color spectrum down into uncompressed 1-byte grayscale layout
					byte[] grayscaleData = ConvertToGrayscaleSkia(flippedBitmap);
					_logger.LogInformation("Successfully received and converted frame matrix for Station {StationId} ({Bytes} bytes).", stationId, grayscaleData.Length);
					_ssenv2Manager.TCON이미지쓰기3(grayscaleData, x, y, w, h);
					_logger.LogInformation("Wrote {Len} bytes to EPD (Skia Vertical).", grayscaleData.Length);
				}
			}
		}
		else
		{
			h = (h + 3) / 4 * 4;

			using (var croppedBitmap = new SKBitmap(w, h))
			{
				using (var canvas = new SKCanvas(croppedBitmap))
				{
					var srcRect = new SKRect(x, y, x + w, y + h);
					var destRect = new SKRect(0, 0, w, h);
					canvas.DrawBitmap(fullBitmap, srcRect, destRect);
				}

				int tW = h; // swapped dimensions
				int tH = w;

				// Legacy Rotate90FlipY equivalent using Skia rotation matrices
				using (var transformedBitmap = new SKBitmap(tW, tH))
				{
					using (var canvas = new SKCanvas(transformedBitmap))
					{
						canvas.Translate(tW, 0);
						canvas.Scale(-1, 1); // Flip Y (X after 90deg rot)
						canvas.RotateDegrees(90);
						canvas.DrawBitmap(croppedBitmap, 0, 0);
					}

					byte[] grayscaleData = ConvertToGrayscaleSkia(transformedBitmap);
					_logger.LogInformation("Successfully received and converted frame matrix for Station {StationId} ({Bytes} bytes).", stationId, grayscaleData.Length);
					int tX = y;
					int tY = fullBitmap.Height - (x + w);

					 _ssenv2Manager.TCON이미지쓰기3(grayscaleData, tX, tY, tW, tH);
					_logger.LogInformation("Wrote {Len} bytes to EPD (Skia Horizontal).", grayscaleData.Length);
				}
			}
		}
	}
	private byte[] ConvertToGrayscaleSkia(SKBitmap bitmap)
	{
		byte[] rawGrayscale = new byte[bitmap.Width * bitmap.Height];
		int index = 0;

		// Direct memory pointer access for maximum speed on Raspberry Pi hardware architectures
		ReadOnlySpan<SKColor> pixels = bitmap.Pixels;

		for (int i = 0; i < pixels.Length; i++)
		{
			SKColor color = pixels[i];
			// Standard high-accuracy luminosity matrix weights
			byte gray = (byte)((color.Red * 0.299) + (color.Green * 0.587) + (color.Blue * 0.114));
			rawGrayscale[index++] = gray;
		}

		return rawGrayscale;
	}
	public override void Dispose()
	{
		_httpListener?.Stop();
		_httpListener?.Close();
		base.Dispose();
	}
}
