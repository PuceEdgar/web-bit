using System;
using System.Collections.Generic;
using System.Text;

namespace BitDisplayDaemo.SSNS_ENV
{
	public class BitConvertUtils
	{
		public static byte CreateCheckSum(byte[] bytes, int bLength)
		{
			try
			{
				byte checkSum = 0;
				for (int index = 0; index < bLength; ++index)
					checkSum ^= bytes[index];
				return checkSum;
			}
			catch (Exception ex)
			{
				return 0;
			}
		}

		public static short ToInt16(byte[] bArray, bool IsLittleEndian = true)
		{
			try
			{
				if (IsLittleEndian)
					bArray = ((IEnumerable<byte>)bArray).Reverse<byte>().ToArray<byte>();
				return BitConverter.ToInt16(bArray, 0);
			}
			catch (Exception ex)
			{
				return -1;
			}
		}

		public static byte[] FromInt16(short nNo, bool IsLittleEndian = false)
		{
			try
			{
				byte[] source = BitConverter.GetBytes(nNo);
				if (IsLittleEndian)
					source = ((IEnumerable<byte>)source).Reverse<byte>().ToArray<byte>();
				return source;
			}
			catch (Exception ex)
			{
				return (byte[])null;
			}
		}

		public static int ToInt32(byte[] bArray, bool IsLittleEndian = true)
		{
			try
			{
				if (IsLittleEndian)
					bArray = ((IEnumerable<byte>)bArray).Reverse<byte>().ToArray<byte>();
				return BitConverter.ToInt32(bArray, 0);
			}
			catch (Exception ex)
			{
				return -1;
			}
		}

		public static byte[] FromInt32(int nNo, bool IsLittleEndian = false)
		{
			try
			{
				byte[] source = BitConverter.GetBytes(nNo);
				if (IsLittleEndian)
					source = ((IEnumerable<byte>)source).Reverse<byte>().ToArray<byte>();
				return source;
			}
			catch (Exception ex)
			{
				return (byte[])null;
			}
		}

		public static string ToSTR(byte[] bArray) => BitConvertUtils.ToSTR(bArray, Encoding.ASCII);

		public static string ToSTR(byte[] bArray, Encoding enc)
		{
			try
			{
				return enc.GetString(bArray).Replace("\0", "");
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		public static byte[] FromSTR(string mStr, int fixByteLength)
		{
			return BitConvertUtils.FromSTR(mStr, fixByteLength, BitDummyType.NULL, Encoding.ASCII);
		}

		public static byte[] FromSTR(string mStr, int fixByteLength, BitDummyType dummyGBN)
		{
			return BitConvertUtils.FromSTR(mStr, fixByteLength, dummyGBN, Encoding.ASCII);
		}

		public static byte[] FromSTR(
		  string mStr,
		  int fixByteLength,
		  BitDummyType dummyGBN,
		  Encoding enc)
		{
			try
			{
				byte[] numArray = new byte[fixByteLength];
				if (mStr == null && dummyGBN == BitDummyType.NULL)
					return numArray;
				if (mStr == null && dummyGBN == BitDummyType.SPACE)
				{
					for (int index = 0; index < fixByteLength; ++index)
						numArray[index] = (byte)32;
					return numArray;
				}
				enc.GetBytes(mStr).CopyTo((Array)numArray, 0);
				enc.GetBytes(mStr.PadRight(15, '0'));
				return numArray;
			}
			catch (Exception ex)
			{
				return (byte[])null;
			}
		}

		public static string ByteArrayToHexSTR(byte[] bArray, string splitStr = " ")
		{
			try
			{
				List<string> stringList = new List<string>();
				foreach (byte b in bArray)
					stringList.Add(Convert.ToString(b, 16).PadLeft(2, '0').ToUpper());
				return string.Join(splitStr, stringList.ToArray());
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		public static byte[] HexStringToByteArray(string logString, char splitCh = ' ')
		{
			try
			{
				List<byte> byteList = new List<byte>();
				List<string> list = ((IEnumerable<string>)logString.Split(splitCh)).ToList<string>();
				int num1 = 0;
				foreach (string str1 in list)
				{
					++num1;
					string str2 = str1.Replace("\r\n", "");
					if (!str2.Equals(""))
					{
						byte num2 = Convert.ToByte(str2, 16);
						byteList.Add(num2);
					}
				}
				return byteList != null && byteList.Count > 0 ? byteList.ToArray() : (byte[])null;
			}
			catch (Exception ex)
			{
				return (byte[])null;
			}
		}

		public static byte[] HexStringToByteArray(string logString)
		{
			try
			{
				byte[] byteArray = new byte[logString.Length / 2];
				for (int index = 0; index < byteArray.Length; ++index)
					byteArray[index] = Convert.ToByte(logString.Substring(index * 2, 2), 16);
				return byteArray;
			}
			catch (Exception ex)
			{
				return (byte[])null;
			}
		}
	}

	public enum BitDummyType : byte
	{
		NULL = 0,
		SPACE = 32, // 0x20
	}
}
