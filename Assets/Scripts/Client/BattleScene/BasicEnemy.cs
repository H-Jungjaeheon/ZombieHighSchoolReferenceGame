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

    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 14;

    [Tooltip("���� �� ũ�� ����� Vector")]
    private Vector2Int mapSize;

    [Tooltip("���� �� ���� Ÿ�� ��ġ Vector (�� ���� �Ʒ� Ÿ��)")]
    private Vector2Int criteriaTilePos;

    //[Tooltip("�߰� �� �÷��̾� ���� ����")]
    //private const int updateDetectedRange = 50;

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    private bool isWall;

    [Tooltip("��� Ž�� �� ���� ������")]
    private Vector3 startPos;

    [Tooltip("��� Ž�� �� ��ǥ ������")]
    private Vector3 targetPos;

    [Tooltip("���� ��� ��� ����Ʈ")]
    public List<Node> finalNodeList;

    //[Tooltip("��ü ��� ���� ��� �迭")]
    //private Node[,] nodeArray = new Node[updateDetectedRange * 2 + 1, updateDetectedRange * 2 + 1];

    [Tooltip("��� ���� ���")]
    private Node startNode;

    [Tooltip("������ ���")]
    private Node targetNode;

    //[Tooltip("���� ��� ���")]
    //private Node curNode;

    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    private List<Node> mapData = new List<Node>();

    //[Tooltip("��� Ž�� �Ϸ��� ��� ����Ʈ")]
    //private List<Node> closedList = new List<Node>();

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
        criteriaTilePos = new Vector2Int(-35, -37);
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

        StartCoroutine(PathFind());
    }

    private IEnumerator PathFind()
    {
        WaitForSeconds pointZeroFiveSec = new WaitForSeconds(0.05f);

        #region  �ʰ�ȭ 

        openList.Clear();
        mapData.Clear();
        MapSetting();

        #endregion

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode.position, endNode.position);
        openList.Add(startNode);

        // PAthFinding
        while (openNodes.Count > 0)
        {
            Node currentNode = GetLowestFCost(openNodes);
            openNodes.Remove(currentNode);


            #region  �ð�ȭ �ϱ� ���� �� ���� ����Ʈ�� �� ��带 �ʷ����� ����
            if (currentNode != startNode)
            {
                currentNode.tile.SetColor(Color.green);
            }
            #endregion

            if (currentNode == endNode) // ���� ��尡 �� �����
            {
                print("Find");
                ShowPath(endNode);
                yield break;
            }

            if (currentNode.moveable == true)
            {
                Right(currentNode);
                yield return pointZeroFiveSec;

                Down(currentNode);
                yield return pointZeroFiveSec;

                Left(currentNode);
                yield return pointZeroFiveSec;

                Up(currentNode);
                yield return pointZeroFiveSec;

                RightUp(currentNode);
                yield return pointZeroFiveSec;

                RightDown(currentNode);
                yield return pointZeroFiveSec;

                LeftUP(currentNode);
                yield return pointZeroFiveSec;

                LeftDown(currentNode);
                yield return pointZeroFiveSec;
            }

            currentNode.moveable = false;    // �ѹ� �� ��� 

            yield return pointZeroFiveSec;
        }
    }

    #region  ������ �˻�

    private bool Right(Node _startNode, bool isMovealbe = false)
    {
        int x = _startNode.position.x;
        int y = _startNode.position.y;

        Node startNode = _startNode;
        Node currentNode = startNode;

        bool isFind = false;

        while (currentNode.moveable == true)
        {
            currentNode.moveable = isMovealbe;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x + 1, y) == false)   // Ž�� ���� ����
            {
                break;
            }
            currentNode = GetGrid(++x, y); // ���� ������ ���� �̵�

            if (currentNode.moveable == false)
            {
                break;
            }

            if (currentNode == this.endNode)
            {
                isFind = true;
                if (isMovealbe == false)
                {
                    AddOpenList(currentNode, startNode);
                }
                break;
            }

            currentNode.tile.SetColor(Color.gray);

            #endregion

            #region ������ ���� �����鼭 ������ ���� �շ��ִ� ���

            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
            {
                if (GetGrid(x, y + 1).moveable == false) // ������ ���̸�
                {
                    if (GetGrid(x + 1, y + 1).moveable == true) // ������ ���� �������� ������
                    {
                        if (isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }
                        isFind = true;
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region ������ ���� �����鼭 ������ �Ʒ��� �շ��ִ� ���

            if (y > 0 && x + 1 < mapSize.x)
            {
                if (GetGrid(x, y - 1).moveable == false) // �Ʒ����� ���̰�
                {
                    if (GetGrid(x + 1, y - 1).moveable == true) // ������ �Ʒ��� ���� ���� ������
                    {
                        if (isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }
                        isFind = true;
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion
        }

        startNode.moveable = true;
        return isFind;
    }

    #endregion


    #region  ���� �˻�

    private bool Left(Node _startNode, bool _isMovealbe = false)
    {
        int x = _startNode.position.x;
        int y = _startNode.position.y;

        Node startNode = _startNode;
        Node currentNode = startNode;

        bool isFind = false;

        while (currentNode.moveable == true)
        {
            currentNode.moveable = _isMovealbe;


            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x - 1, y) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(--x, y); // ���� ���� ���� �̵�
            if (currentNode.moveable == false)
            {
                break;
            }
            currentNode.tile.SetColor(Color.gray);

            if (currentNode == this.endNode)
            {
                isFind = true;

                if (_isMovealbe == false)
                {
                    AddOpenList(currentNode, startNode);
                }
                break;
            }

            #endregion

            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

            if (y + 1 < mapSize.y && x - 1 >= 0)
            {
                if (GetGrid(x, y + 1).moveable == false) // ������ ���̸�
                {
                    if (GetGrid(x - 1, y + 1).moveable == true) // ���� ���� �������� ������
                    {
                        isFind = true;
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

            if (y > 0 && x - 1 >= 0)
            {
                if (GetGrid(x, y - 1).moveable == false) // �Ʒ����� ���̰�
                {
                    if (GetGrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
                    {
                        isFind = true;
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion
        }

        startNode.moveable = true;
        return isFind;
    }

    #endregion


    #region  ���� �˻�

    private bool Up(Node _satrtNode, bool _isMovealbe = false)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;

        bool isFind = false;

        while (currentNode.moveable == true)
        {
            currentNode.moveable = _isMovealbe;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x, y + 1) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(x, ++y); // ���� ���� ���� �̵�
            if (currentNode.moveable == false)
            {
                break;
            }
            currentNode.tile.SetColor(Color.gray);

            if (currentNode == this.endNode)
            {
                isFind = true;
                if (_isMovealbe == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            #endregion

            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
            {
                if (GetGrid(x + 1, y).moveable == false) // �������� ���̸�
                {
                    if (GetGrid(x + 1, y + 1).moveable == true) // ������ ����
                    {
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }

                        isFind = true;
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

            if (y + 1 < mapSize.y && x > 0)
            {
                if (GetGrid(x - 1, y).moveable == false) // ������ ���̰�
                {
                    if (GetGrid(x - 1, y + 1).moveable == true) // ���� ���� ���� ���� ������
                    {
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }

                        isFind = true;
                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion
        }

        _satrtNode.moveable = true;
        return isFind;
    }

    #endregion


    #region  �Ʒ��� �˻�

    private bool Down(Node _satrtNode, bool _isMovealbe = false)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;

        bool isFind = false;

        while (currentNode.moveable == true)
        {
            currentNode.moveable = _isMovealbe;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x, y - 1) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(x, --y); // ���� ���� ���� �̵�
            if (currentNode.moveable == false)
            {
                break;
            }
            currentNode.tile.SetColor(Color.gray);

            if (currentNode == this.endNode)
            {
                isFind = true;
                if (_isMovealbe == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            #endregion

            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

            if (y > 0 && x + 1 < mapSize.x)
            {
                if (GetGrid(x + 1, y).moveable == false) // �������� ���̸�
                {
                    if (GetGrid(x + 1, y - 1).moveable == true) // ������ �Ʒ��� �� �� �ִٸ�
                    {
                        isFind = true;
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

            if (y > 0 && x - 1 >= 0)
            {
                if (GetGrid(x - 1, y).moveable == false) // ������ ���̰�
                {
                    if (GetGrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
                    {
                        isFind = true;
                        if (_isMovealbe == false)
                        {
                            AddOpenList(currentNode, startNode);
                        }

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion
        }

        _satrtNode.moveable = true;
        return isFind;
    }

    #endregion


    #region  ������ �� �밢�� �˻�

    private void RightUp(Node _satrtNode)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;


        while (currentNode.moveable == true)
        {
            currentNode.moveable = false;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x + 1, y + 1) == false)   // Ž�� ���� ����
            {
                break;
            }



            currentNode = GetGrid(++x, ++y); // ���� ���� ���� �̵�

            if (currentNode.moveable == false)
            {
                break;
            }

            if (currentNode == this.endNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            currentNode.tile.SetColor(Color.gray);

            #endregion

            #region ������ ���� �����鼭 ���� ���� �������� ���� ���

            if (y + 1 < mapSize.y && x > 0)
            {
                if (GetGrid(x - 1, y).moveable == false) // ������ ����
                {
                    if (GetGrid(x - 1, y + 1).moveable == true) // ���� ���� �ȸ���
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region �Ʒ��� ���� �����鼭 ������ �Ʒ��� �ȸ�������

            if (y > 0 && x + 1 < mapSize.x)
            {
                if (GetGrid(x, y - 1).moveable == false) // ������ ���̰�
                {
                    if (GetGrid(x + 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }


            #endregion

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

        _satrtNode.moveable = true;
    }

    #endregion


    #region  ������ �Ʒ� �밢�� �˻�

    private void RightDown(Node _satrtNode)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;

        while (currentNode.moveable == true)
        {

            currentNode.moveable = false;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x + 1, y - 1) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(++x, --y); // ���� ���� ���� �̵�

            if (currentNode.moveable == false)
            {
                break;
            }

            if (currentNode == this.endNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            currentNode.tile.SetColor(Color.gray);

            #endregion

            #region ������ �����ְ� ���� �Ʒ��� ���� ���� ������

            if (y > 0 && x > 0)
            {
                if (GetGrid(x - 1, y).moveable == false) // ������ ����
                {
                    if (GetGrid(x - 1, y - 1).moveable == true) // ���� ���� �ȸ���
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region ������ �����ְ� ������ ���� �������� ������

            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
            {
                if (GetGrid(x, y + 1).moveable == false) // ������ ���̰�
                {
                    if (GetGrid(x + 1, y + 1).moveable == true) // ������ ���� �������� ������
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }


            #endregion

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

        _satrtNode.moveable = true;
    }

    #endregion


    #region  ���� �� �밢�� �˻�

    private void LeftUP(Node _satrtNode)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;


        while (currentNode.moveable == true)
        {
            currentNode.moveable = false;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x - 1, y + 1) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(--x, ++y); // ���� ���� ���� �̵�

            if (currentNode.moveable == false)
            {
                break;
            }

            if (currentNode == this.endNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            currentNode.tile.SetColor(Color.gray);

            #endregion

            #region �������� �����ְ� ������ ���� �������� ������

            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
            {
                if (GetGrid(x + 1, y).moveable == false) // �������� ����
                {
                    if (GetGrid(x + 1, y + 1).moveable == true) // ������ ���� �ȸ���
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������

            if (y > 0 && x > 0)
            {
                if (GetGrid(x, y - 1).moveable == false) // �Ʒ��� ��
                {
                    if (GetGrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� �ȸ���
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }


            #endregion

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

        _satrtNode.moveable = true;
    }

    #endregion

    #region  ���� �Ʒ� �밢�� �˻�

    private void LeftDown(Node _satrtNode)
    {
        int x = _satrtNode.position.x;
        int y = _satrtNode.position.y;

        Node startNode = _satrtNode;
        Node currentNode = startNode;


        while (currentNode.moveable == true)
        {
            currentNode.moveable = false;

            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

            if (Comp(x - 1, y - 1) == false)   // Ž�� ���� ����
            {
                break;
            }

            currentNode = GetGrid(--x, --y); // ���� ���� ���� �̵�

            if (currentNode.moveable == false)
            {
                break;
            }

            if (currentNode == this.endNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            currentNode.tile.SetColor(Color.gray);

            #endregion

            #region �������� �����ְ� ������ ���� �������� ������

            if (y > 0 && x + 1 < mapSize.x)
            {
                if (GetGrid(x + 1, y).moveable == false) // �������� ����
                {
                    if (GetGrid(x + 1, y - 1).moveable == true) //������ �Ʒ��� �� ����
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }

            #endregion

            #region �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������

            if (y + 1 < mapSize.y && x < 0)
            {
                if (GetGrid(x, y + 1).moveable == false) // ���� ����
                {
                    if (GetGrid(x - 1, y + 1).moveable == true) // ���� ���� �ȸ���
                    {
                        AddOpenList(currentNode, startNode);

                        break; // �ڳ� �߰��ϸ� �ٷ� ����
                    }
                }
            }


            #endregion

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

        _satrtNode.moveable = true;
    }

    #endregion

    #region  Ž�� ���� ����
    private bool Comp(int _x, int _y)
    {
        if (_x < 0 || _y < 0 || _x >= mapSize.x || _y >= mapSize.y)
        {
            return false;
        }
        return true;
    }
    #endregion


    #region ����Ʈ �ȿ� �ִ� ��带 X�� y��ǥ ������ ��ȯ

    private Node GetGrid(int _x, int _y)
    {
        return mapData[_x + _y * mapSize.x];
    }

    #endregion


    /// <summary>
    /// �� ���� �Լ�
    /// </summary>
    private void MapSetting()
    {
        int x; //
        int y;

        Node newNode;

        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + i, startPos.y + j), 0.2f))
                {
                    if (collider.gameObject.CompareTag(WALL))
                    {
                        isWall = true;
                    }
                }

                newNode = new Node(isWall, criteriaTilePos.x + i, criteriaTilePos.y);
                mapData.Add(newNode);
            }

            // Add Wall
            if (child.CompareTag("Start"))
            {
                startNode = newNode;
                newNode.tile.SetColor(Color.red);
            }
            else if (child.CompareTag("End"))
            {
                endNode = newNode;
                newNode.tile.SetColor(Color.blue);
            }
            else if (child.CompareTag("Wall"))
            {
                newNode.tile.SetColor(Color.black);
            }
            else
            {
                newNode.tile.SetColor(Color.white);
            }
        }
    }
    #endregion


    #region ���� ������ ���� ª�� ����� ���� ��ȯ �޸���ƽ
    private int Heuristic(Vector2Int _currPosition, Vector2Int _endPosition)
    {
        int x = Mathf.Abs(_currPosition.x - _endPosition.x);
        int y = Mathf.Abs(_currPosition.y - _endPosition.y);
        int reming = Mathf.Abs(x - y);

        return MOVE_DIAGONAL_COST * Mathf.Min(x, y) + MOVE_STRAIGHT_COST * reming;
    }

    #endregion


    #region  ����Ʈ �߿� ���� ª�� F���� ���� ��带 ��ȯ
    private Node GetLowestFCost(List<Node> _pathList)
    {
        Node lowestNode = _pathList[0];

        for (int i = 1; i < _pathList.Count; i++)
        {
            if (_pathList[i].fCost < lowestNode.fCost)
            {
                lowestNode = _pathList[i];
            }
        }
        return lowestNode;
    }


    #endregion

    private void ShowPath(Node _node)
    {
        if (_node != null)
        {
            if (_node == startNode)
            {
                _node.tile.SetColor(Color.red);
            }
            else if (_node == endNode)
            {
                _node.tile.SetColor(Color.blue);
            }
            else
            {
                _node.tile.SetColor(Color.cyan);
            }

            if (_node.parentNode != null)
            {
                Vector3 start = _node.tile.transform.position;
                Vector3 end = _node.parentNode.tile.transform.position;
                Debug.DrawLine(start, end, Color.yellow, 5);
            }

            ShowPath(_node.parentNode);
        }
    }

    private void AddOpenList(Node _currentNode, Node _parentNode)
    {
        int nextCost = _parentNode.gCost + Heuristic(_parentNode.position, _currentNode.position);
        //if (nextCost < _currentNode.gCost)
        // {
        _currentNode.parentNode = _parentNode;
        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.position, _currentNode.position);
        _currentNode.hCost = Heuristic(_currentNode.position, endNode.position);
        openNodes.Add(_currentNode);
        // }
    }

    ///// <summary>
    ///// ��� Ž�� �Լ�
    ///// </summary>
    //public void PathFinding()
    //{
    //    startPos = transform.position;
    //    targetPos = playerComponent.moveTargetPos;

    //    for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //�÷��̾� �߰� ������ŭ ��� ����
    //    {
    //        for (int j = -updateDetectedRange; j <= updateDetectedRange; j++)
    //        {
    //            isWall = false;

    //            foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + i, startPos.y + j), 0.2f))
    //            {
    //                if (collider.gameObject.CompareTag(WALL))
    //                {
    //                    isWall = true;
    //                }
    //            }

    //            nodeArray[i + updateDetectedRange, j + updateDetectedRange] = new Node(isWall, (int)startPos.x + i, (int)startPos.y + j);
    //        }
    //    }

    //    startNode = nodeArray[updateDetectedRange, updateDetectedRange];

    //    targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + updateDetectedRange, (int)targetPos.y - (int)startPos.y + updateDetectedRange];

    //    openList.Clear();

    //    openList.Add(startNode);

    //    closedList.Clear();

    //    finalNodeList.Clear();

    //    while (openList.Count > 0)
    //    {
    //        // ���¸���Ʈ �� ���� f�� ���� ��, f�� ���ٸ� h�� ���� ��, h�� ���ٸ� 0��° ���� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
    //        curNode = openList[0];

    //        openList.Remove(curNode);
    //        closedList.Add(curNode);

    //        for (int i = 0; i < openList.Count; i++)
    //        {
    //            if (openList[i].fCost <= curNode.fCost && openList[i].hCost < curNode.hCost)
    //            {
    //                curNode = openList[i];
    //            }
    //        }

    //        // ������
    //        if (curNode == targetNode)
    //        {
    //            Node TargetCurNode = targetNode;

    //            while (TargetCurNode != startNode)
    //            {
    //                finalNodeList.Add(TargetCurNode);
    //                TargetCurNode = TargetCurNode.parentNode;
    //            }

    //            finalNodeList.Add(startNode);
    //            finalNodeList.Reverse();

    //            StartCoroutine(Move());

    //            return;
    //        }

    //        OpenListAdd(curNode.xPos, curNode.yPos + 1);
    //        OpenListAdd(curNode.xPos + 1, curNode.yPos);
    //        OpenListAdd(curNode.xPos, curNode.yPos - 1);
    //        OpenListAdd(curNode.xPos - 1, curNode.yPos);
    //    }
    //}

    ///// <summary>
    ///// ���� ������ ����� ���� ���¸���Ʈ �߰� �Լ�
    ///// </summary>
    ///// <param name="checkX"></param>
    ///// <param name="checkY"></param>
    //private void OpenListAdd(int checkX, int checkY)
    //{
    //    //���� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
    //    if (checkX >= startPos.x - updateDetectedRange && checkX <= startPos.x + updateDetectedRange
    //        && checkY >= startPos.y - updateDetectedRange && checkY <= startPos.y + updateDetectedRange
    //        && nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange].isWall == false
    //        && closedList.Contains(nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange]) == false)
    //    {
    //        // �̿���忡 �ֱ�
    //        Node neighborNode = nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange];
    //        int moveCost = 10;

    //        // �̵������ �̿���� g���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� g, h, ParentNode�� ���� �� ��������Ʈ�� �߰�
    //        if (moveCost < neighborNode.gCost || openList.Contains(neighborNode) == false)
    //        {
    //            neighborNode.gCost = moveCost;
    //            neighborNode.hCost = (Mathf.Abs(neighborNode.xPos - targetNode.xPos) + Mathf.Abs(neighborNode.yPos - targetNode.yPos)) * 10;
    //            neighborNode.parentNode = curNode;

    //            openList.Add(neighborNode);
    //        }
    //    }
    //}

    ///// <summary>
    ///// ������ �Լ�
    ///// </summary>
    //public IEnumerator Move()
    //{
    //    Vector2 curTargetPos;

    //    for (int nowIndex = 0; nowIndex < finalNodeList.Count; nowIndex++)
    //    {
    //        curTargetPos.x = finalNodeList[nowIndex].xPos;
    //        curTargetPos.y = finalNodeList[nowIndex].yPos;

    //        while (true)
    //        {
    //            if (transform.position.x == curTargetPos.x && transform.position.y == curTargetPos.y)
    //            {
    //                break;
    //            }

    //            transform.position = Vector3.MoveTowards(transform.position, curTargetPos, Time.deltaTime * enemyData.Speed);

    //            yield return null;
    //        }

    //        if (playerComponent.GetComponent<Player>().moveTargetPos != targetPos)
    //        {
    //            PathFinding();

    //            yield break;
    //        }
    //    }
    //}

    //void OnDrawGizmos()
    //{
    //    if (finalNodeList.Count != 0)
    //    {
    //        for (int i = 0; i < finalNodeList.Count - 1; i++)
    //        {
    //            Gizmos.DrawLine(new Vector2(finalNodeList[i].xPos, finalNodeList[i].yPos), new Vector2(finalNodeList[i + 1].xPos, finalNodeList[i + 1].yPos));
    //        }
    //    }
    //}
}
