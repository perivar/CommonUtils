using System;
using System.Drawing;
using System.Windows.Forms;
using CommonUtils.MathLib.FFT;

namespace CommonUtils.GUI
{
	/// <summary>
	/// Description of WaveDisplayUserControl.
	/// </summary>
	public partial class WaveDisplayUserControl : UserControl
	{
		private Bitmap bmp;
		private int waveDisplayAmplitude = 1;
		private int waveDisplayStartZoomPosition = 0;
		private int waveDisplayEndZoomPosition = 0;
		private double sampleRate = 44100;
		
		public double SampleRate
		{
			set { sampleRate = value; }
			get { return sampleRate; }
		}

		public int Amplitude
		{
			set { waveDisplayAmplitude = value; }
			get { return waveDisplayAmplitude; }
		}

		public int StartZoomPosition
		{
			set { waveDisplayStartZoomPosition = value; }
			get { return waveDisplayStartZoomPosition; }
		}
		
		public int EndZoomPosition
		{
			set { waveDisplayEndZoomPosition = value; }
			get { return waveDisplayEndZoomPosition; }
		}

		public int NumberOfSamples {
			get {
				int numberOfSamples = 0;
				if (this.audioData != null && this.audioData.Length > 0) {
					numberOfSamples = this.audioData.Length;
					return numberOfSamples;
				} else {
					return 0;
				}
			}
		}
		
		public double DurationInMilliseconds {
			get {
				return this.NumberOfSamples / this.sampleRate * 1000;
			}
		}
		
		public WaveDisplayUserControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
			              ControlStyles.OptimizedDoubleBuffer, true);
			InitializeComponent();
			
		}
		
		/// <summary>
		/// <see cref="Control.OnPaint"/>
		/// </summary>
		protected override void OnPaint(PaintEventArgs pe)
		{
			if (bmp != null) {
				pe.Graphics.DrawImage(bmp, 0, 0);
			}

			// Calling the base class OnPaint
			base.OnPaint(pe);
		}
		
		private float[] audioData;

		/// <summary>
		/// sets graph data
		/// </summary>
		public void SetAudioData(float[] audioData)
		{
			this.audioData = audioData;
			bmp = AudioAnalyzer.DrawWaveformMono(audioData,
			                                     new Size(this.Width, this.Height),
			                                     waveDisplayAmplitude,
			                                     waveDisplayStartZoomPosition,
			                                     waveDisplayEndZoomPosition,
			                                     sampleRate, true);
			
			// force redraw
			this.Invalidate();
		}
	}
}
