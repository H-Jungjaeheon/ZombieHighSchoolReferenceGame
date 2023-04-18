using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("BoxCollider2D 컴포넌트(공격 범위 콜라이더 컴포넌트)")]
    private BoxCollider2D rangeCollider;

    [SerializeField]
    [Tooltip("Player 컴포넌트")]
    private Player player;

    [SerializeField]
    [Tooltip("공격 범위(콜라이더 크기)")]
    private Vector2 atkRange;

    [SerializeField]
    [Tooltip("공격 범위 위치(콜라이더 offset)")]
    private Vector2 atkRangePos;

    [Tooltip("적 태그")]
    private const string ENEMY = "Enemy";

    private void Awake()
    {
        rangeCollider.size = atkRange;

        rangeCollider.offset = atkRangePos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ENEMY))
        {
            player.rangeInEnemy.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ENEMY))
        {
            player.rangeInEnemy.Remove(collision.gameObject);
        }
    }
}
