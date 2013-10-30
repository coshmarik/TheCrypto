//This file is part of THE CRYPTO.
//THE CRYPTO is the program for encrypting files for Windows 8.
// Copyright (C) 2013  Daria V. Korosteleva <coooshmarik@gmail.com>

// THE CRYPTO is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// THE CRYPTO is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;

namespace Crypto
{
    public class Matrix
    {
        private const short max = 255;
        private int width, height;
        public short[,] matxx;
        public static Matrix alp = new Matrix(new List<short> { 1, 2, 4, 8, 16, 32, 64, 128, 29, 58, 116, 232, 205, 135, 19, 38, 76, 152, 45, 90, 180, 117, 234, 201, 143, 3, 6, 12, 24, 48, 96, 192, 
                         157, 39, 78, 156, 37, 74, 148, 53, 106, 212, 181, 119, 238, 193, 159, 35, 70, 140, 5, 10, 20, 40, 80, 160, 93, 186, 105, 210, 185, 111, 222, 161, 
                         95, 190, 97, 194, 153, 47, 94, 188, 101, 202, 137, 15, 30, 60, 120, 240, 253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163, 91, 182, 113, 
                         226, 217, 175, 67, 134, 17, 34, 68, 136, 13, 26, 52, 104, 208, 189, 103, 206, 129, 31, 62, 124, 248, 237, 199, 147, 59, 118, 236, 197, 151, 51, 102, 
                         204, 133, 23, 46, 92, 184, 109, 218, 169, 79, 158, 33, 66, 132, 21, 42, 84, 168, 77, 154, 41, 82, 164, 85, 170, 73, 146, 57, 114, 228, 213, 183, 115, 
                         230, 209, 191, 99, 198, 145, 63, 126, 252, 229, 215, 179, 123, 246, 241, 255, 227, 219, 171, 75, 150, 49, 98, 196, 149, 55, 110, 220, 165, 87, 174, 
                         65, 130, 25, 50, 100, 200, 141, 7, 14, 28, 56, 112, 224, 221, 167, 83, 166, 81, 162, 89, 178, 121, 242, 249, 239, 195, 155, 43, 86, 172, 69, 138, 9, 
                         18, 36, 72, 144, 61, 122, 244, 245, 247, 243, 251, 235, 203, 139, 11, 22, 44, 88, 176, 125, 250, 233, 207, 131, 27, 54, 108, 216, 173, 71, 142, 0 }, 1);
        public static Matrix ind = new Matrix(new List<short> { -1, 0, 1, 25, 2, 50, 26, 198, 3, 223, 51, 238, 27, 104, 199, 75, 4, 100, 224, 14, 52, 141, 239, 129, 28, 193, 105, 248, 200, 8, 76, 113, 
                                        5, 138, 101, 47, 225, 36, 15, 33, 53, 147, 142, 218, 240, 18, 130, 69, 29, 181, 194, 125, 106, 39, 249, 185, 201, 154, 9, 120, 77, 228, 
                                        114, 166, 6, 191, 139, 98, 102, 221, 48, 253, 226, 152, 37, 179, 16, 145, 34, 136, 54, 208, 148, 206, 143, 150, 219, 189, 241, 210, 19, 
                                        92, 131, 56, 70, 64, 30, 66, 182, 163, 195, 72, 126, 110, 107, 58, 40, 84, 250, 133, 186, 61, 202, 94, 155, 159, 10, 21, 121, 43, 78, 212, 
                                        229, 172, 115, 243, 167, 87, 7, 112, 192, 247, 140, 128, 99, 13, 103, 74, 222, 237, 49, 197, 254, 24, 227, 165, 153, 119, 38, 184, 180, 124, 
                                        17, 68, 146, 217, 35, 32, 137, 46, 55, 63, 209, 91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190, 97, 242, 86, 211, 171, 20, 42, 93, 
                                        158, 132, 60, 57, 83, 71, 109, 65, 162, 31, 45, 67, 216, 183, 123, 164, 118, 196, 23, 73, 236, 127, 12, 111, 246, 108, 161, 59, 82, 41, 157, 
                                        85, 170, 251, 96, 134, 177, 187, 204, 62, 90, 203, 89, 95, 176, 156, 169, 160, 81, 11, 245, 22, 235, 122, 117, 44, 215, 79, 174, 213, 233, 230, 
                                        231, 173, 232, 116, 214, 244, 234, 168, 80, 88, 175 }, 1);

