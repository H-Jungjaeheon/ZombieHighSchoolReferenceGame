using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int Id;
    public string UserName;
    public SpriteRenderer Renderer;

    public float Health;
    public float MaxHealth;

    public void Initialize(int _Id, string _UserName)
    {
        Id = _Id;
        UserName = _UserName;
        Health = MaxHealth;
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
