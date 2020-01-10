using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestInteractiveDataDisplay
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //线程中更新曲线
            Thread threadTmp = new Thread(UpdateChart);
            threadTmp.Start();
        }

        private void UpdateChart()
        {
            int nPointNum = 1600;
            Random randm = new Random();
            double[] dArray = new double[nPointNum];
            double[] dX = new double[nPointNum];
            double[] dY = new double[nPointNum];
            double dRandomtTmp = 0;

            while (true)
            {
                Thread.Sleep(100);//每秒刷新一次
                for (int n = 0; n < dArray.Length; n++)
                {
                    dRandomtTmp = randm.NextDouble();
                    dArray[n] = (dRandomtTmp < 0.5) ? -dRandomtTmp * dArray.Length : dRandomtTmp * dArray.Length;
                }
                for (int n = 0; n < dX.Length; n++)
                {
                    dX[n] = n;
                    dY[n] = randm.Next(dX.Length);
                }

                //更新UI
                Dispatcher.Invoke(new Action(delegate
                {
                    //this.BarChart.PlotBars(dArray);
                    this.LineChart.Plot(dX, dY);
                }));
            }
        }
    }
}
