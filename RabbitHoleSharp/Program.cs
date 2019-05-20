using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using WinDivert;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using NSec.Cryptography;

namespace RabbitHoleSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var rb = new RabbitHole.RabbitHoleSrv();

            string hostName = Dns.GetHostName();
            var addressList = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in addressList)
            {
                if (rb.AddSrcAddress(ip))
                {
                    Console.WriteLine("Listen on: {0}", ip.ToString());
                };
            }

            while (true)
            {
                Console.WriteLine("DstIP:");
                var input = Console.ReadLine();
                if (input == "") break;
                try
                {
                    var ip = IPAddress.Parse(input);
                    if (rb.AddDstAddress(ip))
                    {
                        Console.WriteLine("Send to: {0}", ip.ToString());
                    }
                }
                catch
                {
                    break;
                }
            }
            rb.SetKey("bilibilibilibiniconiconiconi");
            //Console.WriteLine("Press any key to exit.");
            rb.Start();
            while (true)
            {
                Console.WriteLine("Msg:");
                var input = Console.ReadLine();
                if (input == "") break;
                rb.TXData.Add(input);
            }
            rb.Close();
        }
    }
}
