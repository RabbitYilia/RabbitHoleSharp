using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WinDivert;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;

namespace RabbitHoleSharp
{
    class DivertPacket
    {
        public byte[] Data;
        public WINDIVERT_ADDRESS Addr;
    }

    class ProtocolPacket
    {
        public long PacketTimestamp;
        public string SrcIP;
        public string DstIP;
        public string PacketMD5Sum;
        public string[] PacketData;
        public int PacketCount;
        public int PacketTotal;
        public string PiecedMsg;
    }

    class RabbitHole
    {
        BlockingCollection<DivertPacket> txBuffer = new BlockingCollection<DivertPacket>();
        BlockingCollection<DivertPacket> rxBuffer = new BlockingCollection<DivertPacket>();

        List<IPAddress> IPv4SrcList = new List<IPAddress>();
        List<IPAddress> IPv6SrcList = new List<IPAddress>();
        List<IPAddress> IPv4DstList = new List<IPAddress>();
        List<IPAddress> IPv6DstList = new List<IPAddress>();

        Dictionary<string, ProtocolPacket> ProtocolPacketPool=new Dictionary<string, ProtocolPacket>();

        IntPtr handle = WinDivert.WinDivertMethods.WinDivertOpen("true", WINDIVERT_LAYER.WINDIVERT_LAYER_NETWORK, 0, 0);
        Random rand = new Random();
        MD5 md5Ctx = new MD5CryptoServiceProvider();
        byte[] key = System.Text.Encoding.Default.GetBytes("NetworkPasswordNetworkPassword12"); //Must 32 bytes
       

        Thread processThread;
        Thread txThread;
        Thread rxThread;

        public RabbitHole()
        {
            rxThread = new Thread(new ThreadStart(RXLoop));
            processThread = new Thread(new ThreadStart(ProcessLoop));
            txThread = new Thread(new ThreadStart(TXLoop));
            try
            {
                GetSrcIP();
                GetDstIP();
                VaildNetworkAccess();
            }
            catch
            {
                return;
            }

            processThread.Start();
            rxThread.Start();
            txThread.Start();


            Send();

            processThread.Abort();
            txThread.Abort();
            rxThread.Abort();
        }

