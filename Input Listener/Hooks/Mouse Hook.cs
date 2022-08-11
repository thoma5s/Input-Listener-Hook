using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace Listener {
    public class MouseHook {
        private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private static MouseHookHandler hookHandler;

        public delegate void MouseHookCallback(Keys key);

        public static event MouseHookCallback ButtonDown;
        public static event MouseHookCallback ButtonUp;

        private static IntPtr hookID = IntPtr.Zero;

        public static void Install(bool ignoreClicks = false) {
            MouseHook.ignoreClicks = ignoreClicks;
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        public void Uninstall() {
            if (hookID == IntPtr.Zero)
                return;

            WinAPI.UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }

        ~MouseHook() => Uninstall();

        private static IntPtr SetHook(MouseHookHandler proc) {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                return SetWindowsHookEx(WH_MOUSE_LL, proc, WinAPI.GetModuleHandle(module.ModuleName), 0);
        }

        private static bool ignoreClicks = false;

        private static IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                if (!ignoreClicks) {
                    if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                        if (ButtonDown != null)
                            ButtonDown(Keys.LButton);

                    if (MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                        if (ButtonUp != null)
                            ButtonUp(Keys.LButton);

                    if (MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
                        if (ButtonDown != null)
                            ButtonDown(Keys.RButton);

                    if (MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
                        if (ButtonUp != null)
                            ButtonUp(Keys.RButton);
                }

                if (MouseMessages.WM_MBUTTONDOWN == (MouseMessages)wParam)
                    if (ButtonDown != null)
                        ButtonDown(Keys.MButton);

                if (MouseMessages.WM_MBUTTONUP == (MouseMessages)wParam)
                    if (ButtonUp != null)
                        ButtonUp(Keys.MButton);

                if (MouseMessages.WM_XBUTTONDOWN == (MouseMessages)wParam)
                    if (ButtonDown != null)
                        if ((XButtons)GetUnsignedHWORD(((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT))).mouseData) == XButtons.WM_XBUTTON1)
                            ButtonDown(Keys.XButton1);
                        else 
                            ButtonDown(Keys.XButton2);

                if (MouseMessages.WM_XBUTTONUP == (MouseMessages)wParam)
                    if (ButtonUp != null)
                        if ((XButtons)GetUnsignedHWORD(((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT))).mouseData) == XButtons.WM_XBUTTON1)
                            ButtonUp(Keys.XButton1);
                        else
                            ButtonUp(Keys.XButton2);
            }
            return WinAPI.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C
        }

        private enum XButtons {
            WM_XBUTTON1 = 0x0001,
            WM_XBUTTON2 = 0x0002
        }

        private struct POINT {
            public int x;
            public int y;
        }

        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static ushort GetUnsignedHWORD(uint _uint) =>
            (ushort)((_uint >> 16) & 0xFFFF);

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            MouseHookHandler lpfn, IntPtr hMod, uint dwThreadId);
    }
}
