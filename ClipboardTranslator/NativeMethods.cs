using System;
using System.Runtime.InteropServices;

namespace ClipboardTranslator
{
	public static class NativeMethods
	{
		const int WS_EX_TRANSPARENT = 0x00000020;
		const int GWL_EXSTYLE = (-20);

		[DllImport("user32.dll")]
		internal static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		internal static void SetWindowExTransparent(IntPtr hwnd, bool status)
		{
			if (status)
			{
				var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
				int result = SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
			}
			else
			{
				var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE) & (~WS_EX_TRANSPARENT);
				int result = SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
			}
		}
	}
}
