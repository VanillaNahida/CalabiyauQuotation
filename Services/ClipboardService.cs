using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CalabiyauQuotation.Services
{
    public static class ClipboardService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        private const int VK_CONTROL = 0x11;
        private const int VK_V = 0x56;
        private const int VK_A = 0x41;
        private const int VK_RETURN = 0x0D;

        public static void CopyTextToClipboard(string text)
        {
            for (int i = 0; i < 10; i++)
            {
                if (TrySetClipboardText(text))
                    return;
                Thread.Sleep(100);
            }
        }

        private static bool TrySetClipboardText(string text)
        {
            IntPtr hMem = IntPtr.Zero;
            IntPtr hGlobal = IntPtr.Zero;

            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(text + '\0');
                hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
                if (hMem == IntPtr.Zero)
                    return false;

                hGlobal = GlobalLock(hMem);
                if (hGlobal == IntPtr.Zero)
                    return false;

                Marshal.Copy(bytes, 0, hGlobal, bytes.Length);
                GlobalUnlock(hMem);
                hGlobal = IntPtr.Zero;

                if (!OpenClipboard(IntPtr.Zero))
                    return false;

                EmptyClipboard();
                if (SetClipboardData(CF_UNICODETEXT, hMem) == IntPtr.Zero)
                {
                    CloseClipboard();
                    return false;
                }

                CloseClipboard();
                hMem = IntPtr.Zero;
                return true;
            }
            finally
            {
                if (hGlobal != IntPtr.Zero)
                    GlobalUnlock(hMem);
            }
        }

        public static void PasteText()
        {
            try
            {
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(VK_V, 0, 0, 0);
                keybd_event(VK_V, 0, 2, 0);
                keybd_event(VK_CONTROL, 0, 2, 0);
            }
            catch
            {
            }
        }

        public static void SelectAllAndPaste()
        {
            try
            {
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(VK_A, 0, 0, 0);
                keybd_event(VK_A, 0, 2, 0);
                keybd_event(VK_CONTROL, 0, 2, 0);
                
                Thread.Sleep(50);
                
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(VK_V, 0, 0, 0);
                keybd_event(VK_V, 0, 2, 0);
                keybd_event(VK_CONTROL, 0, 2, 0);
            }
            catch
            {
            }
        }

        public static void PressEnter()
        {
            try
            {
                Thread.Sleep(50);
                keybd_event(VK_RETURN, 0, 0, 0);
                keybd_event(VK_RETURN, 0, 2, 0);
            }
            catch
            {
            }
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
