//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//[System.Serializable]
//public class Node
//{
//    /// <summary>
//    /// 노드 생성자
//    /// </summary>
//    /// <param name="_isWall"> 현재 벽인지 판별 </param>
//    /// <param name="x"> 노드의 x 포지션 </param>
//    /// <param name="y"> 노드의 y 포지션 </param>
//    public Node(bool _isWall, int x, int y)
//    {
//        isWall = _isWall;

//        nodePos.x = x;
//        nodePos.y = y;
//    }

//    [Tooltip("현재 노드가 벽인지 판별")]
//    public bool isWall;

//    [Tooltip("현재 노드의 부모 노드(이전 노드)")]
//    public Node parentNode;

//    [Tooltip("현재 노드의 포지션")]
//    public Vector2Int nodePos;

//    [Tooltip("시작으로부터 이동한 거리")]
//    public int gCost;

//    [Tooltip("장애물을 무시한 목표까지의 거리 (가로, 세로)")]
//    public int hCost;

//    public int fCost //g + h
//    {
//        get
//        {
//            return gCost + hCost;
//        }
//    }
//}
//public class BasicEnemy : MonoBehaviour
//{
//    [SerializeField]
//    [Tooltip("기본 적 정보(ScriptableObject)")]
//    protected EnemyData enemyData;

//    [Tooltip("체력")]
//    private int hp;

//    [SerializeField]
//    [Tooltip("체력바 이미지 컴포넌트")]
//    protected Image hpBarImg;

//    [SerializeField]
//    [Tooltip("SpriteRenderer 컴포넌트")]
//    protected SpriteRenderer sr;

//    [SerializeField]
//    [Tooltip("현재 감지한 플레이어의 Player 컴포넌트")]
//    protected Player playerComponent;

//    [Tooltip("코루틴 딜레이 : 0.1초")]
//    private WaitForSeconds zeroToOne = new WaitForSeconds(0.1f);

//    [Tooltip("현재 실행중인 타격 효과 코루틴")]
//    private IEnumerator hitEffectCoroutine;

//    #region 이동 관련 요소들 모음
//    [Header("이동 관련 요소들 모음")]

//    const int MOVE_STRAIGHT_COST = 10;
//    const int MOVE_DIAGONAL_COST = 14;

//    [Tooltip("현재 맵 크기 저장용 Vector")]
//    private Vector2Int mapSize;

//    [Tooltip("현재 맵 기준 타일 위치 Vector (맨 왼쪽 아래 타일)")]
//    private Vector2Int criteriaTilePos;

//    //[Tooltip("추격 후 플레이어 감지 범위")]
//    //private const int updateDetectedRange = 50;

//    [Tooltip("현재 노드가 벽인지 판별")]
//    private bool isWall;

//    [Tooltip("경로 탐색 시 시작 포지션")]
//    private Vector3 startPos;

//    [Tooltip("경로 탐색 시 목표 포지션")]
//    private Vector3 targetPos;

//    [Tooltip("최종 경로 노드 리스트")]
//    public List<Node> finalNodeList;

//    //[Tooltip("전체 경로 범위 노드 배열")]
//    //private Node[,] nodeArray = new Node[updateDetectedRange * 2 + 1, updateDetectedRange * 2 + 1];

//    [Tooltip("경로 시작 노드")]
//    private Node startNode;

//    [Tooltip("목적지 노드")]
//    private Node targetNode;

//    //[Tooltip("현재 경로 노드")]
//    //private Node curNode;

//    [Tooltip("경로 탐색 가능한 노드 리스트")]
//    private List<Node> openList = new List<Node>();

//    [Tooltip("현재 맵의 전체 노드 리스트")]
//    private List<Node> curMapNodes = new List<Node>();

//    //[Tooltip("경로 탐색 완료한 노드 리스트")]
//    //private List<Node> closedList = new List<Node>();

//    [Tooltip("0.05초 딜레이")]
//    private WaitForSeconds pointZeroFiveSec = new WaitForSeconds(0.05f);

//    [Tooltip("벽 태그")]
//    private const string WALL = "Wall";
//    #endregion

//    private void Awake()
//    {
//        StartSetting();
//    }

//    /// <summary>
//    /// 시작 세팅 함수
//    /// </summary>
//    private void StartSetting()
//    {
//        hp = enemyData.maxHp;

