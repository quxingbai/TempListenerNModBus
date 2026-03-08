using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TempListenerNModBus.Controls
{
    /// <summary>
    /// 根据Value绘制
    /// </summary>
    public class CustomValueDrawMap : ItemsControl
    {


        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayPathProperty =
            DependencyProperty.Register(nameof(DisplayPath), typeof(string), typeof(CustomValueDrawMap), new PropertyMetadata("Text"));



        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(CustomValueDrawMap), new PropertyMetadata("Value"));




        public string DatePath
        {
            get { return (string)GetValue(DatePathProperty); }
            set { SetValue(DatePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DatePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DatePathProperty =
            DependencyProperty.Register(nameof(DatePath), typeof(string), typeof(CustomValueDrawMap), new PropertyMetadata("Date"));

        protected record MemberItem(object Source, object SelectedValue, double SourceValue, DateTime? Date);

        private List<MemberItem> Members = new();
        private Dictionary<object, MemberItem> ObjectRelation = new();
        private Pen defaultPen = new(Brushes.Gray, 1);

        static CustomValueDrawMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomValueDrawMap), new FrameworkPropertyMetadata(typeof(CustomValueDrawMap)));
        }

        private MemberItem ObjectToMember(object Source)
        {
            var st = Source.GetType();
            double sourceValue = (double)st.GetProperty(this.ValuePath).GetValue(Source);
            object? selectedValue = st.GetProperty(this.DisplayPath)?.GetValue(Source);
            DateTime? selectedDate = st.GetProperty(this.DatePath)?.GetValue(Source) as DateTime?;

            var item = new MemberItem(Source, selectedValue, sourceValue, selectedDate);
            return item;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue is INotifyCollectionChanged nv)
            {
                Members.Clear();
                foreach (var i in newValue)
                {
                    Members.Add(ObjectToMember(i));
                }
                nv.CollectionChanged += Nv_CollectionChanged;
            }
            if (oldValue is INotifyCollectionChanged ov)
            {
                ov.CollectionChanged -= Nv_CollectionChanged;
            }
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        private void Nv_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (false)
            //if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var itme = e.NewItems[i];
                    //Members.Add(ObjectToMember(itme));
                    Members.Insert(e.NewStartingIndex + i, ObjectToMember(itme));
                }
            }
            else
            {

                Members.Clear();
                foreach (var i in Items)
                {
                    Members.Add(ObjectToMember(i));
                }
            }
            InvalidateVisual();
        }


        private Point GetValuePoint(MemberItem item, int index)
        {

            var Val = item.SourceValue;
            var Dat = item.Date;
            var max = MapMax();
            var min = MapMin();
            Rect rect = GetDrawingMapRect();

            var Ypersentage = (1 - (Val - min) / (max - min));
            var Xpersentage = (index * 1.0 / (Members.Count - 1));
            return new(rect.Left + rect.Width * Xpersentage, rect.Top + rect.Height * Ypersentage);
        }
        private double MapMax()
        {
            var max = Members.Count == 0 ? 0 : Members.Max(w => w.SourceValue);
            return Math.Max(40, max);
        }
        private double MapMin()
        {
            return 0;
            var min = Members.Min(w => w.SourceValue);
            return Math.Max(0, min);
        }
        protected virtual Pen GetDrawPenFromItem(MemberItem Item)
        {
            return defaultPen;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {

            var index = 0;
            Point? lastPoint = null;
            Rect rect = GetDrawingMapRect();
            var xlinCount = GetXLineCount();
            var max = MapMax();
            var min = MapMin();
            var wsp = new Pen(Brushes.WhiteSmoke, 1);

            for (int i = 0; i <= xlinCount; i++)
            {
                var yp = i * 1.0 / xlinCount;
                var y = rect.Top + rect.Height * yp;
                var value = Math.Round((max - min) * (1 - yp), 1);


                drawingContext.DrawLine(wsp, new(rect.Left, y), new(rect.Right, y));
                drawingContext.DrawText(CreatText(value.ToString(), Brushes.DodgerBlue,10), new(rect.Right, y));
            }
            foreach (MemberItem i in Members)
            {
                var pen = GetDrawPenFromItem(i);
                var point = GetValuePoint(i, index);
                drawingContext.DrawLine(wsp, new Point(point.X, rect.Top), new(point.X, rect.Bottom));
                drawingContext.DrawLine(pen, lastPoint ?? point, point);
                if(Members.Count<100)
                drawingContext.DrawText(CreatText(i.SourceValue.ToString(), Brushes.Gray,10), point);
                lastPoint = point;
                index++;
            }

            base.OnRender(drawingContext);
        }
        private FormattedText CreatText(string text, Brush brush,double fontSize)
        {
            FormattedText ft = new(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new(""), fontSize, brush);
            return ft;
        }
        private int GetXLineCount()
        {
            return 3;
        }
        private Rect GetDrawingMapRect()
        {
            Thickness tc = new(30, 20, 30, 20);
            Rect rec = new(RenderSize);
            return new()
            {
                X = tc.Left,
                Y = tc.Top,
                Width = rec.Width - tc.Right - tc.Left,
                Height = rec.Height - tc.Bottom - tc.Top
            };
        }
    }
}
