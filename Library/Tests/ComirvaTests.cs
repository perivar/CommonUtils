using System;
using System.Diagnostics;
using NUnit.Framework;

using CommonUtils.CommonMath.Comirva;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class ComirvaTests
	{
		[Test]
		public void TestDctComirva()
		{
			#region Output from Octave
			/*
z = [139 144 149 153 155 155 155 155;
144 151 153 156 159 156 156 156;
150 155 160 163 158 156 156 156;
159 161 162 160 160 159 159 159;
159 160 161 162 162 155 155 155;
161 161 161 161 160 157 157 157;
162 162 161 163 162 157 157 157;
162 162 161 161 163 158 158 158];

octave:55> x = dct(z)
x =
      436.99      444.06      448.31      452.19      452.19         443         443         443
     -21.818     -14.969     -9.3908     -6.4728      -5.921     -1.7745     -1.7745     -1.7745
     -8.8097     -7.5031     -7.3446     -4.6522     -1.2737    -0.46194    -0.46194    -0.46194
     -2.4118     -3.7457     -3.9958     -3.0683     -1.4969     -1.7704     -1.7704     -1.7704
     0.70711    -0.70711    -0.70711     -2.4749     0.35355     0.35355     0.35355     0.35355
       1.365     0.22465     0.90791     0.57409     -1.7777      1.2224      1.2224      1.2224
    -0.94311     -1.4843     0.74614     0.77897     -2.1512    -0.19134    -0.19134    -0.19134
     -1.8165      -1.685     0.14561      2.9764     0.20231     -2.3922     -2.3922     -2.3922

octave:56> idct(x)
ans =
         139         144         149         153         155         155         155         155
         144         151         153         156         159         156         156         156
         150         155         160         163         158         156         156         156
         159         161         162         160         160         159         159         159
         159         160         161         162         162         155         155         155
         161         161         161         161         160         157         157         157
         162         162         161         163         162         157         157         157
         162         162         161         161         163         158         158         158
			 */
			#endregion
			
			var vals = new double[][] {
				new double[] { 139.0, 144.0, 149.0, 153.0, 155.0, 155.0, 155.0, 155.0 },
				new double[] { 144.0, 151.0, 153.0, 156.0, 159.0, 156.0, 156.0, 156.0 },
				new double[] { 150.0, 155.0, 160.0, 163.0, 158.0, 156.0, 156.0, 156.0 },
				new double[] { 159.0, 161.0, 162.0, 160.0, 160.0, 159.0, 159.0, 159.0 },
				new double[] { 159.0, 160.0, 161.0, 162.0, 162.0, 155.0, 155.0, 155.0 },
				new double[] { 161.0, 161.0, 161.0, 161.0, 160.0, 157.0, 157.0, 157.0 },
				new double[] { 162.0, 162.0, 161.0, 163.0, 162.0, 157.0, 157.0, 157.0 },
				new double[] { 162.0, 162.0, 161.0, 161.0, 163.0, 158.0, 158.0, 158.0 } };
			
			DctMethods.PrintMatrix(vals);
			
			var dctCom = new DctComirva(vals.Length, vals[0].Length);
			
			// dct
			double[][] dctVals = dctCom.Dct(vals);
			DctMethods.PrintMatrix(dctVals);
			
			// idct
			double[][] idctVals = dctCom.InverseDct(dctVals);
			DctMethods.PrintMatrix(idctVals);
			
			Assert.That(idctVals, Is.EqualTo(vals).AsCollection.Within(0.001), "fail at [0]");
		}
		
		[Test]
		public void TestDctMethods() {
			
			Test1(false);
			Test1(true);
			
			Test2(false, false);
			Test2(true, false);
			Test2();
		}
		
		#region Output from Octave
		// test 1
		// Octave results:
		// format short g
		// z = [1 2 3; 4 5 6; 7 8 9; 10 11 12];
		//
		//octave:5> dct(z)
		//ans =
		//          11          13          15
		//     -6.6913     -6.6913     -6.6913
		//           0           0           0
		//    -0.47554    -0.47554    -0.47554
		//
		//octave:6> dct2(z)
		//ans =
		//      22.517     -2.8284           0
		//      -11.59           0           0
		//           0           0           0
		//    -0.82366           0           0
		//
		#endregion
		public static void Test1(bool _2D = true)
		{
			var vals = new double[][] {
				new double[] { 1.0, 2.0, 3.0 },
				new double[] { 4.0, 5.0, 6.0 },
				new double[] { 7.0, 8.0, 9.0 },
				new double[] { 10.0, 11.0, 12.0 }
			};
			
			const double offset = 0.0;
			Console.WriteLine("vals: ");
			DctMethods.PrintMatrix(vals);

			Stopwatch stopWatch = Stopwatch.StartNew();
			long startS = stopWatch.ElapsedTicks;
			double[][] result;
			if (_2D) {
				result = DctMethods.dct2(vals, offset);
				Console.WriteLine("dct2 result: ");
			} else {
				result = DctMethods.dct(vals, offset);
				Console.WriteLine("dct result: ");
			}
			long endS = stopWatch.ElapsedTicks;
			DctMethods.PrintMatrix(result);
			Console.WriteLine("time in ticks: " + (endS - startS));

			//result = Filter(result, 1.0);
			//Console.WriteLine("dct2 filtered: ");
			//PrintMatrix(result);

			long startE = stopWatch.ElapsedTicks;
			double[][] ivals;
			if (_2D) {
				ivals = DctMethods.idct2(result, -offset);
				Console.WriteLine("idct2 result: ");
			} else {
				ivals = DctMethods.idct(result, -offset);
				Console.WriteLine("idct result: ");
			}
			long endE = stopWatch.ElapsedTicks;
			DctMethods.PrintMatrix(ivals);
			Console.WriteLine("Time in ticks: " + (endE - startE));
			
			Assert.That(ivals, Is.EqualTo(vals).AsCollection.Within(0.001), "fail at [0]");
		}

		#region Output from Octave
		// test 2
		// Octave results:
		// format short g
		/*
octave:43>
z = [139 144 149 153 155 155 155 155;
144 151 153 156 159 156 156 156;
150 155 160 163 158 156 156 156;
159 161 162 160 160 159 159 159;
159 160 161 162 162 155 155 155;
161 161 161 161 160 157 157 157;
162 162 161 163 162 157 157 157;
162 162 161 161 163 158 158 158];

octave:43> g = dct2(z-128)
g =
      235.62     -1.0333     -12.081     -5.2029       2.125     -1.6724      -2.708      1.3238
      -22.59     -17.484     -6.2405     -3.1574     -2.8557   -0.069456     0.43417     -1.1856
     -10.949     -9.2624     -1.5758      1.5301     0.20295    -0.94186    -0.56694   -0.062924
     -7.0816     -1.9072     0.22479      1.4539     0.89625   -0.079874   -0.042291     0.33154
      -0.625    -0.83811      1.4699      1.5563      -0.125    -0.66099     0.60885      1.2752
      1.7541    -0.20286      1.6205    -0.34244    -0.77554      1.4759       1.041    -0.99296
     -1.2825    -0.35995    -0.31694     -1.4601    -0.48996      1.7348      1.0758    -0.76135
     -2.5999      1.5519     -3.7628     -1.8448      1.8716      1.2139    -0.56788    -0.44564

octave:44> idct2(g)+128
ans =
         139         144         149         153         155         155         155         155
         144         151         153         156         159         156         156         156
         150         155         160         163         158         156         156         156
         159         161         162         160         160         159         159         159
         159         160         161         162         162         155         155         155
         161         161         161         161         160         157         157         157
         162         162         161         163         162         157         157         157
         162         162         161         161         163         158         158         158

octave:49> g = dct(z-128)
g =

      74.953      82.024      86.267      90.156      90.156      80.964      80.964      80.964
     -21.818     -14.969     -9.3908     -6.4728      -5.921     -1.7745     -1.7745     -1.7745
     -8.8097     -7.5031     -7.3446     -4.6522     -1.2737    -0.46194    -0.46194    -0.46194
     -2.4118     -3.7457     -3.9958     -3.0683     -1.4969     -1.7704     -1.7704     -1.7704
     0.70711    -0.70711    -0.70711     -2.4749     0.35355     0.35355     0.35355     0.35355
       1.365     0.22465     0.90791     0.57409     -1.7777      1.2224      1.2224      1.2224
    -0.94311     -1.4843     0.74614     0.77897     -2.1512    -0.19134    -0.19134    -0.19134
     -1.8165      -1.685     0.14561      2.9764     0.20231     -2.3922     -2.3922     -2.3922

octave:50> idct(g)+128
ans =

         139         144         149         153         155         155         155         155
         144         151         153         156         159         156         156         156
         150         155         160         163         158         156         156         156
         159         161         162         160         160         159         159         159
         159         160         161         162         162         155         155         155
         161         161         161         161         160         157         157         157
         162         162         161         163         162         157         157         157
         162         162         161         161         163         158         158         158
		 */
		#endregion
		public static void Test2(bool _2D = true, bool random=false)
		{
			double[][] vals;
			if (random) {
				// Generate random integers between 0 and 255
				const int N = 8;
				var generator = new Random();

				vals = new double[N][];
				int val;
				for (int x=0;x<N;x++)
				{
					vals[x] = new double[N];
					for (int y=0;y<N;y++)
					{
						val = generator.Next(255);
						vals[x][y] = val;
					}
				}
			} else {
				vals = new double[][] {
					new double[] { 139.0, 144.0, 149.0, 153.0, 155.0, 155.0, 155.0, 155.0 },
					new double[] { 144.0, 151.0, 153.0, 156.0, 159.0, 156.0, 156.0, 156.0 },
					new double[] { 150.0, 155.0, 160.0, 163.0, 158.0, 156.0, 156.0, 156.0 },
					new double[] { 159.0, 161.0, 162.0, 160.0, 160.0, 159.0, 159.0, 159.0 },
					new double[] { 159.0, 160.0, 161.0, 162.0, 162.0, 155.0, 155.0, 155.0 },
					new double[] { 161.0, 161.0, 161.0, 161.0, 160.0, 157.0, 157.0, 157.0 },
					new double[] { 162.0, 162.0, 161.0, 163.0, 162.0, 157.0, 157.0, 157.0 },
					new double[] { 162.0, 162.0, 161.0, 161.0, 163.0, 158.0, 158.0, 158.0 } };
			}
			const double offset = -128.0;
			Console.WriteLine("vals: ");
			DctMethods.PrintMatrix(vals);

			Stopwatch stopWatch = Stopwatch.StartNew();
			long startS = stopWatch.ElapsedTicks;
			double[][] result;
			if (_2D) {
				result = DctMethods.dct2(vals, offset);
				Console.WriteLine("dct2 result: ");
			} else {
				result = DctMethods.dct(vals, offset);
				Console.WriteLine("dct result: ");
			}
			long endS = stopWatch.ElapsedTicks;
			DctMethods.PrintMatrix(result);

			Console.WriteLine("Time in ticks: " + (endS - startS));

			//result = Filter(result, 0.25);
			//result = CutLeastSignificantCoefficients(result);
			//Console.WriteLine("dct2 filtered: ");
			//PrintMatrix(result);

			long startE = stopWatch.ElapsedTicks;
			double[][] ivals;
			if (_2D) {
				ivals = DctMethods.idct2(result, -offset);
				Console.WriteLine("idct2 result: ");
			} else {
				ivals = DctMethods.idct(result, -offset);
				Console.WriteLine("idct result: ");
			}
			long endE = stopWatch.ElapsedTicks;
			DctMethods.PrintMatrix(ivals);
			Console.WriteLine("Time in ticks: " + (endE - startE));
			
			Assert.That(ivals, Is.EqualTo(vals).AsCollection.Within(0.001), "fail at [0]");
		}

	}
}
