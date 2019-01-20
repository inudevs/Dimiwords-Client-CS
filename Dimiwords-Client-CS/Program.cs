using System;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Login login = new Login();
            login.ShowDialog();
            if (login.IsClose)
                Application.Run(new Main(login.user));
        }
    }
}
