using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Node
{
    /// <summary>
    /// ��� ������
    /// </summary>
    /// <param name="_isWall"> ���� ������ �Ǻ� </param>
    /// <param name="x"> ����� x ������ </param>
    /// <param name="y"> ����� y ������ </param>
    public Node(bool _isWall, int x, int y)
    {
        isWall = _isWall;

        xPos = x;

        yPos = y;
    }

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    public bool isWall;

    [Tooltip("���� ����� �θ� ���(���� ���)")]
    public Node ParentNode;

    [Tooltip("���� ����� X ������")]
    public int xPos;

    [Tooltip("���� ����� Y ������")]
    public int yPos;

    [Tooltip("�������κ��� �̵��� �Ÿ�")]
    public int g;

    [Tooltip("��ֹ��� ������ ��ǥ������ �Ÿ� (����, ����)")]
    public int h;

    public int f //g + h
    {
        get
        {
            return g + h;
        }
    }
}
public class BasicEnemy : MonoBehaviour
{
    [SerializeField]
    [Tooltip("�⺻ �� ����(ScriptableObject)")]
    protected EnemyData enemyData;

    [Tooltip("ü��")]
    private int hp;

    [SerializeField]
    [Tooltip("ü�¹� �̹��� ������Ʈ")]
    protected Image hpBarImg;

    [SerializeField]
    [Tooltip("SpriteRenderer ������Ʈ")]
    protected SpriteRenderer sr;

    [SerializeField]
    [Tooltip("���� ������ �÷��̾� ������Ʈ")]
    protected GameObject detectedPObj;

    [Tooltip("�ڷ�ƾ ������ : 0.1��")]
    private WaitForSeconds zeroToOne = new WaitForSeconds(0.1f);

    [Tooltip("���� �������� Ÿ�� ȿ�� �ڷ�ƾ")]
    private IEnumerator hitEffectCoroutine;

    #region �̵� ���� ��ҵ� ����
    [Header("�̵� ���� ��ҵ� ����")]

    [Tooltip("�߰� �� �÷��̾� ���� ����")]
    private const int updateDetectedRange = 50;

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    private bool isWall;

    [Tooltip("��� Ž�� �� ���� ������")]
    private Vector2 startPos;

    [Tooltip("��� Ž�� �� ��ǥ ������")]
    private Vector2 targetPos;

    [Tooltip("���� ��� ��� ����Ʈ")]
    public List<Node> finalNodeList;

    [Tooltip("��ü ��� ���� ��� �迭")]
    private Node[,] nodeArray = new Node[updateDetectedRange * 2 + 1, updateDetectedRange * 2 + 1];

    [Tooltip("��� ���� ���")]
    private Node startNode;

    [Tooltip("������ ���")]
    private Node targetNode;

    [Tooltip("���� ��� ���")]
    private Node curNode;

    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    [Tooltip("��� Ž�� �Ϸ��� ��� ����Ʈ")]
    private List<Node> closedList = new List<Node>();

    [Tooltip("�� �±�")]
    private const string WALL = "Wall";
    #endregion

    private void Awake()
    {
        StartSetting();
    }

    /// <summary>
    /// ���� ���� �Լ�
    /// </summary>
    private void StartSetting()
    {
        hp = enemyData.maxHp;
    }

    /// <summary>
    /// �ǰ� �Լ�
    /// </summary>
    /// <param name="damage"> ���� ������ </param>
    public void Hit(int damage)
    {
        hp -= damage;

        hpBarImg.fillAmount = (float)hp / enemyData.maxHp;

        if (hp <= 0)
        {
            Dead();
        }
        else 
        {
            if (hitEffectCoroutine != null)
            {
                StopCoroutine(hitEffectCoroutine);    
            }

            hitEffectCoroutine = HitEffect();
            StartCoroutine(hitEffectCoroutine);
        }
    }

    /// <summary>
    /// Ÿ�� ȿ�� �Լ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitEffect()
    {
        sr.color = Color.red;

        yield return zeroToOne;

        sr.color = Color.white;
    }

    /// <summary>
    /// ��� �Լ�
    /// </summary>
    private void Dead()
    {
        StageManager.instance.PlusScore(enemyData.score);

        Destroy(gameObject);
    }

    /// <summary>
    /// �÷��̾� ���� ����
    /// </summary>
    public virtual void DetectedPlayer(GameObject detectedPlayerObj)
    {
        if (detectedPObj != null)
        {
            return;
        }

        detectedPObj = detectedPlayerObj;

        PathFinding();
    }

