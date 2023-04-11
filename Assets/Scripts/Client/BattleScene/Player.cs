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
    [Tooltip("�÷��̾� �̵��ӵ�")]
    private float speed;

    [HideInInspector]
    [Tooltip("���� �÷��̾� ����")]
    public CurState curState;

    [Tooltip("���� ����� ���̽�ƽ ����")]
    private MoveState changePressState;

    [Tooltip("�̵��� �̸� ������ ���� ��ǥ")]
    public Vector2 moveTargetPos;

    [Tooltip("�̵� ���꿡 ���� Vector")]
    private Vector2 moveVector;

    [Tooltip("�̵� ���� �� ���� ��ġ")]
    private Vector3 endPos;

    [Tooltip("���� �̵� ��� �������� �Ǻ�")]
    private bool isChangeDir;

    /// <summary>
    /// ������ �Լ�
    /// </summary>
    /// <param name="curMoveState"> ���� �̵� ��ǥ ���� </param>
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
            transform.Translate(moveVector * Time.deltaTime * speed);

            TargetPosRoundsSetting();

            if (!Input.GetMouseButton(0) || isChangeDir)
            {
                endPos = transform.position + (Vector3)moveVector;

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

                //moveTargetPos += moveVector;

                while ((curMoveState == MoveState.Up && transform.position.y <= endPos.y) ||
                       (curMoveState == MoveState.Down && transform.position.y >= endPos.y) ||
                       (curMoveState == MoveState.Right && transform.position.x <= endPos.x) ||
                       (curMoveState == MoveState.Left && transform.position.x >= endPos.x))
                {
                    transform.Translate(moveVector * Time.deltaTime * speed);
                    
                    TargetPosRoundsSetting();

                    yield return null;
                }


                transform.position = endPos;
                TargetPosRoundsSetting();

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

    private void TargetPosRoundsSetting()
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

}
