using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinDbgX.Controls;

namespace WinDbgX.Views {
	/// <summary>
	/// Interaction logic for SourceFileView.xaml
	/// </summary>
	public partial class SourceFileView : UserControl {
		public SourceFileView() {
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			_editor.TextArea.LeftMargins.Add(new BreakpointMargin());
		}
	}
}
