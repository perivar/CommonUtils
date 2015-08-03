using System;
using System.IO;

namespace gnu.sound.midi.file
{
	/// <summary>
	/// Description of BinaryWriterBigEndian.
	/// </summary>
	public class BinaryWriterBigEndian : BinaryWriter {
		
		public BinaryWriterBigEndian(Stream stream)  : base(stream) { }
		
		public override void Write(Int32 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);
			base.Write( bytes );
		}
		
		public override void Write(Int16 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);
			base.Write( bytes );
		}
		
		public override void Write(Int64 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);
			base.Write( bytes );
		}
		
		public override void Write(UInt32 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);
			base.Write( bytes );
		}
	}
}
