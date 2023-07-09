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
public struct PathNodeData
{
    [Tooltip("���� ��� ��� ��ġ")]
    public Vector2 pos;

    [Tooltip("���� ��� ��� ����(���� ��忡�� ���� ���� �̵��� ��)")]
    public Vector2 direction;
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

    [Tooltip("���� ��忡�� ���� ���� ���ؾ��� ����")]
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

    //[Tooltip("������ ������� �Ǻ�")]
    //private Dictionary<Vector2, bool> isPassedNode = new Dictionary<Vector2, bool>();

    [SerializeField]
    [Tooltip("���� ���� ��ü ��� ����Ʈ")]
    private Dictionary<Vector2, Node> curMapNodes = new Dictionary<Vector2, Node>();

    [SerializeField]
    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    [SerializeField]
    [Tooltip("���� ��� ��� ����Ʈ")]
    private List<Node> finalNodeList = new List<Node>();

    [SerializeField]
    [Tooltip("���� ��� ������ ����Ʈ")]
    private List<PathNodeData> finalPathDatas;

    [Tooltip("���� ��� ������ ���� ��� ������")]
    private PathNodeData curPathNodeData = new PathNodeData();
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

        mapSize = new Vector2Int(83, 57); //���� �Ŵ������� ���� ���������� �´� �� ũ�� �޾ƿ��� ������ ����
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
    /// ������ ��� �˻�
    /// </summary>
    /// <param name="_startNode"> �˻� ���� ��� </param>
    /// <returns></returns>
    private bool Right(Node _startNode, bool isMoveAble = false)
    {
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;
        
        bool isFind = false;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //isPassedNode[currentNode.nodePos] = isMoveAble;

            currentNode = curMapNodes[curStandardNodePos += Vector2.right]; // ���� ������ ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ���� //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode) //���� ��尡 ��ǥ �����̸� ����
            {
                isFind = true;

                if (isMoveAble == false)
                {
                    currentNode.direction = Direction.Right;
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ������ ���� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ ���� �������� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Right;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �Ʒ��� ���� �����鼭 ������ �Ʒ��� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� ���� ���� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Right;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }
        }

        startNode.isWall = false;
        //isPassedNode[currentNode.nodePos] = false;

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
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //isPassedNode[currentNode.nodePos] = isMoveAble;
            currentNode = curMapNodes[curStandardNodePos += Vector2.left]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ���� //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                isFind = true;

                if (isMoveAble == false)
                {
                    currentNode.direction = Direction.Left;
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up];
            
            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� ���� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.left];

