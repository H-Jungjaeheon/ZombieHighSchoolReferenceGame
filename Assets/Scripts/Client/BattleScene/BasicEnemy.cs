using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Node
{
    /// <summary>
    /// 노드 생성자
    /// </summary>
    /// <param name="_isWall"> 현재 벽인지 판별 </param>
    /// <param name="x"> 노드의 x 포지션 </param>
    /// <param name="y"> 노드의 y 포지션 </param>
    public Node(bool _isWall, int x, int y)
    {
        isWall = _isWall;

        nodePos.x = x;
        nodePos.y = y;
    }

    [Tooltip("현재 노드가 벽인지 판별")]
    public bool isWall;

    [Tooltip("현재 노드의 부모 노드(이전 노드)")]
    public Node parentNode;

    [Tooltip("현재 노드의 포지션")]
    public Vector2Int nodePos;

    [Tooltip("시작으로부터 이동한 거리")]
    public int gCost;

    [Tooltip("장애물을 무시한 목표까지의 거리 (가로, 세로)")]
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
    [Tooltip("기본 적 정보(ScriptableObject)")]
    protected EnemyData enemyData;

    [Tooltip("체력")]
    private int hp;

    [SerializeField]
    [Tooltip("체력바 이미지 컴포넌트")]
    protected Image hpBarImg;

    [SerializeField]
    [Tooltip("SpriteRenderer 컴포넌트")]
    protected SpriteRenderer sr;

    [SerializeField]
    [Tooltip("현재 감지한 플레이어의 Player 컴포넌트")]
    protected Player playerComponent;

    [Tooltip("코루틴 딜레이 : 0.1초")]
    private WaitForSeconds zeroToOne = new WaitForSeconds(0.1f);

    [Tooltip("현재 실행중인 타격 효과 코루틴")]
    private IEnumerator hitEffectCoroutine;

    #region 이동 관련 요소들 모음
    [Header("이동 관련 요소들 모음")]

    const int MOVE_STRAIGHT_COST = 1;
    const int MOVE_DIAGONAL_COST = 2;

    [Tooltip("현재 맵 크기 저장용 Vector")]
    private Vector2Int mapSize;

    [Tooltip("현재 맵 기준 타일 위치 Vector (맨 왼쪽 아래 타일)")]
    private Vector2Int criteriaTilePos;

    [Tooltip("현재 노드가 벽인지 판별")]
    private bool isWall;

    [Tooltip("최종 경로 노드 리스트")]
    public List<Node> finalNodeList;

    [Tooltip("경로 시작 노드")]
    private Node startNode;

    [Tooltip("목적지 노드")]
    private Node targetNode;

    [Tooltip("경로 탐색 가능한 노드 리스트")]
    private List<Node> openList = new List<Node>();

    [Tooltip("현재 맵의 전체 노드 리스트")]
    private List<Node> curMapNodes = new List<Node>();

    [Tooltip("벽 태그")]
    private const string WALL = "Wall";
    #endregion

    private void Awake()
    {
        StartSetting();
    }

    /// <summary>
    /// 시작 세팅 함수
    /// </summary>
    private void StartSetting()
    {
        hp = enemyData.maxHp;

        mapSize = new Vector2Int(87, 66); //게임 매니저에서 현재 스테이지에 맞는 맵 크기 받아오는 것으로 수정
        criteriaTilePos = Vector2Int.zero;
    }

    /// <summary>
    /// 피격 함수
    /// </summary>
    /// <param name="damage"> 받은 데미지 </param>
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
    /// 타격 효과 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitEffect()
    {
        sr.color = Color.red;

        yield return zeroToOne;

        sr.color = Color.white;
    }

    /// <summary>
    /// 사망 함수
    /// </summary>
    private void Dead()
    {
        StageManager.instance.PlusScore(enemyData.score);

        Destroy(gameObject);
    }

    /// <summary>
    /// 플레이어 감지 시작
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

        //경로 탐색
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

            if (currentNode == targetNode) // 현재 노드가 끝 노드라면
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

            currentNode.isWall = true;    // 한번 간 노드 
        }
    }

    /// <summary>
    /// 오른쪽 노드 검사
    /// </summary>
    /// <param name="_startNode"> 검사 시작 노드 </param>
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

            if (CheakNextNode(x + 1, y) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(y * mapSize.x) + ++x];//GetGrid(++x, y); // 다음 오른쪽 노드로 이동

            if (currentNode.isWall) //다음 노드가 벽이면 종료
            {
                break;
            }

            if (currentNode == targetNode) //다음 노드가 목표 지점이면 종료
            {
                isFind = true;

                if (isMoveAble == false)
                {
                    AddOpenList(currentNode, startNode);
                }

                break;
            }

            cornerCheakNode = curMapNodes[(y + 1 * mapSize.x) + x];

            if (cornerCheakNode.isWall) // 코너 체크 : 위쪽이 막혀 있으면서 오른쪽 위는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 위가 막혀있지 않으면
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

            if (cornerCheakNode.isWall) // 코너 체크 : 아래가 막혀 있으면서 오른쪽 아래는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 아래가 막혀 있지 않으면
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
    /// 왼쪽 노드 검사
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

            if (CheakNextNode(x - 1, y) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(y * mapSize.x) + --x]; // 다음 노드로 이동

            if (currentNode.isWall) //다음 노드가 벽이면 종료
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
            
            if (cornerCheakNode.isWall) // 코너 체크 : 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 위가 막혀있지 않으면
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

            if (cornerCheakNode.isWall) // 코너 체크 : 아래가 막혀 있으면서 왼쪽 아래는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];
                
                if (cornerCheakNode.isWall == false) // 왼쪽 아래가 막혀 있지 않으면
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
    /// 위쪽 노드 검사
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

            if (CheakNextNode(x, y + 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(++y * mapSize.x) + x]; // 다음 노드로 이동

            if (currentNode.isWall) //다음 노드가 벽이면 종료
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

            if (cornerCheakNode.isWall) // 코너 체크 : 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우 
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 위가
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

            if (cornerCheakNode.isWall) // 코너 체크 : 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 위가 막혀 있지 않으면
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
    /// 아래쪽 노드 검사
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

            if (CheakNextNode(x, y - 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(--y * mapSize.x) + x]; // 다음 노드로 이동

            if (currentNode.isWall) //다음 노드가 벽이면 종료
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

            if (cornerCheakNode.isWall) // 코너 체크 : 오른쪽이 막혀 있으면서 오른쪽 아래는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 아래가 갈 수 있다면
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

            if (cornerCheakNode.isWall) // 코너 체크 : 왼쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 아래가 막혀 있지 않으면
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
    /// 오른쪽 위 노드 검사
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

            if (CheakNextNode(x + 1, y + 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(++y * mapSize.x) + ++x]; // 다음 노드로 이동

            if (currentNode.isWall) //다음 노드가 벽이면 종료
            {
                break;
            }

            if (currentNode == targetNode)
            {
                AddOpenList(currentNode, startNode);

                break;
            }

            cornerCheakNode = curMapNodes[(y * mapSize.x) + x - 1];

            if (cornerCheakNode.isWall) // 코너 탐색 : 왼쪽이 막혀 있으면서 왼쪽 위가 막혀있지 않은 경우
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 위가 안막힘
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // 코너 탐색 : 아래가 막혀 있으면서 오른쪽 아래가 막혀있지 않은 경우
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 아래가 막혀 있지 않으면
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
    /// 오른쪽 아래 노드 검사
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

            if (CheakNextNode(x + 1, y - 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(--y * mapSize.x) + ++x]; // 다음 노드로 이동

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

            if (cornerCheakNode.isWall) // 코너 탐색 : 왼쪽이 막혀있고 왼쪽 아래가 막혀 있지 않으면
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 아래가 안막힘
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // 코너 탐색 : 위쪽이 막혀있고 오른쪽 위가 막혀있지 않으면
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 위가 막혀있지 않으면
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
    /// 왼쪽 위 노드 검사
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

            if (CheakNextNode(x - 1, y + 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(++y * mapSize.x) + --x]; // 다음 노드로 이동

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

            if (cornerCheakNode.isWall) // 코너 탐색 : 오른쪽이 막혀있고 오른쪽 위가 막혀있지 않으면
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) // 오른쪽 위가 안막힘
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // 코너 탐색 : 아래가 막혀있고 왼쪽 아래가 막혀있지 않으면
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 아래가 안막힘
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
    /// 왼쪽 아래 노드 검사
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

            if (CheakNextNode(x - 1, y - 1) == false) // 탐색 가능 여부(현재 탐색하려는 범위가 맵 크기보다 크면 취소)
            {
                break;
            }

            currentNode = curMapNodes[(--y * mapSize.x) + --x]; // 다음 노드로 이동

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

            if (cornerCheakNode.isWall) // 코너 탐색 : 오른쪽이 막혀있고 오른쪽 아래가 막혀있지 않으면
            {
                cornerCheakNode = curMapNodes[(y - 1) * mapSize.x + x + 1];

                if (cornerCheakNode.isWall == false) //오른쪽 아래가 안막힘
                {
                    AddOpenList(currentNode, startNode);

                    break;
                }
            }

            cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x];

            if (cornerCheakNode.isWall) // 코너 탐색 : 위쪽이 막혀있고 왼쪽 위가 막혀있지 않으면
            {
                cornerCheakNode = curMapNodes[(y + 1) * mapSize.x + x - 1];

                if (cornerCheakNode.isWall == false) // 왼쪽 위가 안막힘
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
    /// 탐색 가능 여부 체크 함수
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
    /// 맵 세팅 함수
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

                // 시작, 목표 노트 판별 후 대입
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
    /// 휴리스틱 함수 (도착 노드까지 가장 짧은 경로의 값을 반환)
    /// </summary>
    /// <param name="_currPosition"> 현재 위치 노드 포지션 </param>
    /// <param name="_endPosition"> 목표 위치 노드 포지션 </param>
    /// <returns></returns>
    private int Heuristic(Vector2Int _currPosition, Vector2Int _endPosition)
    {
        int x = Mathf.Abs(_currPosition.x - _endPosition.x);
        int y = Mathf.Abs(_currPosition.y - _endPosition.y);
        int reming = Mathf.Abs(x - y);

        return MOVE_DIAGONAL_COST * Mathf.Min(x, y) + MOVE_STRAIGHT_COST * reming;
    }

    /// <summary>
    /// 현재 노드 오픈리스트에 넣는 함수
    /// </summary>
    /// <param name="_currentNode"> 오픈리스트에 넣을 노드 </param>
    /// <param name="_parentNode"> 오픈리스트 노드의 부모 노드 </param>
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
    /// 움직임 함수
    /// </summary>
    public IEnumerator Move()
    {
        Vector2 curTargetPos;

        print("실행");

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
