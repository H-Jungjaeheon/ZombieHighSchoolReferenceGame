using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public int SpawnerId;
    public bool HasItem;
    public SpriteRenderer ItemSprite;

    private Vector3 BasePosition;

    public void Initialize(int _SpawnerId, bool _HasItem)
    {
        SpawnerId = _SpawnerId;
        HasItem = _HasItem;
        ItemSprite.enabled = _HasItem;

        BasePosition = transform.position;
    }

    public void ItemSpawned()
    {
        HasItem = true;
        ItemSprite.enabled = true;
    }

    public void ItemPickedUp()
    {
        HasItem = false;
        ItemSprite.enabled = false;
    }
}
