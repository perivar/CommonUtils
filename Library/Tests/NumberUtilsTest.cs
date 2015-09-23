using System;
using NUnit.Framework;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class NumberUtilsTest
	{
		[Test]
		public void TestMethod()
		{
			Console.WriteLine();
			Console.WriteLine("Testing NumberUtils:");
			
			string[] doubleStrings = {"hello", "0.123", "0,123"};
			foreach (var doubleString in doubleStrings)
			{
				decimal d = NumberUtils.DecimalTryParseDecimalPointOrZero(doubleString);
				Console.WriteLine(string.Format("{0} => {1}", doubleString, d));
			}
			Console.WriteLine();
			
			foreach (var doubleString in doubleStrings)
			{
				double d = NumberUtils.DoubleTryParse(doubleString, -1.0);
				Console.WriteLine(string.Format("{0} => {1}", doubleString, d));
			}
			Console.WriteLine();
		}
	}
}
