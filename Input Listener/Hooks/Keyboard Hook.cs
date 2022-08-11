using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Listener {
    public class KeyboardHook {
        public delegate IntPtr KeyboardHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private static KeyboardHookHandler hookHandler;

        public delegate void KeyboardHookCallback(Keys key);

        public static event KeyboardHookCallback KeyDown;
        public static event KeyboardHookCallback KeyUp;

        private static IntPtr hookID = IntPtr.Zero;

        public static void Install() {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        public void Uninstall() => WinAPI.UnhookWindowsHookEx(hookID);

        private static IntPtr SetHook(KeyboardHookHandler proc) {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                return SetWindowsHookEx(13, proc, WinAPI.GetModuleHandle(module.ModuleName), 0);
        }

        private static IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                int iwParam = wParam.ToInt32();

                if ((iwParam == WM_KEYDOWN || iwParam == WM_SYSKEYDOWN))
                    if (KeyDown != null)
                        KeyDown((Keys)Marshal.ReadInt32(lParam));

                if ((iwParam == WM_KEYUP || iwParam == WM_SYSKEYUP))
                    if (KeyUp != null)
                        KeyUp((Keys)Marshal.ReadInt32(lParam));
            }
            return WinAPI.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        ~KeyboardHook() => Uninstall();

        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;


        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookHandler lpfn, IntPtr hMod, uint dwThreadId);
    }
}