                if (cornerCheakNode.isWall == false) // ���� ���� �������� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    isFind = true;

                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Left;
                        AddOpenList(currentNode, startNode);
                    }

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �Ʒ��� ���� �����鼭 ���� �Ʒ��� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down + Vector2.left];
                
                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� ���� ���� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    isFind = true;

                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Left;
                        AddOpenList(currentNode, startNode);
                    }

                    break; 
                }
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
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
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //isPassedNode[currentNode.nodePos] = isMoveAble;
            
            currentNode = curMapNodes[curStandardNodePos += Vector2.up]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ���� //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                if (isMoveAble == false)
                {
                    currentNode.direction = Direction.Up;
                    AddOpenList(currentNode, startNode);
                }

                isFind = true;

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.right];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� ���� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ ���� //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Up;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left + Vector2.up];

                if (cornerCheakNode.isWall == false) // ���� ���� ���� ���� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Up;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break; 
                }
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
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
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        bool isFind = false;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //currentNode.isWall = isMoveAble;

            currentNode = curMapNodes[curStandardNodePos += Vector2.down]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ���� //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                if (isMoveAble == false)
                {
                    currentNode.direction = Direction.Down;
                    AddOpenList(currentNode, startNode);
                }

                isFind = true;

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.right];

            if (cornerCheakNode.isWall) // �ڳ� üũ : �������� ���� �����鼭 ������ �Ʒ��� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.right + Vector2.down];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� �� �� �ִٸ� //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Down;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left];

            if (cornerCheakNode.isWall) // �ڳ� üũ : ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left + Vector2.down];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� ���� ���� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    if (isMoveAble == false)
                    {
                        currentNode.direction = Direction.Down;
                        AddOpenList(currentNode, startNode);
                    }

                    isFind = true;

                    break; 
                }
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
        _startNode.isWall = false;

        return isFind;
    }

    /// <summary>
    /// ������ �� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void RightUp(Node _startNode)
    {
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //currentNode.isWall = true;

            currentNode = curMapNodes[curStandardNodePos += Vector2.up + Vector2.right]; // ���� ���� �̵�

            if (currentNode.isWall) //���� ��尡 ���̸� ���� //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                currentNode.direction = Direction.RightUp;
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ ���� �����鼭 ���� ���� �������� ���� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.left];

                if (cornerCheakNode.isWall == false) // ���� ���� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.RightUp;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �Ʒ��� ���� �����鼭 ������ �Ʒ��� �������� ���� ��� //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ �Ʒ��� ���� ���� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.RightUp;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            if (Right(currentNode, true) == true)
            {
                currentNode.direction = Direction.RightUp;
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Up(currentNode, true) == true)
            {
                currentNode.direction = Direction.RightUp;
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
        _startNode.isWall = false;
    }

    /// <summary>
    /// ������ �Ʒ� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void RightDown(Node _startNode)
    {
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //currentNode.isWall = true;

            currentNode = curMapNodes[curStandardNodePos += Vector2.down + Vector2.right]; // ���� ���� �̵�

            if (currentNode.isWall) //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                currentNode.direction = Direction.RightDown;
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ���� �Ʒ��� ���� ���� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left + Vector2.down];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.RightDown;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ������ ���� �������� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ ���� �������� ������ //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.RightDown;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            if (Right(currentNode, true) == true)
            {
                currentNode.direction = Direction.RightDown;
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Down(currentNode, true) == true)
            {
                currentNode.direction = Direction.RightDown;
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
        _startNode.isWall = false;
    }

    /// <summary>
    /// ���� �� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void LeftUP(Node _startNode)
    {
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //currentNode.isWall = true;

            currentNode = curMapNodes[curStandardNodePos += Vector2.up + Vector2.left]; // ���� ���� �̵�

            if (currentNode.isWall) //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                currentNode.direction = Direction.LeftUp;
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.right];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �������� �����ְ� ������ ���� �������� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up + Vector2.right];

                if (cornerCheakNode.isWall == false) // ������ ���� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.LeftUp;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down + Vector2.left];

                if (cornerCheakNode.isWall == false) // ���� �Ʒ��� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.LeftUp;
                    AddOpenList(currentNode, startNode);

                    break; 
                }
            }

            if (Left(currentNode, true) == true)
            {
                currentNode.direction = Direction.LeftUp;
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Up(currentNode, true) == true)
            {
                currentNode.direction = Direction.LeftUp;
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
        _startNode.isWall = false;
    }

    /// <summary>
    /// ���� �Ʒ� ��� �˻�
    /// </summary>
    /// <param name="_startNode"></param>
    private void LeftDown(Node _startNode)
    {
        Vector2 curStandardNodePos = _startNode.nodePos;

        Node startNode = _startNode;
        Node currentNode = startNode;
        Node cornerCheakNode;

        while (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
        {
            //currentNode.isWall = true;

            currentNode = curMapNodes[curStandardNodePos += Vector2.down + Vector2.left]; // ���� ���� �̵�

            if (currentNode.isWall) //&& isPassedNode[currentNode.nodePos]
            {
                break;
            }

            if (currentNode == targetNode)
            {
                currentNode.direction = Direction.LeftDown;
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.right];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : �������� �����ְ� ������ �Ʒ��� �������� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.down + Vector2.right];

                if (cornerCheakNode.isWall == false) //������ �Ʒ��� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.LeftDown;
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.up];

            if (cornerCheakNode.isWall) // �ڳ� Ž�� : ������ �����ְ� ���� ���� �������� ������ //&& isPassedNode[currentNode.nodePos]
            {
                cornerCheakNode = curMapNodes[curStandardNodePos + Vector2.left + Vector2.up];

                if (cornerCheakNode.isWall == false) // ���� ���� �ȸ��� //&& isPassedNode[currentNode.nodePos] == false
                {
                    currentNode.direction = Direction.LeftDown;
                    AddOpenList(currentNode, startNode);

                    break; 
                }
            }

            if (Left(currentNode, true) == true)
            {
                currentNode.direction = Direction.LeftDown;
                AddOpenList(currentNode, startNode);

                break;
            }

            if (Down(currentNode, true) == true)
            {
                currentNode.direction = Direction.LeftDown;
                AddOpenList(currentNode, startNode);

                break;
            }
        }

        //isPassedNode[currentNode.nodePos] = false;
        _startNode.isWall = false;
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
            curNodePos.y = i;

            for (int j = 0; j < mapSize.x; j++) //���� ���� ���� ���̸�ŭ �ݺ�
            {
                isWall = false;

                curNodePos.x = j;

                nodeCol = Physics2D.OverlapCircleAll(curNodePos, 0.3f);

                for (int k = 0; k < nodeCol.Length; k++)
                {
                    if (nodeCol != null && nodeCol[k].gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                curNode = new Node(isWall, j, i);
                curMapNodes.Add(curNode.nodePos, curNode); //���� �� ��� ����Ʈ�� �߰� (�ݶ��̴��� �Ǻ��� ��� �±װ� ���̸�, �� ���� ����)
            }
        }

        PathFind();
    }

    /// <summary>
    /// �� ã�� �Լ�
    /// </summary>
    /// <returns></returns>
    private void PathFind()
    {
        Node currentNode;
        //Vector2 curSettingNodePos;

        openList.Clear(); //���� ����Ʈ �ʱ�ȭ
        //isPassedNode.Clear(); //������ ��� �Ǻ��ϴ� ����Ʈ �ʱ�ȭ
        startNode = curMapNodes[transform.position]; //���� ��� ����(���� �� ��� ����Ʈ���� �������� �̿��� ��ȣ�� ����)
        targetNode = curMapNodes[playerComponent.moveTargetPos]; //��ǥ ��� ����(���� �� ��� ����Ʈ���� �������� �̿��� ��ȣ�� ����)

        startNode.gCost = 0; //���� ����� �̵� �Ÿ� ������ �ʱ�ȭ
        startNode.hCost = Heuristic(startNode.nodePos, targetNode.nodePos); //���� ����� �޸���ƽ ������ ���� �� �߰�
        openList.Add(startNode); //���� ����Ʈ ���� ��� �߰�

        //for (int i = 0; i < mapSize.y; i++) //���� ���� ���� ���̸�ŭ �ݺ�
        //{
        //    for (int j = 0; j < mapSize.x; j++) //���� ���� ���� ���̸�ŭ �ݺ�
        //    {
        //        curSettingNodePos.x = j;
        //        curSettingNodePos.y = i;

        //        isPassedNode.Add(curSettingNodePos, false);
        //    }
        //}

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
                while (true)
                { 
                    finalNodeList.Add(currentNode);

                    if (currentNode.parentNode == null)
                    {
                        break;
                    }
                    else
                    {
                        currentNode = currentNode.parentNode;
                    }
                }

                finalNodeList.Reverse();

                StartCoroutine(FinalNodeSetting());

                break;
            }

            if (currentNode.isWall == false) //&& isPassedNode[currentNode.nodePos] == false
            {
                Left(currentNode);

                Right(currentNode);

                Down(currentNode);

                Up(currentNode);

                RightUp(currentNode);

                RightDown(currentNode);

                LeftUP(currentNode);

                LeftDown(currentNode);
            }

            currentNode.isWall = true;    // �ѹ� �� ��� 
            //isPassedNode[currentNode.nodePos] = true; //�ѹ� �� ���
        }
    }

    private IEnumerator FinalNodeSetting()
    {    
        Vector2 curDirection = Vector3.zero; //���� ��� ����
        Vector2 curPathPos; //���� ��� ��ġ
        
        bool isUpDownMove; //�밢�� ��� �������� ��, �Ʒ� ���� ��� ������ �������� �Ǻ�

        int curIndex = 0;

        finalPathDatas.Clear(); //���� ��� ����Ʈ �ʱ�ȭ

        curPathPos = finalNodeList[0].nodePos;
        curPathNodeData.pos = curPathPos;
        finalPathDatas.Add(curPathNodeData);

        while (curIndex < finalNodeList.Count - 1) //���� ������ �ݺ�
        {
            if (finalNodeList[curIndex + 1].direction == Direction.Left || finalNodeList[curIndex + 1].direction == Direction.Right || finalNodeList[curIndex + 1].direction == Direction.Down
                || finalNodeList[curIndex + 1].direction == Direction.Up)
            {
                switch (finalNodeList[curIndex + 1].direction)
                {
                    case Direction.Left:
                        curDirection = Vector2.left;
                        break;
                    case Direction.Right:
                        curDirection = Vector2.right;
                        break;
                    case Direction.Down:
                        curDirection = Vector2.down;
                        break;
                    case Direction.Up:
                        curDirection = Vector2.up;
                        break;
                }

                while (curPathPos != finalNodeList[curIndex + 1].nodePos)
                {
                    curPathPos += curDirection;

                    curPathNodeData.pos = curPathPos;
                    curPathNodeData.direction = curDirection;
                    finalPathDatas.Add(curPathNodeData);
                }
            }
            else
            {
                isUpDownMove = false;

                while (curPathPos != finalNodeList[curIndex + 1].nodePos)
                {
                    switch (finalNodeList[curIndex + 1].direction)
                    {
                        case Direction.LeftUp:
                            curDirection = Vector2.left;
                            break;
                        case Direction.LeftDown:
                            curDirection = Vector2.left;
                            break;
                        case Direction.RightUp:
                            curDirection = Vector2.right;
                            break;
                        case Direction.RightDown:
                            curDirection = Vector2.right;
                            break;
                    }

                    if (isUpDownMove)
                    {
                        if ((((finalNodeList[curIndex + 1].direction == Direction.LeftUp || finalNodeList[curIndex + 1].direction == Direction.RightUp) && curMapNodes[curPathPos + Vector2.up].isWall)
                             || (finalNodeList[curIndex + 1].direction == Direction.LeftDown || finalNodeList[curIndex + 1].direction == Direction.RightDown) && curMapNodes[curPathPos + Vector2.down].isWall)
                             && curPathPos.x != finalNodeList[curIndex + 1].nodePos.x)
                        {
                            curPathPos += curDirection;
                        }
                        else
                        {
                            if (curPathPos.y != finalNodeList[curIndex + 1].nodePos.y)
                            {
                                if (finalNodeList[curIndex + 1].direction == Direction.LeftUp || finalNodeList[curIndex + 1].direction == Direction.RightUp)
                                {
                                    curDirection = Vector2.up;
                                    curPathPos += Vector2.up;
                                }
                                else
                                {
                                    curDirection = Vector2.down;
                                    curPathPos += Vector2.down;
                                }
                            }
                        }
                    }
                    else if(isUpDownMove == false)
                    {
                        if ((((finalNodeList[curIndex + 1].direction == Direction.LeftUp || finalNodeList[curIndex + 1].direction == Direction.LeftDown) && curMapNodes[curPathPos + Vector2.left].isWall)
                             || (finalNodeList[curIndex + 1].direction == Direction.RightUp || finalNodeList[curIndex + 1].direction == Direction.RightDown) && curMapNodes[curPathPos + Vector2.right].isWall)
                             && curPathPos.y != finalNodeList[curIndex + 1].nodePos.y)
                        {
                            if (finalNodeList[curIndex + 1].direction == Direction.LeftUp || finalNodeList[curIndex + 1].direction == Direction.RightUp)
                            {
                                curDirection = Vector2.up;
                                curPathPos += Vector2.up;
                            }
                            else
                            {
                                curDirection = Vector2.down;
                                curPathPos += Vector2.down;
                            }
                        }
                        else
                        {
                            if (curPathPos.x != finalNodeList[curIndex + 1].nodePos.x)
                            {
                                curPathPos += curDirection;
                            }
                        }
                    }

                    isUpDownMove = (isUpDownMove == false) ? true : false;

                    curPathNodeData.pos = curPathPos;
                    curPathNodeData.direction = curDirection;
                    finalPathDatas.Add(curPathNodeData);
                }
            }

            curIndex++;

            yield return null;
        }

        StartCoroutine(Move());
    }

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    public IEnumerator Move()
    {
        Vector2 curDirection;
        float curModeIndex;

        for (int nowIndex = 0; nowIndex < finalPathDatas.Count - 1; nowIndex++)
        {
            curModeIndex = 0f;

            curDirection = finalPathDatas[nowIndex + 1].direction;

            while (curModeIndex <= 1f)
            {
                transform.Translate(curDirection * Time.deltaTime * enemyData.Speed);
                
                curModeIndex += Time.deltaTime * enemyData.Speed;

                yield return null;
            }

            if (nowIndex >= (finalPathDatas.Count - 1) / 2 && playerComponent.moveTargetPos != finalNodeList[finalNodeList.Count - 1].nodePos)
            {
                transform.position = finalPathDatas[nowIndex + 1].pos;

                MapNodeSetting();

                yield break;
            }

            yield return null;
        }

        transform.position = finalPathDatas[finalPathDatas.Count - 1].pos;
    }

    private void MapNodeSetting()
    {
        Collider2D[] nodeCol; //�� ��� �Ǻ��� �ݶ��̴�
        Vector2 curNodePos; //�ʿ����� ���� ��� ������

        bool isWall;

        finalNodeList.Clear();

        for (int i = 0; i < mapSize.y; i++) //���� ���� ���� ���̸�ŭ �ݺ�
        {
            curNodePos.y = i;

            for (int j = 0; j < mapSize.x; j++) //���� ���� ���� ���̸�ŭ �ݺ�
            {
                //isWall = false;
                curNodePos.x = j;

                //nodeCol = Physics2D.OverlapCircleAll(curNodePos, 0.3f);

                //for (int k = 0; k < nodeCol.Length; k++)
                //{
                //    if (nodeCol != null && nodeCol[k].gameObject.CompareTag("Wall"))
                //    {
                //        isWall = true;
                //    }
                //}

                //curMapNodes[curNodePos].isWall = isWall;

                curMapNodes[curNodePos].gCost = 0;
                curMapNodes[curNodePos].hCost = 0;
            }
        }

        PathFind();
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
        _currentNode.parentNode = _parentNode;
        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
        _currentNode.hCost = Heuristic(_currentNode.nodePos, targetNode.nodePos);

        openList.Add(_currentNode);
    }

    //private void OnDrawGizmos()
    //{
    //    if (finalNodeList.Count > 0)
    //    {
    //        for (int i = 0; i < finalNodeList.Count - 1; i++)
    //        {
    //            Gizmos.DrawLine((Vector2)finalNodeList[i].nodePos, (Vector2)finalNodeList[i + 1].nodePos);
    //        }
    //    }
    //}
}
