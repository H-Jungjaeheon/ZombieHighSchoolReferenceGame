using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class ServerSend
    {
        private static void SendTcpData(int _ToClient, Packet _Packet)
        {
            _Packet.WriteLength();
            Server.Clients[_ToClient].MyTcp.SendData(_Packet);
        }

        private static void SendTcpDataToAll(Packet _Packet)
        {
            _Packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].MyTcp.SendData(_Packet);
            }
        }

        private static void SendTcpDataToAll(int _ExceptClient, Packet _Packet)
        {
            _Packet.WriteLength();
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(i != _ExceptClient)
                {
                    Server.Clients[i].MyTcp.SendData(_Packet);
                }
            }
        }

        public static void Welcome(int _ToClient, string _Msg)
        {
            using(Packet _Packet = new Packet((int)ServerPackets.welcome))
            {
                _Packet.Write(_Msg);
                _Packet.Write(_ToClient);

                SendTcpData(_ToClient, _Packet);
            }
        }
    }
}
