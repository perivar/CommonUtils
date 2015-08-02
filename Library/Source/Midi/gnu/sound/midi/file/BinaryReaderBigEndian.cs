using System;
using System.Text;
using System.IO;
using System.Linq;

namespace gnu.sound.midi.file
{
	/// <summary>
	/// Description of BinaryReaderBigEndian.
	/// </summary>
	public class BinaryReaderBigEndian : BinaryReader {
		
		private byte[] a16 = new byte[2];
		private byte[] a32 = new byte[4];
		private byte[] a64 = new byte[8];
		
		public BinaryReaderBigEndian(System.IO.Stream stream)  : base(stream) { }
		
		public override int ReadInt32()
		{
			a32 = base.ReadBytes(4);
			Array.Reverse(a32);
			return BitConverter.ToInt32(a32,0);
		}
		
		public override Int16 ReadInt16()
		{
			a16 = base.ReadBytes(2);
			Array.Reverse(a16);
			return BitConverter.ToInt16(a16, 0);
		}
		
		public override Int64 ReadInt64()
		{
			a64 = base.ReadBytes(8);
			Array.Reverse(a64);
			return BitConverter.ToInt64(a64, 0);
		}
		
		public override UInt32 ReadUInt32()
		{
			a32 = base.ReadBytes(4);
			Array.Reverse(a32);
			return BitConverter.ToUInt32(a32, 0);
		}
		
		public sbyte[] ReadSBytes(int length) {
			var bytes = base.ReadBytes(length);
			var sbytes = Convert(bytes);
			return sbytes;
		}
		
		public static sbyte[] Convert(byte[] byteArray)
		{
			//sbyte[] sbytes = Array.ConvertAll(bytes, b => unchecked((sbyte)b));
			var sbyteArray = new sbyte[byteArray.Length];
			for (int i = 0; i < sbyteArray.Length; i++)
			{
				sbyteArray[i] = unchecked((sbyte) byteArray[i]);
			}

			return sbyteArray;
		}

		public static byte[] Convert(sbyte[] sbyteArray)
		{
			var byteArray = new byte[sbyteArray.Length];
			for (int i = 0; i < byteArray.Length; i++)
			{
				byteArray[i] = unchecked((byte) sbyteArray[i]);
			}
			return byteArray;
		}
		
		public static byte[] GetBytes(string str)
		{
			return Encoding.Default.GetBytes(str);
		}
		
		public static string GetString(byte[] bytes)
		{
			return Encoding.Default.GetString(bytes);
		}
	}
}
