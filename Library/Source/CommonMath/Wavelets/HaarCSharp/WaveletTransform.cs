using System;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// Heavily modified by Per Ivar Nerseth
	/// </summary>
	public abstract class WaveletTransform
	{
		// wavelet coefficients:
		// http://wavelets.pybytes.com/wavelet/haar/
		
		protected static double SQRT2 = Math.Sqrt(2.0);
				
		protected WaveletTransform(int iterations)
		{
			this.Iterations = iterations;
		}

		protected WaveletTransform(int width, int height)
		{
			this.Iterations = GetMaxScale(width, height);
		}

		protected int Iterations { get; private set; }

		public static WaveletTransform CreateTransform(bool forward, int iterations)
		{
			if (forward)
			{
				return new ForwardWaveletTransform(iterations);
			}

			return new InverseWaveletTransform(iterations);
		}

		public static int GetMaxScale(int width, int height)
		{
			return (int)(Math.Log(width < height ? width : height) / Math.Log(2));
		}

		public abstract void Transform(ColorChannels channels);
	}
}