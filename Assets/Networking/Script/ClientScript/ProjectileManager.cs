using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int Id;
    public GameObject ExplosionPrefab;

    public void Initialize(int _Id)
    {
       Id = _Id;
    }

    public void Explode(Vector3 _Position)
    {
        transform.position = _Position;
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);

        NetGameManager.Projectiles.Remove(Id);
        Destroy(gameObject);
    }
}
