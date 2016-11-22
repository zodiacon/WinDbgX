using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using WinDbgEx.ViewModels;
using Zodiacon.WPF;

namespace WinDbgEx {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var container = new CompositionContainer(
                new AggregateCatalog(
                    new AssemblyCatalog(typeof(IFileDialogService).Assembly),
                    new AssemblyCatalog(Assembly.GetExecutingAssembly())));

            var vm = _mainViewModel = container.GetExportedValue<MainViewModel>();
            vm.Init();
            var win = new MainWindow { DataContext = vm };
            win.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            _mainViewModel.Dispose();
            base.OnExit(e);
        }
    }
}
