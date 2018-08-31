using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //CreateDir("D:\\Testttttt\\");
            //Test2();
            //Test3();
            TestUdp();
        }

        private static void TestUdp()
        {
            Task.Factory.StartNew(Server);
            Client();
        }

        private static void Server()
        {
            Socket srv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            srv.Bind(new IPEndPoint(IPAddress.Any, 50000));
            while (true)
            {
                EndPoint point = new IPEndPoint(IPAddress.Any, 0);//用来保存发送方的ip和端口号
                byte[] buffer = new byte[1024];
                int len = srv.ReceiveFrom(buffer, ref point);
                string message = Encoding.ASCII.GetString(buffer, 0, len);
                Console.WriteLine(point.ToString() + ":" + message);
            }
        }

        private static void Client()
        {
            UdpClient client = new UdpClient("127.0.0.1", 50000);
            string str = Console.ReadLine();
            while (!string.IsNullOrEmpty(str))
            {
                byte[] data = Encoding.ASCII.GetBytes(str);
                client.Send(data, data.Length);
                str = Console.ReadLine();
            }
        }

        private static void Test3()
        {
            int[] arr1 = new int[] { 1, 2, 3, 4 };
            Console.WriteLine("before------------");
            Show(arr1);
            Test4(ref arr1);
            Console.WriteLine("after------------");
            Show(arr1);
        }

        private static void Show(int[] arr)
        {
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ",";
            }
            Console.WriteLine(str);
        }

        private static void Test4(ref int[] arr)
        {
            arr = new int[5];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i + 10;
            }
        }

        private static void Test2()
        {
            Dictionary<string, Dictionary<string, int>> dic = new Dictionary<string, Dictionary<string, int>>();
            string[] strArr = new string[100000];
            for (int i = 0; i < 100000; i++)
            {
                string str = i.ToString();
                Dictionary<string, int> d = new Dictionary<string, int>();
                for (int j = 0; j < 100; j++)
                {
                    string s = j.ToString();
                    d.Add(s, j);
                }
                dic.Add(str, d);
                strArr[i] = str;
            }

            Console.WriteLine("1--------------");

            double avg = 0;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    for (int j = 0; j < dic.Count; j++)
                    {
                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.WriteLine("2--------------");

            avg = 0;
            int count = dic.Count;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    for (int j = 0; j < count; j++)
                    {
                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.WriteLine("3--------------");

            avg = 0;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    foreach (var pair in dic)
                    {

                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.ReadLine();
            /*
             * 结论 第二种方法效率最高
             * 1--------------
             * 平均用时：24.3
             * 2--------------
             * 平均用时：15.2
             * 3--------------
             * 平均用时：148.5
             */
        }

        private static void CreateDir(string dir)
        {
            if (Directory.Exists(dir))
                return;

            string dir1 = dir.TrimEnd('\\');
            int index = dir1.LastIndexOf('\\');
            string dir2 = dir1;
            if (index > 0)
                dir2 = dir1.Substring(0, index).TrimEnd('\\');
            CreateDir(dir2);
            Directory.CreateDirectory(dir1);
        }

        private static void Test1()
        {
            ConcurrentDictionary<int, string> dic = new ConcurrentDictionary<int, string>();

            for (int i = 0; i < 10; i++)
            {
                dic.TryAdd(i, i.ToString());
                Console.WriteLine(i);
            }
            Console.WriteLine("------------");
            foreach (var pairs in dic)
            {
                Console.WriteLine(pairs.Key);
                if (pairs.Key == 5)
                {
                    string str;
                    dic.TryRemove(pairs.Key, out str);
                    Console.WriteLine("del " + pairs.Key);
                }
            }
            Console.WriteLine("new------------");
            foreach (var pairs in dic)
            {
                Console.WriteLine(pairs.Key);

            }
            Console.ReadLine();
        }

        private static void Test()
        {
            int[] fl = new int[10000];
            Random rd = new Random();
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = rd.Next();
            }
            List<int> list = fl.ToList();
            while (string.IsNullOrEmpty(Console.ReadLine()))
            {
                /*
                    List,lambda 760.000     耗时最久
                    List,For 520.000
                    Array,lambda 704.000
                    Array,For 421.000       耗时最少

                 */
                DateTime dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = list.Max();
                }
                Console.WriteLine("List,lambda " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = list[0];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (max < list[j])
                            max = list[j];
                    }
                }
                Console.WriteLine("List,For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int[] arr = list.ToArray();
                    int max = arr.Max();
                }
                Console.WriteLine("Array,lambda " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int[] arr = list.ToArray();
                    int max = arr[0];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (max < arr[j])
                            max = arr[j];
                    }
                }
                Console.WriteLine("Array,For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
            }
        }
    }
}