        void Send()
        {
            while (true)
            {
                Console.WriteLine("Msg:");
                var input = Console.ReadLine();
                if (input == "")
                {
                    break;
                }
                var Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                var MD5Sum = BitConverter.ToString(md5Ctx.ComputeHash(System.Text.Encoding.Default.GetBytes(input+rand.Next(0,65536).ToString()))).Replace("-", "");

                Stack<string> PiecedMsg = new Stack<string>();
                while (input.Length > 0)
                {
                    if (input.Length <= 5)
                    {
                        PiecedMsg.Push(input);
                        input = "";
                    }
                    else
                    {
                        var cutlen = rand.Next(0, input.Length);
                        PiecedMsg.Push(input.Substring(0, cutlen));
                        input = input.Substring(cutlen);
                    }
                }
                var PacketTotal = PiecedMsg.Count;

                while (PiecedMsg.Count > 0)
                {
                    IPAddress SrcIP, DstIP;
                    switch (rand.Next(0, 2))
                    {
                        case 0:
                            if (IPv6SrcList.Count != 0 && IPv6DstList.Count != 0)
                            {
                                DstIP = IPv6DstList[rand.Next(0, IPv6DstList.Count)];
                                SrcIP = IPv6SrcList[rand.Next(0, IPv6SrcList.Count)];
                            }
                            else
                            {
                                DstIP = IPv4DstList[rand.Next(0, IPv4DstList.Count)];
                                SrcIP = IPv4SrcList[rand.Next(0, IPv4SrcList.Count)];
                            }
                            break;
                        default:
                            if (IPv4SrcList.Count != 0 && IPv4DstList.Count != 0)
                            {
                                DstIP = IPv4DstList[rand.Next(0, IPv4DstList.Count)];
                                SrcIP = IPv4SrcList[rand.Next(0, IPv4SrcList.Count)];
                            }
                            else
                            {
                                DstIP = IPv6DstList[rand.Next(0, IPv6DstList.Count)];
                                SrcIP = IPv6SrcList[rand.Next(0, IPv6SrcList.Count)];
                            }
                            break;
                    }
                    var TXProtocolPacket = new ProtocolPacket { PacketTimestamp = Timestamp, PacketTotal = PacketTotal, PacketMD5Sum = MD5Sum, PacketCount = PiecedMsg.Count,SrcIP=SrcIP.ToString(),DstIP=DstIP.ToString() };
                    TXProtocolPacket.PiecedMsg = PiecedMsg.Pop();
                    var udpPacket = new PacketDotNet.UdpPacket((ushort)rand.Next(1, 65536), (ushort)rand.Next(1, 65536));
                    long nonceTime = (DateTimeOffset.Now.ToUnixTimeSeconds() / 300) * 300;
                    byte[] nonce = md5Ctx.ComputeHash(System.Text.Encoding.Default.GetBytes(nonceTime.ToString())).Take(8).ToArray();                    
                    udpPacket.PayloadData = Sodium.StreamEncryption.EncryptChaCha20(JsonConvert.SerializeObject(TXProtocolPacket), nonce, key); ;
                    if (DstIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var ipv4Packet = new PacketDotNet.IPv4Packet(SrcIP, DstIP);
                        ipv4Packet.PayloadPacket = udpPacket;
                        udpPacket.UpdateCalculatedValues();
                        udpPacket.UpdateUDPChecksum();
                        ipv4Packet.UpdateCalculatedValues();
                        txBuffer.Add(new DivertPacket { Addr = new WINDIVERT_ADDRESS { Direction = 0 }, Data = ipv4Packet.Bytes });
                    }
                    if (DstIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        var ipv6Packet = new PacketDotNet.IPv6Packet(SrcIP, DstIP);
                        ipv6Packet.PayloadPacket = udpPacket;
                        udpPacket.UpdateCalculatedValues();
                        udpPacket.UpdateUDPChecksum();
                        ipv6Packet.UpdateCalculatedValues();
                        txBuffer.Add(new DivertPacket { Addr = new WINDIVERT_ADDRESS { Direction = 0 }, Data = ipv6Packet.Bytes });
                    }
                }
            }
        }

        void ProcessLoop()
        {
            var FakeMACAddr = System.Net.NetworkInformation.PhysicalAddress.Parse("90-90-90-90-90-90");
            var Fakeethernetv4Packet = new PacketDotNet.EthernetPacket(FakeMACAddr, FakeMACAddr,PacketDotNet.EthernetPacketType.IPv4);
            var Fakeethernetv6Packet = new PacketDotNet.EthernetPacket(FakeMACAddr, FakeMACAddr,PacketDotNet.EthernetPacketType.IPv6);

            while (true)
            {
                var packet = rxBuffer.Take();
                int IPversion = (packet.Data[0]) >> 4;
                PacketDotNet.Packet ParsedPacket;
                PacketDotNet.IPProtocolType IPNextProtocol;
                switch (IPversion)
                {
                    case 4:
                        var FullBytev4 = Fakeethernetv4Packet.Bytes.ToList();
                        FullBytev4.AddRange(packet.Data);
                        ParsedPacket = PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Ethernet, FullBytev4.ToArray()).PayloadPacket;
                        var IPv4Header = (PacketDotNet.IPv4Packet)ParsedPacket;
                        IPNextProtocol = IPv4Header.NextHeader;
                        ParsedPacket = IPv4Header.PayloadPacket;
                        break;
                    case 6:
                        var FullBytev6 = Fakeethernetv6Packet.Bytes.ToList();
                        FullBytev6.AddRange(packet.Data);
                        ParsedPacket = PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Ethernet, FullBytev6.ToArray()).PayloadPacket;
                        var IPv6Header = (PacketDotNet.IPv6Packet)ParsedPacket;
                        IPNextProtocol = IPv6Header.NextHeader;
                        ParsedPacket = IPv6Header.PayloadPacket;
                        break;
                    default:
                        txBuffer.Add(packet);
                        continue;

                }

