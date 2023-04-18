using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("BoxCollider2D ������Ʈ(���� ���� �ݶ��̴� ������Ʈ)")]
    private BoxCollider2D rangeCollider;

    [SerializeField]
    [Tooltip("Player ������Ʈ")]
    private Player player;

    [SerializeField]
    [Tooltip("���� ����(�ݶ��̴� ũ��)")]
    private Vector2 atkRange;

    [SerializeField]
    [Tooltip("���� ���� ��ġ(�ݶ��̴� offset)")]
    private Vector2 atkRangePos;

    [Tooltip("�� �±�")]
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
