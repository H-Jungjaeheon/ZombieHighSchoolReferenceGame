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

    public static void PlayerDisconnected(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();

        Destroy(NetGameManager.Players[_Id].gameObject);
        NetGameManager.Players.Remove(_Id);
    }

    public static void PlayerHealth(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();
        float _Health = _Packet.ReadFloat();

        NetGameManager.Players[_Id].SetHealth( _Health );
    }

    public static void PlayerRespawned(Packet _Packet)
    {
        int _Id = _Packet.ReadInt();

        NetGameManager.Players[_Id].Respawn();
    }

    public static void CreateItemSpawner(Packet _Packet)
    {
        int _SpawnerId = _Packet.ReadInt();
        Vector3 _SpawnerPosition = _Packet.ReadVector3();
        bool _HasItem = _Packet.ReadBool();

        NetGameManager.Instance.CreateItemSpawner(_SpawnerId, _SpawnerPosition, _HasItem);
    }

    public static void ItemSpawnd(Packet _Packet)
    {
        int _SpawnerId = _Packet.ReadInt();

        NetGameManager.ItemSpawners[_SpawnerId].ItemSpawned();
    }

    public static void itemPickedUp(Packet _Packet)
    {
        int _SpawnerId = _Packet.ReadInt();
        int _ByPlayer = _Packet.ReadInt();

        NetGameManager.ItemSpawners[_SpawnerId].ItemPickedUp();
        NetGameManager.Players[_ByPlayer].ItemCount++;
    }

    public static void SpawnProjectile(Packet _Packet)
    {
        Debug.Log("SpawnProjectile");

        int _ProjectileId = _Packet.ReadInt();
        Vector3 _Position = _Packet.ReadVector3();
        int _ThrowByPlayer = _Packet.ReadInt();

        NetGameManager.Instance.SpawnProjectile(_ProjectileId, _Position);
        NetGameManager.Players[_ThrowByPlayer].ItemCount--;
    }

    public static void ProjectilePosition(Packet _Packet)
    {
        int _ProjectileId = _Packet.ReadInt();
        Vector3 _Position = _Packet.ReadVector3();

        NetGameManager.Projectiles[_ProjectileId].transform.position = _Position;
    }

    public static void ProjectileExploded(Packet _Packet)
    {
        int _ProjectileId = _Packet.ReadInt();
        Vector3 _Position = _Packet.ReadVector3();

        NetGameManager.Projectiles[_ProjectileId].Explode(_Position);
    }
}
