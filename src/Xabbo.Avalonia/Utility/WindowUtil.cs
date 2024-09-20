using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace Xabbo.Avalonia.Utility;

static class WindowUtility
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    [StructLayout(LayoutKind.Sequential)]
    struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    const uint FLASHW_ALL = 3;

    /// <summary>
    /// Flash continuously until the window comes to the foreground.
    /// </summary>
    const uint FLASHW_TIMERNOFG = 12;

    public static bool FlashWindow(IntPtr windowHandle)
    {
        FLASHWINFO info = new();

        info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
        info.hwnd = windowHandle;
        info.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
        info.uCount = uint.MaxValue;
        info.dwTimeout = 0;

        return FlashWindowEx(ref info);
    }

    public static void ActivateWindow(Window window)
    {
        window.Activate();
    }

    public static void Show(Window window)
    {
        ActivateWindow(window);

        if (!window.IsVisible)
            window.Show();

        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;
    }
}
