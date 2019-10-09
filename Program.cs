using System;
using System.Windows.Forms;

namespace Minesweeper
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GameSystem.Initialize();
            Application.Run(GameSystem.Data.Window);
        }
    }
}
