namespace Messaging.Serialization.Extensions
{
    public static class ArrayConverter
    {
        public static T[] ToOneDimensionalArray<T>(this T[,] twoDimensionalArray)
        {
            var length = twoDimensionalArray.Length;
            var height = twoDimensionalArray.GetLength(0);
            var width = twoDimensionalArray.GetLength(1);
            var oneDimensionalArray = new T[length];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    oneDimensionalArray[i * width + j] = twoDimensionalArray[i, j];
                }
            }

            return oneDimensionalArray;
        }

        public static T[,] ToTwoDimensionalArray<T>(this T[] oneDimensionalArray, int height, int width)
        {
            var twoDimensionalArray = new T[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    twoDimensionalArray[i, j] = oneDimensionalArray[i * width + j];
                }
            }
            return twoDimensionalArray;
        }
    }
}
