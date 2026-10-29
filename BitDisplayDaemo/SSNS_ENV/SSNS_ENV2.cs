using System;
using System.Collections.Generic;
using System.Text;

namespace BitDisplayDaemo.SSNS_ENV
{
	public struct SSNS_ENV2_PROTOCOL
	{
		public byte STX1;
		public byte STX2;
		/// <summary>
		/// SIZE : 2Byte
		/// </summary>
		public byte[] SIZE;
		public byte MACHINE_CODE;
		public byte CTRL_OPCODE;
		public byte OPCODE;
		/// <summary>
		/// DATA : NByte
		/// </summary>
		public byte[] DATA;
		public byte CHECK_SUM;

		//내부용
		public int RESULT_CD;
		public string RESULT_MSG;
		public SSNS_ENV2_MACHINE_TYPE MC_GBN;
		public SSNS_ENV2_EVENT_TYPE EVENT_GBN;
		public SSNS_ENV2_OPCODE_TYPE OP_GBN;
		//public SSNS_ENV2_ToRTU_OPCODE_TYPE TO_GBN;
		//public SSNS_ENV2_FromRTU_OPCODE_TYPE FROM_GBN;
	}

	public enum SSNS_ENV2_MACHINE_TYPE : byte
	{
		BIT = 0x00,
		ENV_BOARD = 0x1,
	}

	public enum SSNS_ENV2_EVENT_TYPE : byte
	{
		DEFAULT = 0x00,
		SHOCK_EVENT = 0x30, //설정
		DOOR_EVENT = 0x31,
	}

	//public enum SSNS_ENV2_ToRTU_OPCODE_TYPE : byte
	//{
	//    상태정보 = 0x11,
	//    장치설정 = 0x12,
	//    볼륨 = 0x15,
	//    볼륨설정 = 0x16,
	//    ENV보드상태 = 0x17,
	//    ENV보드시간설정 = 0x18,
	//    ENV보드설정 = 0x19,
	//    ENV보드설정FAN = 0x1A,
	//    ENV보드설정히터 = 0x1B,
	//    ENV보드설정LAMP = 0x1C,
	//    ENV보드설정LCD = 0x1D,
	//    주기설정정보 = 0x24,
	//    ACK = 0x06,
	//    NAK = 0x05,
	//    //내부용
	//    DEFAULT = 0x00,
	//}
	public enum SSNS_ENV2_OPCODE_TYPE : byte
	{
		//내부용
		DEFAULT = 0x00,
		/// <summary>
		/// 이벤트수신시 응답
		/// </summary>
		ACK = 0x06,
		/// <summary>
		/// 상태정보 요청 To RTU : ACK 0x13
		/// </summary>
		상태정보요청 = 0x11,
		/// <summary>
		/// 장치제어 요청 To RTU : ACK 0x13
		/// </summary>
		장치제어설정 = 0x12,
		/// <summary>
		/// 상태정보 전송 To PC 
		/// </summary>
		상태정보응답 = 0x13,
		/// <summary>
		/// 장치볼륨상태 요청 To RTU : ACK 0x15
		/// </summary>
		볼륨상태요청 = 0x15,
		/// <summary>
		/// 장치볼륨상태 설정 To RTU : ACK 0x16
		/// </summary>
		볼륨상태설정 = 0x16,
		/// <summary>
		/// 환경보드시간청 <- 환경보드상태요청응답 To PC 
		/// </summary>
		ENV보드시간요청 = 0x17,
		ENV보드시간응답 = 0x17,
		/// <summary>
		/// 환경보드시간 설정 To RTU : ACK 0x18
		/// </summary>
		ENV보드시간설정 = 0x18,
		/// <summary>
		/// 환경보드설정값 요청 To RTU : ACK 0x19
		/// 환경보드설정값 응답 To PC
		/// </summary>
		ENV보드설정값 = 0x19,
		/// <summary>
		/// 환경보드FAN설정값 요청 To RTU : ACK 0x1A
		/// 환경보드FAN설정값 응답 To PC
		/// </summary>
		ENV보드설정FAN = 0x1A,
		/// <summary>
		/// 환경보드히터설정값 요청 To RTU : ACK 0x1B
		/// 환경보드히터설정값 응답 To PC
		/// </summary>
		ENV보드설정히터 = 0x1B,
		/// <summary>
		/// 환경보드LAMP설정값 요청 To RTU : ACK 0x1C
		/// 환경보드LAMP설정값 응답 To PC
		/// </summary>
		ENV보드설정LAMP = 0x1C,
		/// <summary>
		/// 환경보드화면장치(DISPLAY)설정값 요청 To RTU : ACK 0x1D
		/// 환경보드화면장치(DISPLAY)설정값 응답 To PC
		/// </summary>
		ENV보드설정화면장치 = 0x1D,

