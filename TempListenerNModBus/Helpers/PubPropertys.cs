using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace TempListenerNModBus.Helpers
{
    public class PubPropertys
    {

        public static readonly DependencyProperty WithObjectDataProperty =
            DependencyProperty.Register("WithObjectData", typeof(object), typeof(UIElement), new PropertyMetadata(null));
        public static object GetWithObjectData(UIElement Element) => Element.GetValue(WithObjectDataProperty);
        public static void SetWithObjectData(UIElement Element,object Data)=>Element.SetValue(WithObjectDataProperty, Data);



    }
}
