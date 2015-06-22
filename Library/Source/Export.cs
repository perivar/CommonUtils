using System;

using System.IO;
using System.Linq;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using ZedGraph;

namespace CommonUtils
{
	/// <summary>
	/// Description of Export.
	/// </summary>
	public static class Export
	{
		#region exportCSV
		public static void ExportCSV(string filenameToSave, Array data, int length=0) {
			
			if (length == 0 || length > data.Length) {
				length = data.Length;
			}
			
			var arr = new object[length][];
			
			int count = 1;
			for (int i = 0; i < length; i++)
			{
				arr[i] = new object[2] {
					count,
					data.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}
		
		public static void ExportCSV(string filenameToSave, float[][] data) {
			var arr = new object[data.Length][];

			for (int i = 0; i < data.Length; i++)
			{
				arr[i] = new object[data[i].Length];
				for (int j = 0; j < data[i].Length; j++)
				{
					arr[i][j] = data[i][j];
				}
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, double[][] data) {
			var arr = new object[data.Length][];

			for (int i = 0; i < data.Length; i++)
			{
				arr[i] = new object[data[i].Length];
				for (int j = 0; j < data[i].Length; j++)
				{
					arr[i][j] = data[i][j];
				}
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2) {
			if (column1.Length != column2.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[3] {
					count,
					column1.GetValue(i),
					column2.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3) {
			if (column1.Length != column2.Length || column1.Length != column3.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[4] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[5] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[6] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5, Array column6) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length
			    || column1.Length != column6.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[7] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i),
					column6.GetValue(i),
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5, Array column6, Array column7) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length
			    || column1.Length != column6.Length
			    || column1.Length != column7.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[8] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i),
					column6.GetValue(i),
					column7.GetValue(i),
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}
		#endregion
		
		#region Draw Graph
		/// <summary>
		/// Graphs an array of doubles varying between -1 and 1
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="fileName">filename to save png to</param>
		/// <param name="onlyCanvas">true if no borders should be printed</param>
		public static void DrawGraph(double[] data, string fileName, bool onlyCanvas=false)
		{
			var myPane = new GraphPane( new RectangleF( 0, 0, 1200, 600 ), "", "", "" );
			
			if (onlyCanvas) {
				myPane.Chart.Border.IsVisible = false;
				myPane.Chart.Fill.IsVisible = false;
				myPane.Fill.Color = Color.Black;
				myPane.Margin.All = 0;
				myPane.Title.IsVisible = false;
				myPane.XAxis.IsVisible = false;
				myPane.YAxis.IsVisible = false;
			}
			myPane.XAxis.Scale.Max = data.Length - 1;
			myPane.XAxis.Scale.Min = 0;
			//myPane.YAxis.Scale.Max = 1;
			//myPane.YAxis.Scale.Min = -1;
			
			// add pretty stuff
			myPane.Fill = new Fill( Color.WhiteSmoke, Color.Lavender, 0F );
			myPane.Chart.Fill = new Fill( Color.FromArgb( 255, 255, 245 ),
			                             Color.FromArgb( 255, 255, 190 ), 90F );
			
			var timeData = Enumerable.Range(0, data.Length)
				.Select(i => (double) i)
				.ToArray();
			myPane.AddCurve(null, timeData, data, Color.Blue, SymbolType.None);
			
			var bmp = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bmp ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
		/// <summary>
		/// Writes the float array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(float[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}

		/// <summary>
		/// Writes the double array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(double[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the text file to create, e.g. "C:\\temp\\data.f3.txt"</param>
		public static void WriteF3Formatted(float[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the text file to create, e.g. "C:\\temp\\data.f3.txt"</param>
		public static void WriteF3Formatted(double[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		#endregion
	}
}