//        mapSize = new Vector2Int(87, 66); //게임 매니저에서 현재 스테이지에 맞는 맵 크기 받아오는 것으로 수정
//        criteriaTilePos = new Vector2Int(-35, -37);
//    }

//    /// <summary>
//    /// 피격 함수
//    /// </summary>
//    /// <param name="damage"> 받은 데미지 </param>
//    public void Hit(int damage)
//    {
//        hp -= damage;

//        hpBarImg.fillAmount = (float)hp / enemyData.maxHp;

//        if (hp <= 0)
//        {
//            Dead();
//        }
//        else 
//        {
//            if (hitEffectCoroutine != null)
//            {
//                StopCoroutine(hitEffectCoroutine);    
//            }

//            hitEffectCoroutine = HitEffect();
//            StartCoroutine(hitEffectCoroutine);
//        }
//    }

//    /// <summary>
//    /// 타격 효과 함수
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator HitEffect()
//    {
//        sr.color = Color.red;

//        yield return zeroToOne;

//        sr.color = Color.white;
//    }

//    /// <summary>
//    /// 사망 함수
//    /// </summary>
//    private void Dead()
//    {
//        StageManager.instance.PlusScore(enemyData.score);

//        Destroy(gameObject);
//    }

//    /// <summary>
//    /// 플레이어 감지 시작
//    /// </summary>
//    public virtual void DetectedPlayer(GameObject detectedPlayerObj)
//    {
//        if (playerComponent != null)
//        {
//            return;
//        }

//        playerComponent = detectedPlayerObj.GetComponent<Player>();

//        StartCoroutine(PathFind());
//    }

//    private IEnumerator PathFind()
//    {
//        openList.Clear();
//        curMapNodes.Clear();
//        MapSetting(); //초기화

//        startNode.gCost = 0;
//        startNode.hCost = Heuristic(startNode.nodePos, targetNode.nodePos);
//        openList.Add(startNode);

//        Node currentNode = openList[0];

//        //경로 탐색
//        while (openList.Count > 0)
//        {
//            for (int i = 1; i < openList.Count; i++)
//            {
//                if (openList[i].fCost < currentNode.fCost)
//                {
//                    currentNode = openList[i];
//                }
//            }

//            openList.Remove(currentNode);

//            if (currentNode == targetNode) // 현재 노드가 끝 노드라면
//            {
//                ShowPath(targetNode);

//                yield break;
//            }

//            if (currentNode.isWall == false)
//            {
//                Right(currentNode);
//                yield return pointZeroFiveSec;

//                Down(currentNode);
//                yield return pointZeroFiveSec;

//                Left(currentNode);
//                yield return pointZeroFiveSec;

//                Up(currentNode);
//                yield return pointZeroFiveSec;

//                RightUp(currentNode);
//                yield return pointZeroFiveSec;

//                RightDown(currentNode);
//                yield return pointZeroFiveSec;

//                LeftUP(currentNode);
//                yield return pointZeroFiveSec;

//                LeftDown(currentNode);
//                yield return pointZeroFiveSec;
//            }

//            currentNode.isWall = true;    // 한번 간 노드 
//        }
//    }

//    /// <summary>
//    /// 오른쪽 노드 검사
//    /// </summary>
//    /// <param name="_startNode"> 검사 시작 노드 </param>
//    /// <returns></returns>
//    private bool Right(Node _startNode, bool isMoveable = false)
//    {
//        int x = _startNode.nodePos.x;
//        int y = _startNode.nodePos.y;

//        Node startNode = _startNode;
//        Node currentNode = _startNode;

//        bool isFind = false;

//        while (currentNode.isWall)
//        {
//            currentNode.isWall = isMoveable;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크
//            if (CheakWallNode(x + 1, y) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(++x, y); // 다음 오른쪽 노드로 이동

//            if (currentNode.isWall == false)
//            {
//                break;
//            }

//            if (currentNode == targetNode)
//            {
//                isFind = true;

//                if (isMoveable == false)
//                {
//                    AddOpenList(currentNode, startNode);
//                }

//                break;
//            }
//            #endregion

//            #region 위쪽이 막혀 있으면서 오른쪽 위는 뚫려있는 경우

//            if (y + 1 < playerComponent.moveTargetPos.y && x + 1 < playerComponent.moveTargetPos.x)
//            {
//                if (GetGrid(x, y + 1).isWall) // 위쪽이 벽이면
//                {
//                    if (GetGrid(x + 1, y + 1).isWall == true) // 오른쪽 위가 막혀있지 않으면
//                    {
//                        if (isMoveable == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        isFind = true;

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 오른쪽 아래는 뚫려있는 경우

