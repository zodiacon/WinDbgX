using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using DebuggerEngine;
using Prism.Mvvm;
using WinDbgEx.UICore;
using Zodiacon.WPF;

namespace WinDbgEx.ViewModels {
    [Export]
    class MainViewModel : BindableBase, IDisposable {
        ObservableCollection<MenuItemViewModel> _menuItems = new ObservableCollection<MenuItemViewModel>();
        public DebugClient Debugger { get; private set; }

        public static MainViewModel Instance { get; private set; }

        [Import]
        public IFileDialogService FileDialogService { get; private set; }

        public MainViewModel() {
            Instance = this;
        }

        public void Init() {
            Debugger = DebugClient.CreateAsync().Result;
            DebugContext = new DebugContext(Debugger) {
                FileDialogService = FileDialogService
            };
        }

        public IList<MenuItemViewModel> MenuItems => _menuItems;

        public DebugContext DebugContext { get; private set; }

        public void Dispose() {
            Debugger.Dispose();
        }
    }
}