		/// <summary>
		/// 상태정보 전송주기 설정 기존 : 분
		/// </summary>
		ENV주기값등설정 = 0x24,

		/// <summary>
		/// ENV주기정보요청 To RTU 
		/// </summary>
		ENV주기정보요청 = 0x25,

		///// <summary>
		///// 충격센서 이벤트 To PC : 기본적인 충격값은 상태정보 0x13 
		///// </summary>
		//충격센서EVENT = 0x30,

		///// <summary>
		///// 도어센서 이벤트 To PC : 기본적인 충격값은 상태정보 0x13 
		///// </summary>
		//도어센서EVENT = 0x31,

		/// <summary>
		/// 키패드 입력 이벤트 To PC : ACK 0x06 OR 0x05        
		/// </summary>
		키패드입력EVENT = 0x32,
		//키패드입력ACK = 0x06,
		//키패드입력NAK = 0x05,

		/// <summary>
		/// PC POWER OFF
		/// </summary>
		PC_PWR_OFF_EVENT = 0x33,

		/// <summary>
		/// TCON IMAGE WRITE To RTU :
		/// </summary>
		TCON_비트맵_쓰기 = 0x40,
		TCON_비트맵_쓰기1 = 0x41,
		TCON_비트맵_쓰기2 = 0x42,
		TCON_비트맵_쓰기3 = 0x43,
		TCON_비트맵_쓰기4 = 0x44,
	}

	//ENV2 장치의 상태 값
	public class SSNS_ENV2_STATE
	{
		public int nVersion { get; set; }
		public string 버전정보
		{
			get
			{
				string disp = string.Format("V {0}.{1}", (nVersion >> 8), (nVersion & 0xff));
				return disp;
			}
		}
		public float MainVoltage { get; set; }
		public float MainCurrent { get; set; }
		public float BattVoltage { get; set; }
		public float BattCurrent { get; set; }
		public float PanlVoltage { get; set; }
		public float PanlCurrent { get; set; }
		public sbyte Temparature1 { get; set; }
		public sbyte Temparature2 { get; set; }
		public int Humidity { get; set; }
		public int Luminance { get; set; }
		public int Shock1 { get; set; }
		public int Door { get; set; }
		public int Illumination { get; set; }
		public int BattPercent { get; set; }
		public int RemainBat { get; set; }

		public string 메인전원
		{
			get
			{
				string disp = string.Format("{0:2} v", (nMainPower * 4 * 30 / 1024) + 0.5);
				return disp;
			}
		}
		public int nMainPower { get; set; }
		public int nBattPower { get; set; }
		public string 보조전원
		{
			get
			{
				string disp = string.Format("{0:2} v", (nBattPower * 4 * 30 / 1024) + 0.5);
				return disp;
			}
		}
		public string 온도1
		{
			get
			{
				string disp = string.Format("{0} °C", Temparature1);
				return disp;
			}
		}
		public string 온도2
		{
			get
			{
				string disp = string.Format("{0} °C", Temparature2);
				return disp;
			}
		}
		public string 습도
		{
			get
			{
				string disp = string.Format("{0} %", Humidity);
				return disp;
			}
		}
		public string 조도
		{
			get
			{
				string disp = string.Format("{0} nit", Luminance);
				return disp;
			}
		}
		public string 충격1
		{
			get
			{
				string disp = string.Format("{0} kg·m/s", Shock1);
				return disp;
			}
		}
		public string 휘도
		{
			get
			{
				string disp = string.Format("{0} lx", Illumination);
				return disp;
			}
		}
		/// <summary>
		/// 예비1
		/// </summary>
		public int nNone1 { get; set; }
		/// <summary>
		/// 예비2
		/// </summary>
		public int nNone2 { get; set; }
		public string 도어상태
		{
			get
			{
				return Door == 1 ? "Open" : "Close";
			}
		}
		public byte baControl1 { get; set; }
		public byte baControl2 { get; set; }
		public SSNS_ENV2_CONTROL Controls { get; set; }
		public int nPeriod { get; set; }
		public string 주기제어
		{
			get
			{
				string disp = string.Format("{0}분", nPeriod);
				return disp;
			}
		}
	}

