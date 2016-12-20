using DebuggerEngine.Interop;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using WinDbgX.Controls;
using WinDbgX.Models;
using WinDbgX.UICore;

#pragma warning disable 649

namespace WinDbgX.ViewModels {
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    [TabItem("")]
    class SourceCodeViewModel : TabItemViewModelBase, IPartImportsSatisfiedNotification {
        [Import]
        DebugManager DebugManager;

        [Import]
        UIManager UIManager;

        [Import]
        AppManager AppManager;

        BreakpointMargin _margin;

        public void OnImportsSatisfied() {
            DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
        }

        private void Debugger_StatusChanged(object sender, DebuggerEngine.StatusChangedEventArgs e) {
            UIManager.Dispatcher.InvokeAsync(() => ToggleBreakpointCommand.RaiseCanExecuteChanged());
        }

        public void OpenFile(string filename) {
            _document = new TextDocument(FileReader.ReadFileContent(filename, Encoding.Unicode));
            _document.FileName = filename;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Icon));
            var ext = Path.GetExtension(filename);
            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        }

        private IHighlightingDefinition _syntaxHighlighting;

        public IHighlightingDefinition SyntaxHighlighting {
            get { return _syntaxHighlighting; }
            set { SetProperty(ref _syntaxHighlighting, value); }
        }

        public override string Title => Path.GetFileName(_document.FileName);
        public override string Icon => GetIconFromFileType();

        private string GetIconFromFileType() {
            switch (Path.GetExtension(_document.FileName).ToLower()) {
                case ".c":
                    return Icons.SourceFileC;

                case ".cpp":
                case ".cc":
                case ".cxx":
                case ".h":
                case ".hpp":
                case ".hxx":
                    return Icons.SourceFileCpp;

                default:
                    return Icons.GenericSourceFile;
            }
        }

        private TextDocument _document;

        public TextDocument Document {
            get { return _document; }
            set { SetProperty(ref _document, value); }
        }

        public DelegateCommandBase ToggleBreakpointCommand => new DelegateCommand<TextArea>(async textArea => {
            int line = textArea.Caret.Line;
            if (_margin == null)
                _margin = AppManager.Container.GetExportedValue<BreakpointMargin>(_document.FileName);

            try {
                var symbol = await DebugManager.Debugger.GetClosestSourceEntryByLineAsync(line, _document.FileName);
                var bp = DebugManager.Debugger.CreateBreakpoint(DEBUG_BREAKPOINT_TYPE.CODE);
                bp.SetOffset(symbol.Offset);
                bp.IsEnabled = true;
                _margin.Breakpoints.Add(new BreakpointViewModel(bp, DebugManager) {
                    Line = (int)symbol.EndLine
                });
            }
            catch (Exception ex) {
                UIManager.ReportError(ex);
            }
        }, _ => DebugManager.Status != DEBUG_STATUS.NO_DEBUGGEE);
    }
}
