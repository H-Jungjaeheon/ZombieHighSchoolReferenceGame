using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObject/EnemyData", order = int.MaxValue)]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    [Tooltip("�̸�")]
    private string enemyName;

    public string EnemyName
    {
        get
        {
            return enemyName;
        }
    }

    [SerializeField]
    [Tooltip("�ִ� ü��")]
    private int maxHp;

    public int MaxHp
    {
        get
        {
            return maxHp;
        }
    }

    [SerializeField]
    [Tooltip("ü��")]
    private int hp;

    public int Hp
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
        }
    }

    [SerializeField]
    [Tooltip("�÷��̾� �ν� ����")]
    private float sightRange;

    public float SightRange
    {
        get
        {
            return sightRange; 
        }
    }

    [SerializeField]
    [Tooltip("�̵��ӵ�")]
    private float speed;
    
    public float Speed
    {
        get 
        {
            return speed;
        }
    }

    [SerializeField]
    [Tooltip("����")]
    private float score;

    public float Score
    {
        get
        {
            return score;
        }
    }
}
