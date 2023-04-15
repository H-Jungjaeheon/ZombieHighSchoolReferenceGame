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
    [Tooltip("�� ���� ������Ʈ")]
    private GameObject detectObj;

    [SerializeField]
    [Tooltip("�� ���� ������Ʈ�� ������ ����")]
    private Vector3[] detectObjAngles;

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
    #endregion

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    /// <param name="curMoveState"> ���� �̵� ��ǥ ���� </param>
    /// <returns></returns>
    public IEnumerator Move(MoveState curMoveState)
    {
        moveVector = moveVectors[(int)curMoveState];

        detectObj.transform.rotation = Quaternion.Euler(detectObjAngles[(int)curMoveState]);

        if (isWallInPath == false)
        {
            while (Input.GetMouseButton(0) && isChangeDir == false)
            {
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
        isChangeDir = true;
        changePressState = curChangeMoveState;
    }

    /// <summary>
    /// �� ���� ������Ʈ�� ���� ���
    /// </summary>
    public void WallDetectionResults(bool isExistenceWall)
    {
        isWallInPath = isExistenceWall;
    }
}
