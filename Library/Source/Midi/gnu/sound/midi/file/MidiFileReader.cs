// MidiFileReader.java -- Read MIDI files.
//   Copyright (C) 2006 Free Software Foundation, Inc.

using System;
using System.IO;
using System.Net;

namespace gnu.sound.midi.file
{
	/// A MIDI file reader.
	///
	/// This code reads MIDI file types 0 and 1.
	///
	/// There are many decent documents on the web describing the MIDI file
	/// format.  I didn't bother looking for the official document.  If it
	/// exists, I'm not even sure if it is freely available.  We should
	/// update this comment if we find out anything helpful here.
	///
	/// @author Anthony Green (green@redhat.com)
	public class MidiFileReader : gnu.sound.midi.spi.MidiFileReader
	{
		/// <summary>
		/// Get the MidiFileFormat for the given input stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// @see gnu.sound.midi.spi.MidiFileReader#GetMidiFileFormat(java.io.Stream)
		public override MidiFileFormat GetMidiFileFormat(Stream stream)
		{
			var din = new BinaryReaderBigEndian(stream);

			int header, type, ntracks, division, resolution, bytes;
			float divisionType;
			
			if ((header = din.ReadInt32()) != 0x4d546864) // "MThd"
				throw new InvalidMidiDataException("Invalid MIDI chunk header.");

			bytes = din.ReadInt32();
			if (bytes < 6)
				throw new InvalidMidiDataException("Invalid MIDI chunk header length: " + bytes);

			type = din.ReadInt16();
			if (type < 0 || type > 2)
				throw new InvalidMidiDataException("Invalid MIDI file type value: " + type);

			ntracks = din.ReadInt16();
			if (ntracks <= 0)
				throw new InvalidMidiDataException("Invalid number of MIDI tracks: " + ntracks);

			division = din.ReadInt16();
			if ((division & 0x8000) != 0)
			{
				division = (int) -(((uint)division >> 8) & 0xFF);
				switch (division)
				{
					case 24:
						divisionType = Sequence.SMPTE_24;
						break;

					case 25:
						divisionType = Sequence.SMPTE_25;
						break;

					case 29:
						divisionType = Sequence.SMPTE_30DROP;
						break;

					case 30:
						divisionType = Sequence.SMPTE_30;
						break;

					default:
						throw new InvalidMidiDataException("Invalid MIDI frame division type: " + division);
				}
				resolution = division & 0xff;
			}
			else
			{
				divisionType = Sequence.PPQ;
				resolution = division & 0x7fff;
			}

			// If we haven't read every byte in the header now, just skip the rest.
			//din.skip(bytes - 6);
			din.ReadBytes(bytes - 6);

			return new ExtendedMidiFileFormat(type, divisionType, resolution, MidiFileFormat.UNKNOWN_LENGTH, MidiFileFormat.UNKNOWN_LENGTH, ntracks);
		}

		/// <summary>
		/// Get the MidiFileFormat from the given URL.
		/// @see gnu.sound.midi.spi.MidiFileReader#GetMidiFileFormat(java.net.URL)
		/// </summary>
		public override MidiFileFormat GetMidiFileFormat(string url)
		{
			Stream stream = GetStreamFromUrl(url);
			try
			{
				return GetMidiFileFormat(stream);
			}
			finally
			{
				stream.Close();
			}
		}

		/// <summary>
		/// Get the MidiFileFormat from the given file.
		/// @see gnu.sound.midi.spi.MidiFileReader#GetMidiFileFormat(java.io.File)
		/// </summary>
		public override MidiFileFormat GetMidiFileFormat(FileInfo file)
		{
			Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
			try
			{
				return GetMidiFileFormat(stream);
			}
			finally
			{
				stream.Close();
			}
		}

