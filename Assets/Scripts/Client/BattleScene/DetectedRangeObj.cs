using UnityEngine;

public class DetectedRangeObj : MonoBehaviour
{
    [SerializeField]
    [Tooltip("BasicEnemy ������Ʈ")]
    private BasicEnemy basicEnemy;

    [SerializeField]
    [Tooltip("�߰� �� �÷��̾� ���� ����")]
    private int sensingRange;

    [SerializeField]
    [Tooltip("�÷��̾� ���� ���� �ݶ��̴�")]
    private BoxCollider2D boxCollider;

    [Tooltip("���� ���� �ݶ��̴� ������ Vector")]
    private Vector2 colliderSizeVec;

    [Tooltip("�÷��̾� �±�")]
    private const string PLAYER = "Player";

    private void Start()
    {
        ColliderSizeSetting(sensingRange);
    }

    /// <summary>
    /// ���� ���� �ݶ��̴� ũ�� ����
    /// </summary>
    /// <param name="size"> ���� ������ �ݶ��̴� ũ�� </param>
    private void ColliderSizeSetting(int size)
    {
        if (size > 1)
        {
            size += 2;
        }

        colliderSizeVec.x = size;
        colliderSizeVec.y = size;
        boxCollider.size = colliderSizeVec;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PLAYER))
        {
            basicEnemy.DetectedPlayer(collision.gameObject);

            ColliderSizeSetting(100);
        }
    }
}
