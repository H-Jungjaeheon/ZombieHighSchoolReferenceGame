using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _Packet)
    {
        string _Msg = _Packet.ReadString();
        int _MyId = _Packet.ReadInt();

        Debug.Log($"Message From Server: {_Msg}");
        Client.Instance.MyId = _MyId;

        //TODO: 수신 했다는 패킷 전송
    }
}
