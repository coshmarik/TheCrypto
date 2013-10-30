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
using System.IO;
using Windows.Storage;

namespace Crypto.Classes
{

    public static class Crypt
    {
        public const int k = 12, t = 2, r = 2 * t, n = k + r, max = 255;
        static short[] g = { 116, 231, 216, 30, 1 };//порождающий полином кода Рида-Соломона, 
        //        static Pass pass;//ключ криптосистемы

        //public Crypt(StorageFile file)
        //{
        //    try
        //    {
        //        pass = Pass.LoadPass(file);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    };
        //}


        public static List<short> EnCrypt(List<short> data, Pass pass)
        {
            List<short> Arr = new List<short>();//выходной массив

            int ss = (int)Math.Sqrt((double)pass.S.Count);
            int pp = (ss / k) * n;

            Matrix s = new Matrix(pass.S, ss);//ключ
            Matrix p = new Matrix(pass.P, ss);//ключ
            Matrix dat = new Matrix(ss, 1); //массивы, на которые разбивается входящее сообщение для последующей шифровки
            Matrix datt = new Matrix(pp, 1); //массивы, получившиеся при кодировании и шифровании
            Matrix dt = new Matrix(k, 1); //массивы, применяемые при кодировании

            int i, j, l;// счетчики

            if (data.Count % ss != 0)
            {
                for (i = data.Count % ss; i < ss; i++) data.Add(0);
            }
            s.Triangle();
            p.Triangle();
            for (l = 0; l <= data.Count; l++)
            {
                if (l % ss == 0 && l != 0)
                {
                    dat = dat.Multi(s, dat);//Умножение на матрицу s
                    dat = dat.Multi(p, dat);//Умножение на матрицу p

                    for (j = 0; j <= ss; j++)
                    {
                        if (j % k == 0 && j != 0)
                        {
                            Arr.AddRange(EnCode(dt));
                            if (j < ss) dt.matxx[j % k, 0] = dat.matxx[j, 0];
                        }
                        else dt.matxx[j % k, 0] = dat.matxx[j, 0];
                    }
                    if (l < data.Count) dat.matxx[l % ss, 0] = data[l];
                }
                else dat.matxx[l % ss, 0] = data[l];
            }
            i = Arr.Count - 1;
            while (Arr[i] == 0)
            {
                Arr.RemoveAt(i);
                i--;
            }
            return HoloCode(Arr, pass);
        }

        public static List<short> DeCrypt(List<short> arr, Pass pass)
        {
            List<short> data = new List<short>();//выходной массив

            int ss = (int)Math.Sqrt((double)pass.S.Count);
            int pp = (ss / k) * n;

            Matrix s = new Matrix(pass.S, ss);//ключ
            Matrix p = new Matrix(pass.P, ss);//ключ
            Matrix datt = new Matrix(pp, 1); //массивы, получившиеся при раскодировании и расшифровании
            Matrix dat = new Matrix(ss, 1); //массивы, на которые разбивается расшифрованное сообщение
            Matrix recd = new Matrix(n, 1);//массивы, на которые разбивается входящий массив
            Matrix daat = new Matrix(k, 1);//раскодированный массив

            int a, b, i, j;// счетчики

            arr = HoloDeCode(arr, pass);

            s = s.Reverse();
            p = p.Reverse();

            if (arr.Count % pp != 0)
            {
                for (i = arr.Count % pp; i < pp; i++) arr.Add(0);
            }
            for (a = 0; a <= arr.Count; a++)
            {
                if (a % pp == 0 && a != 0)
                {
                    j = 0;
                    for (b = 0; b <= pp; b++)
                    {
                        if (b % n == 0 && b != 0)
                        {
                            daat = DeCode(recd);
                            for (i = 0; i < k; i++) dat.matxx[i + (j * k), 0] = daat.matxx[i, 0];
                            j++;
                            if (b < pp) recd.matxx[b % n, 0] = datt.matxx[b, 0];
                        }
                        else recd.matxx[b % n, 0] = datt.matxx[b, 0];
                    }
                    dat = dat.Multi(p, dat);
                    dat = daat.Multi(s, dat);
                    for (i = 0; i < ss; i++) data.Add(dat.matxx[i, 0]);

                    if (a < arr.Count) datt.matxx[a % pp, 0] = arr[a];
                }
                else datt.matxx[a % pp, 0] = arr[a];
            }
            i = data.Count - 1;
            while (data[i] == 0)
            {
                data.RemoveAt(i);
                i--;
            }
            return data;
        }

