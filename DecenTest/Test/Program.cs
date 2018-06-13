using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test();
        }

        private static void Test()
        {
            int[] fl = new int[10000];
            Random rd = new Random();
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = rd.Next();
            }
            while (string.IsNullOrEmpty(Console.ReadLine()))
            {

                DateTime dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = fl.Max();
                }
                Console.WriteLine("lambda " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = fl[0];
                    for (int j = 0; j < fl.Length; j++)
                    {
                        if (max < fl[j])
                            max = fl[j];
                    }
                }
                Console.WriteLine("For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
            }
        }
    }
}
