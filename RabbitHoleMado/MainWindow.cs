using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitHoleMado
{
    public partial class MainWindow : Form
    {
        Thread flushThread;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void flush()
        {
            while (true)
            {
                var msg= Program.rb.RXData.Take();
                listBox1.Items.Add(msg);
            }
        }
        private void MainWindow_Load(object sender, EventArgs e)
        {
            flushThread = new Thread(new ThreadStart(flush));
            flushThread.Start();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                Program.rb.TXData.Add(textBox1.Text);
            }
        }
    }
}
