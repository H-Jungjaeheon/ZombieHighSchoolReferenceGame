using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject StartMenu;
    public InputField UserName;

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

    public void ConnectToServer()
    {
        StartMenu.SetActive(false);
        UserName.interactable = false;

        Client.Instance.ConnectToServer();
    }
}
