using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveState
{
    None,
    Up,
    Down,
    Left,
    Right
}

public class JoyStick : MonoBehaviour
{
    [Tooltip("전투 씬의 메인 카메라 Camera 컴포넌트")]
    private Camera mainCam;

    [Tooltip("현재 터치(마우스) 포지션")]
    private Vector3 curMousePos;

    [Tooltip("목표 지점까지 이동하기 위해 더할 Vector")]
    private Vector3 plusPos;

    [Tooltip("현재 누른 조이스틱 상태")]
    private MoveState curPressState;

    [SerializeField]
    [Tooltip("Player 컴포넌트")]
    private Player player;

    private RaycastHit hit;

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

            StartCoroutine(UpdateClickBtn());
        }
    }

    /// <summary>
    /// 눌린 조이스틱 버튼 업데이트 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateClickBtn()
    {
        Ray raycast;

        while (player.curState == CurState.Moving)
        {
            raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            print(raycast);
            if (Physics.Raycast(raycast, out hit, LayerMask.GetMask("JoyStickArea")))
            {
                if (hit.collider.CompareTag("JoyStickUpArea") && curPressState != MoveState.Up)
                {
                    curPressState = MoveState.Up;
                }
                else if (hit.collider.CompareTag("JoyStickDownArea") && curPressState != MoveState.Down)
                {
                    curPressState = MoveState.Down;
                }
                else if (hit.collider.CompareTag("JoyStickLeftArea") && curPressState != MoveState.Left)
                {
                    curPressState = MoveState.Left;
                }
                else if (hit.collider.CompareTag("JoyStickRightArea") && curPressState != MoveState.Right)
                {
                    curPressState = MoveState.Right;
                }

                player.ChangeMoveDirection(curPressState);
            }

            yield return null;
        }
    }
}
