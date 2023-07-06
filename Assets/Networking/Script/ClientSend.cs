using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet _Packet)
    {
        _Packet.WriteLength();
        Client.Instance.MyTcp.SendData(_Packet);
    }

    private static void SendUdpData(Packet _Packet)
    {
        _Packet.WriteLength();
        Client.Instance.MyUdp.SendData(_Packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using(Packet _Packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _Packet.Write(Client.Instance.MyId);
            _Packet.Write(UIManager.Instance.UserName.text);

            SendTcpData(_Packet);
        }
    }

    public static void UdpTestReceived()
    {
        using (Packet _Packet = new Packet((int)ClientPackets.udpTestReceived))
        {
            _Packet.Write("Received a Udp Packet");

            SendTcpData(_Packet);
        }
    }
    #endregion
}
