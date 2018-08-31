using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestWinform
{
    public partial class Form1 : Form
    {
        Dictionary<int, ProgressBar> progressBars = new Dictionary<int, ProgressBar>();
        public Form1()
        {
            InitializeComponent();

            Image img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(Brushes.Red, 0, 0, img.Width, img.Height);
            g.DrawString("asfd", this.Font, Brushes.Black, 0, 0);
            pictureBox1.Image = img;
            //dataGridView1.Rows.Add("100%", "100%");
            //dataGridView1.Rows.Add("50%", "50%");
            //dataGridView1.Rows.Add("1%", "10%");
            //dataGridView1.Rows.Add("0.9%", "0.9%");
            //dataGridView1.Rows.Add("0.1%", "0.1%");
            //dataGridView1.Rows.Add("0%", "0%");
            DataTable dt = new DataTable();
            dt.Columns.Add("选择", typeof(bool));
            dt.Columns.Add("test1", typeof(string));
            dt.Columns.Add("test2", typeof(string));
            dt.Rows.Add(true, "aaa", "bbb");
            dt.Rows.Add(false, "aaa", "bbb");
            dataGridView1.DataSource = dt;
            test();
        }

        private void test()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("col1", typeof(int));
            dt.Columns.Add("col2", typeof(string));
            dt.Columns.Add("col3", typeof(double));
            dataGridView2.DataSource = dt;

            Task.Factory.StartNew(scan);
        }

        private void scan()
        {
            int index = 0;
            Random rd = new Random();
            DataTable dt = (DataTable)dataGridView2.DataSource;
            // 测试Datagridview异步显示
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    int[] arr = new int[3 + index];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = i + 1;
                        if (i < dt.Rows.Count)
                        {
                            DataRow dr = dt.Rows[i];
                            dr[0] = arr[i];
                            dr[1] = Guid.NewGuid().ToString();
                            dr[2] = rd.NextDouble();
                        }
                        else
                        {
                            dataGridView2.Invoke(new Action(() =>
                            {
                                DataRow dr = dt.NewRow();
                                dr[0] = arr[i];
                                dr[1] = Guid.NewGuid().ToString();
                                dr[2] = rd.NextDouble();
                                dt.Rows.Add(dr);
                            }));
                        }
                    }
                    while (dt.Rows.Count > arr.Length)
                    {
                        dataGridView2.Invoke(new Action(() =>
                        {
                            dt.Rows.RemoveAt(dt.Rows.Count - 1);
                        }));
                    }



                    index++;
                    if (index == 3)
                        index = 0;
                }
                catch (Exception ex)
                {
                }
             
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null) return;
            if (e.ColumnIndex == 0)
            {
                string str = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (!str.EndsWith("%"))
                    return;
                float val = 0;
                if (!float.TryParse(str.TrimEnd('%'), out val))
                    return;
                // 设置进度条的前景色以及背景色，与表格的颜色相反
                Brush forColor = new Pen(dataGridView1.BackgroundColor).Brush;
                Brush backColor = new Pen(dataGridView1.ForeColor).Brush;
                // 如果本行已选中，需要对应修改颜色
                if (dataGridView1.SelectedRows.Contains(dataGridView1.Rows[e.RowIndex]))
                {
                    forColor = new Pen(dataGridView1.ForeColor).Brush;
                    backColor = new Pen(dataGridView1.BackgroundColor).Brush;
                    e.PaintBackground(e.CellBounds, true);// 绘制背景色
                }
                else
                    e.PaintBackground(e.CellBounds, false);// 绘制背景色
                // 设置绘图区域
                Rectangle rect = new Rectangle(e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width - 1, e.CellBounds.Height - 1);
                // 设置字体格式-垂直居中显示，不换行
                StringFormat format = new StringFormat(StringFormatFlags.NoWrap);
                format.LineAlignment = StringAlignment.Center;
                // 绘制底色文字，与表格背景颜色相反
                e.Graphics.DrawString(str, dataGridView1.Font, backColor, rect, format);
                // 获取进度条区域
                float drawWidth = (float)rect.Width / 100 * val;
                Rectangle rectTop = new Rectangle(rect.X, rect.Y, (int)drawWidth, rect.Height);
                // 进行进度条的绘制
                Image img = new Bitmap(rect.Width, rect.Height);// 这里的宽度需要使用表格的宽度，否则文字会有缺失
                Graphics grap = Graphics.FromImage(img);
                var rectImg = grap.VisibleClipBounds;
                // 填充进度条背景，与表格背景颜色相反
                grap.FillRectangle(backColor, rectImg);
                // 绘制进度条文字，与进度条背景颜色相反
                grap.DrawString(str, dataGridView1.Font, forColor, rectImg, format);
                // 将进度条截取绘制到表格中
                RectangleF rectImgClip = new RectangleF(rectImg.X, rectImg.Y, rectTop.Width, rectTop.Height);
                e.Graphics.DrawImage(img, rectTop, rectImgClip, GraphicsUnit.Pixel);
                e.Handled = true;
            }

            if (e.ColumnIndex == 1)
            {
                string str = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (!str.EndsWith("%"))
                    return;
                float val = 0;
                if (!float.TryParse(str.TrimEnd('%'), out val))
                    return;
                if (!progressBars.ContainsKey(e.RowIndex))
                {
                    ProgressBar progressBar = new ProgressBar();
                    progressBar.Maximum = 100;
                    progressBars.Add(e.RowIndex, progressBar);
                    dataGridView1.Controls.Add(progressBar);
                }
                ProgressBar bar = progressBars[e.RowIndex];
                bar.Left = e.CellBounds.X;
                bar.Top = e.CellBounds.Y;
                bar.Height = e.CellBounds.Height - 1;
                bar.Width = e.CellBounds.Width - 1;
                bar.Value = (int)val;
                bar.BackColor = dataGridView1.ForeColor;
                bar.ForeColor = dataGridView1.BackgroundColor;
                bar.Visible = true;
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

        }
    }
}
