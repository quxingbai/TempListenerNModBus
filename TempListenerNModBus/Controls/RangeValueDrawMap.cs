using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TempListenerNModBus.Controls
{
    public class RangeValueDrawMap:CustomValueDrawMap
    {



        public double BigValue
        {
            get { return (double)GetValue(BigValueProperty); }
            set { SetValue(BigValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BigValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BigValueProperty =
            DependencyProperty.Register(nameof(BigValue), typeof(double), typeof(RangeValueDrawMap), new PropertyMetadata(100.0));




        public double SmallValue
        {
            get { return (double)GetValue(SmallValueProperty); }
            set { SetValue(SmallValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallValueProperty =
            DependencyProperty.Register(nameof(SmallValue), typeof(double), typeof(RangeValueDrawMap), new PropertyMetadata(0.0));


        private Pen RedPen = new(Brushes.Red, 1);
        private Pen BluePen = new(Brushes.DodgerBlue, 1);

        protected override Pen GetDrawPenFromItem(MemberItem Item)
        {
            if (Item.SourceValue > BigValue) return RedPen;
            if(Item.SourceValue < SmallValue) return BluePen;
            return base.GetDrawPenFromItem(Item);
        }


    }
}