//            if (y > playerComponent.moveTargetPos.y && x + 1 < playerComponent.moveTargetPos.x)
//            {
//                if (GetGrid(x, y - 1).isWall == false) // 아래쪽이 벽이고
//                {
//                    if (GetGrid(x + 1, y - 1).isWall == true) // 오른쪽 아래가 막혀 있지 않으면
//                    {
//                        if (isMoveable == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        isFind = true;

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        startNode.isWall = true;
//        return isFind;
//    }

//    #region  왼쪽 검사

//    private bool Left(Node _startNode, bool _isMovealbe = false)
//    {
//        int x = _startNode.nodePos.x;
//        int y = _startNode.nodePos.y;

//        Node startNode = _startNode;
//        Node currentNode = startNode;

//        bool isFind = false;

//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = _isMovealbe;


//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x - 1, y) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(--x, y); // 다음 왼쪽 노드로 이동
//            if (currentNode.moveable == false)
//            {
//                break;
//            }
//            currentNode.tile.SetColor(Color.gray);

//            if (currentNode == this.endNode)
//            {
//                isFind = true;

//                if (_isMovealbe == false)
//                {
//                    AddOpenList(currentNode, startNode);
//                }
//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y + 1 < mapSize.y && x - 1 >= 0)
//            {
//                if (GetGrid(x, y + 1).moveable == false) // 위쪽이 벽이면
//                {
//                    if (GetGrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 막혀있지 않으면
//                    {
//                        isFind = true;
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (GetGrid(x, y - 1).moveable == false) // 아래쪽이 벽이고
//                {
//                    if (GetGrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        isFind = true;
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        startNode.moveable = true;
//        return isFind;
//    }

//    #endregion


//    #region  위쪽 검사

//    private bool Up(Node _satrtNode, bool _isMovealbe = false)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;

//        bool isFind = false;

//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = _isMovealbe;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(x, ++y); // 다음 왼쪽 노드로 이동
//            if (currentNode.moveable == false)
//            {
//                break;
//            }
//            currentNode.tile.SetColor(Color.gray);

//            if (currentNode == this.endNode)
//            {
//                isFind = true;
//                if (_isMovealbe == false)
//                {
//                    AddOpenList(currentNode, startNode);
//                }

//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x + 1, y).moveable == false) // 오른쪽이 벽이면
//                {
//                    if (GetGrid(x + 1, y + 1).moveable == true) // 오른쪽 위가
//                    {
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        isFind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y + 1 < mapSize.y && x > 0)
//            {
//                if (GetGrid(x - 1, y).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (GetGrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 막혀 있지 않으면
//                    {
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        isFind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtNode.moveable = true;
//        return isFind;
//    }

//    #endregion


//    #region  아래쪽 검사

//    private bool Down(Node _satrtNode, bool _isMovealbe = false)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;

//        bool isFind = false;

//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = _isMovealbe;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(x, --y); // 다음 왼쪽 노드로 이동
//            if (currentNode.moveable == false)
//            {
//                break;
//            }
//            currentNode.tile.SetColor(Color.gray);

//            if (currentNode == this.endNode)
//            {
//                isFind = true;
//                if (_isMovealbe == false)
//                {
//                    AddOpenList(currentNode, startNode);
//                }

