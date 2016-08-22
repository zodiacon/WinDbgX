using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using DebuggerEngine;
using Prism.Mvvm;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
    [Export]
    class MainViewModel : BindableBase, IDisposable {
        ObservableCollection<MenuItemViewModel> _menuItems = new ObservableCollection<MenuItemViewModel>();
        public readonly DebugClient Debugger;

        public static MainViewModel Instance { get; private set; }

        public MainViewModel() {
            Instance = this;
            Debugger = DebugClient.CreateAsync().Result;

            DebugContext = new DebugContext(Debugger);
        }

        public IList<MenuItemViewModel> MenuItems => _menuItems;

        public DebugContext DebugContext { get; }

        public void Dispose() {
            Debugger.Dispose();
        }
    }
}
