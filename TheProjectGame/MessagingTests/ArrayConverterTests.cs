using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Messaging.Serialization.Extensions;

namespace MessagingTests
{
    public class ArrayConverterTests
    {
        [Test]
        public void ToTwoDimensionalArray_ShouldNotChangeElements()
        {
            var oneDimensionalArray = Enumerable.Range(1, 100).ToArray();
            var height = 20;
            var width = 5;

            var twoDimensionalArray = oneDimensionalArray.ToTwoDimensionalArray(height, width);

            int i = 0;
            foreach (var elem in twoDimensionalArray)
            {
                Assert.AreEqual(oneDimensionalArray[i], elem);
                i++;
            }

            Assert.AreEqual(100, twoDimensionalArray.Length);
            Assert.AreEqual(height, twoDimensionalArray.GetLength(0));
            Assert.AreEqual(width, twoDimensionalArray.GetLength(1));
        }

        [Test]
        public void ToOneDimensionalArray_ShouldNotChangeElements()
        {
            var height = 20;
            var width = 5;
            var twoDimensionalArray = new int[height, width];
            var targetList = new List<int>();

            int value = 1;
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    twoDimensionalArray[i, j] = value;
                    targetList.Add(value);
                    value++;
                }
            }

            var oneDimensionalArray = twoDimensionalArray.ToOneDimensionalArray();

            Assert.AreEqual(targetList.ToArray(), oneDimensionalArray);
        }

        [Test]
        public void ArrayConversion_ShouldBeReversibleStartingFromOneDimensional()
        {
            var sourceOneDimensional = Enumerable.Range(1, 100).ToArray();
            var resultOneDimensional = sourceOneDimensional.ToTwoDimensionalArray(50, 2).ToOneDimensionalArray();

            Assert.AreEqual(sourceOneDimensional, resultOneDimensional);
        }

        [Test]
        public void ArrayConversion_ShouldBeReversibleStartingFromTwoDimensional()
        {
            var height = 20;
            var width = 5;
            var sourceTwoDimensional = new int[height, width];

            int value = 1;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    sourceTwoDimensional[i, j] = value;
                    value++;
                }
            }

            var resultTwoDimensional = sourceTwoDimensional.ToOneDimensionalArray().ToTwoDimensionalArray(height, width);

            Assert.AreEqual(sourceTwoDimensional, resultTwoDimensional);
        }
    }
}