        private static List<short> EnCode(Matrix dt)
        {
            int i, j;
            short bb;
            List<short> encoded = new List<short>();
            Matrix a = new Matrix(n, 1);
            for (i = 0; i < k; i++) a.matxx[i + r, 0] = dt.matxx[i, 0];
            for (i = 0; i < r; i++) a.matxx[i, 0] = 0;
            for (i = n - 1; i > r - 1; i--)
            {
                bb = Matrix.Div(a.matxx[i, 0], g[r]);
                for (j = 0; j <= r; j++) a.matxx[i - j, 0] = (short)(a.matxx[i - j, 0] ^ Matrix.Mullt(bb, g[r - j]));
            }
            for (i = 0; i < r; i++) encoded.Add(a.matxx[i, 0]);
            for (i = 0; i < k; i++) encoded.Add(dt.matxx[i, 0]);
            return encoded;
        }

        private static Matrix DeCode(Matrix recd)
        {
            int i, j, m, l, ll, q, d;
            bool syn_error = false;//признак ошибки
            Matrix decoded = new Matrix(k, 1);
            //вычисляем синдром
            short[] syn = new short[r + 1];//полином синдрома ошибки
            for (i = 1; i <= r; i++)
            {
                syn[i] = 0;
                for (j = 0; j < n; j++) syn[i] ^= (short)(Matrix.Mullt(recd.matxx[j, 0], Matrix.alp.matxx[0, (i * j)]));
                if (syn[i] != 0) syn_error = true;
            }
            // Коррекция ошибок, вычисляем полином локаторов по алгоритму Берлекэмпа-Месси 
            if (syn_error)
            {
                short[] lam = new short[r + 1];//полином локатора ошибки
                short[] lamz = new short[r + 1];//вспомогательный массив
                short[] bb = new short[r + 1];//вспомогательный массив
                for (i = 0; i < r; i++)
                {
                    lam[i] = 0;
                    lamz[i] = 0;
                    bb[i] = 0;
                }
                lam[0] = 1;
                bb[1] = 1;
                q = 0;
                l = 0;
                m = -1;
                while (q < r)
                {
                    d = 0;
                    for (i = 0; i < l + 1; i++) d = d ^ Matrix.Mullt(lam[i], syn[q - i + 1]);
                    if (d != 0)
                    {
                        for (i = 0; i <= r; i++) lamz[i] = (short)(Matrix.Mullt(bb[i], (short)d) ^ lam[i]);
                        if (l < q - m)
                        {
                            ll = q - m;
                            m = q - l;
                            l = ll;
                            for (i = 0; i <= r; i++)
                                bb[i] = Matrix.Mullt(Matrix.alp.matxx[0, 255 - Matrix.ind.matxx[0, d]], lam[i]);
                        }
                        for (i = 0; i <= r; i++) lam[i] = lamz[i];
                    }
                    for (i = r; i > 0; i--) bb[i] = bb[i - 1];
                    bb[0] = 0;
                    q = q + 1;
                }
                i = 0;
                while (lam[i] != 0 && lam[i + 1] != 0) i++;
                //если степень полинома локаторов равна предполагаемому кол-ву искаженных байтов, то
                //найдем корни, в которых полином локаторов обращается в 0
                if (i == l)
                {
                    short[] loc = new short[t];//локаторы ошибок
                    for (ll = 0; ll < t; ll++) loc[ll] = 0;
                    ll = 0;
                    for (i = 1; i <= max; i++)
                    {
                        d = 0;
                        for (j = 0; j < l + 1; j++) d = d ^ Matrix.Mullt(lam[j], Matrix.alp.matxx[0, (i * j) % max]);
                        if (d == 0)
                        {
                            loc[ll] = (short)i;
                            ll++;
                        }
                    }
                    // вычислим формальную производную полинома локаторов
                    d = 0;
                    for (i = 0; i < t + 1; i++)
                    {
                        if (i % 2 != 0) d = d ^ Matrix.Mullt(lam[i], Matrix.alp.matxx[0, i - 1]);
                    }
                    //вычислим полином величины ошибок
                    short[] val = new short[r + 2];//полином величин ошибок
                    for (i = 0; i <= l; i++)
                    {
                        for (j = 1; j < syn.Length; j++) val[i + j - 1] ^= Matrix.Mullt(lam[i], syn[j]);
                    }
                    //вычислим полином значений ошибок
                    short[] vall = new short[t];//вспомогательный массив
                    for (i = 0; i < t; i++) vall[i] = 0;
                    for (i = 0; i < t; i++)
                    {
                        for (j = 0; j < t; j++) vall[i] = (short)(vall[i] ^ Matrix.Mullt(val[j], Matrix.alp.matxx[0, loc[i] * j]));
                        vall[i] = Matrix.Div(vall[i], (short)d);
                    }
                    // вычислим локаторы ошибок
                    for (i = 0; i < t; i++)
                    {
                         if (max - loc[i] >= 0 && max - loc[i] <= n - 1)
                            loc[i] = (short)(max - loc[i]);
                        else loc[i] = -1;
                    }
                    //убираем искажения из полинома
                    for (j = 0; j < t; j++)
                    {
                        if (loc[j] == -1) continue;
                        recd.matxx[loc[j], 0] = (short)(recd.matxx[loc[j], 0] ^ vall[j]);
                    }
                }
            }
            for (i = r; i < n; i++) decoded.matxx[i - r, 0] = recd.matxx[i, 0];
            return decoded;
        }

