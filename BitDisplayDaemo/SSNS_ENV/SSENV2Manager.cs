using System;
using System.Collections.Generic;
using System.Text;

namespace BitDisplayDaemo.SSNS_ENV
{
	public class SSENV2Manager
	{
		private readonly ILogger<SSENV2Manager> _logger;
		private readonly ILogger<SSENV2Modules> _loggerModules;
		public SSENV2Manager(ILogger<SSENV2Manager> logger, ILogger<SSENV2Modules> loggerModules)
		{
			_logger = logger;
			_loggerModules = loggerModules;


			try
			{
				//if (_dtStatusResponse == null)
				//{
				//	_dtStatusResponse = new System.Windows.Threading.DispatcherTimer();
				//	_dtStatusResponse.Interval = TimeSpan.FromSeconds(1 * 60); // 1 min send period;
				//	_dtStatusResponse.Tick += _dtStatusResponse_Tick;
				//}
				//_dtStatusResponse.Start();
				//_dtStatusResponse_Tick(null, null);

				_dtStatusResponse ??= new Timer(_ => _dtStatusResponse_Tick(null, null), null, 0, 60 * 1000);
			}
			catch (Exception ee)
			{
				//TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		static SSENV2Modules mENV2 = null;

		public delegate void 보드상태정보Handler(SSNS_ENV2_BOARD_STATE _data);
		public static event 보드상태정보Handler On보드상태정보Event;

		public delegate void 상태정보Handler(SSNS_ENV2_STATE _data);
		public static event 상태정보Handler On상태정보Event;

		public delegate void 설정정보Handler(SSNS_ENV2_CONFIG _data);
		public static event 설정정보Handler On설정정보Event;

		public delegate void 센서정보Handler(SSNS_ENV2_SENSOR _data);
		public static event 센서정보Handler On센서정보Event;

		public delegate void 연결종료Handler();
		public static event 연결종료Handler On연결종료Event;

		public delegate void 연결재시도Handler();
		public static event 연결종료Handler On연결재시도Event;

		public delegate void SerialPortErrorHandler();
		public static event SerialPortErrorHandler OnSerialPortErrorEvent;

		static Timer? _dt시간 = null;
		static Timer? _dtStatusInfo = null;
		static Timer? _dtStatusResponse = null;
		//static System.Windows.Threading.DispatcherTimer _dtStatusInfo = null;
		//static System.Windows.Threading.DispatcherTimer _dtStatusResponse = null;

		public void ConnectProc(string strPortName, int baudRate)
		{
			try
			{
				if (mENV2 == null)
				{
					mENV2 = new SSENV2Modules(_loggerModules);
					mENV2.OnDataReceivedEvent += mENV2_OnDataReceivedEvent;
					mENV2.OnDisConnectedEvent += mENV2_OnDisConnectedEvent;
					mENV2.OnSpErrorEvent += MENV2_OnSpErrorEvent;
				}

				var result = mENV2.InitProc(strPortName, baudRate);
				Console.WriteLine($"SerialPort InitProc result: {result}");
				_dt시간 ??= new Timer(_ => _dt시간_Tick(null, null), null, 0, 100);

				_dtStatusInfo ??= new Timer(_ => _dtStatusInfo_Tick(null, null), null, 0, 30 * 1000);
				//if (_dt시간 == null)
				//{
				//	_dt시간 = new();
				//	_dt시간.Interval = TimeSpan.FromMilliseconds(100); //  send period;
				//	_dt시간.Tick += _dt시간_Tick;
				//	_dt시간.Tag = 0;
				//}
				//_dt시간.Start();
				//_dt시간_Tick(null, null);

				//if (_dtStatusInfo == null)
				//{
				//	_dtStatusInfo = new System.Windows.Threading.DispatcherTimer();
				//	_dtStatusInfo.Interval = TimeSpan.FromSeconds(30);// 60); send period;
				//	_dtStatusInfo.Tick += _dtStatusInfo_Tick;
				//}
				//_dtStatusInfo.Start();
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		private void MENV2_OnSpErrorEvent()
		{
			OnSerialPortErrorEvent?.Invoke();
		}

		public void DisConnectProc()
		{
			try
			{
				if (mENV2 == null)
				{
					_logger.LogWarning("SerialPort가 연결되어 있지않습니다.");
					return;
				}

				mENV2.DoFinal();
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public bool ConnectYN()
		{
			try
			{
				if (mENV2 == null) return false;

				return mENV2.IsOpen;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return false;
			}
		}

		//public static bool DoFinal()
		//{
		//	try
		//	{
		//		if (mENV2 != null)
		//		{
		//			mENV2.DoFinal();
		//			mENV2 = null;
		//		}

		//		if (_dt시간 != null)
		//		{
		//			_dt시간.Stop();
		//			_dt시간.Tick -= _dt시간_Tick;
		//			_dt시간 = null;
		//		}

		//		if (_dtStatusInfo != null)
		//		{
		//			_dtStatusInfo.Stop();
		//			_dtStatusInfo.Tick -= _dtStatusInfo_Tick;
		//			_dtStatusInfo = null;
		//		}

		//		return true;
		//	}
		//	catch (Exception ee)
		//	{
		//		TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//		System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//		return false;
		//	}
		//}

		private void _dt시간_Tick(object sender, EventArgs e)  // 100mS
		{
			try
			{
				//if (Convert.ToInt32(_dt시간.Tag) == 1) return;
				//_dt시간.Tag = 1;

				TickTCON이미지전송();

				//_dt시간.Tag = 0;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				//_dt시간.Tag = 0;
			}
		}

		private void _dtStatusInfo_Tick(object sender, EventArgs e)  // 60 Sec
		{
			try
			{
				Send상태정보요청();
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		static int processKillTimerSecondCount = 0;
		private void _dtStatusResponse_Tick(object sender, EventArgs e)  // 60 Sec
		{
			if (processKillTimerSecondCount++ >= 2) //5분
			{
				processKillTimerSecondCount = 0;

				_logger.LogInformation("상태보고가 오지않아 프로그램이 종료됩니다.");
				// TraceManager.AddLog(string.Format("### {0} ###", "상태보고가 오지않아 프로그램이 종료됩니다."));
				//System.Diagnostics.Process.GetCurrentProcess().Kill();

				// 5miniutes later, reopen that port. not to kill CurrentProcess.
				DisConnectProc();
				_logger.LogInformation("상태보고가 오지않아 연결을 재시도 합니다.");
				//TraceManager.AddLog(string.Format("### {0} ###", "상태보고가 오지않아 연결을 재시도 합니다."));
				On연결재시도Event?.Invoke(); //pjh
			}
		}

		private void mENV2_OnDisConnectedEvent()
		{
			try
			{
				_logger.LogInformation("연결이 종료되었습니다.");
				On연결종료Event?.Invoke();
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		#region PC To ENV

		static bool bResponseTconOk = true;
		static int nNoResponseTime = 0;
		static int nResponseRestartCnt = 0;
		static int nSendCnt = 0;
		static int nTotalDiscardQue = 0;

		static Queue<Array> arrayTx = new Queue<Array>();

		/// <summary>
		/// TickTCON이미지전송
		/// </summary>
		public async void TickTCON이미지전송()
		{
			try
			{
				Array aTx;
				if (bResponseTconOk)
				{
					if (arrayTx.Count > 0)
					{
						// if (SearchNewItem()) // always send it.
						{
							nNoResponseTime = 0;
							bResponseTconOk = false;
							nSendCnt++;

							aTx = arrayTx.Dequeue();  // InvalidOperationException
							Console.WriteLine($"atx size: {aTx.Length}");
							//mENV2.SendData((byte[])aTx, 0, ((byte[])aTx).Length);
							await Task.Run(() =>
							{
								mENV2.SendData((byte[])aTx, 0, ((byte[])aTx).Length);
							});

							_logger.LogInformation("Tx Queue Count = {0}, Length = {1}", arrayTx.Count, aTx.Length);
							//DisplayLog(Log4Level.Info, string.Format("[UART] Tx Queue Count = {0}, Length = {1}", arrayTx.Count, aTx.Length), LogSource.Board);
						}
					}
				}
				else  // Wait for response
				{
					if (++nNoResponseTime >= 600)  //60초로 증가   200*0.1=20초  1*60=60초=1분
					{
						nNoResponseTime = 0;
						while (arrayTx.Count > 9)
						{
							aTx = arrayTx.Dequeue();
							nTotalDiscardQue++;
							//DisplayLog(Log4Level.Info, string.Format("[UART] Tx nTotalDiscardQue Count = {0}", nTotalDiscardQue), LogSource.Board);
						}

						bResponseTconOk = true; // forced to restart
						nResponseRestartCnt++;

						Console.WriteLine("###### Tx Forced to Restart-Count = {0} Send-Count = {1} 버린큐 = {2}", nResponseRestartCnt, nSendCnt, nTotalDiscardQue);
						//DisplayLog(Log4Level.Info, string.Format("[UART] ###### Forced to Restart-Count = {0}", nResponseRestartCnt), LogSource.Board);
						//Log4Manager.WriteENVLog(Log4Level.Debug, string.Format("[UART] ###### Forced to Restart-Count = {0}", nResponseRestartCnt));

						OnSerialPortErrorEvent?.Invoke();
					}
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}
		private  bool SearchNewItem()
		{
			// 큐가 비어 있는 경우 바로 반환
			if (arrayTx.Count == 0)
				return false;
			if (arrayTx.Count == 1)
				return true;

			// 큐의 첫 번째 요소 길이 확인
			int txlen = arrayTx.Peek().GetLength(0);

			// 동일한 길이의 오래된 항목 제거
			while (arrayTx.Count > 1 && arrayTx.Skip(1).Any(member => txlen == member.GetLength(0)))
			{
				arrayTx.Dequeue(); // 오래된 항목 제거
			}

			// 큐에 남아 있는 항목이 1개 이상인지 확인
			return arrayTx.Count > 0;
		}

		/// <summary>
		/// 로그파일
		/// </summary>
		//public delegate void DisplayLogHandler(Log4Data log4);
		//public static event DisplayLogHandler OnDisplayLogEvent;
		//public static void DisplayLog(Log4Level lv, string log, LogSource logSource = LogSource.Other)
		//{
		//	Log4Data log4 = new Log4Data();
		//	log4.Level = lv;
		//	log4.REGDATE = DateTime.Now;
		//	log4.로그내용 = log;
		//	log4.LogSource = logSource;
		//	OnDisplayLogEvent?.Invoke(log4);
		//	//TraceManager.AddInfoLog(log);
		//	//Log4Manager.WriteLog(lv, log);
		//}

		/// <summary>
		/// TCON이미지쓰기
		/// </summary>
		public void TCON이미지쓰기(byte[] bStr, int X, int Y, int W, int H)   //
		{
			try
			{
				if (bStr.Length > (2560 * 1440) / 4)
				{
					throw new Exception("TCON쓰기에서 바이트 크기가 초과하였습니다.");
				}

				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);

				int nSize = (int)(3 + bStr.Length + 8);
				byte[] byteSize = BitConvertUtils.FromInt32(nSize, true);
				bytes.Add(byteSize[2]);
				bytes.Add(byteSize[3]);

				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add(byteSize[1]); //bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)(SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기));

				byteSize = BitConvertUtils.FromInt16((short)X, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)Y, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)W, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)H, true);
				bytes.AddRange(byteSize);
				bytes.AddRange(bStr);
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				arrayTx.Enqueue(bytes.ToArray());
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public byte wf_mode = 1;
		//public static void TCON이미지쓰기1(byte[] bStr, int X, int Y, int W, int H)
		//{
		//	try
		//	{
		//		if (bStr.Length > (2560 * 1440) / 4)
		//		{
		//			throw new Exception("TCON쓰기에서 바이트 크기가 초과하였습니다.");
		//		}

		//		byte[] dst = new byte[W * H * 2]; // 충분한 크기의 배열 초기화
		//		int compressedSize = RLE_Encode(bStr, W * H, dst);

		//		List<byte> bytes = new List<byte>();
		//		bytes.Add(STX);
		//		bytes.Add(STX);

		//		//int nSize = (int)(3 + bStr.Length + 8);
		//		int nSize = (int)(3 + compressedSize + 8); // 압축된 크기를 사용
		//		byte[] byteSize = BitConvertUtils.FromInt32(nSize, true);
		//		bytes.Add(byteSize[2]);
		//		bytes.Add(byteSize[3]);

		//		bytes.Add(wf_mode);     //SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
		//		bytes.Add(byteSize[1]); //bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
		//		bytes.Add((byte)(SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기1));

		//		byteSize = BitConvertUtils.FromInt16((short)X, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)Y, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)W, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)H, true);
		//		bytes.AddRange(byteSize);

		//		//bytes.AddRange(bStr);
		//		bytes.AddRange(dst.Take(compressedSize)); // 압축된 데이터를 추가
		//		bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

		//		arrayTx.Enqueue(bytes.ToArray());
		//	}
		//	catch (Exception ee)
		//	{
		//		TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//		System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//	}
		//}

		//public static void TCON이미지쓰기2(byte[] bStr, int X, int Y, int W, int H)
		//{
		//	try
		//	{
		//		if (bStr.Length > (2560 * 1440) / 4)
		//		{
		//			throw new Exception("TCON쓰기에서 바이트 크기가 초과하였습니다.");
		//		}

		//		byte[] dst = new byte[W * H * 2]; // 충분한 크기의 배열 초기화
		//		int compressedSize = RLE_EncodeBW(bStr, W * H, dst);

		//		List<byte> bytes = new List<byte>();
		//		bytes.Add(STX);
		//		bytes.Add(STX);

		//		//int nSize = (int)(3 + bStr.Length + 8);
		//		int nSize = (int)(3 + compressedSize + 8); // 압축된 크기를 사용
		//		byte[] byteSize = BitConvertUtils.FromInt32(nSize, true);
		//		bytes.Add(byteSize[2]);
		//		bytes.Add(byteSize[3]);

		//		bytes.Add((byte)1); // SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
		//		bytes.Add(byteSize[1]); //bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
		//		bytes.Add((byte)(SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기2));

		//		byteSize = BitConvertUtils.FromInt16((short)X, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)Y, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)W, true);
		//		bytes.AddRange(byteSize);
		//		byteSize = BitConvertUtils.FromInt16((short)H, true);
		//		bytes.AddRange(byteSize);

		//		//bytes.AddRange(bStr);
		//		bytes.AddRange(dst.Take(compressedSize)); // 압축된 데이터를 추가
		//		bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

		//		arrayTx.Enqueue(bytes.ToArray());
		//	}
		//	catch (Exception ee)
		//	{
		//		TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//		System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
		//	}
		//}

		public void TCON이미지쓰기3(byte[] bStr, int X, int Y, int W, int H)  //pjhpjh
		{
			try
			{
				if (bStr.Length > (2560 * 1440))
				{
					throw new Exception("TCON쓰기에서 바이트 크기가 초과하였습니다.");
				}

				byte[] dst = new byte[W * H * 2]; // 충분한 크기의 배열 초기화
				int compressedSize = RLE_Encode(bStr, W * H, dst);

				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);

				//int nSize = (int)(3 + bStr.Length + 8);
				int nSize = (int)(3 + compressedSize + 8); // 압축된 크기를 사용
				byte[] byteSize = BitConvertUtils.FromInt32(nSize, true);
				bytes.Add(byteSize[2]);
				bytes.Add(byteSize[3]);

				bytes.Add(wf_mode);     //SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add(byteSize[1]); //bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)(SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기3));

				byteSize = BitConvertUtils.FromInt16((short)X, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)Y, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)W, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)H, true);
				bytes.AddRange(byteSize);

				//bytes.AddRange(bStr);
				bytes.AddRange(dst.Take(compressedSize)); // 압축된 데이터를 추가
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				arrayTx.Enqueue(bytes.ToArray());
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public void TCON이미지쓰기4(byte[] bStr, int X, int Y, int W, int H)
		{
			try
			{
				if (bStr.Length > (2560 * 1440))
				{
					throw new Exception("TCON쓰기에서 바이트 크기가 초과하였습니다.");
				}

				byte[] dst = new byte[W * H * 2]; // 충분한 크기의 배열 초기화
				int compressedSize = RLE_EncodeBW(bStr, W * H, dst);

				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);

				//int nSize = (int)(3 + bStr.Length + 8);
				int nSize = (int)(3 + compressedSize + 8); // 압축된 크기를 사용
				byte[] byteSize = BitConvertUtils.FromInt32(nSize, true);
				bytes.Add(byteSize[2]);
				bytes.Add(byteSize[3]);

				bytes.Add((byte)1); // SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add(byteSize[1]); //bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)(SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기4));

				byteSize = BitConvertUtils.FromInt16((short)X, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)Y, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)W, true);
				bytes.AddRange(byteSize);
				byteSize = BitConvertUtils.FromInt16((short)H, true);
				bytes.AddRange(byteSize);

				//bytes.AddRange(bStr);
				bytes.AddRange(dst.Take(compressedSize)); // 압축된 데이터를 추가
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				arrayTx.Enqueue(bytes.ToArray());
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public int RLE_Encode(byte[] src, int pixel_count, byte[] dst)
		{
			int src_idx = 0;
			int dst_idx = 0;

			while (src_idx < pixel_count)
			{
				byte color = (byte)(src[src_idx] & 0xF0);  // 상위 4bit만 사용
				byte length = 1;

				// run 계산
				while ((src_idx + length < pixel_count) && ((src[src_idx + length] & 0xF0) == color) && (length < 255))
				{
					length++;
				}

				// RLE 기록
				dst[dst_idx++] = length;
				dst[dst_idx++] = color;

				src_idx += length;
			}

			return dst_idx; // 압축된 바이트 수
		}
		public int RLE_EncodeBW(byte[] src, int pixel_count, byte[] dst)
		{
			int src_idx = 0;
			int dst_idx = 0;

			while (src_idx < pixel_count)
			{
				// src 값이 0xC0 이상이면 0xFF, 아니면 0
				byte color = (src[src_idx] >= 0xC0) ? (byte)0xFF : (byte)0x00;
				byte length = 1;

				// run 계산
				while ((src_idx + length < pixel_count) &&
					   ((src[src_idx + length] >= 0xC0 ? (byte)0xFF : (byte)0x00) == color) &&
					   (length < 255))
				{
					length++;
				}

				// RLE 기록
				dst[dst_idx++] = length;
				dst[dst_idx++] = color;

				src_idx += length;
			}

			return dst_idx; // 압축된 바이트 수
		}


		/// <summary>
		/// 상태정보 요청 0x11
		/// </summary>
		public void Send상태정보요청()
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				byte[] byteSize = BitConvertUtils.FromInt16(3, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.상태정보요청);

				//DATA NULL
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[상태정보요청] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 장치설정요청 0x12 - PC제어 : 기본 (0) , PC리셋 (1)
		/// </summary>
		public void Send장치설정요청(byte bControl1, byte bControl2, int 주기값, int PC제어)
		{
		}

		/// <summary>
		/// 장치볼륨상태요청 0x15
		/// </summary>
		public void Send볼륨상태요청()
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.볼륨상태요청);
				//DATA NULL

				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[볼륨상태요청] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 장치볼륨설정 0x16 - 볼륨크기 0~10
		/// </summary>
		public void Send볼륨설정(int 볼륨크기)
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 1;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.볼륨상태설정);
				//DATA 
				byte bVolumns = (byte)볼륨크기;
				bytes.Add(bVolumns);

				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[볼륨상태설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 환경보드상태요청->보드시간요청 0x17
		/// </summary>
		public void Send환경보드시간요청()
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드시간요청);
				//DATA NULL
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[보드시간요청] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 환경보드상태요청->보드시간요청 0x18
		/// </summary>
		public void Send환경보드시간설정(string mDateTime)
		{
			try
			{
				if (mDateTime.Length != 14) return;

				//7E-7E-00-11-01-00-18-32-30-32-32-30-35-31-32-31-30-30-36-32-32-0B
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 14;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드시간설정);
				//DATA
				byte[] bRTC = Encoding.Default.GetBytes(mDateTime);
				bytes.AddRange(bRTC);
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));
				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[보드시간설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 환경보드설정값요청 0x19
		/// </summary>
		public void Send환경보드설정값요청()
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드설정값);
				//DATA NULL
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[ENV설정값요청] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// DC FAN설정 0x1A - 0 : 수동 , 1 : 자동  , 온도는 -128 ~ 127
		/// </summary>
		public void SendDCFAN설정(int 동작Mode, int ON온도, int OFF온도)
		{
			try
			{
				//7E-7E-00-06-01-00-1A-00-1B-19-1F : DC FAN 설정                
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 3;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드설정FAN);
				//DATA 
				byte bMode = (byte)동작Mode;
				byte bOn = (byte)ON온도;
				byte bOff = (byte)OFF온도;
				bytes.Add(bMode);
				bytes.Add(bOn);
				bytes.Add(bOff);
				//
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));
				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[DCFAN설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 히터설정 0x1B -  0 : 수동 , 1 : 자동  , 온도는 -128 ~ 127
		/// </summary>
		public void Send히터설정(int 동작Mode, int ON온도, int OFF온도)
		{
			try
			{
				//7E-7E-00-06-01-00-1B-01-0A-32-25 : AC1 히터온도설정
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 3;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드설정히터);
				//DATA 
				byte bMode = (byte)동작Mode;
				byte bOn = (byte)ON온도;
				byte bOff = (byte)OFF온도;
				bytes.Add(bMode);
				bytes.Add(bOn);
				bytes.Add(bOff);
				//
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));
				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[히터설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// LAMP설정 0x1C - 0 : 수동 , 1 : 자동 
		/// </summary>
		public void SendLAMP설정(int 동작Mode, int ON시간, int ON분, int OFF시간, int OFF분)
		{
			try
			{
				//7E-7E-00-06-01-00-1B-01-0A-32-25 : AC1 히터온도설정
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 5;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드설정LAMP);
				//DATA 
				byte bMode = (byte)동작Mode;
				byte bOnHH = (byte)ON시간;
				byte bOnMM = (byte)ON분;
				byte bOffHH = (byte)OFF시간;
				byte bOffMM = (byte)OFF분;
				bytes.Add(bMode);
				bytes.Add(bOnHH);
				bytes.Add(bOnMM);
				bytes.Add(bOffHH);
				bytes.Add(bOffMM);
				//
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));
				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[LAMP설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 화면장치설정 0x1D - 0 : 수동 , 1 : 자동 
		/// </summary>
		public void Send화면장치설정(int 동작Mode, int ON시간, int ON분, int OFF시간, int OFF분)
		{
			try
			{
				//7E-7E-00-06-01-00-1B-01-0A-32-25 : AC1 히터온도설정
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				short nSize = 3 + 5;
				byte[] byteSize = BitConvertUtils.FromInt16(nSize, true);
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV보드설정화면장치);
				//DATA 
				byte bMode = (byte)동작Mode;
				byte bOnHH = (byte)ON시간;
				byte bOnMM = (byte)ON분;
				byte bOffHH = (byte)OFF시간;
				byte bOffMM = (byte)OFF분;
				bytes.Add(bMode);
				bytes.Add(bOnHH);
				bytes.Add(bOnMM);
				bytes.Add(bOffHH);
				bytes.Add(bOffMM);
				//
				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));
				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[화면장치설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		/// <summary>
		/// 주기값외 설정 0x24
		/// </summary>
		public void Send주기및충격값설정(int 주기값, int 충격값, int 조도값, int 휘도값)
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				byte[] byteSize = BitConvertUtils.FromInt16(3 + 4, true); //데이터 4byte
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV주기값등설정);
				//DATA 
				byte bPeriod = (byte)주기값;
				byte bShock = (byte)충격값;
				byte bLuminance = (byte)조도값;
				byte bIllumination = (byte)휘도값;
				bytes.Add(bPeriod);
				bytes.Add(bShock);
				bytes.Add(bLuminance);
				bytes.Add(bIllumination);

				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[주기값등설정] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}

		public void Send주기충격조도휘도값요청()
		{
			try
			{
				List<byte> bytes = new List<byte>();
				bytes.Add(STX);
				bytes.Add(STX);
				byte[] byteSize = BitConvertUtils.FromInt16(3, true); //데이터 4byte
				bytes.AddRange(byteSize);
				bytes.Add((byte)SSNS_ENV2_MACHINE_TYPE.ENV_BOARD);
				bytes.Add((byte)SSNS_ENV2_EVENT_TYPE.DEFAULT);
				bytes.Add((byte)SSNS_ENV2_OPCODE_TYPE.ENV주기정보요청);
				//DATA              

				bytes.Add(BitConvertUtils.CreateCheckSum(bytes.ToArray(), bytes.Count));

				string hexString = BitConvertUtils.ByteArrayToHexSTR(bytes.ToArray(), "-");
				Console.WriteLine(hexString);
				if (mENV2 == null)
				{
					_logger.LogError("[주기값등요청] 환경보드 미접속 : {0}", hexString);
				}
				else
				{
					arrayTx.Enqueue(bytes.ToArray());
				}
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}
		#endregion

		#region ENV To PC 
		public SSNS_ENV2_STATE Recv상태정보(byte[] bytes)
		{
			try
			{
				if (bytes.Length < 22)
					return null;

				SSNS_ENV2_STATE item = new SSNS_ENV2_STATE();

				int sIndex = 0;
				item.nVersion = BitConverter.ToInt16(bytes, sIndex); sIndex += 2;

				item.MainVoltage = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //메인전원
				item.MainCurrent = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //메인전류
				item.BattVoltage = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //배터리전원
				item.BattCurrent = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //배터리전류
				item.PanlVoltage = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //패널전압
				item.PanlCurrent = ((float)BitConverter.ToInt16(bytes, sIndex)) / 100; sIndex += 2; //패널전류

				item.Temparature1 = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Humidity = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Luminance = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Shock1 = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Door = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Illumination = (sbyte)bytes[sIndex]; sIndex += 1;
				item.Temparature2 = (sbyte)bytes[sIndex]; sIndex += 1;
				item.BattPercent = (sbyte)bytes[sIndex]; sIndex += 1;
				item.RemainBat = BitConverter.ToInt16(bytes, sIndex); sIndex += 2;

				//System.Collections.BitArray ba1 = new System.Collections.BitArray(BitConverter.GetBytes((short)item.baControl1).ToArray());
				//System.Collections.BitArray ba2 = new System.Collections.BitArray(BitConverter.GetBytes((short)item.baControl2).ToArray());

				//SSNS_ENV2_CONTROL data = new SSNS_ENV2_CONTROL();
				//for (int i = 0; i < ba1.Length; i++)
				//{
				//	switch (i)
				//	{
				//		case 0: data.bAC5 = ba1[i]; break;
				//		case 1: data.bDCADJ = ba1[i]; break;
				//		case 7: data.bDC12V3 = ba1[i]; break;
				//	}
				//}

				//item.Controls = data;

				return item;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return null;
			}
		}

		public SSNS_ENV2_CONFIG RecvENV보드설정값(byte[] bytes)
		{
			try
			{
				if (bytes.Length != 16)
				{
					return null;
				}
				SSNS_ENV2_CONFIG item = new SSNS_ENV2_CONFIG();

				item.nFanMode = (int)bytes[0];
				item.nFANOnTemp = (int)bytes[1];
				item.nFANOffTemp = (int)bytes[2];

				item.nHeaterMode = (int)bytes[3];
				item.nHeaterOnTemp = (int)bytes[4];
				item.nHeaterOffTemp = (int)bytes[5];

				item.nLampMode = (int)bytes[6];
				item.nLampOnHour = (int)bytes[7];
				item.nLampOnMin = (int)bytes[8];
				item.nLampOffHour = (int)bytes[9];
				item.nLampOffMin = (int)bytes[10];

				item.nScreenMode = (int)bytes[11];
				item.nScreenOnHour = (int)bytes[12];
				item.nScreenOnMin = (int)bytes[13];
				item.nScreenOffHour = (int)bytes[14];
				item.nScreenOffMin = (int)bytes[15];

				return item;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return null;
			}
		}
		public SSNS_ENV2_SENSOR Convert충격값정보ByteArray(byte[] bytes)
		{
			try
			{
				if (bytes.Length != 4)
				{
					return null;
				}

				SSNS_ENV2_SENSOR item = new SSNS_ENV2_SENSOR();

				item.SetPeriod = (int)bytes[0];
				item.SetShock = (int)bytes[1];
				item.SetIllumination = (int)bytes[2];
				item.SetLuminance = (int)bytes[3];
				return item;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return null;
			}
		}

		public SSNS_ENV2_BOARD_STATE Convert보드상태정보ByteArray(byte[] bytes)
		{
			try
			{
				if (bytes.Length != 30)
				{
					return null;
				}

				SSNS_ENV2_BOARD_STATE item = new SSNS_ENV2_BOARD_STATE();

				byte[] data1 = new byte[14];
				byte[] data2 = new byte[14];
				item.nNone1 = (int)bytes[14];
				item.nNone2 = (int)bytes[15];
				Buffer.BlockCopy(bytes, 0, data1, 0, data1.Length);
				Buffer.BlockCopy(bytes, 16, data2, 0, data2.Length);

				item.mActiveDateTime = Encoding.Default.GetString(data1);
				item.mBoardDateTime = Encoding.Default.GetString(data2);

				return item;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return null;
			}
		}

		#endregion

		//public delegate void 이벤트정보Handler(SSNS_ENV2_EVENT_TYPE _data);
		//public static event  이벤트정보Handler On이벤트정보Event;

		// 글로벌 저장소
		public  SSNS_ENV2_STATE EnvData { get; private set; } = new SSNS_ENV2_STATE();

		// 상태 정보 업데이트 메서드
		private  void UpdateEnvData(SSNS_ENV2_STATE newState)
		{
			if (newState != null)
			{
				EnvData = newState;
				//System.Diagnostics.Debug.WriteLine($"EnvData updated: 온도1={EnvData.Temparature1}, 온도2={EnvData.Temparature2}");
			}
		}

		 int 재시도횟수 = 0;
		private  void mENV2_OnDataReceivedEvent(byte[] receiveData)
		{
			try
			{
				SSNS_ENV2_PROTOCOL data = CreateENV2프로토콜ByteArray(receiveData); // parsing
				if (data.RESULT_CD.Equals(1) == false)
				{
					_logger.LogInformation("[{0}] {1}", data.RESULT_CD, data.RESULT_MSG);
					return;
				}

				_logger.LogInformation("{0:X2}=>{1} Len:{2}", data.OPCODE, data.OP_GBN.ToString(), data.DATA.Length);

				switch (data.OP_GBN)
				{
					case SSNS_ENV2_OPCODE_TYPE.ENV보드시간요청:
						SSNS_ENV2_BOARD_STATE item보드상태 = Convert보드상태정보ByteArray(data.DATA);
						if (item보드상태 != null && On보드상태정보Event != null) On보드상태정보Event(item보드상태);
						break;

					case SSNS_ENV2_OPCODE_TYPE.볼륨상태설정:
					case SSNS_ENV2_OPCODE_TYPE.볼륨상태요청:
						if (data.DATA.Length > 0)
						{ }
						break;

					case SSNS_ENV2_OPCODE_TYPE.상태정보응답:
						switch (data.EVENT_GBN)
						{
							case SSNS_ENV2_EVENT_TYPE.SHOCK_EVENT:
								SSNS_ENV2_STATE item상태2 = Recv상태정보(data.DATA);
								if (item상태2 != null && On상태정보Event != null) On상태정보Event(item상태2);
								break;

							case SSNS_ENV2_EVENT_TYPE.DOOR_EVENT:
								SSNS_ENV2_STATE item상태3 = Recv상태정보(data.DATA);
								if (item상태3 != null && On상태정보Event != null) On상태정보Event(item상태3);
								break;

							default:
								SSNS_ENV2_STATE item상태 = Recv상태정보(data.DATA);
								if (item상태 != null)
								{
									On상태정보Event?.Invoke(item상태); // 기존 이벤트 호출
									UpdateEnvData(item상태);           // 추가로 글로벌 저장소 업데이트
								}
								_logger.LogInformation("[UART] Rx {0:X2}=>{1}, Len:{2}", data.OPCODE, data.OP_GBN.ToString(), data.DATA.Length);
								break;
						}

						processKillTimerSecondCount = 0;
						break;

					case SSNS_ENV2_OPCODE_TYPE.ENV보드설정값:
						SSNS_ENV2_CONFIG item설정 = RecvENV보드설정값(data.DATA);
						if (item설정 != null && On설정정보Event != null) On설정정보Event(item설정);
						break;

					case SSNS_ENV2_OPCODE_TYPE.ENV주기정보요청:
					case SSNS_ENV2_OPCODE_TYPE.ENV주기값등설정:
						SSNS_ENV2_SENSOR item센서 = Convert충격값정보ByteArray(data.DATA);
						if (item센서 != null && On센서정보Event != null) On센서정보Event(item센서);
						break;
					case SSNS_ENV2_OPCODE_TYPE.PC_PWR_OFF_EVENT:
						if (data.DATA[0] == 0xa5)   // magic
						{
							TraceManager.AddInfoLog(string.Format("[전원종료] 전원종료를 시도합니다. {0}회", 재시도횟수++));
							System.Diagnostics.Process.Start("shutdown", "-s -f -t 10");    // after 10sec, pc off
						}
						break;

					case SSNS_ENV2_OPCODE_TYPE.ENV보드설정FAN:
						if (data.DATA.Length > 0)
						{ }
						break;

					case SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기:
					case SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기1:
					case SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기2:
					case SSNS_ENV2_OPCODE_TYPE.TCON_비트맵_쓰기3:
						_logger.LogInformation("[UART] Rx TCON Count = {0}, Ack = {1}", arrayTx.Count, data.DATA[0]);
						break;

				}
				bResponseTconOk = true;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
		}


		const byte STX = 0x7E;
		public SSNS_ENV2_PROTOCOL CreateENV2프로토콜ByteArray(byte[] bytes)
		{
			try
			{
				SSNS_ENV2_PROTOCOL item = new SSNS_ENV2_PROTOCOL();
				item.STX1 = bytes[0];
				item.STX2 = bytes[1];
				item.SIZE = new byte[2];
				item.SIZE[0] = bytes[2];
				item.SIZE[1] = bytes[3];

				int SZ = BitConvertUtils.ToInt16(item.SIZE) - 3;

				item.MACHINE_CODE = bytes[4];
				item.MC_GBN = (SSNS_ENV2_MACHINE_TYPE)item.MACHINE_CODE;
				item.CTRL_OPCODE = bytes[5];
				item.EVENT_GBN = (SSNS_ENV2_EVENT_TYPE)item.CTRL_OPCODE;
				item.OPCODE = bytes[6];
				item.OP_GBN = (SSNS_ENV2_OPCODE_TYPE)item.OPCODE;
				item.DATA = new byte[SZ];

				for (int idxNo = 0; idxNo < SZ; idxNo++)
					item.DATA[idxNo] = bytes[idxNo + 7];

				item.CHECK_SUM = bytes[bytes.Length - 1];

				item.RESULT_CD = 1;
				item.RESULT_MSG = "변환완료";
				return item;
			}
			catch (Exception ee)
			{
				TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				return new SSNS_ENV2_PROTOCOL() { RESULT_CD = -1, RESULT_MSG = "Exception" };
			}
		}

	}
}
