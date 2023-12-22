using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class UDPHelper
    {
        public UDPHelper()
        {

        }

        public void Send(string message)
        {
            UDPSender.Instance.Send(message);
        }

        public string Receive()
        {
            return UDPReceive.Instance.Receive();
        }
    }

    internal class UDPSender
    {
        private static UDPSender instance;
        private static readonly object lockObject = new object();
        public static UDPSender Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new UDPSender();
                        }
                    }
                }
                return instance;
            }
        }


        UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork);
        IPEndPoint broadcastIp = new IPEndPoint(IPAddress.Broadcast, 2000);

        public void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, broadcastIp);
        }
    }

    internal class UDPReceive
    {
        private static UDPReceive instance;
        private static readonly object lockObject = new object();
        public static UDPReceive Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new UDPReceive();
                        }
                    }
                }
                return instance;
            }
        }


        UdpClient receiveUdpClient = new UdpClient(2000);
        IPEndPoint localIP = new IPEndPoint(IPAddress.Any, 0);

        public string Receive()
        {
            byte[] r = receiveUdpClient.Receive(ref localIP);
            return Encoding.UTF8.GetString(r);
        }
    }
}
