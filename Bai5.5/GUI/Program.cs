using System;
using System.Windows.Forms;
using InventoryManagementSystem.GUI; // Gọi namespace chứa Form1 của tầng GUI

namespace InventoryManagementSystem
{
    static class Program
    {
        /// <summary>
        /// Điểm entry point chính cho ứng dụng.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Khởi chạy Form1 thuộc tầng GUI
            Application.Run(new Form1());
        }
    }
}
