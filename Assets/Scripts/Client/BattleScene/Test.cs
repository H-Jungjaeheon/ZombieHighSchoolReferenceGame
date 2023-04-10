using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y)
    {
        isWall = _isWall;

        x = _x;

        y = _y;
    }

    public bool isWall;
    public Node ParentNode;

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    [Tooltip("현재 노드의 X 포지션")]
    public int x;

    [Tooltip("현재 노드의 Y 포지션")]
    public int y;

    public int g;

    public int h;

    public int f
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

                nodeArray[i + 50, j + 50] = new Node(isWall, (int)startPos.x + i, (int)startPos.y + j);
            }
        }

        // 시작과 끝 노드 재설정, 오픈 리스트, 클로즈 리스트, 최종 경로 리스트 초기화
        startNode = nodeArray[50, 50];

        targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + 50, (int)targetPos.y - (int)startPos.y + 50];

        closedList.Clear();

        finalNodeList.Clear();

        openList.Clear();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            curNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f <= curNode.f && openList[i].h < curNode.h)
                {
                    curNode = openList[i];
                }
            }

            openList.Remove(curNode);
            closedList.Add(curNode);

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
                    print(i + "번째는 " + finalNodeList[i].x + ", " + finalNodeList[i].y);
                }

                return;
            }

            // ↑ → ↓ ←
            OpenListAdd(curNode.x, curNode.y + 1);
            OpenListAdd(curNode.x + 1, curNode.y);
            OpenListAdd(curNode.x, curNode.y - 1);
            OpenListAdd(curNode.x - 1, curNode.y);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= -updateDetectedRange && checkX < updateDetectedRange + 1 && checkY >= -updateDetectedRange && checkY < updateDetectedRange + 1 && !nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange].isWall && !closedList.Contains(nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange]))
        {
            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange];
            int MoveCost = 10;

            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.g || !openList.Contains(NeighborNode))
            {
                NeighborNode.g = MoveCost;
                NeighborNode.h = (Mathf.Abs(NeighborNode.x - targetNode.x) + Mathf.Abs(NeighborNode.y - targetNode.y)) * 10;
                NeighborNode.ParentNode = curNode;

                openList.Add(NeighborNode);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (finalNodeList.Count != 0)
        {
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector2(finalNodeList[i].x, finalNodeList[i].y), new Vector2(finalNodeList[i + 1].x, finalNodeList[i + 1].y));
            }
        }
    }
}