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

    // G : �������κ��� �̵��ߴ� �Ÿ�, H : |����|+|����| ��ֹ� �����Ͽ� ��ǥ������ �Ÿ�, F : G + H
    [Tooltip("���� ����� X ������")]
    public int x;

    [Tooltip("���� ����� Y ������")]
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
    [Tooltip("�߰� �� �÷��̾� ���� ����")]
    private int sensingRange;

    [Tooltip("���� ������ �÷��̾� ������Ʈ")]
    private GameObject detectedPObj;

    [Tooltip("�߰� �� �÷��̾� ���� ����")]
    private const int updateDetectedRange = 50;

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    private bool isWall;

    [Tooltip("��� Ž�� �� ���� ������")]
    private Vector2 startPos;

    [Tooltip("��� Ž�� �� ��ǥ ������")]
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
    /// ��� Ž�� �Լ�
    /// </summary>
    public void PathFinding()
    {
        startPos = new Vector2(transform.position.x, transform.position.y);

        for (int i = -updateDetectedRange; i <= updateDetectedRange; i++) //�÷��̾� �߰� ������ŭ ��� ����
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

        // ���۰� �� ��� �缳��, ���� ����Ʈ, Ŭ���� ����Ʈ, ���� ��� ����Ʈ �ʱ�ȭ
        startNode = nodeArray[50, 50];

        targetNode = nodeArray[(int)targetPos.x - (int)startPos.x + 50, (int)targetPos.y - (int)startPos.y + 50];

        closedList.Clear();

        finalNodeList.Clear();

        openList.Clear();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
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

            // ������
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
                    print(i + "��°�� " + finalNodeList[i].x + ", " + finalNodeList[i].y);
                }

                return;
            }

            // �� �� �� ��
            OpenListAdd(curNode.x, curNode.y + 1);
            OpenListAdd(curNode.x + 1, curNode.y);
            OpenListAdd(curNode.x, curNode.y - 1);
            OpenListAdd(curNode.x - 1, curNode.y);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // �����¿� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if (checkX >= -updateDetectedRange && checkX < updateDetectedRange + 1 && checkY >= -updateDetectedRange && checkY < updateDetectedRange + 1 && !nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange].isWall && !closedList.Contains(nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange]))
        {
            // �̿���忡 �ְ�, ������ 10, �밢���� 14���
            Node NeighborNode = nodeArray[checkX + updateDetectedRange, checkY + updateDetectedRange];
            int MoveCost = 10;

            // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
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