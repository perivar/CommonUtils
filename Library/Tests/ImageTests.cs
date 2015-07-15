using System;
using System.Linq;
using NUnit.Framework;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class ImageTests
	{
		[Test]
		public void TestReadBMPGrayscale()
		{
			var bmpWhite24Bit = ImageUtils.ReadBMPGrayscale(@"Tests\10x10white24bit.bmp");
			
			Assert.IsTrue(bmpWhite24Bit.All(list => list.All(item => item == 1)));
			
			var bmpBlack24Bit = ImageUtils.ReadBMPGrayscale(@"Tests\10x10black24bit.bmp");
			
			Assert.IsTrue(bmpBlack24Bit.All(list => list.All(item => item == 0)));
		}
	}
}
