using System.Collections.Generic;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// </summary>
	public class ForwardWaveletTransform : WaveletTransform
	{
		public ForwardWaveletTransform(int iterations)
			: base(iterations)
		{
		}

		public ForwardWaveletTransform(int width, int height)
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
				temp[i] = (data[k] * S0) + (data[k + 1] * S1);
				temp[i + h] = (data[k] * W0) + (data[k + 1] * W1);
			}

			for (var i = 0; i < data.Length; i++)
			{
				data[i] = temp[i] * SQRT2; // PIN: added multiply with SQRT2 to make this a haar transform
			}
		}

		public static void Transform2D(double[][] data, int iterations)
		{
			var rows = data.Length;
			var cols = data[0].Length;

			var row = new double[cols];
			var col = new double[rows];

			for (var k = 0; k < iterations; k++)
			{
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

				for (var j = 0; j < cols; j++)
				{
					for (var i = 0; i < col.Length; i++)
					{
						col[i] = data[i][j];
					}

					Transform1D(col);

					for (var i = 0; i < col.Length; i++)
					{
						data[i][j] = col[i];
					}
				}
			}
		}
	}
}