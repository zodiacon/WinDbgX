using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using WinDbgEx.ViewModels;

namespace WinDbgEx.Extensions {
    [MarkupExtensionReturnType(typeof(DebugContext))]
    sealed class CommandContextExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider sp) {
            return MainViewModel.Instance.DebugContext;
        }
    }
}