	public class SSNS_ENV2_CONFIG
	{
		public int nFanMode { get; set; }
		public string FAN동작모드
		{
			get
			{
				string disp = nFanMode == 1 ? "자동" : "수동";
				return disp;
			}
		}
		public int nHeaterMode { get; set; }
		public string 히터동작모드
		{
			get
			{
				string disp = nHeaterMode == 1 ? "자동" : "수동";
				return disp;
			}
		}
		public int nLampMode { get; set; }
		public string LAMP동작모드
		{
			get
			{
				string disp = nLampMode == 1 ? "자동" : "수동";
				return disp;
			}
		}
		public int nFANOnTemp { get; set; }
		public string FAN동작ON온도
		{
			get
			{
				string disp = string.Format("{0} °C", nFANOnTemp);
				return disp;
			}
		}
		public int nFANOffTemp { get; set; }
		public string FAN동작OFF온도
		{
			get
			{
				string disp = string.Format("{0} °C", nFANOffTemp);
				return disp;
			}
		}
		public int nHeaterOnTemp { get; set; }
		public string 히터동작ON온도
		{
			get
			{
				string disp = string.Format("{0} °C", nHeaterOnTemp);
				return disp;
			}
		}
		public int nHeaterOffTemp { get; set; }
		public string 히터동작OFF온도
		{
			get
			{
				string disp = string.Format("{0} °C", nHeaterOffTemp);
				return disp;
			}
		}
		public int nLampOnHour { get; set; }
		public int nLampOnMin { get; set; }
		public string LAMPON시간
		{
			get
			{
				string disp = string.Format("{0}시 {1}분", nLampOnHour, nLampOnMin);
				return disp;
			}
		}
		public int nLampOffHour { get; set; }
		public int nLampOffMin { get; set; }
		public string LAMPOFF시간
		{
			get
			{
				string disp = string.Format("{0}시 {1}분", nLampOffHour, nLampOffMin);
				return disp;
			}
		}
		public int nScreenMode { get; set; }
		public string 화면장치동작모드
		{
			get
			{
				string disp = nScreenMode == 1 ? "자동" : "수동";
				return disp;
			}
		}
		public int nScreenOnHour { get; set; }
		public int nScreenOnMin { get; set; }
		public string 화면장치ON시간
		{
			get
			{
				string disp = string.Format("{0}시 {1}분", nScreenOnHour, nScreenOnMin);
				return disp;
			}
		}
		public int nScreenOffHour { get; set; }
		public int nScreenOffMin { get; set; }
		public string 화면장치OFF시간
		{
			get
			{
				string disp = string.Format("{0}시 {1}분", nScreenOffHour, nScreenOffMin);
				return disp;
			}
		}
	}

