using System;
using System.Windows.Forms;

namespace HotelManagementSystem
{
    static class Program
    {
        /// <summary>
        /// Điểm entry point chính cho ứng dụng Quản lý Khách sạn.
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