		/// <summary>
		/// Get the MIDI Sequence found in this input stream.
		/// @see gnu.sound.midi.spi.MidiFileReader#GetSequence(java.io.Stream)
		/// </summary>
		public override Sequence GetSequence(Stream stream)
		{
			// http://www.ccarh.org/courses/253/handout/smf/
			
			var din = new MidiDataInputStream(stream);
			var mff = (ExtendedMidiFileFormat) GetMidiFileFormat(din.BaseStream);

			var seq = new Sequence(mff.GetDivisionType(), mff.GetResolution(), 0, mff.GetType());

			int ntracks = mff.GetNumberTracks();

			while (ntracks-- > 0)
			{
				Track track = seq.CreateTrack();
				int Mtrk = din.ReadInt32();
				if (Mtrk != 0x4d54726b)
					throw new InvalidMidiDataException("Invalid MIDI track header.");
				int length = din.ReadInt32();

				int runningStatus = -1;
				int click = 0;

				// Set this to true when we've hit an End of Track meta event.
				bool done = false;

				// Read all events.
				while (! done)
				{
					MidiMessage mm;
					int dtime = din.ReadVariableLengthInt();
					click += dtime;

					// in Java bytes are signed (-128, +127)
					// where in C# it's not (0, 255).
					//int statusByte = din.readUnsignedByte();
					int statusByte = din.ReadByte();
					
					if (statusByte < (int) MidiHelper.MidiEventType.System_Exclusive)
					{
						ShortMessage sm;
						switch (statusByte & 0xf0)
						{
							case (int) MidiHelper.MidiEventType.Note_Off:
							case (int) MidiHelper.MidiEventType.Note_On:
							case (int) MidiHelper.MidiEventType.Note_Aftertouch:
							case (int) MidiHelper.MidiEventType.Controller:
							case (int) MidiHelper.MidiEventType.Pitch_Bend:
							case ShortMessage.SONG_POSITION_POINTER:
								sm = new ShortMessage();
								sm.SetMessage(statusByte, din.ReadSByte(), din.ReadSByte());
								runningStatus = statusByte;
								break;

							case (int) MidiHelper.MidiEventType.Program_Change:
							case (int) MidiHelper.MidiEventType.Channel_Aftertouch:
							case ShortMessage.SONG_SELECT:
							case 0xF5: // FIXME: unofficial bus select. Not in spec??
								sm = new ShortMessage();
								sm.SetMessage(statusByte, din.ReadSByte(), 0);
								runningStatus = statusByte;
								break;

							case ShortMessage.TUNE_REQUEST:
							case ShortMessage.END_OF_EXCLUSIVE:
							case ShortMessage.TIMING_CLOCK:
							case ShortMessage.START:
							case ShortMessage.CONTINUE:
							case ShortMessage.STOP:
							case ShortMessage.ACTIVE_SENSING:
							case ShortMessage.SYSTEM_RESET:
								sm = new ShortMessage();
								sm.SetMessage(statusByte, 0, 0);
								runningStatus = statusByte;
								break;

							default:
								if (runningStatus != - 1)
								{
									switch (runningStatus & 0xf0)
									{
										case (int) MidiHelper.MidiEventType.Note_Off:
										case (int) MidiHelper.MidiEventType.Note_On:
										case (int) MidiHelper.MidiEventType.Note_Aftertouch:
										case (int) MidiHelper.MidiEventType.Controller:
										case (int) MidiHelper.MidiEventType.Pitch_Bend:
										case ShortMessage.SONG_POSITION_POINTER:
											sm = new ShortMessage();
											sm.SetMessage(runningStatus, statusByte, din.ReadSByte());
											break;

										case (int) MidiHelper.MidiEventType.Program_Change:
										case (int) MidiHelper.MidiEventType.Channel_Aftertouch:
										case ShortMessage.SONG_SELECT:
										case 0xF5: // FIXME: unofficial bus select. Not in
											// spec??
											sm = new ShortMessage();
											sm.SetMessage(runningStatus, statusByte, 0);
											continue;

										case ShortMessage.TUNE_REQUEST:
										case ShortMessage.END_OF_EXCLUSIVE:
										case ShortMessage.TIMING_CLOCK:
										case ShortMessage.START:
										case ShortMessage.CONTINUE:
										case ShortMessage.STOP:
										case ShortMessage.ACTIVE_SENSING:
										case ShortMessage.SYSTEM_RESET:
											sm = new ShortMessage();
											sm.SetMessage(runningStatus, 0, 0);
											continue;

										default:
											throw new InvalidMidiDataException("Invalid Short MIDI Event: " + statusByte);
									}
								}
								else
									throw new InvalidMidiDataException("Invalid Short MIDI Event: " + statusByte);
								
								break;
								
						}
						mm = sm;
					}
					else if (statusByte == 0xf0 || statusByte == 0xf7)
					{
						// System Exclusive event
						int slen = din.ReadVariableLengthInt();
						var sysex = din.ReadSBytes(slen);
						var sm = new SysexMessage();
						sm.SetMessage(statusByte, sysex, slen);
						mm = sm;
						runningStatus = - 1;
					}
					else if (statusByte == (int) MidiHelper.MidiEventType.MetaEvent)
					{
						// Meta Message
						sbyte mtype = din.ReadSByte();
						int mlen = din.ReadVariableLengthInt();
						var meta = din.ReadSBytes(mlen);
						var metam = new MetaMessage();
						metam.SetMessage(mtype, meta, mlen);
						mm = metam;

						if (mtype == 0x2f) // End of Track
							done = true;

						runningStatus = - 1;
					}
					else
					{
						throw new InvalidMidiDataException("Invalid status byte: " + statusByte);
					}

					track.Add(new MidiEvent(mm, click));
				}
			}

			return seq;
		}
		
		/// <summary>
		/// Get the MIDI Sequence found at the given URL.
		/// @see gnu.sound.midi.spi.MidiFileReader#GetSequence(java.net.URL)
		/// </summary>
		public override Sequence GetSequence(string url)
		{
			Stream stream = GetStreamFromUrl(url);
			try
			{
				return GetSequence(stream);
			}
			finally
			{
				stream.Close();
			}
		}

		/// <summary>
		/// Get the MIDI Sequence found in the given file.
		/// @see gnu.sound.midi.spi.MidiFileReader#GetSequence(java.io.File)
		/// </summary>
		public override Sequence GetSequence(FileInfo file)
		{
			Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
			try
			{
				return GetSequence(stream);
			}
			finally
			{
				stream.Close();
			}
		}

		// TODO: move this to a util class
		private static Stream GetStreamFromUrl(string url)
		{
			byte[] data = null;

			using (var wc = new System.Net.WebClient())
				data = wc.DownloadData(url);

			return new MemoryStream(data);
		}
		
	}
}