using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetGameManager : MonoBehaviour
{
    public static NetGameManager Instance;

    public static Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> ItemSpawners = new Dictionary<int, ItemSpawner>();

    public GameObject LocalPlayerPrefab;
    public GameObject PlayerPrefab;
    public GameObject ItemSpawnerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        else if (Instance != this)
        {
            Debug.Log("Instance Already Exists, Destroying Object");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _Id, string _UserName, Vector3 _Position, Quaternion _Rotation)
    {
        GameObject _Player;

        if(_Id == Client.Instance.MyId)
        {
            _Player = Instantiate(LocalPlayerPrefab, _Position, _Rotation);
        }

        else
        {
            _Player = Instantiate(PlayerPrefab, _Position, _Rotation);
        }

        _Player.GetComponent<PlayerManager>().Initialize(_Id, _UserName);
        Players.Add(_Id, _Player.GetComponent<PlayerManager>());
    }

    public void CreateItemSpawner(int _SpawnerId, Vector3 _Position, bool _HasItem)
    {
        GameObject _Spawner = Instantiate(ItemSpawnerPrefab, _Position, ItemSpawnerPrefab.transform.rotation);
        _Spawner.GetComponent<ItemSpawner>().Initialize(_SpawnerId, _HasItem);
        ItemSpawners.Add(_SpawnerId, _Spawner.GetComponent<ItemSpawner>());
    }
}
