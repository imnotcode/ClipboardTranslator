using System;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Xaml.Behaviors;

namespace ClipboardTranslator.Controls
{
	public class WindowFocusBehavior : Behavior<Window>
	{
		public static readonly DependencyProperty WindowProperty = DependencyProperty.Register(
			"Window",
			typeof(Window),
			typeof(WindowFocusBehavior),
			new PropertyMetadata(null));

		public Window Window
		{
			get
			{
				return (Window) GetValue(WindowProperty);
			}
			set
			{
				SetValue(WindowProperty, value);
			}
		}

		public static readonly DependencyProperty AllowFocusProperty = DependencyProperty.Register(
			"AllowFocus",
			typeof(bool),
			typeof(WindowFocusBehavior),
			new PropertyMetadata(false, OnAllowFocusChanged));

		public bool AllowFocus
		{
			get
			{
				return (bool) GetValue(AllowFocusProperty);
			}
			set
			{
				SetValue(AllowFocusProperty, value);
			}
		}

		private static void OnAllowFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = (d as WindowFocusBehavior).Window;
			if (window == null)
			{
				return;
			}

			if ((bool) e.NewValue)
			{
				var hwnd = new WindowInteropHelper(window).Handle;
				NativeMethods.SetWindowExTransparent(hwnd, true);
			}
			else
			{
				var hwnd = new WindowInteropHelper(window).Handle;
				NativeMethods.SetWindowExTransparent(hwnd, false);
			}
		}
	}
}
