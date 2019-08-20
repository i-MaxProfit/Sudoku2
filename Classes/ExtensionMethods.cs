using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sudoku.Classes
{
    public static class ExtensionMethods
    {
        public static bool ContainsValue<T>(this T[,] matrix, T value)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (matrix[x, y].Equals(value))
                        return true;
                }
            }

            return false;
        }
    }
}