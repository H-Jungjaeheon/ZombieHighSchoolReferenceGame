using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurState
{
    Idle,
    Move
}

public class Player : MonoBehaviour
{
    [SerializeField]
    [Tooltip("플레이어 이동속도")]
    private float speed;

    [HideInInspector]
    [Tooltip("현재 플레이어 상태")]
    public CurState curState;

    /// <summary>
    /// 움직임 함수
    /// </summary>
    /// <param name="curMoveState"> 현재 이동 목표 상태 </param>
    /// <returns></returns>
    public IEnumerator Move(MoveState curMoveState)
    {
        Vector3 targetPos;

        float maxSec = 0.35f;
        float curSec = 0f;

        while (true)
        {
            targetPos = Vector3.zero;

            if (curMoveState == MoveState.Up || curMoveState == MoveState.Down)
            {
                targetPos.y = (curMoveState == MoveState.Up) ? 1f : -1f;
            }
            else
            {
                targetPos.x = (curMoveState == MoveState.Right) ? 1f : -1f;
            }

            targetPos += transform.position;

            while (curSec <= maxSec)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, curSec / maxSec);

                curSec += Time.deltaTime * speed;

                yield return null;
            }

            curSec = 0;

            if (!Input.GetMouseButton(0)) 
            {
                print("실행");
                break;
            }
        }

        curState = CurState.Idle;

        //curMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        //if (curMousePos.x > -8f && curMousePos.x > -9.8f)
        //{

        //}
        //while (true)
        //{
        //    //RaycastHit2D hit = Physics2D.Raycast()
        //}
    }

    public void ClickMoveBtn()
    {
        
    }

}