                switch (IPNextProtocol)
                {
                    case PacketDotNet.IPProtocolType.UDP:
                        break;
                    default:
                        txBuffer.Add(packet);
                        continue;
                }

                try
                {
                    long nonceTime = (DateTimeOffset.Now.ToUnixTimeSeconds() / 300) * 300;
                    byte[] nonce = md5Ctx.ComputeHash(System.Text.Encoding.Default.GetBytes(nonceTime.ToString())).Take(8).ToArray();
                    byte[] decryptedData =Sodium.StreamEncryption.DecryptChaCha20(ParsedPacket.PayloadData,nonce,key);
                    ProtocolPacket RXProtocolPacket = JsonConvert.DeserializeObject<ProtocolPacket>(System.Text.Encoding.Default.GetString(decryptedData));
                    if (RXProtocolPacket.PacketMD5Sum == "") continue;
                    ProcessProtocolPacket(RXProtocolPacket);
                }
                catch
                {
                    txBuffer.Add(packet);
                }
            }
        }

        void ProcessProtocolPacket(ProtocolPacket RXProtocolPacket)
        {
            Console.WriteLine("[{0}][{1}/{2}] From {3} to {4}", RXProtocolPacket.PacketMD5Sum, RXProtocolPacket.PacketCount, RXProtocolPacket.PacketTotal, RXProtocolPacket.SrcIP, RXProtocolPacket.DstIP);
            if (ProtocolPacketPool.ContainsKey(RXProtocolPacket.PacketMD5Sum))
            {
                var ExistPacket = ProtocolPacketPool[RXProtocolPacket.PacketMD5Sum];
                if (ExistPacket.PacketData[RXProtocolPacket.PacketCount] != RXProtocolPacket.PiecedMsg)
                {
                    ExistPacket.PacketData[RXProtocolPacket.PacketCount] = RXProtocolPacket.PiecedMsg;
                    ExistPacket.PacketCount += 1;
                }
                if (ExistPacket.PacketCount == ExistPacket.PacketTotal)
                {
                    var DataStr = "";
                    for (int i = 1; i <= ExistPacket.PacketTotal; i++)
                    {
                        DataStr += ExistPacket.PacketData[i];
                    }
                    Console.WriteLine("Full Msg[{0}] From {1} to {2}:\n{3}", ExistPacket.PacketMD5Sum, ExistPacket.SrcIP, ExistPacket.DstIP, DataStr);
                    ProtocolPacketPool.Remove(ExistPacket.PacketMD5Sum);
                }
            }
            else
            {
                RXProtocolPacket.PacketData = new string[RXProtocolPacket.PacketTotal+1];
                RXProtocolPacket.PacketData[RXProtocolPacket.PacketCount] = RXProtocolPacket.PiecedMsg;
                RXProtocolPacket.PacketCount = 1;
                ProtocolPacketPool.Add(RXProtocolPacket.PacketMD5Sum, RXProtocolPacket);
                if (RXProtocolPacket.PacketCount == RXProtocolPacket.PacketTotal)
                {
                    var DataStr = "";
                    for(int i = 1; i <= RXProtocolPacket.PacketTotal; i++)
                    {
                        DataStr += RXProtocolPacket.PacketData[i];
                        
                    }
                    Console.WriteLine("Full Msg[{0}] From {1} to {2}:\n{3}", RXProtocolPacket.PacketMD5Sum, RXProtocolPacket.SrcIP, RXProtocolPacket.DstIP, DataStr);
                    ProtocolPacketPool.Remove(RXProtocolPacket.PacketMD5Sum);
                }
            }
        }

        void RXLoop()
        {
            while (true)
            {
                uint rxLen = 0;
                var rxPacket = new DivertPacket { Addr = new WINDIVERT_ADDRESS { Direction = 0 }, Data = new byte[65535] };
                WinDivertMethods.WinDivertRecv(handle, rxPacket.Data, (uint)rxPacket.Data.Length, ref rxPacket.Addr, ref rxLen);
                rxPacket.Data = rxPacket.Data.Take((int)rxLen).ToArray();
                rxBuffer.Add(rxPacket);
            }
        }


        void TXLoop()
        {
            uint slen = 0;
            while (true)
            {
                var txPacket = txBuffer.Take();
                if (!WinDivertMethods.WinDivertSend(handle, txPacket.Data, (uint)txPacket.Data.Length, ref txPacket.Addr, ref slen))
                {
                    continue;
                    Console.WriteLine("Send Failed");
                    Console.WriteLine(BitConverter.ToString(txPacket.Data).Replace("-", ""));
                }
            }
        }
        void VaildNetworkAccess()
        {
            if (IPv4SrcList.Count == 0 && IPv4DstList.Count == 0)
            {
                if (IPv6SrcList.Count == 0 || IPv6DstList.Count == 0)
                {
                    Console.WriteLine("Network Unreachable");
                    throw new Exception("Network Unreachable");
                }
            }
            if (IPv6SrcList.Count == 0 && IPv6DstList.Count == 0)
            {
                if (IPv4SrcList.Count == 0 || IPv4DstList.Count == 0)
                {
                    Console.WriteLine("Network Unreachable");
                    throw new Exception("Network Unreachable");
                }
            }
        }
        void GetDstIP()
        {
            if (File.Exists(@"./dst.txt"))
            {
                StreamReader sr = new StreamReader(@"./dst.txt");
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var ip = IPAddress.Parse(line);
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        IPv4DstList.Add(ip);
                    }
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        IPv6DstList.Add(ip);
                    }
                    Console.WriteLine("Send to: {0}", ip.ToString());
                }
                sr.Close();
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("DstIP:");
                    var input = Console.ReadLine();
                    if (input == "") break;
                    try
                    {
                        var ip = IPAddress.Parse(input);
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IPv4DstList.Add(ip);
                        }
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            IPv6DstList.Add(ip);
                        }
                        Console.WriteLine("Send to: {0}", ip.ToString());
                    }
                    catch
                    {
                        break;
                    }
                }
                if (IPv4DstList.Count == 0 && IPv6DstList.Count == 0)
                {
                    Console.WriteLine("No Dst Addr.");
                    throw new Exception("No Dst Addr.");
                }
            }
        }
        void GetSrcIP()
        {
            string hostName = Dns.GetHostName();
            var addressList = Dns.GetHostAddresses(hostName);

            IPNetwork localnet192 = IPNetwork.Parse("192.168.0.0/16");
            IPNetwork localnet172 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork localnet169 = IPNetwork.Parse("169.254.0.0/16");
            IPNetwork localnet10 = IPNetwork.Parse("10.0.0.0/8");


            foreach (IPAddress ip in addressList)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo) continue;
                if (localnet192.Contains(ip) || localnet172.Contains(ip) || localnet169.Contains(ip) || localnet10.Contains(ip)) continue;
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPv4SrcList.Add(ip);
                }
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    IPv6SrcList.Add(ip);
                }
                Console.WriteLine("Listen on: {0}", ip.ToString());
            }
            if (IPv4SrcList.Count == 0 && IPv6SrcList.Count == 0)
            {
                Console.WriteLine("No Address to Listen.");
                throw new Exception("No Address to Listen.");
            }
        }

        ~RabbitHole()
        {
            WinDivertMethods.WinDivertClose(handle);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            new RabbitHole();

            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
        }
    }
}
