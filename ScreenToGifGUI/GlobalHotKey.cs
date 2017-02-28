using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenToGifGUI
{
    class GlobalHotKey
    {

        public delegate void HotkeyHandler();
        
        private static Dictionary<int, HotkeyHandler> _callbacks = new Dictionary<int, HotkeyHandler>();
        private static int _idCount = 0;
        
        public enum Modifier
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void Register(IntPtr hwnd, Keys key, HotkeyHandler callback)
        {
            bool b = RegisterHotKey(hwnd, ++_idCount, 0, (uint)key);
            _callbacks.Add(_idCount, callback);
        }

        public static void Register(IntPtr hwnd, Modifier modifier, Keys key, HotkeyHandler callback)
        {
            bool b = RegisterHotKey(hwnd, ++_idCount, (uint)modifier, (uint)key);
            _callbacks.Add(_idCount, callback);
        }

        public static void Register(IntPtr hwnd, Modifier modifier1, Modifier modifier2, Keys key, HotkeyHandler callback)
        {
            bool b = RegisterHotKey(hwnd, ++_idCount, (uint)modifier1 + (uint)modifier2, (uint)key);
            _callbacks.Add(_idCount, callback);
        }

        public static void Unregister(IntPtr hwnd, HotkeyHandler callback)
        {
            var item = _callbacks.FirstOrDefault(cb => cb.Value == callback);
            if (!item.Equals(default(KeyValuePair<int, HotkeyHandler>)))
            {
                UnregisterHotKey(hwnd, item.Key);
                _callbacks.Remove(item.Key);
            }
        }

        public static IntPtr CheckHotKeyMessage(int msg, IntPtr wideParam)
        {
            if (msg == 0x312 && _callbacks.ContainsKey(wideParam.ToInt32()))
            {
                _callbacks[wideParam.ToInt32()]();
            }
            return IntPtr.Zero;
        }
    }
}
