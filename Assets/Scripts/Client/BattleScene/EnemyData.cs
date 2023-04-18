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

    [Tooltip("�ִ� ü��")]
    public int maxHp;

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

    [Tooltip("����")]
    public int score;
}
