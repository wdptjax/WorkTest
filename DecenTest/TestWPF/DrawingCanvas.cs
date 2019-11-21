using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWPF
{
    public class DrawingCanvas : Canvas
    {
        private List<Visual> visuals = new List<Visual>();
        protected override int VisualChildrenCount
        {
            get
            { return visuals.Count; }
        }
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);


            if (visual is Button btn)
            {
                Children.Add(btn);       //在Canvas的Children中添加子元素
            }
            else
            {
                base.AddVisualChild(visual);
                base.AddLogicalChild(visual);
            }

        }
        public void DeleteVisual(Visual visual)
        {
            visuals.Remove(visual);

            if (visual is Button btn)
            {
                Children.Remove(btn);
            }
            else
            {
                base.RemoveVisualChild(visual);
                base.RemoveLogicalChild(visual);
            }
        }
        public void ClearVisual()
        {
            foreach (var visual in visuals)
            {
                if (visual is Button btn)
                {
                    Children.Remove(btn);
                }
                else
                {
                    base.RemoveLogicalChild(visual);
                    base.RemoveVisualChild(visual);

                }
            }
            visuals.Clear();
        }

        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;

        }

    }
}
