using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WinDbgEx.UICore.Controls {
    public partial class CommandResultHistory {
		Paragraph _paragraph = new Paragraph { 
			TextAlignment = System.Windows.TextAlignment.Left,
			IsHyphenationEnabled = false,
			KeepTogether = false
		};
		ScrollViewer _scrollViwer;

		public CommandResultHistory() {
			InitializeComponent();

			_document.Blocks.Add(_paragraph);

		}

		private void GetScroller() {
			DependencyObject element = _scroller;
			if(VisualTreeHelper.GetChildrenCount(element) == 0)
				return;

			element = VisualTreeHelper.GetChild(element, 0);
			var border = VisualTreeHelper.GetChild(element, 0) as Decorator;
			_scrollViwer = border.Child as ScrollViewer;
			Debug.Assert(_scrollViwer != null);
		}

		public IList<CommandHistoryItem> Items {
			get { return (IList<CommandHistoryItem>)GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		public static readonly DependencyProperty ItemsProperty =
			 DependencyProperty.Register(nameof(Items), typeof(IList<CommandHistoryItem>), typeof(CommandResultHistory), new PropertyMetadata(null, OnItemsChanged));

		private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((CommandResultHistory)d).OnItemsChanged(e);
		}

		public Brush DefaultForeground {
			get { return (Brush)GetValue(DefaultForegroundProperty); }
			set { SetValue(DefaultForegroundProperty, value); }
		}

		public static readonly DependencyProperty DefaultForegroundProperty =
			 DependencyProperty.Register(nameof(DefaultForeground), typeof(Brush), typeof(CommandResultHistory), new PropertyMetadata(Brushes.Black));


		void OnItemsChanged(DependencyPropertyChangedEventArgs e) {
			var oldItems = e.OldValue as IList<CommandHistoryItem>;
			INotifyCollectionChanged change;

			if(oldItems != null) {
				change = oldItems as INotifyCollectionChanged;
				if(change != null)
					change.CollectionChanged -= OnCollectionChanged;
			}

			_paragraph.Inlines.Clear();
			var items = (IList<CommandHistoryItem>)e.NewValue;
			if(items == null)
				return;

			AddItems(items as IList);

			change = items as INotifyCollectionChanged;
			if(change != null)
				change.CollectionChanged += OnCollectionChanged;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			switch(e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddItems(e.NewItems);
				break;

			case NotifyCollectionChangedAction.Reset:
				_paragraph.Inlines.Clear();
				break;

			}
		}

		void AddItems(IList items) {
			Debug.Assert(items != null);

			foreach(CommandHistoryItem item in items) {
				_paragraph.Inlines.Add(new Run {
					Foreground = item.Color != null ? new SolidColorBrush(Color.FromRgb(item.Color.R, item.Color.G, item.Color.B)) : DefaultForeground,
					Text = item.Text,
					FontWeight = item.Bold ? FontWeights.Bold : FontWeights.Normal
				});
			}

			if(_scrollViwer == null)
				GetScroller();

			if(_scrollViwer != null)
				_scrollViwer.ScrollToEnd();
		}

	}
}
