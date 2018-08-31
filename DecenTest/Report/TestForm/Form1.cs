using OpenXmlReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestForm
{
    public partial class Form1 : Form
    {
        DataSet ds = new DataSet();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            Task.Factory.StartNew(Test);
        }

        private void Test()
        {
            if (ds.Tables.Count > 0)
                ds.Tables.Clear();
            DataTable dt;
            Random rd = new Random();
            for (int i = 0; i < 10; i++)
            {
                string name = string.Format("数据{0}", i + 1);
                dt = new DataTable(name);
                for (int j = 0; j < i + 10; j++)
                {
                    string colName = string.Format("示例列{0}", j + 1);
                    if (j == 0)
                        dt.Columns.Add(colName, typeof(DateTime));
                    else if (j % 2 == 0)
                        dt.Columns.Add(colName, typeof(double));
                    else
                        dt.Columns.Add(colName, typeof(string));
                }
                dt.Columns.Add("测试公式", typeof(double));

                this.Invoke(new Action(() => label1.Text = string.Format("开始生成第{0}组数据====", i + 1)));
                for (int r = 0; r < (i + 1) * 1000; r++)
                {
                    DataRow dr = dt.NewRow();
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        if (c == 0)
                        {
                            DateTime time = DateTime.Now.AddHours(r);
                            dr[c] = time;
                        }
                        else if (c == dt.Columns.Count - 1)
                        {
                            dr[c] = 0;
                        }
                        else if (c % 2 == 0)
                        {
                            dr[c] = rd.NextDouble() * 100;
                        }
                        else
                        {
                            dr[c] = Guid.NewGuid().ToString();
                        }
                    }
                    dt.Rows.Add(dr);
                }
                this.Invoke(new Action(() => label1.Text = string.Format("第{0}组数据生成完毕----", i + 1)));
                ds.Tables.Add(dt);
            }
            this.Invoke(new Action(() => label1.Text = "数据生成完毕"));
            this.Invoke(new Action(() =>
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenXmlReportExport export = new OpenXmlReportExport(this.Handle);
            export.ExportData = ds;
            export.Start();
            Console.ReadLine();
        }
    }
}
