
namespace CommonUtils.MathLib.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// Heavily modified by Per Ivar Nerseth
	/// </summary>
	public class InverseWaveletTransform : WaveletTransform
	{
		public InverseWaveletTransform(int iterations)
			: base(iterations)
		{
		}

		public InverseWaveletTransform(int width, int height)
			: base(width, height)
		{
		}

		public override void Transform(ColorChannels channels)
		{
			foreach (var color in new[] { channels.Red, channels.Green, channels.Blue })
			{
				Transform2D(color, false, this.Iterations);
			}
		}

		/// <summary>
		/// A 1D inverse Haar transform optionally using all levels
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="doAllLevels">determine whether to transform all levels</param>
		public static void Transform1D(double[] data, bool doAllLevels=false) {
			
			if (doAllLevels) {
				int n = data.Length;
				int m = 1;
				while (m <= n)
				{
					Transform1DStep(data, m);
					m *= 2;
				}
			} else {
				Transform1DStep(data, data.Length);
			}
		}
		
		/// <summary>
		/// A Modified version of 1D Haar Transform, used by the 2D Haar Transform function
		/// </summary>
		/// <param name="data"></param>
		/// <param name="h">length of transform</param>
		public static void Transform1DStep(double[] data, int h)
		{
			var temp = new double[h];

			h /= 2;
			for (int i = 0; i < h; i++)
			{
				var k = i << 1;
				temp[k] = (data[i] + data[i + h]) / SQRT2;
				temp[k + 1] = (data[i] - data[i + h]) / SQRT2;
			}
			
			for (int i = 0; i < h * 2; i++)
			{
				data[i] = temp[i];
			}
		}

		/// <summary>
		/// Standard 2D Transform. One iteration here gives the standard transform
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="doAllLevels">determine whether to transform all levels</param>
		/// <param name="iterations">number of iterations, 1 is standard</param>
		public static void Transform2D(double[][] data, bool doAllLevels=true, int iterations = 1)
		{
			var rows = data.Length;
			var cols = data[0].Length;
			
			var col = new double[rows];
			var row = new double[cols];

			for (var l = 0; l < iterations; l++)
			{
				for (var j = 0; j < cols; j++)
				{
					for (var i = 0; i < row.Length; i++)
					{
						col[i] = data[i][j];
					}

					Transform1D(col, doAllLevels);

					for (var i = 0; i < col.Length; i++)
					{
						data[i][j] = col[i];
					}
				}

				for (var i = 0; i < rows; i++)
				{
					for (var j = 0; j < row.Length; j++)
					{
						row[j] = data[i][j];
					}

					Transform1D(row, doAllLevels);

					for (var j = 0; j < row.Length; j++)
					{
						data[i][j] = row[j];
					}
				}
			}
		}
		
	}
}