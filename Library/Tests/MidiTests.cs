using System;
using System.IO;

using NUnit.Framework;

using gnu.sound.midi;
using gnu.sound.midi.file;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class MidiTests
	{
		[Test]
		public void TestReadWriteMidi()
		{
			string fileName = @"Tests\Passacaglia, Handel_Sample.mid";
			
			var fileIn = new FileInfo(fileName);
			var sequence = new MidiFileReader().GetSequence(fileIn);
			
			int trackno = 1;
			string outputTextPath = fileIn.Name + ".txt";
			using (var outfile = new StreamWriter(outputTextPath, false)) {
				foreach (var track in sequence.Tracks) {
					outfile.WriteLine("Track {0}", trackno);
					foreach(var ev in track.Events) {
						long tick = ev.Tick;
						int beat = (int) tick / sequence.Resolution;
						int tickRemainder = (int) tick % sequence.Resolution;
						MidiMessage msg = ev.Message;
						outfile.WriteLine("{0:0000}:{1:000} {2}", beat, tickRemainder, msg);
					}
					trackno++;
				}
			}
			
			string outputFileName = fileIn.DirectoryName + "\\" + fileIn.Name + "_generated.mid";
			var fileOut = new FileInfo(outputFileName);
			new MidiFileWriter().Write(sequence, sequence.MidiFileType, fileOut);
			
			if (FileCompare(fileIn.FullName, fileOut.FullName)) {
				Assert.Pass("The midi files are identical.");
			} else {
				Assert.Fail("The midi files are different!");
			}
		}
		
		private bool FileCompare(string srcFileName, string dstFileName)
		{
			const int BUFFER_SIZE = 1 << 16;
			
			var src = new FileInfo(srcFileName);
			var dst = new FileInfo(dstFileName);
			if ( src.Length != dst.Length )
				return false;
			
			using ( Stream srcStream = src.OpenRead(),
			       dstStream = dst.OpenRead() )
			{
				var srcBuf = new byte[BUFFER_SIZE];
				var dstBuf = new byte[BUFFER_SIZE];
				int len;
				while ((len = srcStream.Read(srcBuf, 0, srcBuf.Length)) > 0)
				{
					dstStream.Read(dstBuf, 0, dstBuf.Length);
					for ( int i = 0; i < len; i++)
						if ( srcBuf[i] != dstBuf[i])
							return false;
				}
				return true;
			}
		}
	}
}
