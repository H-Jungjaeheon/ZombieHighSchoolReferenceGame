using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet _Packet)
    {
        _Packet.WriteLength();
        Client.Instance.MyTcp.SendData(_Packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Instance.MyUdp.SendData(_packet);
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

    public static void PlayerMovement(bool[] _Inputs)
    {
        using (Packet _Packet = new Packet((int)ClientPackets.playerMovement))
        {
            _Packet.Write(_Inputs.Length);
            foreach (bool _Input in _Inputs)
            {
                _Packet.Write(_Input);
            }
            _Packet.Write(NetGameManager.Players[Client.Instance.MyId].transform.rotation);

            SendUDPData(_Packet);
        }
    }

    public static void PlayerShoot(Vector3 _Facing)
    {
        using (Packet _Packet = new Packet((int)ClientPackets.playerShoot))
        {
            _Packet.Write(_Facing);

            SendTcpData(_Packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _Facing)
    {
        using (Packet _Packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            _Packet.Write(_Facing);

            SendTcpData(_Packet);
        }
    }
    #endregion
}
