using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDbgX.UICore {
    public sealed class MenuItemTemplateSelector : ItemContainerTemplateSelector {
        public object MenuItemTemplateKey { get; set; }
        public object SeparatorTemplateKey { get; set; }
        public override DataTemplate SelectTemplate(object item, ItemsControl parent) {
            if(item == null)
                return parent.FindResource(SeparatorTemplateKey) as DataTemplate;

            var menuItem = item as MenuItemViewModel;
            Debug.Assert(menuItem != null);

            if(menuItem.IsSeparator)
                return parent.FindResource(SeparatorTemplateKey) as DataTemplate;

            return parent.FindResource(MenuItemTemplateKey) as DataTemplate;
        }
    }
}
