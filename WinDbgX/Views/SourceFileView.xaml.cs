using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using WinDbgX.Models;

namespace WinDbgX.Views {
	/// <summary>
	/// Interaction logic for SourceFileView.xaml
	/// </summary>
	public partial class SourceFileView : UserControl {
		public SourceFileView() {
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var margin = new BreakpointMargin();
            _editor.TextArea.LeftMargins.Add(margin);
            var doc = _editor.TextArea.GetService(typeof(IDocument)) as IDocument;
            AppManager.Instance.Container.ComposeExportedValue(doc.FileName, margin);

        }
	}
}
