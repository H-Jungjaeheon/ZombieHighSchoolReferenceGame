using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObject/EnemyData", order = int.MaxValue)]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    [Tooltip("이름")]
    private string enemyName;

    public string EnemyName
    {
        get
        {
            return enemyName;
        }
    }

    [Tooltip("최대 체력")]
    public int maxHp;

    [SerializeField]
    [Tooltip("플레이어 인식 범위")]
    private float sightRange;

    public float SightRange
    {
        get
        {
            return sightRange; 
        }
    }

    [SerializeField]
    [Tooltip("이동속도")]
    private float speed;
    
    public float Speed
    {
        get 
        {
            return speed;
        }
    }

    [Tooltip("점수")]
    public int score;
}
