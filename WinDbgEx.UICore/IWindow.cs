using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.UICore {
	public interface IWindow {
		bool Topmost { get; set; }

		T FindResource<T>(object key) where T : class;
	}
}