//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y > 0 && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x + 1, y).moveable == false) // 오른쪽이 벽이면
//                {
//                    if (GetGrid(x + 1, y - 1).moveable == true) // 오른쪽 아래가 갈 수 있다면
//                    {
//                        isFind = true;
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (GetGrid(x - 1, y).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (GetGrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        isFind = true;
//                        if (_isMovealbe == false)
//                        {
//                            AddOpenList(currentNode, startNode);
//                        }

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtNode.moveable = true;
//        return isFind;
//    }

//    #endregion


//    #region  오른쪽 위 대각선 검사

//    private void RightUp(Node _satrtNode)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;


//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x + 1, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }



//            currentNode = GetGrid(++x, ++y); // 다음 왼쪽 노드로 이동

//            if (currentNode.moveable == false)
//            {
//                break;
//            }

//            if (currentNode == this.endNode)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            currentNode.tile.SetColor(Color.gray);

//            #endregion

//            #region 왼쪽이 막혀 있으면서 왼쪽 위가 막혀있지 않은 경우

//            if (y + 1 < mapSize.y && x > 0)
//            {
//                if (GetGrid(x - 1, y).moveable == false) // 왼쪽이 막힘
//                {
//                    if (GetGrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀 있으면서 오른쪽 아래가 안막혔으면

//            if (y > 0 && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x, y - 1).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (GetGrid(x + 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (Right(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            if (Up(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }
//        }

//        _satrtNode.moveable = true;
//    }

//    #endregion


//    #region  오른쪽 아래 대각선 검사

//    private void RightDown(Node _satrtNode)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;

//        while (currentNode.moveable == true)
//        {

//            currentNode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x + 1, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(++x, --y); // 다음 왼쪽 노드로 이동

//            if (currentNode.moveable == false)
//            {
//                break;
//            }

//            if (currentNode == this.endNode)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            currentNode.tile.SetColor(Color.gray);

//            #endregion

//            #region 왼쪽이 막혀있고 왼쪽 아래가 막혀 있지 않으면

//            if (y > 0 && x > 0)
//            {
//                if (GetGrid(x - 1, y).moveable == false) // 왼쪽이 막힘
//                {
//                    if (GetGrid(x - 1, y - 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x, y + 1).moveable == false) // 위쪽이 벽이고
//                {
//                    if (GetGrid(x + 1, y + 1).moveable == true) // 오른쪽 위가 막혀있지 않으면
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (Right(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            if (Down(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }
//        }

//        _satrtNode.moveable = true;
//    }

//    #endregion


//    #region  왼쪽 위 대각선 검사

//    private void LeftUP(Node _satrtNode)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;


//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x - 1, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(--x, ++y); // 다음 왼쪽 노드로 이동

//            if (currentNode.moveable == false)
//            {
//                break;
//            }

//            if (currentNode == this.endNode)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            currentNode.tile.SetColor(Color.gray);

//            #endregion

//            #region 오른쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y + 1 < mapSize.y && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x + 1, y).moveable == false) // 오른쪽이 막힘
//                {
//                    if (GetGrid(x + 1, y + 1).moveable == true) // 오른쪽 위가 안막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀있고 왼쪽 아래가 막혀있지 않으면

//            if (y > 0 && x > 0)
//            {
//                if (GetGrid(x, y - 1).moveable == false) // 아래가 벽
//                {
//                    if (GetGrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 안막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (Left(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            if (Up(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }
//        }

//        _satrtNode.moveable = true;
//    }

//    #endregion

//    #region  왼쪽 아래 대각선 검사

//    private void LeftDown(Node _satrtNode)
//    {
//        int x = _satrtNode.position.x;
//        int y = _satrtNode.position.y;

//        Node startNode = _satrtNode;
//        Node currentNode = startNode;


//        while (currentNode.moveable == true)
//        {
//            currentNode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (CheakWallNode(x - 1, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentNode = GetGrid(--x, --y); // 다음 왼쪽 노드로 이동

//            if (currentNode.moveable == false)
//            {
//                break;
//            }

//            if (currentNode == this.endNode)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            currentNode.tile.SetColor(Color.gray);

//            #endregion

//            #region 오른쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y > 0 && x + 1 < mapSize.x)
//            {
//                if (GetGrid(x + 1, y).moveable == false) // 오른쪽이 막힘
//                {
//                    if (GetGrid(x + 1, y - 1).moveable == true) //오른쪽 아래가 안 막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀있고 왼쪽 아래가 막혀있지 않으면

//            if (y + 1 < mapSize.y && x < 0)
//            {
//                if (GetGrid(x, y + 1).moveable == false) // 위가 막힘
//                {
//                    if (GetGrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        AddOpenList(currentNode, startNode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (Left(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }

//            if (Down(currentNode, true) == true)
//            {
//                AddOpenList(currentNode, startNode);

//                break;
//            }
//        }

//        _satrtNode.moveable = true;
//    }

//    #endregion

//    /// <summary>
//    /// 탐색 가능 여부 체크 함수
//    /// </summary>
//    /// <param name="nodeXPos"></param>
//    /// <param name="nodeYPos"></param>
//    /// <returns></returns>
//    private bool CheakWallNode(int nodeXPos, int nodeYPos)
//    {
//        if (nodeXPos < criteriaTilePos.x || nodeYPos < criteriaTilePos.y || nodeXPos >= criteriaTilePos.x + mapSize.x || nodeYPos >= criteriaTilePos.y + mapSize.y)
//        {
//            return false;
//        }

//        return true;
//    }

//    #region 리스트 안에 있는 노드를 X와 y좌표 값으로 반환

//    private Node GetGrid(int _x, int _y)
//    {
//        return curMapNodes[_x + _y * mapSize.x];
//    }

//    #endregion

//    /// <summary>
//    /// 맵 세팅 함수
//    /// </summary>
//    private void MapSetting()
//    {
//        Node curMapNode;

//        for (int i = 0; i < mapSize.y; i++)
//        {
//            for (int j = 0; j < mapSize.x; j++)
//            {
//                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + j, startPos.y + i), 0.2f))
//                {
//                    if (!collider.gameObject.CompareTag(WALL))
//                    {
//                        isWall = true;
//                    }
//                }

//                curMapNode = new Node(isWall, criteriaTilePos.x + j, criteriaTilePos.y + i);
//                curMapNodes.Add(curMapNode);

//                // 시작, 목표 노트 판별 후 대입
//                if (criteriaTilePos.x + j == transform.position.x && criteriaTilePos.y + i == transform.position.y)
//                {
//                    startNode = curMapNode;
//                }
//                else if (criteriaTilePos.x + j == playerComponent.moveTargetPos.x && criteriaTilePos.y + i == playerComponent.moveTargetPos.y)
//                {
//                    targetNode = curMapNode;
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// 휴리스틱 함수 (도착 노드까지 가장 짧은 경로의 값을 반환)
//    /// </summary>
//    /// <param name="_currPosition"> 현재 위치 노드 포지션 </param>
//    /// <param name="_endPosition"> 목표 위치 노드 포지션 </param>
//    /// <returns></returns>
//    private int Heuristic(Vector2Int _currPosition, Vector2Int _endPosition)
//    {
//        int x = Mathf.Abs(_currPosition.x - _endPosition.x);
//        int y = Mathf.Abs(_currPosition.y - _endPosition.y);
//        int reming = Mathf.Abs(x - y);

//        return MOVE_DIAGONAL_COST * Mathf.Min(x, y) + MOVE_STRAIGHT_COST * reming;
//    }

//    /// <summary>
//    /// 경로 표시 함수(라인 그림)
//    /// </summary>
//    /// <param name="_node"> 경로 내의 현재 노드 </param>
//    private void ShowPath(Node _node)
//    {
//        if (_node != null)
//        {
//            if (_node.parentNode != null)
//            {
//                Vector3 start = Vector3.zero;
//                start.x = _node.nodePos.x;
//                start.y = _node.nodePos.y;

//                Vector3 end = Vector3.zero;
//                end.x = _node.parentNode.nodePos.x;
//                end.y = _node.parentNode.nodePos.y;

//                Debug.DrawLine(start, end, Color.yellow, 5);
//            }

//            ShowPath(_node.parentNode);
//        }
//    }

//    private void AddOpenList(Node _currentNode, Node _parentNode)
//    {
//        //int nextCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
//        //if (nextCost < _currentNode.gCost)
//        // {
//        _currentNode.parentNode = _parentNode;
//        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
//        _currentNode.hCost = Heuristic(_currentNode.nodePos, targetNode.nodePos);
//        openList.Add(_currentNode);
//        // }
//    }

//    ///// <summary>
//    ///// 경로 탐색 함수
//    ///// </summary>
//    //public void PathFinding()
//    //{
//    //    startPos = transform.position;
//    //    targetPos = playerComponent.moveTargetPos;

//    //    for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //플레이어 추격 범위만큼 노드 세팅
//    //    {
//    //        for (int j = -updateDetectedRange; j <= updateDetectedRange; j++)
//    //        {
//    //            isWall = false;

//    //            foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + i, startPos.y + j), 0.2f))
//    //            {
//    //                if (collider.gameObject.CompareTag(WALL))
//    //                {
//    //                    isWall = true;
//    //                }
//    //            }

//    //            nodeArray[i + updateDetectedRange, j + updateDetectedRange] = new Node(isWall, (int)startPos.x + i, (int)startPos.y + j);
//    //        }
//    //    }

//    //    startNode = nodeArray[updateDetectedRange, updateDetectedRange];

//    //    targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + updateDetectedRange, (int)targetPos.y - (int)startPos.y + updateDetectedRange];

//    //    openList.Clear();

//    //    openList.Add(startNode);

//    //    closedList.Clear();

//    //    finalNodeList.Clear();

//    //    while (openList.Count > 0)
//    //    {
//    //        // 오픈리스트 중 가장 f가 작은 것, f가 같다면 h가 작은 것, h도 같다면 0번째 것을 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
//    //        curNode = openList[0];

//    //        openList.Remove(curNode);
//    //        closedList.Add(curNode);

//    //        for (int i = 0; i < openList.Count; i++)
//    //        {
//    //            if (openList[i].fCost <= curNode.fCost && openList[i].hCost < curNode.hCost)
//    //            {
//    //                curNode = openList[i];
//    //            }
//    //        }

//    //        // 마지막
//    //        if (curNode == targetNode)
//    //        {
//    //            Node TargetCurNode = targetNode;

//    //            while (TargetCurNode != startNode)
//    //            {
//    //                finalNodeList.Add(TargetCurNode);
//    //                TargetCurNode = TargetCurNode.parentNode;
//    //            }

//    //            finalNodeList.Add(startNode);
//    //            finalNodeList.Reverse();

//    //            StartCoroutine(Move());

//    //            return;
//    //        }

//    //        OpenListAdd(curNode.xPos, curNode.yPos + 1);
//    //        OpenListAdd(curNode.xPos + 1, curNode.yPos);
//    //        OpenListAdd(curNode.xPos, curNode.yPos - 1);
//    //        OpenListAdd(curNode.xPos - 1, curNode.yPos);
//    //    }
//    //}

//    ///// <summary>
//    ///// 진행 가능한 경로의 노드들 오픈리스트 추가 함수
//    ///// </summary>
//    ///// <param name="checkX"></param>
//    ///// <param name="checkY"></param>
//    //private void OpenListAdd(int checkX, int checkY)
//    //{
//    //    //감지 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
//    //    if (checkX >= startPos.x - updateDetectedRange && checkX <= startPos.x + updateDetectedRange
//    //        && checkY >= startPos.y - updateDetectedRange && checkY <= startPos.y + updateDetectedRange
//    //        && nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange].isWall == false
//    //        && closedList.Contains(nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange]) == false)
//    //    {
//    //        // 이웃노드에 넣기
//    //        Node neighborNode = nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange];
//    //        int moveCost = 10;

//    //        // 이동비용이 이웃노드 g보다 작거나 또는 열린리스트에 이웃노드가 없다면 g, h, ParentNode를 설정 후 열린리스트에 추가
//    //        if (moveCost < neighborNode.gCost || openList.Contains(neighborNode) == false)
//    //        {
//    //            neighborNode.gCost = moveCost;
//    //            neighborNode.hCost = (Mathf.Abs(neighborNode.xPos - targetNode.xPos) + Mathf.Abs(neighborNode.yPos - targetNode.yPos)) * 10;
//    //            neighborNode.parentNode = curNode;

//    //            openList.Add(neighborNode);
//    //        }
//    //    }
//    //}

//    ///// <summary>
//    ///// 움직임 함수
//    ///// </summary>
//    //public IEnumerator Move()
//    //{
//    //    Vector2 curTargetPos;

//    //    for (int nowIndex = 0; nowIndex < finalNodeList.Count; nowIndex++)
//    //    {
//    //        curTargetPos.x = finalNodeList[nowIndex].xPos;
//    //        curTargetPos.y = finalNodeList[nowIndex].yPos;

//    //        while (true)
//    //        {
//    //            if (transform.position.x == curTargetPos.x && transform.position.y == curTargetPos.y)
//    //            {
//    //                break;
//    //            }

//    //            transform.position = Vector3.MoveTowards(transform.position, curTargetPos, Time.deltaTime * enemyData.Speed);

//    //            yield return null;
//    //        }

//    //        if (playerComponent.GetComponent<Player>().moveTargetPos != targetPos)
//    //        {
//    //            PathFinding();

//    //            yield break;
//    //        }
//    //    }
//    //}

//    //void OnDrawGizmos()
//    //{
//    //    if (finalNodeList.Count != 0)
//    //    {
//    //        for (int i = 0; i < finalNodeList.Count - 1; i++)
//    //        {
//    //            Gizmos.DrawLine(new Vector2(finalNodeList[i].xPos, finalNodeList[i].yPos), new Vector2(finalNodeList[i + 1].xPos, finalNodeList[i + 1].yPos));
//    //        }
//    //    }
//    //}
//}
