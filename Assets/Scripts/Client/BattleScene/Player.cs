using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurState
{
    Idle,
    Moving
}

public class Player : MonoBehaviour
{
    [SerializeField]
    [Tooltip("플레이어 이동속도")]
    private float speed;

    [HideInInspector]
    [Tooltip("현재 플레이어 상태")]
    public CurState curState;

    [Tooltip("현재 변경된 조이스틱 상태")]
    private MoveState changePressState;

    [Tooltip("이동 연산에 사용될 Vector")]
    private Vector3 moveVector;

    [Tooltip("이동 종료 시 멈출 위치")]
    private Vector3 endPos;

    [Tooltip("현재 이동 경로 변경할지 판별")]
    private bool isChangeDir;

    /// <summary>
    /// 움직임 함수
    /// </summary>
    /// <param name="curMoveState"> 현재 이동 목표 상태 </param>
    /// <returns></returns>
    public IEnumerator Move(MoveState curMoveState)
    {
        moveVector = Vector3.zero;

        if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
        {
            moveVector.y = (curMoveState == MoveState.Up) ? 1f : -1f;
        }
        else
        {
            moveVector.x = (curMoveState == MoveState.Right) ? 1f : -1f;
        }

        while (true)
        {
            transform.position += moveVector * Time.deltaTime * speed;

            if (!Input.GetMouseButton(0) || isChangeDir)
            {
                endPos = transform.position + moveVector;

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
                    transform.position += moveVector * Time.deltaTime * speed;

                    yield return null;
                }

                transform.position = endPos;

                if (isChangeDir)
                {
                    StartCoroutine(Move(changePressState));
                }
                else
                {
                    curState = CurState.Idle;
                }
                
                yield break;
            }

            yield return null;
        }

        //curMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        //if (curMousePos.x > -8f && curMousePos.x > -9.8f)
        //{

        //}
        //while (true)
        //{
        //    //RaycastHit2D hit = Physics2D.Raycast()
        //}
    }

    /// <summary>
    /// 조이스틱 방향 바꿀 시 실행하는 함수
    /// </summary>
    public void ChangeMoveDirection(MoveState curChangeMoveState)
    {
        isChangeDir = true;
        changePressState = curChangeMoveState;
    }

}