    /// <summary>
    /// ��� Ž�� �Լ�
    /// </summary>
    public void PathFinding()
    {
        startPos = new Vector2(transform.position.x, transform.position.y);
        targetPos = detectedPObj.GetComponent<Player>().moveTargetPos;
        
        for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //�÷��̾� �߰� ������ŭ ��� ����
        {
            for (int j = -updateDetectedRange; j <= updateDetectedRange; j++)
            {
                isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + i, startPos.y + j), 0.2f))
                {
                    if (collider.gameObject.CompareTag(WALL))
                    {
                        isWall = true;
                    }
                }

                nodeArray[i + updateDetectedRange, j + updateDetectedRange] = new Node(isWall, (int)startPos.x + i, (int)startPos.y + j);
            }
        }

        startNode = nodeArray[updateDetectedRange, updateDetectedRange];

        targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + updateDetectedRange, (int)targetPos.y - (int)startPos.y + updateDetectedRange];

        openList.Clear();

        openList.Add(startNode);

        closedList.Clear();

        finalNodeList.Clear();
        
        while (openList.Count > 0)
        {
            // ���¸���Ʈ �� ���� f�� ���� ��, f�� ���ٸ� h�� ���� ��, h�� ���ٸ� 0��° ���� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
            curNode = openList[0];

            openList.Remove(curNode);
            closedList.Add(curNode);

            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].f <= curNode.f && openList[i].h < curNode.h)
                {
                    curNode = openList[i];
                }
            }

            // ������
            if (curNode == targetNode)
            {
                Node TargetCurNode = targetNode;

                while (TargetCurNode != startNode)
                {
                    finalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }

                finalNodeList.Add(startNode);
                finalNodeList.Reverse();

                StartCoroutine(Move());

                return;
            }

            OpenListAdd(curNode.xPos, curNode.yPos + 1);
            OpenListAdd(curNode.xPos + 1, curNode.yPos);
            OpenListAdd(curNode.xPos, curNode.yPos - 1);
            OpenListAdd(curNode.xPos - 1, curNode.yPos);
        }
    }

    /// <summary>
    /// ���� ������ ����� ���� ���¸���Ʈ �߰� �Լ�
    /// </summary>
    /// <param name="checkX"></param>
    /// <param name="checkY"></param>
    private void OpenListAdd(int checkX, int checkY)
    {
        //���� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if (checkX >= startPos.x - updateDetectedRange && checkX <= startPos.x + updateDetectedRange
            && checkY >= startPos.y - updateDetectedRange && checkY <= startPos.y + updateDetectedRange
            && nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange].isWall == false
            && closedList.Contains(nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange]) == false)
        {
            // �̿���忡 �ֱ�
            Node neighborNode = nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange];
            int moveCost = 10;

            // �̵������ �̿���� g���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� g, h, ParentNode�� ���� �� ��������Ʈ�� �߰�
            if (moveCost < neighborNode.g || openList.Contains(neighborNode) == false)
            {
                neighborNode.g = moveCost;
                neighborNode.h = (Mathf.Abs(neighborNode.xPos - targetNode.xPos) + Mathf.Abs(neighborNode.yPos - targetNode.yPos)) * 10;
                neighborNode.ParentNode = curNode;

                openList.Add(neighborNode);
            }
        }
    }

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    public IEnumerator Move()
    {
        Vector2 curTargetPos;

        for (int nowIndex = 0; nowIndex < finalNodeList.Count; nowIndex++)
        {
            curTargetPos.x = finalNodeList[nowIndex].xPos;
            curTargetPos.y = finalNodeList[nowIndex].yPos;

            while (true)
            {
                if (transform.position.x == curTargetPos.x && transform.position.y == curTargetPos.y)
                {
                    break;
                }

                transform.position = Vector3.MoveTowards(transform.position, curTargetPos, Time.deltaTime * enemyData.Speed);

                yield return null;
            }

            if (detectedPObj.GetComponent<Player>().moveTargetPos != targetPos)
            {
                PathFinding();

                yield break;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (finalNodeList.Count != 0)
        {
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector2(finalNodeList[i].xPos, finalNodeList[i].yPos), new Vector2(finalNodeList[i + 1].xPos, finalNodeList[i + 1].yPos));
            }
        }
    }
}
