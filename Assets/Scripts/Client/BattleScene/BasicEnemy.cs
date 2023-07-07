using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Left,
    Right,
    Down,
    Up,
    LeftUp,
    LeftDown,
    RightUp,
    RightDown
}

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

        nodePos.x = x;
        nodePos.y = y;
    }

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    public bool isWall;

    [Tooltip("���� ����� �θ� ���(���� ���)")]
    public Node parentNode;

    [Tooltip("���� ����� ������")]
    public Vector2Int nodePos;

    [Tooltip("���� ������ ����")]
    public Direction direction;

    [Tooltip("�������κ��� �̵��� �Ÿ�")]
    public int gCost;

    [Tooltip("��ֹ��� ������ ��ǥ������ �Ÿ� (����, ����)")]
    public int hCost;

    public int fCost //g + h
    {
        get
        {
            return gCost + hCost;
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
    [Tooltip("���� ������ �÷��̾��� Player ������Ʈ")]
    protected Player playerComponent;

    [Tooltip("�ڷ�ƾ ������ : 0.1��")]
    private WaitForSeconds zeroToOne = new WaitForSeconds(0.1f);

    [Tooltip("���� �������� Ÿ�� ȿ�� �ڷ�ƾ")]
    private IEnumerator hitEffectCoroutine;

    #region �̵� ���� ��ҵ� ����
    [Header("�̵� ���� ��ҵ� ����")]

    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 14;

    [Tooltip("���� �� ũ�� ����� Vector")]
    private Vector2Int mapSize;

    [Tooltip("��� ���� ���")]
    private Node startNode;

    [Tooltip("������ ���")]
    private Node targetNode;

    [SerializeField]
    [Tooltip("���� ��� ��� ����Ʈ")]
    public List<Node> finalNodeList;

    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    [SerializeField]
    [Tooltip("���� ���� ��ü ��� ����Ʈ")]
    private List<Node> curMapNodes = new List<Node>();
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

        mapSize = new Vector2Int(87, 66); //���� �Ŵ������� ���� ���������� �´� �� ũ�� �޾ƿ��� ������ ����
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
        playerComponent = detectedPlayerObj.GetComponent<Player>();

        MapSetting();
    }

    /// <summary>
    /// ������ ��� �˻�
    /// </summary>
    /// <param name="_startNode"> �˻� ���� ��� </param>
    /// <returns></returns>
    private bool Right(Node _startNode, bool isMoveAble = false)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = isMoveAble;

            currentNode = curMapNodes[(y * mapSize.x) + ++x]; // ���� ������ ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ����
            {
                break;
            }

            if (currentNode == targetNode) //���� ��尡 ��ǥ �����̸� ����
            {
                isFind = true;

                if (isMoveAble == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            cornerCheakNode = curMapNodes[(y + 1 * mapSize.x) + x];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ������ ���� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ ���� �������� ������
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �Ʒ��� ���� �����鼭 ������ �Ʒ��� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� ���� ���� ������
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }
        }

        startNode.isWall = false;

        return isFind;
    }

    
    /// <summary>
    /// ���� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    /// <param name="isMoveAble"></param>
    /// <returns></returns>
    private bool Left(Node _startNode, bool isMoveAble = false)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = isMoveAble;

            currentNode = curMapNodes[(y * mapSize.x) + --x]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ����
            {
                break;
            }

            if (currentNode == targetNode)
            {
                isFind = true;

                if (isMoveAble == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];
            
            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� ���� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� ���� �������� ������
                {
                    isFind = true;

                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �Ʒ��� ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];
                
                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� ���� ���� ������
                {
                    isFind = true;

                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    break; 
                }
            }
        }

        startNode.isWall = false;

        return isFind;
    }

    /// <summary>
    /// ���� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    /// <param name="isMoveAble"></param>
    /// <returns></returns>
    private bool Up(Node _startNode, bool isMoveAble = false)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = isMoveAble;

            currentNode = curMapNodes[(++y * mapSize.x) + x]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ����
            {
                break;
            }

            if (currentNode == targetNode)
            {
                if (isMoveAble == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                isFind = true;

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x + 1];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� ���� �շ��ִ� ��� 
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ ����
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x - 1];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� ���� ���� ���� ������
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break; 
                }
            }
        }

        _startNode.isWall = false;

        return isFind;
    }

    /// <summary>
    /// �Ʒ��� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    /// <param name="isMoveAble"></param>
    /// <returns></returns>
    private bool Down(Node _startNode, bool isMoveAble = false)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = isMoveAble;

            currentNode = curMapNodes[(--y * mapSize.x) + x]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ����
            {
                break;
            }

            if (currentNode == targetNode)
            {
                if (isMoveAble == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                isFind = true;

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x + 1];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �������� ���� �����鼭 ������ �Ʒ��� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� �� �� �ִٸ�
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x - 1];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� ���� ���� ������
                {
                    if (isMoveAble == false)
                    {
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break; 
                }
            }
        }

        _startNode.isWall = false;

        return isFind;
    }

    /// <summary>
    /// ������ �� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void RightUp(Node _startNode)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = true;

            currentNode = curMapNodes[(++y * mapSize.x) + ++x]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ����
            {
                break;
            }

            if (currentNode == targetNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x - 1];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ ���� �����鼭 ���� ���� �������� ���� ���
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� ���� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �Ʒ��� ���� �����鼭 ������ �Ʒ��� �������� ���� ���
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� ���� ���� ������
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            if (Right(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Up(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        _startNode.isWall = false;
    }

    /// <summary>
    /// ������ �Ʒ� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void RightDown(Node _startNode)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = true;

            currentNode = curMapNodes[(--y * mapSize.x) + ++x]; // ���� ���� �̵�

            if (currentNode.isWall)
            {
                break;
            }

            if (currentNode == targetNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x - 1];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ���� �Ʒ��� ���� ���� ������
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ������ ���� �������� ������
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ ���� �������� ������
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            if (Right(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Down(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        _startNode.isWall = false;
    }

    /// <summary>
    /// ���� �� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void LeftUP(Node _startNode)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = true;

            currentNode = curMapNodes[(++y * mapSize.x) + --x]; // ���� ���� �̵�

            if (currentNode.isWall)
            {
                break;
            }

            if (currentNode == targetNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x + 1];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �������� �����ְ� ������ ���� �������� ������
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // ������ ���� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break; 
                }
            }

            if (Left(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Up(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        _startNode.isWall = false;
    }

    /// <summary>
    /// ���� �Ʒ� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void LeftDown(Node _startNode)
    {
        int x = _startNode.nodePos.x;
        int y = _startNode.nodePos.y;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false)
        {
            currentNode.isWall = true;

            currentNode = curMapNodes[(--y * mapSize.x) + --x]; // ���� ���� �̵�

            if (currentNode.isWall)
            {
                break;
            }

            if (currentNode == targetNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x + 1];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �������� �����ְ� ������ �Ʒ��� �������� ������
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) //������ �Ʒ��� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ���� ���� �������� ������
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� ���� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break; 
                }
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ���� ���� �������� ������
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // ���� ���� �ȸ���
                {
                    AddOpenList(currentNode, startNode);

                    break; 
                }
            }

            if (Left(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Down(currentNode, true) == true)
            {
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        _startNode.isWall = false;
    }

    /// <summary>
    /// ���� �� ��� ���� �Լ�
    /// </summary>
    private void MapSetting()
    {
        Collider2D[] nodeCol; //�� ��� �Ǻ��� �ݶ��̴�
        Vector2 curNodePos; //�ʿ����� ���� ��� ������
        Node curNode;
        bool isWall;

        curMapNodes.Clear(); //���� �� ��� ����Ʈ �ʱ�ȭ(����)

        for (int i = 0; i < mapSize.y; i++) //���� ���� ���� ���̸�ŭ �ݺ�
        {
            for (int j = 0; j < mapSize.x; j++) //���� ���� ���� ���̸�ŭ �ݺ�
            {
                isWall = false;

                curNodePos.x = j;
                curNodePos.y = i;

                nodeCol = Physics2D.OverlapCircleAll(curNodePos, 0.2f);

                for (int k = 0; k < nodeCol.Length; k++)
                {
                    if (nodeCol[k].gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                curNode = new Node(isWall, j, i);
                curMapNodes.Add(curNode); //���� �� ��� ����Ʈ�� �߰� (�ݶ��̴��� �Ǻ��� ��� �±װ� ���̸�, �� ���� ����)
            }
        }

        StartCoroutine(PathFind());
    }

    /// <summary>
    /// �� ã�� �Լ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator PathFind()
    {
        Node currentNode;

        openList.Clear(); //���� ����Ʈ �ʱ�ȭ
        finalNodeList.Clear(); //���� ��� ����Ʈ �ʱ�ȭ

        startNode = curMapNodes[(int)transform.position.y * mapSize.x + (int)transform.position.x]; //���� ��� ����(���� �� ��� ����Ʈ���� �������� �̿��� ��ȣ�� ����)
        targetNode = curMapNodes[(int)playerComponent.moveTargetPos.y * mapSize.x + (int)playerComponent.moveTargetPos.x - 1]; //��ǥ ��� ����(���� �� ��� ����Ʈ���� �������� �̿��� ��ȣ�� ����)

        startNode.gCost = 0; //���� ����� �̵� �Ÿ� ������ �ʱ�ȭ
        startNode.hCost = Heuristic(startNode.nodePos, targetNode.nodePos); //���� ����� �޸���ƽ ������ ���� �� �߰�
        openList.Add(startNode); //���� ����Ʈ ���� ��� �߰�

        while (openList.Count > 0) //��� Ž�� ���� (���¸���Ʈ�� ���� �� ���� �ݺ�)
        {
            currentNode = openList[0]; //���� ��带 ���¸���Ʈ�� 0��° ������ �ֱ�

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost) //��ü ���¸���Ʈ�� ��ȸ�Ͽ� ���� ��庸�� �޸���ƽ ���� ���� ��带 ���� ���� ����
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode); //���� ����Ʈ �� �޸���ƽ ���� ���� ���� ��� ����

            if (currentNode == targetNode) // ���� ��尡 �� ����� �θ� ���� �����Ͽ� ������ ����
            {
                while (currentNode.parentNode != null)
                {
                    finalNodeList.Add(currentNode);
                    currentNode = currentNode.parentNode;
                }

                finalNodeList.Reverse();

                StartCoroutine(Move());

                yield break;
            }

            if (currentNode.isWall == false)
            {
                Right(currentNode);

                Down(currentNode);

                Left(currentNode);

                Up(currentNode);

                RightUp(currentNode);

                RightDown(currentNode);

                LeftUP(currentNode);

                LeftDown(currentNode);
            }

            currentNode.isWall = true;    // �ѹ� �� ��� 
        }
    }

    //private void Test()
    //{
    //    for (int i = 0; i < curMapNodes.Count; i++)
    //    {
    //        isWall = false;

    //        foreach (Collider2D collider in Physics2D.OverlapCircleAll(curMapNodes[i].nodePos, 0.2f))
    //        {
    //            if (collider.gameObject.CompareTag(WALL))
    //            {
    //                isWall = true;
    //            }
    //        }

    //        curMapNodes[i].isWall = isWall;

    //        curMapNodes[i].gCost = 0;
    //        curMapNodes[i].hCost = 0;
    //    }

    //    StartCoroutine(PathFind());
    //}

    /// <summary>
    /// �޸���ƽ �Լ� (���� ������ ���� ª�� ����� ���� ��ȯ)
    /// </summary>
    /// <param name="_currPosition"> ���� ��ġ ��� ������ </param>
    /// <param name="_endPosition"> ��ǥ ��ġ ��� ������ </param>
    /// <returns></returns>
    private int Heuristic(Vector2Int _currPosition, Vector2Int _endPosition)
    {
        int x = Mathf.Abs(_currPosition.x - _endPosition.x);
        int y = Mathf.Abs(_currPosition.y - _endPosition.y);
        int reming = Mathf.Abs(x - y);

        return MOVE_DIAGONAL_COST * Mathf.Min(x, y) + MOVE_STRAIGHT_COST * reming;
    }

    /// <summary>
    /// ���� ��� ���¸���Ʈ�� �ִ� �Լ�
    /// </summary>
    /// <param name="_currentNode"> ���¸���Ʈ�� ���� ��� </param>
    /// <param name="_parentNode"> ���¸���Ʈ ����� �θ� ��� </param>
    private void AddOpenList(Node _currentNode, Node _parentNode)
    {
        _currentNode.parentNode = _parentNode;
        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
        _currentNode.hCost = Heuristic(_currentNode.nodePos, targetNode.nodePos);

        openList.Add(_currentNode);
    }

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    public IEnumerator Move()
    {
        Vector2 curTargetPos;
        Vector3 targetPos = Vector3.zero;

        targetPos.x = targetNode.nodePos.x;
        targetPos.y = targetNode.nodePos.y;
        
        for (int nowIndex = 0; nowIndex < finalNodeList.Count; nowIndex++)
        {
            curTargetPos.x = finalNodeList[nowIndex].nodePos.x;
            curTargetPos.y = finalNodeList[nowIndex].nodePos.y;

            while (true)
            {
                transform.Translate(Vector3.right * Time.deltaTime * enemyData.Speed);
                
                yield return null;
            }


            //while (transform.position.x != curTargetPos.x && transform.position.y != curTargetPos.y)
            //{
            //    transform.position = Vector3.MoveTowards(transform.position, curTargetPos, Time.deltaTime * enemyData.Speed);

            //    yield return null;
            //}

            //if (playerComponent.moveTargetPos != targetPos)
            //{
            //    MapSetting();
            //    //Test();

            //    break;
            //}
        }
    }

    private void OnDrawGizmos()
    {
        if (finalNodeList.Count > 0)
        {
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine((Vector2)finalNodeList[i].nodePos, (Vector2)finalNodeList[i + 1].nodePos);
            }
        }
    }
}
