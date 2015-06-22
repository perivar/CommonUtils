using System.Collections.Generic;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
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
				Transform2D(color, this.Iterations);
			}
		}

		public static void Transform1D(double[] data)
		{
			var temp = new double[data.Length];

			var h = data.Length >> 1;
			for (var i = 0; i < h; i++)
			{
				var k = i << 1;
				temp[k] = ((data[i] * S0) + (data[i + h] * W0)) / W0;
				temp[k + 1] = ((data[i] * S1) + (data[i + h] * W1)) / S0;
			}

			for (var i = 0; i < data.Length; i++)
			{
				data[i] = temp[i] / SQRT2; // PIN: added divide with SQRT2 to make this a haar transform
			}
		}

		public static void Transform2D(double[][] data, int iterations)
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

					Transform1D(col);

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

					Transform1D(row);

					for (var j = 0; j < row.Length; j++)
					{
						data[i][j] = row[j];
					}
				}
			}
		}
	}
}