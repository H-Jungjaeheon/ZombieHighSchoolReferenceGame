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

    [SerializeField]
    [Tooltip("체력")]
    private int hp;

    public int Hp
    {
        get
        {
            return hp;
        }
    }

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

    [SerializeField]
    [Tooltip("점수")]
    private float score;

    public float Score
    {
        get
        {
            return score;
        }
    }
}
