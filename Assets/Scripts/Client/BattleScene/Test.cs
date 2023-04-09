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

    public int G;

    public int H;

    public int f
    {
        get
        {
            return G + H;
        }
    }
}


public class Test : MonoBehaviour
{
    public Vector2Int bottomLeft, topRight, startPos, targetPos;
    public List<Node> finalNodeList;

    int sizeX, sizeY;
    Node[,] nodeArray;
    Node startNode, targetNode, curNode;
    List<Node> openList, closedList;

    private void Start()
    {
        PathFinding();
    }

    /// <summary>
    /// ��� Ž�� �Լ�
    /// </summary>
    public void PathFinding()
    {
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;
        nodeArray = new Node[sizeX, sizeY];

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
                {
                    if (collider.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                nodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }


        // ���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        startNode = nodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        targetNode = nodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        openList = new List<Node>() { startNode };
        closedList = new List<Node>();
        finalNodeList = new List<Node>();


        while (openList.Count > 0)
        {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
            curNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
                if (openList[i].f <= curNode.f && openList[i].H < curNode.H) curNode = openList[i];

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
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !closedList.Contains(nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            // �̿���忡 �ְ�, ������ 10, �밢���� 14���
            Node NeighborNode = nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = 10;

            // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
            if (MoveCost < NeighborNode.G || !openList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - targetNode.x) + Mathf.Abs(NeighborNode.y - targetNode.y)) * 10;
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