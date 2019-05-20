using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitHoleMado
{
    static class Program
    {
        public static RabbitHole.RabbitHoleSrv rb=new RabbitHole.RabbitHoleSrv();
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConfigWizard());
        }
    }
}
