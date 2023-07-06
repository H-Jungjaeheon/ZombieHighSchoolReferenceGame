using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetGameManager : MonoBehaviour
{
    public static NetGameManager Instance;

    public static Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();

    public GameObject LocalPlayerPrefab;
    public GameObject PlayerPrefab;

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

        _Player.GetComponent<PlayerManager>().Id = _Id;
        _Player.GetComponent<PlayerManager>().UserName = _UserName;
        Players.Add(_Id, _Player.GetComponent<PlayerManager>());
    }
}
