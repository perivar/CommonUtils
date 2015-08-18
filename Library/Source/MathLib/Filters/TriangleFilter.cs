// Copyright (c) 2011 Sebastian Böhm sebastian@sometimesfood.org
//                    Heinrich Fink hf@hfink.eu
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.IO;

namespace CommonUtils.MathLib.Filters
{
	/// <summary>
	/// A triangle-shaped filter that can be applied on an array of doubles. This
	/// filter is windows the array of doubles with the triangle and returns the
	/// sum of the windowed data, i.e. its the dot-product between the triangle
	/// and the data to be filtered. This class operators on array indices, i.e.
	/// a left and right border is defined by indices. This is useful when e.g.
	/// applying this filter on FFT bins. The filter uses optimized vectorized
	/// versions for filter creaetion and execution.
	/// </summary>
	public class TriangleFilter
	{
		private int leftEdge;
		private int rightEdge;
		private double height;
		private int size;

		private double[] filterData;
		
		public int LeftEdge {
			get {
				return leftEdge;
			}
		}
		
		public int RightEdge {
			get {
				return rightEdge;
			}
		}

		public double Height {
			get {
				return height;
			}
		}

		public int Size {
			get {
				return size;
			}
		}

		public double[] FilterData {
			get {
				return filterData;
			}
		}
		
		/// <summary>
		/// Creates a new TriangleFilter.
		/// <param name="leftEdge">An index defining the left edge of the triangle. (including)</param>
		/// <param name="rightEdge">An index defining the right edge of the triangle. (including)</param>
		/// <param name="height">The height of the triangle.</param>
		/// </summary>
		/// <description>
		/// Note that the array passed to TriangleFilter.Apply is required to be
		/// valid for index range defined by this constructor, i.e. it has to
		/// accomodate at least right_edge + 1 number of double values.
		/// </description>
		public TriangleFilter(int leftEdge, int rightEdge, double height)
		{
			this.leftEdge = leftEdge;
			this.rightEdge = rightEdge;
			this.height = height;
			
			size = rightEdge - leftEdge + 1;
			
			filterData = new double[size];
			
			if ((leftEdge >= rightEdge) || (leftEdge < 0) || (rightEdge < 0))
			{
				throw new ArgumentException(String.Format("TriangleFilter: edge values are invalid: left_edge = '{0}' right_edge = '{1}'.",leftEdge, rightEdge));
			}
			
			if (height == 0)
			{
				throw new ArgumentException("Invalid height input: height == 0.");
			}
			
			int center = (int)((leftEdge + rightEdge) * 0.5 + 0.5);
			
			// left rising part with positive slope, without setting center
			int left_side_length = (center - leftEdge);
			double left_dx = height / left_side_length;
			double zero = 0;
			
			for (int i = 0; i < left_side_length; i++) {
				filterData[i] = zero + i * left_dx;
			}
			
			// right falling part with negative slope, also setting center
			int right_side_length = rightEdge - center;
			double right_dx = - height / right_side_length;
			
			for (int i = 0; i < right_side_length+1; i++) {
				double val = height + i * right_dx;
				val = Math.Round(val, 6);
				filterData[size-right_side_length-1 + i] = (double) val;
			}
		}

		/// <summary>
		/// Applies the filter to a double buffer.
		/// Note that the array passed to TriangleFilter.Apply is required to be
		/// valid for index range defined by this constructor, i.e. it has to
		/// accomodate at least right_edge + 1 number of double values.
		/// </summary>
		/// <param name="buffer">The array to be filter by this TriangleFilter.
		/// @return The result of the filtering operation, i.e. the dot-product
		/// between the triangle shape and the elements of the buffer as defined
		/// by the triangles left and right edge indices.</param>
		public double Apply(double[] buffer)
		{
			// we can simply apply the filter as the dot product with the sample buffer
			// within its range
			double result = 0;
			for(int i = 0; i < size; i++)
			{
				result += buffer[leftEdge + i] * filterData[i];
			}
			
			return result;
		}

		public override string ToString()
		{
			var writer = new StringWriter();
			Print(writer, this);
			writer.Close();
			return writer.ToString();
		}

		/// <summary>
		/// Used for debugging purposes.
		/// </summary>
		public static void Print(TextWriter writer, TriangleFilter f)
		{
			for (int i = 0; i < f.leftEdge; ++i)
			{
				if (i != 0) {
					writer.Write(", ");
				}
				writer.Write("0");
			}

			for (int i = 0; i < f.size; ++i)
			{
				writer.Write(", " + f.FilterData[i].ToString("0.000", CultureInfo.InvariantCulture));
			}
		}
	}
}

