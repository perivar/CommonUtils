using System;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// </summary>
	public abstract class WaveletTransform
	{
		// wavelet coefficients:
		// http://wavelets.pybytes.com/wavelet/haar/
		
		protected static double SQRT2 = Math.Sqrt(2.0);
		
		protected const double S0 = 0.5;
		protected const double S1 = 0.5;
		protected const double W0 = 0.5;
		protected const double W1 = -0.5;
		
		/*
		// sqrt(2) / 2 = 0.7071067811865476
		protected const double S0 = 0.7071067811865476;
		protected const double S1 = 0.7071067811865476;
		protected const double W0 = 0.7071067811865476;
		protected const double W1 = -0.7071067811865476;
		 */		
		
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