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
    [Tooltip("전투 씬의 메인 카메라 Camera 컴포넌트")]
    private Camera mainCam;

    [Tooltip("현재 누른 조이스틱 상태")]
    private MoveState curPressState;

    [SerializeField]
    [Tooltip("Player 컴포넌트")]
    private Player player;

    [Tooltip("마우스와 조이스틱 사이의 각도")]
    private float joyStickAngle;

    void Start()
    {
        StartSetting();
    }

    /// <summary>
    /// 시작 세팅 함수
    /// </summary>
    private void StartSetting()
    {
        mainCam = Camera.main;
    }

    /// <summary>
    /// 이동 조이스틱 터치 시작 판별 함수
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
    /// 조이스틱, 마우스간의 각도 업데이트 함수
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
    /// 플레이어 이동중 이동 방향 바꾸는 함수
    /// </summary>
    /// <param name="changeMoveState"></param>
    private void ChangePlayerMoveDirection(MoveState changeMoveState)
    {
        curPressState = changeMoveState;
        player.ChangeMoveDirection(changeMoveState);
    }
}
