using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitHoleMado
{
    public partial class ConfigWizard : Form
    {
        public ConfigWizard()
        {
            InitializeComponent();
        }

        private void ConfigWizard_Load(object sender, EventArgs e)
        {
            AddListenIpToList();
        }

        private void AddListenIpToList ()
        {
            string hostName = Dns.GetHostName();
            var addressList = Dns.GetHostAddresses(hostName);

            IPNetwork localnet192 = IPNetwork.Parse("192.168.0.0/16");
            IPNetwork localnet172 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork localnet169 = IPNetwork.Parse("169.254.0.0/16");
            IPNetwork localnet10 = IPNetwork.Parse("10.0.0.0/8");


            foreach (IPAddress ip in addressList.Distinct().ToList())
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo) continue;
                if (localnet192.Contains(ip) || localnet172.Contains(ip) || localnet169.Contains(ip) || localnet10.Contains(ip)) continue;
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    listListenIP.Items.Add(ip);
                }
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    listListenIP.Items.Add(ip);
                }
            }
        }

        private void ListListenIP_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int current;
            if (e.NewValue == CheckState.Checked)
            {
                current = +1;
            }
            else
            {
                current = -1;
            }

            if ((current + listListenIP.CheckedItems.Count) > 0)
            {
                btnListenNext.Enabled = true;
            } else
            {
                btnListenNext.Enabled = false;
            }
        }

        private void BtnAddSendIP_Click(object sender, EventArgs e)
        {
            try
            {
                var ip = IPAddress.Parse(inputSendIp.Text);
            }
            catch
            {
                return;
            }
            listSendIP.Items.Add(inputSendIp.Text);
        }

        private void ListSendIP_MouseUp(object sender, MouseEventArgs e)
        {
            if (listSendIP.SelectedItems.Count != 0)
            {
                btnRemoveSelectedSendIP.Enabled = true;
            } else
            {
                btnRemoveSelectedSendIP.Enabled = false;
            }
        }

        private void BtnListenNext_Click(object sender, EventArgs e)
        {
            SettingsTab.SelectedIndex = 1;
        }

        private void BtnSendNext_Click(object sender, EventArgs e)
        {
            if (listSendIP.Items.Count <= 0)
            {
                MessageBox.Show("至少添加一个目标IP");
                return;
            }
            SettingsTab.SelectedIndex = 2;
        }

        private void BtnStartListen_Click(object sender, EventArgs e)
        {
            if (inputEncryptPassword.Text.Trim() == "")
            {
                MessageBox.Show("加解密密码不能为空");
                return;
            }
            foreach (IPAddress item in listListenIP.CheckedItems)
            {
                Program.rb.AddSrcAddress(item);
            }
            foreach (ListViewItem item in listSendIP.Items)
            {
                Program.rb.AddDstAddress(IPAddress.Parse(item.Text));
            }
            Program.rb.SetKey(inputEncryptPassword.Text);
            Program.rb.Start();
            MainWindow MWindow = new MainWindow();
            MWindow.Show();
            this.Hide();
        }
    }
}
