using Dragablz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDbgX.UICore;
using WinDbgX.ViewModels;
using WinDbgX.Windows;

namespace WinDbgX {
	class CustomTabClient : IInterTabClient {
		public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source) {
			var win = new MainWindow();
			var vm = new MainViewModel(false, new IWindowImpl(win));
			win.DataContext = vm;
			return new NewTabHost<Window>(win, win.MainView.TabablzControl);
		}

		public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window) {
			return TabEmptiedResponse.CloseWindowOrLayoutBranch;
		}
	}
}