	public class SSNS_ENV2_CONTROL
	{
		public bool bAC5 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string AC5
		{
			get
			{
				return bAC5 == false ? "OFF" : "ON";
			}
		}
		public bool bDCADJ { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DCADJ
		{
			get
			{
				return bDCADJ == false ? "OFF" : "ON";
			}
		}
		public bool bDC5V1 { get; set; }
		/// <summary>
		/// DC5V1 
		/// </summary>
		public string DC5V1
		{
			get
			{
				return bDC5V1 == false ? "OFF" : "ON";
			}
		}
		public bool bDC5V2 { get; set; }
		/// <summary>
		/// DC5V2 
		/// </summary>
		public string DC5V2
		{
			get
			{
				return bDC5V2 == false ? "OFF" : "ON";
			}
		}
		public bool bDC5V3 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DC5V3
		{
			get
			{
				return bDC5V3 == false ? "OFF" : "ON";
			}
		}
		public bool bDC12V1 { get; set; }
		/// <summary>
		/// DC12V1 (메인PC)
		/// </summary>
		public string DC12V1
		{
			get
			{
				return bDC12V1 == false ? "OFF" : "ON";
			}
		}
		public bool bDC12V2 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DC12V2
		{
			get
			{
				return bDC12V2 == false ? "OFF" : "ON";
			}
		}
		public bool bDC12V3 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DC12V3
		{
			get
			{
				return bDC12V3 == false ? "OFF" : "ON";
			}
		}
		//2번째 라인
		public bool bDC24V1 { get; set; }
		/// <summary>
		/// DC24V1 (메인PC)
		/// </summary>
		public string DC24V1
		{
			get
			{
				return bDC24V1 == false ? "OFF" : "ON";
			}
		}
		public bool bDC24V2 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DC24V2
		{
			get
			{
				return bDC24V2 == false ? "OFF" : "ON";
			}
		}

		public bool bDCFAN { get; set; }
		/// <summary>
		/// 사용
		/// </summary>
		public string DCFAN
		{
			get
			{
				return bDCFAN == false ? "OFF" : "ON";
			}
		}

		public bool bAC1 { get; set; }
		/// <summary>
		/// 사용
		/// </summary>
		public string AC1
		{
			get
			{
				return bAC1 == false ? "OFF" : "ON";
			}
		}
		public bool bAC2 { get; set; }
		/// <summary>
		/// 사용
		/// </summary>
		public string AC2
		{
			get
			{
				return bAC2 == false ? "OFF" : "ON";
			}
		}
		public bool bAC3 { get; set; }
		/// <summary>
		/// 사용
		/// </summary>
		public string AC3
		{
			get
			{
				return bAC3 == false ? "OFF" : "ON";
			}
		}
		public bool bAC4 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string AC4
		{
			get
			{
				return bAC4 == false ? "OFF" : "ON";
			}
		}
		public bool bDC12V4 { get; set; }
		/// <summary>
		/// 미사용
		/// </summary>
		public string DC12V4
		{
			get
			{
				return bDC12V4 == false ? "OFF" : "ON";
			}
		}
	}

	public class SSNS_ENV2_SENSOR
	{
		/// <summary>
		/// 주기 설정값
		/// </summary>
		public int SetPeriod { get; set; }
		/// <summary>
		/// 충격 설정값
		/// </summary>
		public int SetShock { get; set; }
		/// <summary>
		/// 휘도 설정값
		/// </summary>
		public int SetLuminance { get; set; }

		/// <summary>
		/// 조도 설정값
		/// </summary>
		public int SetIllumination { get; set; }
	}

	public class SSNS_ENV2_BOARD_STATE
	{
		public string mActiveDateTime { get; set; }
		public string 초기동작시간
		{
			get
			{
				DateTime? dt = ConvertUtils.DateTimeByString(mActiveDateTime);
				if (dt.HasValue == true)
				{
					return dt.Value.ToString("yyyy-MM-dd HH:mm:sss");
				}
				return "";
			}
		}
		public int nNone1 { get; set; }
		public int nNone2 { get; set; }
		public string mBoardDateTime { get; set; }
		public string 환경보드시간
		{
			get
			{
				DateTime? dt = ConvertUtils.DateTimeByString(mBoardDateTime);
				if (dt.HasValue == true)
				{
					return dt.Value.ToString("yyyy-MM-dd HH:mm:sss");
				}
				return "";
			}
		}

	}

