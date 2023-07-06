using System.Collections;
using UnityEngine;

public enum MoveState
{
    Up,
    Down,
    Left,
    Right
}

public class JoyStick : MonoBehaviour
{
    [Tooltip("���� ���� ���� ī�޶� Camera ������Ʈ")]
    private Camera mainCam;

    [Tooltip("���� ���� ���̽�ƽ ����")]
    private MoveState curPressState;

    [SerializeField]
    [Tooltip("Player ������Ʈ")]
    private Player player;

    [Tooltip("���콺�� ���̽�ƽ ������ ����")]
    private float joyStickAngle;

    void Start()
    {
        StartSetting();
    }

    /// <summary>
    /// ���� ���� �Լ�
    /// </summary>
    private void StartSetting()
    {
        mainCam = Camera.main;
    }

    /// <summary>
    /// �̵� ���̽�ƽ ��ġ ���� �Ǻ� �Լ�
    /// </summary>
    /// <param name="moveStateIndex"></param>
    public void JoyStickTouchStart(int moveStateIndex)
    {
        if (player.curState != CurState.Moving)
        {
            player.curState = CurState.Moving;

            curPressState = (MoveState)moveStateIndex;

            player.StartCoroutine(player.Move(curPressState));

            StartCoroutine(CheackAngle());
        }
    }

    /// <summary>
    /// ���̽�ƽ, ���콺���� ���� ������Ʈ �Լ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheackAngle()
    {
        while (Input.GetMouseButton(0))
        {
            joyStickAngle = Mathf.Atan2(mainCam.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y, mainCam.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x) * Mathf.Rad2Deg;

            if (joyStickAngle < 0)
            {
                joyStickAngle += 360;
            }

            if (joyStickAngle >= 46f && joyStickAngle <= 135f && curPressState != MoveState.Up)
            {
                ChangePlayerMoveDirection(MoveState.Up);
            }
            else if (joyStickAngle >= 136f && joyStickAngle <= 225f && curPressState != MoveState.Left)
            {
                ChangePlayerMoveDirection(MoveState.Left);
            }
            else if (joyStickAngle >= 226f && joyStickAngle <= 315f && curPressState != MoveState.Down)
            {
                ChangePlayerMoveDirection(MoveState.Down);
            }
            else if ((joyStickAngle >= 316f || joyStickAngle <= 45f) && curPressState != MoveState.Right)
            {
                ChangePlayerMoveDirection(MoveState.Right);
            }

            yield return null;
        }
    }

    /// <summary>
    /// �÷��̾� �̵��� �̵� ���� �ٲٴ� �Լ�
    /// </summary>
    /// <param name="changeMoveState"></param>
    private void ChangePlayerMoveDirection(MoveState changeMoveState)
    {
        curPressState = changeMoveState;
        player.ChangeMoveDirection(changeMoveState);
    }
}
