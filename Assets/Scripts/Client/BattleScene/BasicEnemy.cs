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

        nodePos.x = x;
        nodePos.y = y;
    }

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    public bool isWall;

    [Tooltip("���� ����� �θ� ���(���� ���)")]
    public Node parentNode;

    [Tooltip("���� ����� ������")]
    public Vector2Int nodePos;

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

    const int MOVE_STRAIGHT_COST = 1;
    const int MOVE_DIAGONAL_COST = 2;

    [Tooltip("���� �� ũ�� ����� Vector")]
    private Vector2Int mapSize;

    [Tooltip("���� �� ���� Ÿ�� ��ġ Vector (�� ���� �Ʒ� Ÿ��)")]
    private Vector2Int criteriaTilePos;

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    private bool isWall;

    [Tooltip("���� ��� ��� ����Ʈ")]
    public List<Node> finalNodeList;

    [Tooltip("��� ���� ���")]
    private Node startNode;

    [Tooltip("������ ���")]
    private Node targetNode;

    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    [Tooltip("���� ���� ��ü ��� ����Ʈ")]
    private List<Node> curMapNodes = new List<Node>();

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

        mapSize = new Vector2Int(87, 66); //���� �Ŵ������� ���� ���������� �´� �� ũ�� �޾ƿ��� ������ ����
        criteriaTilePos = Vector2Int.zero;
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
        if (playerComponent != null)
        {
            return;
        }

        playerComponent = detectedPlayerObj.GetComponent<Player>();

        openList.Clear();
        curMapNodes.Clear();
        MapSetting();
    }

    private IEnumerator PathFind()
    {
        startNode = curMapNodes[(int)transform.position.y * mapSize.x + (int)transform.position.x];
        targetNode = curMapNodes[(int)playerComponent.moveTargetPos.y * mapSize.x + (int)playerComponent.moveTargetPos.x];

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode.nodePos, targetNode.nodePos);
        openList.Add(startNode);

        Node currentNode;

        //��� Ž��
        while (openList.Count > 0)
        {
            currentNode = openList[0];
            
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);

            if (currentNode == targetNode) // ���� ��尡 �� �����
            {
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

            if (CheakNextNode(x + 1, y) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

            currentNode = curMapNodes[(y * mapSize.x) + ++x];//GetGrid(++x, y); // ���� ������ ���� �̵�

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

            if (CheakNextNode(x - 1, y) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x, y + 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x, y - 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x + 1, y + 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x + 1, y - 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x - 1, y + 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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

            if (CheakNextNode(x - 1, y - 1) == false) // Ž�� ���� ����(���� Ž���Ϸ��� ������ �� ũ�⺸�� ũ�� ���)
            {
                break;
            }

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
    /// Ž�� ���� ���� üũ �Լ�
    /// </summary>
    /// <param name="nodeXPos"></param>
    /// <param name="nodeYPos"></param>
    /// <returns></returns>
    private bool CheakNextNode(int nodeXPos, int nodeYPos)
    {
        if (nodeXPos < 0 || nodeYPos < 0 || nodeXPos >= mapSize.x || nodeYPos >= mapSize.y)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// �� ���� �Լ�
    /// </summary>
    private void MapSetting()
    {
        Node curMapNode;

        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(criteriaTilePos.x + j, criteriaTilePos.y + i), 0.2f))
                {
                    if (collider.gameObject.CompareTag(WALL))
                    {
                        isWall = true;
                    }
                }

                curMapNode = new Node(isWall, j, i);
                curMapNodes.Add(curMapNode);

                // ����, ��ǥ ��Ʈ �Ǻ� �� ����
                if (j == transform.position.x && i == transform.position.y)
                {
                    startNode = curMapNode;
                }
                else if (j == playerComponent.moveTargetPos.x && i == playerComponent.moveTargetPos.y)
                {
                    targetNode = curMapNode;
                }
            }
        }

        StartCoroutine(PathFind());
    }

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
        //int nextCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
        //if (nextCost < _currentNode.gCost)
        // {
        _currentNode.parentNode = _parentNode;
        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
        _currentNode.hCost = Heuristic(_currentNode.nodePos, targetNode.nodePos);
        openList.Add(_currentNode);
        // }
    }

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    public IEnumerator Move()
    {
        Vector2 curTargetPos;

        print("����");

        yield return null;
        //for (int nowIndex = 0; nowIndex < finalNodeList.Count; nowIndex++)
        //{
        //    //curTargetPos.x = finalNodeList[nowIndex].xPos;
        //    //curTargetPos.y = finalNodeList[nowIndex].yPos;

        //    while (true)
        //    {
        //        if (transform.position.x == curTargetPos.x && transform.position.y == curTargetPos.y)
        //        {
        //            break;
        //        }

        //        transform.position = Vector3.MoveTowards(transform.position, curTargetPos, Time.deltaTime * enemyData.Speed);

        //        yield return null;
        //    }

        //    if (playerComponent.moveTargetPos != targetPos)
        //    {
        //        PathFinding();

        //        yield break;
        //    }
        //}
    }
}
