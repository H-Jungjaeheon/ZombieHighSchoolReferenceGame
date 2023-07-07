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

    [Tooltip("다음 노드로의 방향")]
    public Direction direction;

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

    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 14;

    [Tooltip("현재 맵 크기 저장용 Vector")]
    private Vector2Int mapSize;

    [Tooltip("경로 시작 노드")]
    private Node startNode;

    [Tooltip("목적지 노드")]
    private Node targetNode;

    [SerializeField]
    [Tooltip("최종 경로 노드 리스트")]
    public List<Node> finalNodeList;

    [Tooltip("경로 탐색 가능한 노드 리스트")]
    private List<Node> openList = new List<Node>();

    [SerializeField]
    [Tooltip("현재 맵의 전체 노드 리스트")]
    private List<Node> curMapNodes = new List<Node>();
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
        playerComponent = detectedPlayerObj.GetComponent<Player>();

        MapSetting();
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

            currentNode = curMapNodes[(y * mapSize.x) + ++x]; // 다음 오른쪽 노드로 이동

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
    /// 현재 맵 노드 세팅 함수
    /// </summary>
    private void MapSetting()
    {
        Collider2D[] nodeCol; //벽 노드 판별용 콜라이더
        Vector2 curNodePos; //맵에서의 현재 노드 포지션
        Node curNode;
        bool isWall;

        curMapNodes.Clear(); //현재 맵 노드 리스트 초기화(비우기)

        for (int i = 0; i < mapSize.y; i++) //현재 맵의 세로 길이만큼 반복
        {
            for (int j = 0; j < mapSize.x; j++) //현재 맵의 가로 길이만큼 반복
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
                curMapNodes.Add(curNode); //현재 맵 노드 리스트에 추가 (콜라이더로 판별한 노드 태그가 벽이면, 벽 노드로 생성)
            }
        }

        StartCoroutine(PathFind());
    }

    /// <summary>
    /// 길 찾기 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator PathFind()
    {
        Node currentNode;

        openList.Clear(); //오픈 리스트 초기화
        finalNodeList.Clear(); //최종 경로 리스트 초기화

        startNode = curMapNodes[(int)transform.position.y * mapSize.x + (int)transform.position.x]; //시작 노드 세팅(현재 맵 노드 리스트에서 포지션을 이용한 번호로 구함)
        targetNode = curMapNodes[(int)playerComponent.moveTargetPos.y * mapSize.x + (int)playerComponent.moveTargetPos.x - 1]; //목표 노드 세팅(현재 맵 노드 리스트에서 포지션을 이용한 번호로 구함)

        startNode.gCost = 0; //시작 노드의 이동 거리 데이터 초기화
        startNode.hCost = Heuristic(startNode.nodePos, targetNode.nodePos); //시작 노드의 휴리스틱 데이터 연산 후 추가
        openList.Add(startNode); //오픈 리스트 시작 노드 추가

        while (openList.Count > 0) //경로 탐색 시작 (오픈리스트가 없을 때 까지 반복)
        {
            currentNode = openList[0]; //현재 노드를 오픈리스트의 0번째 값으로 넣기

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost) //전체 오픈리스트를 순회하여 현재 노드보다 휴리스틱 값이 적은 노드를 현재 노드로 선택
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode); //오픈 리스트 중 휴리스틱 값이 제일 적은 노드 빼기

            if (currentNode == targetNode) // 현재 노드가 끝 노드라면 부모 노드들 추적하여 움직임 시작
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

            currentNode.isWall = true;    // 한번 간 노드 
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
        _currentNode.parentNode = _parentNode;
        _currentNode.gCost = _parentNode.gCost + Heuristic(_parentNode.nodePos, _currentNode.nodePos);
        _currentNode.hCost = Heuristic(_currentNode.nodePos, targetNode.nodePos);

        openList.Add(_currentNode);
    }

    /// <summary>
    /// 움직임 함수
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
