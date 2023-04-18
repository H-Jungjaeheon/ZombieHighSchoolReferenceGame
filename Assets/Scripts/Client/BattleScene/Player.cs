using System.Collections;
using UnityEngine;

public enum CurState
{
    Idle,
    Moving
}

public class Player : MonoBehaviour
{
    [HideInInspector]
    [Tooltip("���� �÷��̾� ����")]
    public CurState curState;

    #region �̵� ���� ����
    [Header("�̵� ���� ����")]

    [SerializeField]
    [Tooltip("�÷��̾� �̵��ӵ�")]
    private float speed;

    [SerializeField]
    [Tooltip("�̵� ���꿡 ���� Vector ����")]
    private Vector2[] moveVectors;

    [SerializeField]
    [Tooltip("���� �̵� ��ο� ���� �ִ��� �Ǻ�")]
    private bool isWallInPath;

    [HideInInspector]
    [Tooltip("�̵��� �̸� ������ ���� ��ǥ")]
    public Vector2 moveTargetPos;

    [Tooltip("�̵� ���꿡 ���� Vector")]
    private Vector2 moveVector;

    [Tooltip("�̵� ���� �� ���� ��ġ")]
    private Vector2 endPos;

    [Tooltip("���� �̵� ��� �������� �Ǻ�")]
    private bool isChangeDir;

    [Tooltip("���� ����� ���̽�ƽ ����")]
    private MoveState changePressState;

    [Tooltip("�÷��̾� �±�")]
    private const string WALL = "Wall";
    #endregion

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    /// <param name="curMoveState"> ���� �̵� ��ǥ ���� </param>
    /// <returns></returns>
    public IEnumerator Move(MoveState curMoveState)
    {
        moveVector = moveVectors[(int)curMoveState];

        foreach (Collider2D collider in Physics2D.OverlapCircleAll((Vector2)transform.position + moveVector, 0.45f))
        {
            if (collider.gameObject.CompareTag(WALL))
            {
                isWallInPath = true;
            }
        }

        if (isWallInPath == false)
        {
            while (Input.GetMouseButton(0) && isChangeDir == false && isWallInPath == false)
            {
                foreach (Collider2D collider in Physics2D.OverlapCircleAll((Vector2)transform.position + moveVector, 0.45f))
                {
                    if (collider.gameObject.CompareTag(WALL))
                    {
                        isWallInPath = true;
                    }
                }

                transform.Translate(moveVector * Time.deltaTime * speed);

                TargetPosSetting();

                yield return null;
            }

            endPos = (Vector3)moveVector + transform.position;

            if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
            {
                endPos.y = Mathf.FloorToInt(endPos.y);

                if (curMoveState == MoveState.Down)
                {
                    endPos.y += 1;
                }
            }
            else
            {
                endPos.x = Mathf.FloorToInt(endPos.x);

                if (curMoveState == MoveState.Left)
                {
                    endPos.x += 1;
                }
            }

            while ((curMoveState == MoveState.Up && transform.position.y <= endPos.y) ||
                   (curMoveState == MoveState.Down && transform.position.y >= endPos.y) ||
                   (curMoveState == MoveState.Right && transform.position.x <= endPos.x) ||
                   (curMoveState == MoveState.Left && transform.position.x >= endPos.x))
            {
                transform.Translate(moveVector * Time.deltaTime * speed);

                TargetPosSetting();

                yield return null;
            }
        }

        isWallInPath = false;

        transform.position = endPos;
        TargetPosSetting();

        if (isChangeDir)
        {
            isChangeDir = false;
            StartCoroutine(Move(changePressState));
        }
        else
        {
            curState = CurState.Idle;
        }
    }

    /// <summary>
    /// ���� ������ ��ǥ ��ġ ����
    /// </summary>
    private void TargetPosSetting()
    {
        moveTargetPos.x = Mathf.Ceil(transform.position.x);
        moveTargetPos.y = Mathf.Ceil(transform.position.y);
    }

    /// <summary>
    /// ���̽�ƽ ���� �ٲ� �� �����ϴ� �Լ�
    /// </summary>
    public void ChangeMoveDirection(MoveState curChangeMoveState)
    {
        changePressState = curChangeMoveState;

        if (curState == CurState.Moving)
        {
            isChangeDir = true;
        }
        else
        {
            curState = CurState.Moving;
            StartCoroutine(Move(changePressState));
        }
    }

    /// <summary>
    /// �� ���� ������Ʈ�� ���� ���
    /// </summary>
    public void WallDetectionResults(bool isExistenceWall)
    {
        isWallInPath = isExistenceWall;
    }
}
