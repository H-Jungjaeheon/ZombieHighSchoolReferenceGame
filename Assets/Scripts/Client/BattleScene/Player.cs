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
    #region 서버
    public int MyId;
    public int MyType;
    #endregion

    [HideInInspector]
    [Tooltip("플레이어 상태")]
    public CurState curState;

    [Tooltip("공격 범위 내의 적 리스트")]
    public List<GameObject> rangeInEnemy = new List<GameObject>();

    #region 이동 관련 모음
    [Header("이동 관련 모음")]

    [SerializeField]
    [Tooltip("플레이어 이동속도")]
    private float speed;

    [SerializeField]
    [Tooltip("이동 연산에 사용될 Vector 모음")]
    private Vector3[] moveVectors;

    [SerializeField]
    [Tooltip("현재 이동 경로에 벽이 있는지 판별")]
    private bool isWallInPath;

    [Tooltip("이동시 미리 도달할 기준 좌표")]
    public Vector2 moveTargetPos;

    [Tooltip("이동 연산에 사용될 Vector")]
    private Vector3 moveVector;

    [Tooltip("이동 종료 시 멈출 위치")]
    private Vector3 endPos;

    [Tooltip("현재 이동 경로 변경할지 판별")]
    private bool isChangeDir;

    [Tooltip("현재 변경된 조이스틱 상태")]
    private MoveState changePressState;

    [Tooltip("플레이어 태그")]
    private const string WALL = "Wall";
    #endregion

    /// <summary>
    /// 공격 실행 함수(버튼 클릭)
    /// </summary>
    public void Attack()
    {
        for (int i = 0; i < rangeInEnemy.Count; i++)
        {
            rangeInEnemy[i].GetComponent<BasicEnemy>().Hit(1);
        }
    }

    /// <summary>
    /// 스킬 실행 함수(버튼 클릭)
    /// </summary>
    public void Skill()
    {

    }

    /// <summary>
    /// 움직임 함수
    /// </summary>
    /// <param name="curMoveState"> 이동 목표 상태 </param>
    /// <returns></returns>
    public IEnumerator Move(MoveState curMoveState)
    {
        moveVector = moveVectors[(int)curMoveState];

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position + moveVector, 0.45f))
        {
            if (collider.CompareTag(WALL))
            {
                isWallInPath = true;
            }
        }

        if (isWallInPath == false)
        {
            while (Input.GetMouseButton(0) && isChangeDir == false && isWallInPath == false)
            {
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position + moveVector, 0.45f))
                {
                    if (collider.CompareTag(WALL))
                    {
                        isWallInPath = true;
                    }
                }

                transform.Translate(moveVector * Time.deltaTime * speed);

                TargetPosSetting();

                yield return null;
            }

            endPos = moveVector + transform.position;

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
    /// 움직임 목표 위치 세팅
    /// </summary>
    private void TargetPosSetting()
    {
        moveTargetPos.x = Mathf.Ceil(transform.position.x);
        moveTargetPos.y = Mathf.Ceil(transform.position.y);
    }

    /// <summary>
    /// 조이스틱 방향 바꿀 시 실행하는 함수
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
    /// 벽 감지 오브젝트의 감지 결과
    /// </summary>
    public void WallDetectionResults(bool isExistenceWall)
    {
        isWallInPath = isExistenceWall;
    }
}
