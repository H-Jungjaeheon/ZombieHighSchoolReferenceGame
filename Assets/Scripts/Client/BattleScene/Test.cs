using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

public class Test : MonoBehaviour
{
    [SerializeField]
    [Tooltip("추격 전 플레이어 감지 범위")]
    private int sensingRange;

    [Tooltip("현재 감지한 플레이어 오브젝트")]
    private GameObject detectedPObj;

    [Tooltip("추격 후 플레이어 감지 범위")]
    private const int updateDetectedRange = 50;

    [Tooltip("현재 노드가 벽인지 판별")]
    private bool isWall;

    [Tooltip("경로 탐색 시 시작 포지션")]
    private Vector2 startPos;

    [Tooltip("경로 탐색 시 목표 포지션")]
    public Vector2 targetPos;

    public List<Node> finalNodeList;

    Node[,] nodeArray = new Node[101, 101];

    Node startNode, targetNode, curNode;

    List<Node> openList = new List<Node>();

    List<Node> closedList = new List<Node>();

    private void Start()
    {
        PathFinding();
    }

    /// <summary>
    /// 경로 탐색 함수
    /// </summary>
    public void PathFinding()
    {
        startPos = new Vector2(transform.position.x, transform.position.y);

        for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //플레이어 추격 범위만큼 노드 세팅
        {
            for (int j = -updateDetectedRange; j <= updateDetectedRange; j++)
            {
                isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(startPos.x + i, startPos.y + j), 0.40f))
                {
                    if (collider.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                nodeArray[i + updateDetectedRange, j + updateDetectedRange] = new Node(isWall, (int)startPos.x + i, (int)startPos.y + j);
            }
        }

        startNode = nodeArray[updateDetectedRange, updateDetectedRange];

        targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + updateDetectedRange, (int)targetPos.y - (int)startPos.y + updateDetectedRange];

        closedList.Clear();

        finalNodeList.Clear();

        openList.Clear();

        openList.Add(startNode);

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

                for (int i = 0; i < finalNodeList.Count; i++)
                {
                    print(i + "번째는 " + finalNodeList[i].xPos + ", " + finalNodeList[i].yPos);
                }

                return;
            }

            // ↑ → ↓ ←
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