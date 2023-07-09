using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int Id;
    public string UserName;
    public int Type;

    public SpriteRenderer Renderer;

    public float Health;
    public float MaxHealth;

    public int ItemCount = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Initialize(int _Id, string _UserName)
    {
        Id = _Id;
        UserName = _UserName;
        Health = MaxHealth;
    }

    public void MyPlayerSetting()
    {
        if (this.gameObject.TryGetComponent(out Player _MyPlayer))
        {
            _MyPlayer.MyId = Id;
            _MyPlayer.MyType = Type;
        }
    }

    public void SetHealth(float _Health)
    {
        Health = _Health;

        if(Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Renderer.enabled = false;
    }

    public void Respawn()
    {
        Renderer.enabled = true;
        SetHealth(MaxHealth);
    }
}
