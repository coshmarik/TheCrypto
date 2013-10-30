using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Windows;

namespace Crypto.Classes
{
    [Serializable]
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

        public static Pass FillPass(int kk, int k, int r)
        {
            Pass pss = new Pass();
            int i, j;
            int length = k, errors = r / 2;

            i = kk * kk * k * k;

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

            string path = string.Format("{0}.{1}", DateTime.Now.ToString("ddMMyyyyhhmmss"), "pss");
            pss.SavePass(path);
            MessageBox.Show(string.Format("Файл ключа сохранен в {0}", path));

            return pss;
        }

        public static Pass LoadPass(string passname)
        {
            Pass p = null;
            if (File.Exists(passname))
            {
                XmlSerializer xmlSerr = new XmlSerializer(typeof(Pass));
                StreamReader strReader = new StreamReader(passname);
                p = xmlSerr.Deserialize(strReader) as Pass;
            }
            return p;
        }

        public void SavePass(string path)
        {
            XmlSerializer xmlSerr = new XmlSerializer(typeof(Pass));
            StreamWriter strWriter = new StreamWriter(path);
            xmlSerr.Serialize(strWriter, this);
        }
    }

}
