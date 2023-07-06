using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _Packet)
    {
        string _Msg = _Packet.ReadString();
        int _MyId = _Packet.ReadInt();

        Debug.Log($"Message From Server: {_Msg}");
        Client.Instance.MyId = _MyId;
        ClientSend.WelcomeReceived();

        Client.Instance.MyUdp.Connect(((IPEndPoint)Client.Instance.MyTcp.Socket.Client.LocalEndPoint).Port);
    }

    public static void UdpTest(Packet _Packet)
    {
        string _Msg = _Packet.ReadString();

        Debug.Log($"Receive Packet Via Udp. Contains Message: {_Msg}");
        ClientSend.UdpTestReceived();
    }
}
