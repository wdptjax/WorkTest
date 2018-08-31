using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestForm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            DateTime dt = DateTime.Now;
            double span = dt.Subtract(new DateTime(1900,1,1)).TotalDays;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
