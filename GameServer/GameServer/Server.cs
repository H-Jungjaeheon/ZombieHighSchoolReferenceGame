using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        private static TcpListener TcpListener;

        public static void Start(int _MaxPlayers, int _Port)
        {
            MaxPlayers = _MaxPlayers;
            Port = _Port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            TcpListener = new TcpListener(IPAddress.Any, Port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on {Port}");
        }

        private static void TCPConnectCallback(IAsyncResult _Result) 
        {
            TcpClient _Client = TcpListener.EndAcceptTcpClient(_Result);
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming Connection from {_Client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].MyTcp.Socket == null)
                {
                    Clients[i].MyTcp.Connect(_Client);
                    return;
                }
            }

            Console.WriteLine($"{_Client.Client.RemoteEndPoint} Failed To Connect: Server Full");
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
        }
    }
}
