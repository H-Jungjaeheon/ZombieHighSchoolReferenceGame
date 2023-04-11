using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        xPos = x;

        yPos = y;
    }

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    public bool isWall;

    [Tooltip("���� ����� �θ� ���(���� ���)")]
    public Node ParentNode;

    [Tooltip("���� ����� X ������")]
    public int xPos;

    [Tooltip("���� ����� Y ������")]
    public int yPos;

    [Tooltip("�������κ��� �̵��� �Ÿ�")]
    public int g;

    [Tooltip("��ֹ��� ������ ��ǥ������ �Ÿ� (����, ����)")]
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
    [Tooltip("���� ������ �÷��̾� ������Ʈ")]
    private GameObject detectedPObj;

    #region �̵� ���� ��ҵ� ����
    [Header("�̵� ���� ��ҵ� ����")]

    [Tooltip("�߰� �� �÷��̾� ���� ����")]
    private const int updateDetectedRange = 50;

    [Tooltip("���� ��尡 ������ �Ǻ�")]
    private bool isWall;

    [Tooltip("��� Ž�� �� ���� ������")]
    private Vector2 startPos;

    [Tooltip("���� ��� ��� ����Ʈ")]
    public List<Node> finalNodeList;

    [Tooltip("��ü ��� ���� ��� �迭")]
    private Node[,] nodeArray = new Node[updateDetectedRange * 2 + 1, updateDetectedRange * 2 + 1];

    [Tooltip("��� ���� ���")]
    private Node startNode;

    [Tooltip("������ ���")]
    private Node targetNode;

    [Tooltip("���� ��� ���")]
    private Node curNode;

    [Tooltip("��� Ž�� ������ ��� ����Ʈ")]
    private List<Node> openList = new List<Node>();

    [Tooltip("��� Ž�� �Ϸ��� ��� ����Ʈ")]
    private List<Node> closedList = new List<Node>();
    #endregion

    /// <summary>
    /// �÷��̾� ���� ����
    /// </summary>
    public virtual void DetectedPlayer(GameObject detectedPlayerObj)
    {
        if (detectedPObj != null)
        {
            return;
        }

        detectedPObj = detectedPlayerObj;

        PathFinding(detectedPlayerObj.transform.position);
    }

    /// <summary>
    /// ��� Ž�� �Լ�
    /// </summary>
    public void PathFinding(Vector2 targetPos)
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
            // ���¸���Ʈ �� ���� f�� ���� ��, f�� ���ٸ� h�� ���� ��, h�� ���ٸ� 0��° ���� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
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

                //for (int i = 0; i < finalNodeList.Count; i++)
                //{
                //    print(i + "��°�� " + finalNodeList[i].xPos + ", " + finalNodeList[i].yPos);
                //}

                StartCoroutine(Move());

                return;
            }

            // �� �� �� ��
            OpenListAdd(curNode.xPos, curNode.yPos + 1);
            OpenListAdd(curNode.xPos + 1, curNode.yPos);
            OpenListAdd(curNode.xPos, curNode.yPos - 1);
            OpenListAdd(curNode.xPos - 1, curNode.yPos);
        }
    }

    /// <summary>
    /// ���� ������ ����� ���� ���¸���Ʈ �߰� �Լ�
    /// </summary>
    /// <param name="checkX"></param>
    /// <param name="checkY"></param>
    private void OpenListAdd(int checkX, int checkY)
    {
        //���� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if (checkX >= startPos.x - updateDetectedRange && checkX <= startPos.x + updateDetectedRange
            && checkY >= startPos.y - updateDetectedRange && checkY <= startPos.y + updateDetectedRange
            && nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange].isWall == false
            && closedList.Contains(nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange]) == false)
        {
            // �̿���忡 �ֱ�
            Node neighborNode = nodeArray[(int)(checkX - startPos.x) + updateDetectedRange, (int)(checkY - startPos.y) + updateDetectedRange];
            int moveCost = 10;

            // �̵������ �̿���� g���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� g, h, ParentNode�� ���� �� ��������Ʈ�� �߰�
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
    /// ������ �Լ�
    /// </summary>
    /// <param name="curMoveState"> ���� �̵� ��ǥ ���� </param>
    /// <returns></returns>
    public IEnumerator Move() //MoveState curMoveState
    {
        //moveVector = Vector3.zero;

        //if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
        //{
        //    moveVector.y = (curMoveState == MoveState.Up) ? 1f : -1f;
        //}
        //else
        //{
        //    moveVector.x = (curMoveState == MoveState.Right) ? 1f : -1f;
        //}

        //while (true)
        //{
        //    transform.position += moveVector * Time.deltaTime * speed;

        //    if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
        //    {
        //        moveTargetPos.y = Mathf.FloorToInt(transform.position.y);
        //    }
        //    else
        //    {
        //        moveTargetPos.x = Mathf.FloorToInt(transform.position.x);
        //    }

        //    if (!Input.GetMouseButton(0) || isChangeDir)
        //    {
        //        endPos = transform.position + moveVector;

        //        if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
        //        {
        //            endPos.y = Mathf.FloorToInt(endPos.y);

        //            if (curMoveState == MoveState.Down)
        //            {
        //                endPos.y += 1;
        //            }
        //        }
        //        else
        //        {
        //            endPos.x = Mathf.FloorToInt(endPos.x);

        //            if (curMoveState == MoveState.Left)
        //            {
        //                endPos.x += 1;
        //            }
        //        }

        //        while ((curMoveState == MoveState.Up && transform.position.y <= endPos.y) ||
        //               (curMoveState == MoveState.Down && transform.position.y >= endPos.y) ||
        //               (curMoveState == MoveState.Right && transform.position.x <= endPos.x) ||
        //               (curMoveState == MoveState.Left && transform.position.x >= endPos.x))
        //        {
        //            transform.position += moveVector * Time.deltaTime * speed;

        //            yield return null;
        //        }

        //        transform.position = endPos;

        //        if (isChangeDir)
        //        {
        //            StartCoroutine(Move(changePressState));
        //        }
        //        else
        //        {
        //            curState = CurState.Idle;
        //        }

        //        yield break;
        //    }

        //    yield return null;
        //}

        //curMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        //if (curMousePos.x > -8f && curMousePos.x > -9.8f)
        //{

        //}
        //while (true)
        //{
        //    //RaycastHit2D hit = Physics2D.Raycast()
        //}
        yield return null;
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
