// MidiFileWriter.java -- Write MIDI files.
//   Copyright (C) 2006 Free Software Foundation, Inc.

using System;
using System.IO;

namespace gnu.sound.midi.file
{
	/// A MIDI file writer.
	/// This code writes MIDI file types 0 and 1.
	/// There are many decent documents on the web describing the MIDI file
	/// format.  I didn't bother looking for the official document.  If it
	/// exists, I'm not even sure if it is freely available.  We should
	/// update this comment if we find out anything helpful here.
	/// @author Anthony Green (green@redhat.com)
	public class MidiFileWriter : gnu.sound.midi.spi.MidiFileWriter
	{
		/// <summary>
		/// Return an array indicating which midi file types are supported.
		/// <see cref="gnu.sound.midi.spi.MidiFileWriter#getMidiFileTypes()"/>
		/// </summary>
		public override int[] GetMidiFileTypes()
		{
			return new int[]{0, 1};
		}

		/// <summary>
		/// Return an array indicating which midi file types are supported
		/// for a given Sequence.
		/// <see cref="gnu.sound.midi.spi.MidiFileWriter#getMidiFileTypes(Sequence)"/>
		/// </summary>
		public override int[] GetMidiFileTypes(Sequence sequence)
		{
			if (sequence.Tracks.Count == 1) {
				return new int[]{0};
			} else {
				return new int[]{1};
			}
		}

		/// <summary>
		/// Write a sequence to an output stream in standard midi format.
		/// <see cref="gnu.sound.midi.spi.MidiFileWriter#write(Sequence, int, Stream)"/>
		/// </summary>
		public override int Write(Sequence stream, int fileType, Stream outputStream)
		{
			var dos = new MidiDataOutputStream (outputStream);
			Track[] tracks = stream.Tracks.ToArray();
			dos.Write((int)0x4D546864); // MThd
			dos.Write((int)6);
			dos.Write((Int16)fileType);
			dos.Write((Int16)tracks.Length);
			
			float divisionType = stream.DivisionType;
			int resolution = stream.Resolution;
			
			// FIXME: division computation is incomplete.
			int division = 0;
			if (divisionType == Sequence.PPQ) {
				division = resolution & 0x7fff;
			}
			dos.Write((Int16)division);
			
			int length = 14;
			for (int i = 0; i < tracks.Length; i++) {
				length += WriteTrack(tracks[i], dos);
			}
			return length;
		}

		/// <summary>
		/// Compute the length of a track as it will be written to the
		/// output stream.
		/// <param name="track">the track to measure</param>
		/// <param name="dos">a MidiDataOutputStream used for helper method</param>
		/// <returns>the length of the track</returns>
		/// </summary>
		private int ComputeTrackLength(Track track, MidiDataOutputStream dos)
		{
			int length = 0;
			int i = 0;
			int eventCount = track.EventCount();
			long ptick = 0;
			
			while (i < eventCount)
			{
				MidiEvent me = track.Get(i);
				long tick = me.Tick;
				length += MidiDataOutputStream.VariableLengthIntLength((int)(tick - ptick));
				ptick = tick;
				length += me.Message.Length;
				i++;
			}
			return length;
		}

		/// <summary>
		/// Write a track to an output stream.
		/// <param name="track">the track to write</param>
		/// <param name="dos">a MidiDataOutputStream to write to</param>
		/// <returns>the number of bytes written</returns>
		/// </summary>
		private int WriteTrack(Track track, MidiDataOutputStream dos)
		{
			int i = 0;
			int elength = track.EventCount();
			int trackLength = 0;

			MidiEvent pme = null;
			dos.Write((int)0x4D54726B); // "MTrk"
			trackLength = ComputeTrackLength(track, dos);
			dos.Write((int)trackLength);
			
			while (i < elength)
			{
				MidiEvent me = track.Get(i);
				int dtime = 0;
				if (pme != null)
					dtime = (int)(me.Tick - pme.Tick);
				
				dos.WriteVariableLengthInt(dtime);
				
				// FIXME: use running status byte
				byte[] msg = me.Message.GetMessage();
				dos.Write(msg);
				pme = me;
				
				i++;
			}

			// We're done if the last event was an End of Track meta message.
			if (pme != null && (pme.Message is MetaMessage))
			{
				var mm = (MetaMessage) pme.Message;
				
				// End of Track message
				if (mm.GetMetaMessageType() == 0x2F) {
					return trackLength + 8;
				}
			}

			// Write End of Track meta message
			dos.WriteVariableLengthInt(0); // Delta time of 0
			dos.Write((byte)0xFF); // Meta Message
			dos.Write((byte)0x2F); // End of Track message
			dos.WriteVariableLengthInt(0); // Length of 0

			return trackLength + 8 + 4;
		}

		/// <summary>
		/// Write a Sequence to a file.
		/// <see cref="MidiFileWriter#Write(Sequence, int, File)"/>
		/// </summary>
		public override int Write(Sequence stream, int fileType, FileInfo @out)
		{
			Stream os = new FileStream(@out.FullName, FileMode.Create, FileAccess.Write);
			try
			{
				return Write(stream, fileType, os);
			}
			finally
			{
				os.Close();
			}
		}
	}
}