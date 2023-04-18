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

        xPos = x;

        yPos = y;
    }

    [Tooltip("현재 노드가 벽인지 판별")]
    public bool isWall;

    [Tooltip("현재 노드의 부모 노드(이전 노드)")]
    public Node ParentNode;

    [Tooltip("현재 노드의 X 포지션")]
    public int xPos;

    [Tooltip("현재 노드의 Y 포지션")]
    public int yPos;

    [Tooltip("시작으로부터 이동한 거리")]
    public int g;

    [Tooltip("장애물을 무시한 목표까지의 거리 (가로, 세로)")]
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
    [Tooltip("현재 감지한 플레이어 오브젝트")]
    protected GameObject detectedPObj;

    [Tooltip("코루틴 딜레이 : 0.1초")]
    private WaitForSeconds zeroToOne = new WaitForSeconds(0.1f);

    [Tooltip("현재 실행중인 타격 효과 코루틴")]
    private IEnumerator hitEffectCoroutine;

    #region 이동 관련 요소들 모음
    [Header("이동 관련 요소들 모음")]

    [Tooltip("추격 후 플레이어 감지 범위")]
    private const int updateDetectedRange = 50;

    [Tooltip("현재 노드가 벽인지 판별")]
    private bool isWall;

    [Tooltip("경로 탐색 시 시작 포지션")]
    private Vector2 startPos;

    [Tooltip("경로 탐색 시 목표 포지션")]
    private Vector2 targetPos;

    [Tooltip("최종 경로 노드 리스트")]
    public List<Node> finalNodeList;

    [Tooltip("전체 경로 범위 노드 배열")]
    private Node[,] nodeArray = new Node[updateDetectedRange * 2 + 1, updateDetectedRange * 2 + 1];

    [Tooltip("경로 시작 노드")]
    private Node startNode;

    [Tooltip("목적지 노드")]
    private Node targetNode;

    [Tooltip("현재 경로 노드")]
    private Node curNode;

    [Tooltip("경로 탐색 가능한 노드 리스트")]
    private List<Node> openList = new List<Node>();

    [Tooltip("경로 탐색 완료한 노드 리스트")]
    private List<Node> closedList = new List<Node>();

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
        if (detectedPObj != null)
        {
            return;
        }

        detectedPObj = detectedPlayerObj;

        PathFinding();
    }

    /// <summary>
    /// 경로 탐색 함수
    /// </summary>
    public void PathFinding()
    {
        startPos = new Vector2(transform.position.x, transform.position.y);
        targetPos = detectedPObj.GetComponent<Player>().moveTargetPos;
        
        for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //플레이어 추격 범위만큼 노드 세팅
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
            // 오픈리스트 중 가장 f가 작은 것, f가 같다면 h가 작은 것, h도 같다면 0번째 것을 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
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

            // 마지막
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
    /// 진행 가능한 경로의 노드들 오픈리스트 추가 함수
    /// </summary>
    /// <param name="checkX"></param>
    /// <param name="checkY"></param>
    private void OpenListAdd(int checkX, int checkY)
    {
        //감지 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= startPos.x - updateDetectedRange && checkX <= startPos.x + updateDetectedRange
            && checkY >= startPos.y - updateDetectedRange && checkY <= startPos.y + updateDetectedRange
            && nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange].isWall == false
            && closedList.Contains(nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange]) == false)
        {
            // 이웃노드에 넣기
            Node neighborNode = nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange];
            int moveCost = 10;

            // 이동비용이 이웃노드 g보다 작거나 또는 열린리스트에 이웃노드가 없다면 g, h, ParentNode를 설정 후 열린리스트에 추가
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
    /// 움직임 함수
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
