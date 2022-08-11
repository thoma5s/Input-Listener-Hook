using System;
using System.Windows.Forms;

namespace Listener {
    class Program {
        static void Main() {
            Install(true);
            Application.Run();
        }

        static void Install(bool ignoreClicks = false) {
            KeyboardHook.KeyDown += Action;
            MouseHook.ButtonDown += Action;
            KeyboardHook.Install();
            MouseHook.Install(ignoreClicks);
        }

        static void Action(Keys key) => Console.WriteLine($"{key} pressed.");
    }
}
