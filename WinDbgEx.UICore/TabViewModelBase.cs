using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.UICore {
	public sealed class TabItemAttribute : Attribute {
		public string Title { get; }
		public TabItemAttribute(string title) {
			Title = title;
		}

		public string Icon { get; set; }
		public bool CanClose { get; set; } = true;
	}

	public abstract class TabViewModelBase : BindableBase {
		public virtual string Title { get; } = "Item";
		public virtual string Icon { get; } = null;
		public virtual bool CanClose { get; } = true;

		public TabViewModelBase() {
			var attr = GetType().GetCustomAttribute<TabItemAttribute>();
			if (attr != null) {
				Title = attr.Title;
				Icon = attr.Icon;
			}
		}
	}
}
