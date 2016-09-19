using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using NimUtility.Json;

namespace NIMDemo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new LoginForm());
            //Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (e.Exception == null)
                return;
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            //NIM.ClientAPI.Logout(NIM.NIMLogoutType.kNIMLogoutAppExit, HandleLogoutResult);
            //_semaphore.WaitOne();
            //DemoTrace.Close();
        }
    }
}
