using System;
using NUnit.Framework;
using System.Drawing;
using CommonUtils.ImageHash;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class ImageHashTest
	{
		const string image1Path = @"Tests\forest-high.jpg";
		const string image2Path = @"Tests\forest-copyright.jpg";

		[Test]
		public void TestImagePHash()
		{
			var theImage = new Bitmap(image1Path);
			var theOtherImage  = new Bitmap(image2Path);
			
			var phash = new ImagePHash(64,16);
			string hash1s = phash.GetHash(theImage);
			string hash2s = phash.GetHash(theOtherImage);
			Console.WriteLine(hash1s + "\t" + image1Path);
			Console.WriteLine(hash2s + "\t" + image2Path);
			
			double similarity = ImagePHash.Similarity(hash1s, hash2s);
			Console.WriteLine("Similarity: {0:00.00} % ", similarity);
			
			if (similarity > 0.90) {
				Assert.Pass("The images are probably identical.");
			} else {
				Assert.Fail("The images are probably different!");
			}
		}
		
		[Test]
		public void TestImageAverageHash()
		{
			var theImage = new Bitmap(image1Path);
			var theOtherImage  = new Bitmap(image2Path);

			ulong hash1 = ImageAverageHash.AverageHash(theImage);
			ulong hash2 = ImageAverageHash.AverageHash(theOtherImage);
			
			Console.WriteLine(hash1.ToString("x16") + "\t" + image1Path);
			Console.WriteLine(hash2.ToString("x16") + "\t" + image2Path);

			double similarity = ImageAverageHash.Similarity(hash1, hash2);
			Console.WriteLine("Similarity: {0:00.00} % ", similarity);
			Console.WriteLine("\n\n");

			if (similarity > 0.90) {
				Assert.Pass("The images are probably identical.");
			} else {
				Assert.Fail("The images are probably different!");
			}
		}
	}
}
