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
using System.Xml.Serialization;
using System.Windows;
using System.Runtime.Serialization;
using System.Xml;
using Windows.Storage;
using System.Threading.Tasks;

namespace Crypto.Classes
{
    public class Pass
    {
        private const short max = 255;
        public List<short> S, P; //матрицы ключа криптосистемы
        public List<short> First, Second;//матрицы для перестановок
        public int Length, Errors;

        public static List<short> NonRepeate(int length)
        {
            List<short> arr = new List<short>();
            int i;
            bool boo;
            short a;
            Random rand = new Random();
            for (i = 0; i < length; i++)
            {
                boo = false;
                while (!boo)
                {
                    a = (short)rand.Next(0, length);
                    if (!arr.Contains(a))
                    {
                        arr.Add(a);
                        boo = true;
                    }
                }
            }
            return arr;
        }

        public Pass() { }

        private static Pass FillPass(int count)
        {
            int k = Crypt.k, r = Crypt.r;
            Pass pss = new Pass();
            try
            {
                int i, j;
                int length = k, errors = r / 2;

                i = count * count * k * k;

                List<short> first = new List<short>();
                first = NonRepeate(i);

                List<short> second = new List<short>();
                second = NonRepeate(i);

                List<short> s = new List<short>();
                List<short> p = new List<short>();

                Random rand = new Random();
                Matrix sp = new Matrix();
                while (sp.Reverse() == null)
                {
                    s = new List<short>();
                    for (j = 0; j < i; j++) s.Add((short)rand.Next(1, max));
                    sp = new Matrix(s.ToArray());
                }

                sp = new Matrix();
                while (sp.Reverse() == null)
                {
                    p = new List<short>();
                    for (j = 0; j < i; j++) p.Add((short)rand.Next(1, max));
                    sp = new Matrix(p.ToArray());
                }

                pss.S = s;
                pss.P = p;
                pss.First = first;
                pss.Second = second;
                pss.Length = length;
                pss.Errors = errors;
                return pss;
            }
            catch (Exception ex)
            {
                Engine.LogMessage(ex);
                Engine.SendMessage(ex.Message);
                return null;
            }
        }

        public async static Task<Pass> LoadPass(StorageFile file)
        {
            Pass p = null;
            DataContractSerializer dcSer = new DataContractSerializer(typeof(Pass));
            p = dcSer.ReadObject(await file.OpenStreamForReadAsync()) as Pass;
            return p;
        }

        private async Task<StorageFile> SavePass(string fileName, StorageFolder cryptFolder)
        {
            try
            {
                StorageFile file = await cryptFolder.CreateFileAsync(fileName);
                DataContractSerializer dcSer = new DataContractSerializer(typeof(Pass));
                dcSer.WriteObject(await file.OpenStreamForWriteAsync(), this);
                return file;
            }
            catch (Exception ex)
            {
                Engine.LogMessage(ex);
                Engine.SendMessage(ex.Message);
                return null;
            }
        }

        public async static Task<StorageFile> MakePass(int size, string fileName, StorageFolder cryptFolder)
        {
            try
            {
                Pass pss = FillPass(size);
                if (pss != null)
                {
                    StorageFile file = await pss.SavePass(DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pss", cryptFolder);
                    return file;
                }
                else return null;
            }
            catch (Exception ex)
            {
                Engine.LogMessage(ex);
                Engine.SendMessage(ex.Message);
                return null;
            }
        }
    }

}