        public Matrix() { }

        public Matrix(int widt, int heit)
        {
            width = widt;
            height = heit;
            matxx = new short[width, height];
        }

        public Matrix(short[] arr)
        {
            width = (int)Math.Sqrt(arr.Length);
            height = arr.Length % width == 0 ? arr.Length / Width : (arr.Length / width) + 1;
            matxx = new short[width, height];
            int i = 0, j = 0;
            foreach (short by in arr)
            {
                if (i == width)
                {
                    j++;
                    i = 0;
                }
                matxx[i, j] = by;
                i++;
            }
        }

        public Matrix(List<short> arr, int with)
        {
            width = with;
            height = arr.Count % width == 0 ? arr.Count / Width : (arr.Count / width) + 1;
            matxx = new short[width, height];
            int i = 0, j = 0;
            foreach (short by in arr)
            {
                if (i == width)
                {
                    j++;
                    i = 0;
                }
                matxx[i, j] = by;
                i++;
            }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public static short Mullt(short a, short b)
        {
            if (a * b != 0)
                return Matrix.alp.matxx[0, (Matrix.ind.matxx[0, a] + Matrix.ind.matxx[0, b]) % max];
            else return 0;
        }

        public static short Div(short a, short b)
        {
            if (a * b != 0)
                return Matrix.alp.matxx[0, (Matrix.ind.matxx[0, a] - Matrix.ind.matxx[0, b] + max) % max];
            else return 0;
        }

        public Matrix Multi(Matrix mat1, Matrix mat2)
        {
            int i, j, l;
            Matrix z = new Matrix(mat1.Width, mat2.Height);
            if (mat1.Height == mat2.Width)
            {
                for (i = 0; i < z.Width; i++)
                {
                    for (j = 0; j < z.Height; j++)
                    {
                        z.matxx[i, j] = 0;
                        for (l = 0; l < z.Width; l++)
                            z.matxx[i, j] = (short)(z.matxx[i, j] ^ Mullt(mat1.matxx[i, l], mat2.matxx[l, j]));
                    }
                }
            }
            return z;
        }

        public void Add(Matrix newMatrix)
        {
            if (newMatrix.Height == Height && newMatrix.Width == Width)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Width; j++) matxx[i, j] = (short)(matxx[i, j] & newMatrix.matxx[i, j]);
                }
            }
        }

        public void NumMullt(short byt)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++) matxx[i, j] = (short)Mullt(matxx[i, j], byt);
            }
        }

        public bool Triangle()
        {
            int i, j;
            j = 0;
            bool boo = false;
            while (j < width - 1 && !boo)
            {
                for (i = j + 1; i < width; i++)
                {
                    if (matxx[j, j] != 0)
                    {
                        short q = Div(matxx[i, j], matxx[j, j]);
                        for (int k = 0; k < width; k++) matxx[i, k] = (short)(matxx[i, k] ^ Mullt(matxx[j, k], (short)q));
                    }
                    else boo = true;
                }
                j = j + 1;
            }
            return !boo;
        }

        public Matrix Reverse()
        {
            if (height == width && width != 0)
            {
                Matrix z = new Matrix(width, height);
                Matrix o = new Matrix(width, height);
                Matrix x = new Matrix(width, height);
                int i, j, k;
                for (i = 0; i < width; i++)
                {
                    for (j = 0; j < height; j++)
                    {
                        z.matxx[i, j] = matxx[i, j];
                        x.matxx[i, j] = 0;
                        if (i == j) o.matxx[i, j] = 1;
                        else o.matxx[i, j] = 0;
                    }
                };
                if (z.Triangle())
                {
                    for (k = 0; k < width; k++)
                    {
                        for (i = width - 1; i >= 0; i--)
                        {
                            j = i + 1;
                            while (j < height)
                            {
                                o.matxx[i, k] = (short)(o.matxx[i, k] ^ Mullt(z.matxx[i, j], x.matxx[j, k]));
                                j = j + 1;
                            };
                            x.matxx[i, k] = Div(o.matxx[i, k], z.matxx[i, i]);
                        }
                    }
                }
                else return null;
                i = j = 0;
                bool isNull = true;
                while (isNull && i < x.Width && j < x.Height)
                {
                    for (j = 0; j < x.Height; j++) if (x.matxx[i, j] != 0)
                        {
                            isNull = false;
                            break;
                        }
                    i++;
                }
                if (isNull) return null;
                return x;
            }
            else return null;
        }

    }

}
