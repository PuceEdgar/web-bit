using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace BitDisplayDaemo.SSNS_ENV
{
	public class SSENV2Modules
	{
		readonly ILogger<SSENV2Modules> _logger;
		public delegate void DataReceivedHandler(byte[] receiveData);
		public event DataReceivedHandler OnDataReceivedEvent;

		public delegate void DisConnectedHandler();
		public event DisConnectedHandler OnDisConnectedEvent;

		public delegate void SpErrorHandler();
		public event SpErrorHandler OnSpErrorEvent;

		private SerialPort sp;

		private Thread dtCheckSerialPort;
		private bool CheckSerialPortYN = false;

		public SSENV2Modules(ILogger<SSENV2Modules> logger)
		{
			_logger = logger;
		}

		//Return current state of Serial Port(is open or not)
		public bool IsOpen
		{
			get
			{
				if (sp != null)
				{
					return sp.IsOpen;
				}
				else
				{
					return false;
				}
			}
		}

		public bool InitProc(string strPortName, int baudRate
			, int dataBits = 8
			, StopBits stopBits = StopBits.One  // .One.Two
			, Parity parity = Parity.None
			, Handshake handShake = Handshake.None)
		{
			//Start & Open Serial Port
			SerialPort port = null;
			try
			{
				//if (sp != null) return false;

				//sp = new SerialPort();

				//sp.PortName = strPortName;
				//sp.BaudRate = baudRate;
				//sp.DataBits = dataBits;
				//sp.StopBits = stopBits;
				//sp.Parity = parity;
				//sp.Handshake = Handshake.None;

				//sp.Encoding = new System.Text.ASCIIEncoding();
				//sp.NewLine = "\r\n";
				//sp.ErrorReceived += Sp_ErrorReceived;
				//sp.DataReceived += Sp_DataReceived;
				//sp.WriteTimeout = -1;
				//sp.ReadTimeout = -1;
				//sp.Open();
				port = new SerialPort(strPortName, baudRate, parity, dataBits, stopBits)
				{
					Handshake = handShake,          // you were ignoring this parameter
					Encoding = new System.Text.ASCIIEncoding(),
					NewLine = "\r\n",
					ReadTimeout = -1,             // finite timeouts are safer on Linux
					WriteTimeout = -1
				};
				port.ErrorReceived += Sp_ErrorReceived;
				port.DataReceived += Sp_DataReceived;
				port.Open();
				sp = port; // assign to the class field only after successful open
				Console.WriteLine($"Serial Port Opened: {strPortName}, BaudRate: {baudRate}, DataBits: {dataBits}, StopBits: {stopBits}, Parity: {parity}, Handshake: {handShake}");
				StartCheckSerialPort();

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				port?.Dispose();
				sp = null;
				return false;
			}
		}

		private List<byte> _receiveBuffer = new List<byte>();

		private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				int bytesToRead = sp.BytesToRead;
				if (bytesToRead == 0) return;

				byte[] bytesBuffer = new byte[bytesToRead];
				sp.Read(bytesBuffer, 0, bytesToRead);

				lock (_receiveBuffer)
				{
					_receiveBuffer.AddRange(bytesBuffer);

					while (true)
					{
						// 최소 패킷 헤더(7E 7E + SIZE 2 + 3바이트 + 최소 1바이트 데이터 + 체크섬 1) = 8바이트
						if (_receiveBuffer.Count < 8)
							break;

						// STX1, STX2 연속 위치 찾기
						int stxIndex = -1;
						for (int i = 0; i < _receiveBuffer.Count - 1; i++)
						{
							if (_receiveBuffer[i] == 0x7E && _receiveBuffer[i + 1] == 0x7E)
							{
								stxIndex = i;
								break;
							}
						}

						if (stxIndex < 0)
						{
							// STX1, STX2 연속이 없으면, 마지막 0x7E만 남기고 모두 버림
							int lastStx = _receiveBuffer.LastIndexOf(0x7E);
							if (lastStx >= 0)
								_receiveBuffer.RemoveRange(0, lastStx);
							else
								_receiveBuffer.Clear();
							break;
						}

						// STX1, STX2 앞 데이터 제거
						if (stxIndex > 0)
							_receiveBuffer.RemoveRange(0, stxIndex);

						// 패킷 헤더가 충분한지 확인
						if (_receiveBuffer.Count < 8)
							break;

						// SIZE 추출
						int size = BitConvertUtils.ToInt16(new byte[] { _receiveBuffer[2], _receiveBuffer[3] }) - 3;
						int totalPacketLength = 7 + size + 1; // 7(header) + size + 1(checksum)

						if (_receiveBuffer.Count < totalPacketLength)
							break; // 전체 패킷이 아직 도착하지 않음

						// 패킷 추출
						byte[] packet = _receiveBuffer.Take(totalPacketLength).ToArray();

						// 체크섬 검사
						byte calcChecksum = BitConvertUtils.CreateCheckSum(packet, packet.Length - 1);
						if (packet[packet.Length - 1] != calcChecksum)
						{
							_logger.LogError($"[Rx] Checksum Error: {BitConvertUtils.ByteArrayToHexSTR(packet, "-")}");
							_receiveBuffer.RemoveRange(0, totalPacketLength);
							continue;
						}

						// 패킷 처리
						_logger.LogDebug($"[Rx] {packet.Length:d6}: {BitConvertUtils.ByteArrayToHexSTR(packet, "-")}");
						OnDataReceivedEvent?.Invoke(packet);

						// 버퍼에서 처리한 패킷 제거
						_receiveBuffer.RemoveRange(0, totalPacketLength);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"[Rx] Exception: {ex.Message}");

				Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
			}
		}

		private void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			try
			{
				_logger.LogError($"[Rx] Serial Error: {e.EventType}");
				OnSpErrorEvent?.Invoke();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[Rx] Exception: {ex.Message}");
				Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
			}
		}

		public void DoFinal()
		{
			//Close Serial Communication
			try
			{
				//ClearCheckSerialPort();

				if (sp != null)
				{
					sp.Close();
					sp = null;
				}
			}
			catch (Exception ex)
			{
				//TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
			}
		}

		public async void SendTask(byte[] byteSendData)
		{
			try
			{
				if (sp != null && sp.IsOpen)
				{
					Task task = Task.Run(() => sp.Write(byteSendData, 0, byteSendData.Length));
					await task;
					return;
				}
			}
			catch (Exception ee)
			{
				//TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public bool SendData(byte[] byteSendData)
		{
			SendTask(byteSendData);
			return true;
		}

		public bool SendData(byte[] byteSendData, int offSet, int count)
		{
			try
			{

				Console.WriteLine($"sp != null: {sp != null} | sp.IsOpen: {sp?.IsOpen}");
				if (sp != null && sp.IsOpen)
				{
					Console.WriteLine("trying to send data");
					sp.Write(byteSendData, offSet, count);

					if (count > 30)
					{
						byte[] sends = new byte[30];
						Buffer.BlockCopy(byteSendData, 0, sends, 0, sends.Length);
						_logger.LogDebug($"[Tx] {count:d6}: {BitConvertUtils.ByteArrayToHexSTR(sends, "-")}");
					}
					else
						_logger.LogDebug($"[Tx] {count:d6}: {BitConvertUtils.ByteArrayToHexSTR(byteSendData, "-")}");

					return true;
				}
			}
			catch (Exception ee)
			{
				_logger.LogError($"[Tx] Exception: {ee.Message}");
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
			return false;
		}

		private void StartCheckSerialPort()
		{
			try
			{
				if (dtCheckSerialPort == null)
				{
					dtCheckSerialPort = new Thread(CheckSerialPortComm) { IsBackground = true };
					dtCheckSerialPort.Start();
				}
				CheckSerialPortYN = true;
			}
			catch (Exception ee)
			{
				_logger.LogError($"[Error] Exception: {ee.Message}");
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		//private void StopCheckSerialPort()
		//{
		//	try
		//	{
		//		if (CheckSerialPortYN)
		//		{
		//			CheckSerialPortYN = false;
		//			if (Thread.CurrentThread != dtCheckSerialPort)
		//			{
		//				dtCheckSerialPort.Join();
		//			}
		//		}
		//	}
		//	catch (Exception ee)
		//	{
		//		TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//		System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//	}
		//}

		private void ClearCheckSerialPort()
		{
			try
			{
				if (dtCheckSerialPort != null)
				{
					if (dtCheckSerialPort.IsAlive)
					{
						//dtCheckSerialPort.Join()
						dtCheckSerialPort.Abort();
						Thread.Sleep(100);
					}
					dtCheckSerialPort = null;
				}

				CheckSerialPortYN = false;

				GC.Collect();
			}
			catch (Exception ee)
			{
				//TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		private void CheckSerialPortComm()  // Worker Thread
		{
			try
			{
				while (CheckSerialPortYN)
				{
					try
					{
						if (sp == null || sp.IsOpen == false)
						{
							_logger.LogDebug("SerialPort DisConnected");

							if (OnDisConnectedEvent != null)
								OnDisConnectedEvent();
							break;
						}

						Thread.Sleep(1000);
						GC.Collect();
					}
					catch (Exception ex)
					{
						_logger.LogError($"[Error] Exception: {ex.Message}");
						System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
					}

				}
			}
			catch (Exception ee)
			{
				_logger.LogError($"[Error] Exception: {ee.Message}");
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}
	}
}
