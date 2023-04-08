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
        if (player.curState != CurState.Move)
        {
            player.curState = CurState.Move;

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
        while (player.curState == CurState.Move)
        {

            yield return null;
        }
    }
}