        private static List<short> HoloCode(List<short> Arr, Pass pass)
        {
            int i, j, l;
            List<short> arr = new List<short>();//псевдоголографическое кодирование
            if (pass.First.Count > Arr.Count)
            {
                List<short> fit = new List<short>();

                for (i = 0; i < Arr.Count; i++) fit.Add(pass.First[i]);
                fit.Sort();
                foreach (short sh in fit) arr.Add(Arr[pass.First.IndexOf(sh)]);
                Arr = arr;

                fit = new List<short>();
                arr = new List<short>();
                for (i = 0; i < Arr.Count; i++) fit.Add(pass.Second[i]);
                fit.Sort();
                foreach (short sh in fit) arr.Add(Arr[pass.Second.FindIndex(ps => ps == sh)]);
                Arr = arr;
            }
            else
            {
                i = Arr.Count / pass.First.Count;
                for (j = 0; j < pass.First.Count; j++)
                {
                    for (l = 0; l < (pass.First[j] == pass.First.Count - 1 ? (Arr.Count % pass.First.Count + i) : i); l++)
                        arr.Add(Arr[l + pass.First[j] * i]);
                }

                Arr = new List<short>();
                for (j = 0; j < pass.Second.Count; j++)
                {
                    for (l = 0; l < (pass.Second[j] == pass.Second.Count - 1 ? (arr.Count % pass.Second.Count + i) : i); l++)
                        Arr.Add(arr[l + pass.Second[j] * i]);
                }
            }
            return Arr;
        }

        private static List<short> HoloDeCode(List<short> arr, Pass pass)
        {
            int i, j, m, d, l, ll;
            if (pass.First.Count > arr.Count)
            {
                short[] arrr = new short[arr.Count];
                List<short> fit = new List<short>();

                for (i = 0; i < arr.Count; i++) fit.Add(pass.Second[i]);
                fit.Sort();
                for (i = 0; i < fit.Count; i++) arrr[pass.Second.IndexOf(fit[i])] = arr[i];

                fit = new List<short>();
                for (i = 0; i < arr.Count; i++) fit.Add(pass.First[i]);
                fit.Sort();
                for (i = 0; i < fit.Count; i++) arr[pass.First.FindIndex(ps => ps == fit[i])] = arrr[i];
            }
            else
            {
                List<short> arrr = new List<short>();
                i = arr.Count / pass.First.Count;
                m = pass.Second.FindIndex(f => f == (pass.Second.Count - 1));
                for (j = 0; j < pass.Second.Count; j++)
                {
                    l = pass.Second.FindIndex(f => f == j);
                    ll = l > m ? l * i + (arr.Count % pass.Second.Count) : l * i;
                    for (d = 0; d < (j == pass.Second.Count - 1 ? i + (arr.Count % pass.Second.Count) : i); d++) arrr.Add(arr[ll + d]);
                }

                arr = new List<short>();

                m = pass.First.FindIndex(f => f == (pass.First.Count - 1));
                for (j = 0; j < pass.First.Count; j++)
                {
                    l = pass.First.FindIndex(f => f == j);
                    ll = l > m ? l * i + (arrr.Count % pass.First.Count) : l * i;
                    for (d = 0; d < (j == pass.First.Count - 1 ? i + (arrr.Count % pass.First.Count) : i); d++) arr.Add(arrr[ll + d]);
                }
            }
            return arr;
        }

    }
}
