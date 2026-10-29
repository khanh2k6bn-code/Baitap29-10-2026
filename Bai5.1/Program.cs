using System;
using System.Windows.Forms;

namespace CafePOSSystem
{
    static class Program
    {
        /// <summary>
        /// Điểm entry point chính cho ứng dụng Cafe POS.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
