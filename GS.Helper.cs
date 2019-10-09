using System;
using System.Runtime.InteropServices;

namespace Minesweeper
{
    static partial class GameSystem
    {
        public static class Helper
        {
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        }
    }
}
