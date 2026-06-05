using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SRTPluginUIRE5DirectXOverlay
{
    public static class PInvoke
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public static IList<IntPtr> GetWindowHandles(Process proc, string className)
        {
            IList<IntPtr> windowHandles = new List<IntPtr>();

            StringBuilder classNameStringBuilder = new StringBuilder(256);
            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                classNameStringBuilder.Clear();
                if (
                GetWindowThreadProcessId(hWnd, out int processId) != 0
                && proc.Id == processId
                && GetClassName(hWnd, classNameStringBuilder, classNameStringBuilder.Capacity) != 0
                && string.Equals(classNameStringBuilder.ToString(), className, StringComparison.Ordinal)
                )
                    windowHandles.Add(hWnd);

                return true;
            }, IntPtr.Zero);

            return windowHandles;
        }

        public static IList<IntPtr> GetWindowHandles(Process proc)
        {
            IList<IntPtr> windowHandles = new List<IntPtr>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (
                GetWindowThreadProcessId(hWnd, out int processId) != 0
                && proc.Id == processId
                )
                    windowHandles.Add(hWnd);

                return true;
            }, IntPtr.Zero);

            return windowHandles;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }
}
