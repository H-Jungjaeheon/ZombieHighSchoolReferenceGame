using UnityEngine;

public class DetectedRangeObj : MonoBehaviour
{
    [SerializeField]
    [Tooltip("BasicEnemy 컴포넌트")]
    private BasicEnemy basicEnemy;

    [SerializeField]
    [Tooltip("추격 전 플레이어 감지 범위")]
    private int sensingRange;

    [SerializeField]
    [Tooltip("플레이어 감지 범위 콜라이더")]
    private BoxCollider2D boxCollider;

    [Tooltip("감지 범위 콜라이더 사이즈 Vector")]
    private Vector2 colliderSizeVec;

    [Tooltip("플레이어 태그")]
    private const string PLAYER = "Player";

    private void Start()
    {
        ColliderSizeSetting(sensingRange);
    }

    /// <summary>
    /// 감지 범위 콜라이더 크기 설정
    /// </summary>
    /// <param name="size"> 현재 설정할 콜라이더 크기 </param>
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
