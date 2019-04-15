using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDivert;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace RabbitHoleMado
{
    class RabbitHoleService
    {
        class DivertPacket
        {
            public byte[] Data;
            public WINDIVERT_ADDRESS Addr;
        }

        public class ProtocolPacket
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

        BlockingCollection<DivertPacket> txBuffer = new BlockingCollection<DivertPacket>();
        BlockingCollection<DivertPacket> rxBuffer = new BlockingCollection<DivertPacket>();
        BlockingCollection<ProtocolPacket> processBuffer = new BlockingCollection<ProtocolPacket>();

        List<IPAddress> IPv4SrcList = new List<IPAddress>();
        List<IPAddress> IPv6SrcList = new List<IPAddress>();
        public void AddSrcIP(IPAddress IP)
        {
            if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPv4SrcList.Add(IP);
            }
            if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                IPv6SrcList.Add(IP);
            }
        }
        List<IPAddress> IPv4DstList = new List<IPAddress>();
        List<IPAddress> IPv6DstList = new List<IPAddress>();
        public void AddDstIP(IPAddress IP)
        {
            if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPv4DstList.Add(IP);
            }
            if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                IPv6DstList.Add(IP);
            }
        }

        IntPtr handle = WinDivert.WinDivertMethods.WinDivertOpen("true", WINDIVERT_LAYER.WINDIVERT_LAYER_NETWORK, 0, 0);
        bool Runflag = true;
        Thread processThread;
        void ProcessLoop()
        {
            var FakeMACAddr = System.Net.NetworkInformation.PhysicalAddress.Parse("90-90-90-90-90-90");
            var Fakeethernetv4Packet = new PacketDotNet.EthernetPacket(FakeMACAddr, FakeMACAddr, PacketDotNet.EthernetPacketType.IPv4);
            var Fakeethernetv6Packet = new PacketDotNet.EthernetPacket(FakeMACAddr, FakeMACAddr, PacketDotNet.EthernetPacketType.IPv6);

            while (Runflag)
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
                    byte[] decryptedData = Sodium.StreamEncryption.DecryptChaCha20(ParsedPacket.PayloadData, nonce, key);
                    ProtocolPacket RXProtocolPacket = JsonConvert.DeserializeObject<ProtocolPacket>(System.Text.Encoding.Default.GetString(decryptedData));
                    if (RXProtocolPacket.PacketMD5Sum == "") continue;
                    processBuffer.Add(RXProtocolPacket);
                }
                catch
                {
                    txBuffer.Add(packet);
                }
            }
        }
        Thread txThread;
        void TXLoop()
        {
            uint slen = 0;
            while (Runflag)
            {
                var txPacket = txBuffer.Take();
                if (!WinDivertMethods.WinDivertSend(handle, txPacket.Data, (uint)txPacket.Data.Length, ref txPacket.Addr, ref slen))
                {
                    continue;
                }
            }
        }
        Thread rxThread;
        void RXLoop()
        {
            while (Runflag)
            {
                uint rxLen = 0;
                var rxPacket = new DivertPacket { Addr = new WINDIVERT_ADDRESS { Direction = 0 }, Data = new byte[65535] };
                WinDivertMethods.WinDivertRecv(handle, rxPacket.Data, (uint)rxPacket.Data.Length, ref rxPacket.Addr, ref rxLen);
                rxPacket.Data = rxPacket.Data.Take((int)rxLen).ToArray();
                rxBuffer.Add(rxPacket);
            }
        }

        Random rand = new Random();
        MD5 md5Ctx = new MD5CryptoServiceProvider();
        public byte[] key = System.Text.Encoding.Default.GetBytes("NetworkPasswordNetworkPassword12");//Must 32 bytes
        public void SetKey(string password)
        {
            key = md5Ctx.ComputeHash(System.Text.Encoding.Default.GetBytes(password));
        }

        public void AddMsg(string MessageBody)
        {
            if (MessageBody == "")
            {
                return;
            }
            var Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var MD5Sum = BitConverter.ToString(md5Ctx.ComputeHash(System.Text.Encoding.Default.GetBytes(MessageBody + rand.Next(0, 65536).ToString()))).Replace("-", "");
            Stack<string> PiecedMsg = new Stack<string>();
            while (MessageBody.Length > 0)
            {
                if (MessageBody.Length <= 5)
                {
                    PiecedMsg.Push(MessageBody);
                    MessageBody = "";
                }
                else
                {
                    var cutlen = rand.Next(0, MessageBody.Length);
                    PiecedMsg.Push(MessageBody.Substring(0, cutlen));
                    MessageBody = MessageBody.Substring(cutlen);
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
                var TXProtocolPacket = new ProtocolPacket { PacketTimestamp = Timestamp, PacketTotal = PacketTotal, PacketMD5Sum = MD5Sum, PacketCount = PiecedMsg.Count, SrcIP = SrcIP.ToString(), DstIP = DstIP.ToString() };
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

        public void StartSrv()
        {
            rxThread = new Thread(new ThreadStart(RXLoop));
            processThread = new Thread(new ThreadStart(ProcessLoop));
            txThread = new Thread(new ThreadStart(TXLoop));

            processThread.Start();
            rxThread.Start();
            txThread.Start();
        }
        public void StopSrv()
        {
            Runflag = false;
            processThread.Abort();
            txThread.Abort();
            rxThread.Abort();
        }
    }
}
