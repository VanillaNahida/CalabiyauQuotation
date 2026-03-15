using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CalabiyauQuotation.Services
{
    public class HotKeyManager : IDisposable
    {
        private IntPtr _windowHandle;
        private HwndSource? _source;
        private readonly Dictionary<int, Action> _hotkeyActions = new Dictionary<int, Action>();
        private int _nextId = 1;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int WM_HOTKEY = 0x0312;

        public HotKeyManager(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(id, out var action))
                {
                    action?.Invoke();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        public bool RegisterHotKey(string hotkeyString, Action action)
        {
            if (string.IsNullOrEmpty(hotkeyString))
                return false;

            try
            {
                var (modifiers, key) = ParseHotkey(hotkeyString);
                int id = _nextId++;
                if (RegisterHotKey(_windowHandle, id, modifiers | MOD_NOREPEAT, (uint)KeyInterop.VirtualKeyFromKey(key)))
                {
                    _hotkeyActions[id] = action;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private (uint modifiers, Key key) ParseHotkey(string hotkeyString)
        {
            uint modifiers = 0;
            Key key = Key.None;

            string[] parts = hotkeyString.Split('+');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_ALT;
                else if (trimmed.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || 
                         trimmed.Equals("Control", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_CONTROL;
                else if (trimmed.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_SHIFT;
                else if (trimmed.Equals("Win", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_WIN;
                else
                {
                    if (Enum.TryParse<Key>(trimmed, true, out var parsedKey))
                        key = parsedKey;
                }
            }

            return (modifiers, key);
        }

        public void UnregisterAll()
        {
            foreach (int id in _hotkeyActions.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _hotkeyActions.Clear();
        }

        public void Dispose()
        {
            UnregisterAll();
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                _source = null;
            }
        }
    }
}
