using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class ServerHandle
    {
        public static void WelcomeReceived(int _FromClient, Packet _Packet)
        {
            int _ClientIdCheck = _Packet.ReadInt();
            string _UserName = _Packet.ReadString();

            Console.WriteLine($"{Server.Clients[_FromClient].MyTcp.Socket.Client.RemoteEndPoint} " +
                $"Connected Successfully And Is Now Player {_FromClient} : {_UserName}.");

            if(_FromClient != _ClientIdCheck)
            {
                Console.WriteLine($"Player \"{_UserName}\" (ID: {_FromClient}) " +
                    $"Has Assumed The Wrong Client ID ({_ClientIdCheck})");
            }
            Server.Clients[_FromClient].SendIntoGame(_UserName);
        }
    }
}
