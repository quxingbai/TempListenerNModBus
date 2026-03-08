using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace TempListenerNModBus.Controls
{
    public class DateValuDrawMap : ItemsControl
    {


        public int MaxDisplayCount
        {
            get { return (int)GetValue(MaxDisplayCountProperty); }
            set { SetValue(MaxDisplayCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxDisplayCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxDisplayCountProperty =
            DependencyProperty.Register(nameof(MaxDisplayCount), typeof(int), typeof(DateValuDrawMap), new PropertyMetadata(0));




        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMember.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberProperty =
            DependencyProperty.Register(nameof(DisplayMember), typeof(string), typeof(DateValuDrawMap), new PropertyMetadata("Text"));




        public record DataValDisplayItem(DateTime Date, double Value);
        private List<DataValDisplayItem> Items = new();

        static DateValuDrawMap()
        {

        }
        public DateValuDrawMap()
        {
            ItemsSourceProperty.OverrideMetadata(typeof(DateValuDrawMap), new FrameworkPropertyMetadata((d, e) =>
            {
                if(e.NewValue is INotifyCollectionChanged nv)
                {
                    nv.CollectionChanged += Nv_CollectionChanged;
                }
            }));
        }
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
        }
        private  void Nv_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Items.Clear();
            foreach (DataValDisplayItem i in e.NewItems)
            {
                Items.Add(i);
            }
            InvalidateVisual();
        }

        public void AppendItem(DateTime Date, double Value)
        {
            while (Items.Count > MaxDisplayCount)
            {
                Items.RemoveAt(0);
            }
            var itme = new DataValDisplayItem(Date, Value);
            Items.Add(itme);
        }

        private Point GetValuePoint(DataValDisplayItem item, int index)
        {
            var Val = item.Value;
            var Dat = item.Date;
            var max = MapMax();
            var min = MapMin();
            Rect rect = new Rect(RenderSize);

            var Ypersentage = (Val - min) / (max - min);
            var Xpersentage = (index / Items.Count);
            return new(rect.Left + rect.Width * Xpersentage, rect.Top + rect.Height * Ypersentage);
        }
        private double MapMax()
        {
            return Items.Max(x => x.Value);
        }
        private double MapMin()
        {
            return Items.Min(x => x.Value);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen pen = new(Brushes.DimGray, 1);
            
            var index = 0;
            Point? lastPoint = null;
            foreach (DataValDisplayItem i in Items)
            {
                var point = GetValuePoint(i, index++);
                drawingContext.DrawLine(pen, lastPoint ?? point, point);
                lastPoint = point;
            }

            base.OnRender(drawingContext);
        }
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {

            base.OnItemsChanged(e);
        }
    }
}