	public class ConvertUtils
	{
		public static DateTime? DateTimeByString(string YMD)
		{
			try
			{
				DateTime dateTime = new DateTime();
				if (YMD.Equals("") || YMD.Equals("00000000"))
					return new DateTime?();
				switch (YMD.Length)
				{
					case 6:
						dateTime = new DateTime(Convert.ToInt32(YMD.Substring(0, 2)) + 2000, Convert.ToInt32(YMD.Substring(2, 2)), Convert.ToInt32(YMD.Substring(4, 2)));
						break;
					case 8:
						dateTime = new DateTime(Convert.ToInt32(YMD.Substring(0, 4)), Convert.ToInt32(YMD.Substring(4, 2)), Convert.ToInt32(YMD.Substring(6, 2)));
						break;
					case 10:
						dateTime = new DateTime(Convert.ToInt32(YMD.Substring(0, 4)), Convert.ToInt32(YMD.Substring(4, 2)), Convert.ToInt32(YMD.Substring(6, 2)), Convert.ToInt32(YMD.Substring(8, 2)), 0, 0);
						break;
					case 12:
						dateTime = new DateTime(Convert.ToInt32(YMD.Substring(0, 4)), Convert.ToInt32(YMD.Substring(4, 2)), Convert.ToInt32(YMD.Substring(6, 2)), Convert.ToInt32(YMD.Substring(8, 2)), Convert.ToInt32(YMD.Substring(10, 2)), 0);
						break;
					case 14:
						dateTime = new DateTime(Convert.ToInt32(YMD.Substring(0, 4)), Convert.ToInt32(YMD.Substring(4, 2)), Convert.ToInt32(YMD.Substring(6, 2)), Convert.ToInt32(YMD.Substring(8, 2)), Convert.ToInt32(YMD.Substring(10, 2)), Convert.ToInt32(YMD.Substring(12, 2)));
						break;
					case 19:
						dateTime = Convert.ToDateTime(YMD);
						break;
					default:
						dateTime = Convert.ToDateTime(YMD);
						break;
				}
				return new DateTime?(dateTime);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return new DateTime?();
			}
		}

		public static string StringByDateTime(DateTime dtYMD)
		{
			try
			{
				return dtYMD.ToString("yyyyMMdd");
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return "00000000";
			}
		}

		public static string StringByObject(object objData)
		{
			try
			{
				return objData == DBNull.Value || objData == null ? "" : objData.ToString();
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return "";
			}
		}

		public static bool BoolByObject(object objData)
		{
			try
			{
				if (objData == DBNull.Value || objData == null || objData.ToString().ToUpper().Equals("F"))
					return false;
				return objData.ToString().ToUpper().Equals("T") || Convert.ToBoolean(objData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return false;
			}
		}

		public static string StringByBool(bool _isYN)
		{
			try
			{
				return _isYN ? "T" : "F";
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return "";
			}
		}

		public static int IntByBool(bool isYN)
		{
			try
			{
				return isYN ? 1 : 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return 0;
			}
		}

		public static int IntByObject(object objData)
		{
			try
			{
				return objData == DBNull.Value || objData == null || objData.ToString().Equals("") || objData.ToString().Equals(" ") || objData.Equals("-") ? 0 : Convert.ToInt32(objData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return 0;
			}
		}

		public static int IntByString(string strData)
		{
			try
			{
				switch (strData)
				{
					case null:
						return 0;
					case "":
						return 0;
					default:
						return Convert.ToInt32(strData);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return 0;
			}
		}

		public static double DoubleByObject(object objData)
		{
			try
			{
				return objData == DBNull.Value || objData == null ? 0.0 : Convert.ToDouble(objData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return 0.0;
			}
		}

		public static DateTime DateTimeByObject(object objData)
		{
			try
			{
				if (objData == DBNull.Value)
					return new DateTime();
				if (objData == null)
					return new DateTime();
				if (objData.Equals(""))
					return new DateTime();
				DateTime now = DateTime.Now;
				return Convert.ToDateTime(objData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("### {0}\r\n{1} ###", ex.StackTrace, ex.Message));
				return new DateTime();
			}
		}
		
	}
}
