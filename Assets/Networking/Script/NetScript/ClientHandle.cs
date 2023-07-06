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

    public static void SpawnPlayer(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();
        string _UserName = _Packet.ReadString();
        Vector3 _Position = _Packet.ReadVector3();
        Quaternion _Rotation = _Packet.ReadQuaternion();

        NetGameManager.Instance.SpawnPlayer(_Id, _UserName, _Position, _Rotation);
    }

    public static void PlayerPosition(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();
        Vector3 _Position = _Packet.ReadVector3();

        NetGameManager.Players[_Id].transform.position = _Position;
    }

    public static void PlayerRotation(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();
        Quaternion _Rotation = _Packet.ReadQuaternion();

        NetGameManager.Players[_Id].transform.rotation = _Rotation;
    }
}
