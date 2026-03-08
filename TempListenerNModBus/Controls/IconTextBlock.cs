using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TempListenerNModBus.Controls
{
    public class IconTextBlock : Control
    {



        //public String Text
        //{
        //    get { return (String)GetValue(TextBlock.TextProperty); }
        //    set { SetValue(TextBlock.TextProperty, value); }
        //}




        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(IconTextBlock), new PropertyMetadata(null));





        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(object), typeof(IconTextBlock), new PropertyMetadata(null));





        static IconTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTextBlock), new FrameworkPropertyMetadata(typeof(IconTextBlock)));
        }

    }
}
