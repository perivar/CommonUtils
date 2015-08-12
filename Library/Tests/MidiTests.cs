using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using NUnit.Framework;

using gnu.sound.midi;
using gnu.sound.midi.file;

using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class MidiTests
	{
		[Test]
		public void TestReadWriteMidi()
		{
			string inputFileName = @"Tests\Passacaglia, Handel_Sample.mid";
			
			var fileIn = new FileInfo(inputFileName);
			var sequence = new MidiFileReader().GetSequence(fileIn);
			
			string outputTextPath = fileIn.Name + ".txt";
			sequence.DumpMidi(outputTextPath);
			
			string outputFileName = fileIn.Name + "_parsed.mid";
			sequence.Save(outputFileName);
			
			if (FileCompare(inputFileName, outputFileName)) {
				Assert.Pass("The midi files are identical.");
			} else {
				Assert.Fail("The midi files are different!");
			}
		}
		
		[Test]
		public void TestConvertMidi()
		{
			// File to test against (result file)
			string baselineFilePath = @"Tests\Passacaglia, Handel_Sample_Cubase_Format0.mid";
			var baselineFileInfo = new FileInfo(baselineFilePath);
			var sequenceBaseline = new MidiFileReader().GetSequence(baselineFileInfo);

			// Dump Baseline Midi
			string baselineTextPath = baselineFileInfo.Name + "_dump.txt";
			sequenceBaseline.DumpMidi(baselineTextPath);
			
			// File to test with
			string fileName = @"Tests\Passacaglia, Handel_Sample.mid";
			var fileInfo = new FileInfo(fileName);
			var sequence = new MidiFileReader().GetSequence(fileInfo);
			
			// Convert Midi to Format 0
			var convertedSequence = sequence.Convert((int) MidiHelper.MidiFormat.SingleTrack, SequenceExtensions.FormatConversionOption.NoteOffZero2NoteOnZero, 480, "SongNameForType0");
			
			// Dump Converted Midi
			string convertedOutputTextPath = fileInfo.Name + "_converted_dump.txt";
			convertedSequence.DumpMidi(convertedOutputTextPath);
			
			if (FileCompare(baselineTextPath, convertedOutputTextPath)) {
				Assert.Pass("The midi files are identical.");
			} else {
				Assert.Fail("The midi files are different!");
			}
		}
		
		[Test]
		public void TestGenerateMidi()
		{
			//string fileName = @"Tests\Passacaglia, Handel_Sample.mid";
			string fileName = @"Tests\agnes-release_me.mid";
			
			var fileIn = new FileInfo(fileName);
			var sequence = new MidiFileReader().GetSequence(fileIn);
			
			string outputTextPath = fileIn.Name + "_dump.txt";
			sequence.DumpMidi(outputTextPath);
			
			// Generate C# Code
			string outputCodePath = fileIn.Name + "_code.cs";
			sequence.SaveGenerateMidiCode(outputCodePath);

			if (!CompileAndRunSource(outputCodePath)) {
				Assert.Fail("Could not compile and run generated code!");
			}
			
			// read the generated midi dump file and compare against original
			if (!FileCompare(outputTextPath, "generated_dump.txt")) {
				Assert.Fail("The midi dump files are different!");
			}

			// read the generated midi file and compare against original
			//if (!FileCompare(fileName, "generated.mid")) {
			//	Assert.Fail("The midi files are different!");
			//}

			Assert.Pass("The midi files are identical.");
		}
		
		private bool CompileAndRunSource(string filePath) {
			
			string source = File.ReadAllText(filePath);
			
			var providerOptions = new Dictionary<string, string>
			{
				{"CompilerVersion", "v3.5"}
			};
			var cSharpCodeProvider = new CSharpCodeProvider(providerOptions);

			var compilerParameters = new CompilerParameters {
				GenerateInMemory = true,
				GenerateExecutable = false
			};

			compilerParameters.ReferencedAssemblies.Add("System.dll");
			compilerParameters.ReferencedAssemblies.Add("CommonUtils.dll");
			
			CompilerResults results = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, source);

			if (results.Errors.HasErrors)
			{
				var sb = new StringBuilder();
				foreach (CompilerError error in results.Errors)
				{
					sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
				}
				throw new InvalidOperationException(sb.ToString());
			}

			Assembly assembly = results.CompiledAssembly;
			Type program = assembly.GetType("MidiGeneration.GenerateMidi");
			MethodInfo method = program.GetMethod("Main");
			method.Invoke(null, new object[] { new string[] {""} });
			
			return true;
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
