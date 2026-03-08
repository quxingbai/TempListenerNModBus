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
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // 新增项
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var item = e.NewItems[i];
                        var itm = ObjectToMember(item);

                        // 注意：e.NewStartingIndex 是添加开始的索引
                        // 如果是连续添加多个，每个项应该依次插入
                        int insertIndex = e.NewStartingIndex + i;
                        Members.Insert(insertIndex, itm);
                        ObjectRelation.Add(item, itm);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    // 移除项
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var item = e.OldItems[i];

                        // 从ObjectRelation中查找对应的itm
                        if (ObjectRelation.TryGetValue(item, out var itm))
                        {
                            // 从Members中移除
                            Members.Remove(itm);
                            // 从关系字典中移除
                            ObjectRelation.Remove(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // 替换项（例如：collection[2] = new Item()）
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var oldItem = e.OldItems[i];
                        var newItem = e.NewItems[i];
                        int index = e.NewStartingIndex + i;

                        // 从关系字典中获取旧的itm
                        if (ObjectRelation.TryGetValue(oldItem, out var oldItm))
                        {
                            // 创建新的itm
                            var newItm = ObjectToMember(newItem);

                            // 替换Members中的项
                            Members[index] = newItm;

                            // 更新关系字典
                            ObjectRelation.Remove(oldItem);
                            ObjectRelation.Add(newItem, newItm);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    // 移动项（例如：collection.Move(oldIndex, newIndex)）
                    // 注意：Move可能涉及多个项的同时移动
                    if (e.OldItems.Count > 0)
                    {
                        // 方法1：先移除再插入（保持顺序）
                        var movedItems = new List<object>();
                        var movedItms = new List<object>();

                        // 记录要移动的项（按原顺序）
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var item = e.OldItems[i];
                            movedItems.Add(item);

                            if (ObjectRelation.TryGetValue(item, out var itm))
                            {
                                movedItms.Add(itm);
                            }
                        }

                        // 从原位置移除（从后往前移除，避免索引变化）
                        for (int i = e.OldItems.Count - 1; i >= 0; i--)
                        {
                            int removeIndex = e.OldStartingIndex + i;
                            Members.RemoveAt(removeIndex);
                        }

                        // 插入到新位置
                        for (int i = 0; i < movedItms.Count; i++)
                        {
                            int insertIndex = e.NewStartingIndex + i;
                            Members.Insert(insertIndex, ObjectToMember(movedItms[i]));
                        }

                        // 注意：ObjectRelation不需要更新，因为对象本身没变
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // 集合被清空或完全重置（例如：collection.Clear() 或 重新赋值）

                    // 方法1：清空所有
                    Members.Clear();
                    ObjectRelation.Clear();

                    // 方法2：如果Reset后集合还有内容，需要重新添加
                    if (sender is IEnumerable<object> collection)
                    {
                        int index = 0;
                        foreach (var item in collection)
                        {
                            var itm = ObjectToMember(item);
                            Members.Insert(index++, itm);
                            ObjectRelation.Add(item, itm);
                        }
                    }
                    break;
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
            var Xpersentage = (index * 1.0 / (Items.Count - 1));
            return new(rect.Left + rect.Width * Xpersentage, rect.Top + rect.Height * Ypersentage);
        }
        private double MapMax()
        {
            var max = Members.Count == 0 ? 0: Members.Max(w => w.SourceValue);
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
                var yp = i*1.0/xlinCount;
                var y = rect.Top+rect.Height*yp;
                var value = Math.Round((max - min) * (1 - yp),1);

                
                drawingContext.DrawLine(wsp, new(rect.Left, y), new(rect.Right, y));
                drawingContext.DrawText(CreatText(value.ToString(), Brushes.DodgerBlue), new(rect.Right,y));
            }
            foreach (MemberItem i in Members)
            {
                var pen = GetDrawPenFromItem(i);
                var point = GetValuePoint(i, index);
                drawingContext.DrawLine(wsp, new Point(point.X, rect.Top), new(point.X, rect.Bottom));
                drawingContext.DrawLine(pen, lastPoint ?? point, point);
                drawingContext.DrawText(CreatText(i.SourceValue.ToString(), Brushes.Gray), point);
                lastPoint = point;
                index++;
            }

            base.OnRender(drawingContext);
        }
        private FormattedText CreatText(string text, Brush brush)
        {
            FormattedText ft = new(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new(""), 10, brush);
            return ft;
        }
        private int GetXLineCount()
        {
            return 3;
        }
        private Rect GetDrawingMapRect()
        {
            Thickness tc = new(30,20,30,20);
            Rect rec = new(RenderSize);
            return new()
            {
                X = tc.Left,
                Y = tc.Top,
                Width = rec.Width - tc.Right-tc.Left,
                Height = rec.Height - tc.Bottom-tc.Top
            };
        }
    }
}
