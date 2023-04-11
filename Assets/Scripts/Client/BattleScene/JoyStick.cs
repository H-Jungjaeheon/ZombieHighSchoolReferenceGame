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
    [Tooltip("���� ���� ���� ī�޶� Camera ������Ʈ")]
    private Camera mainCam;

    [Tooltip("���� ��ġ(���콺) ������")]
    private Vector3 curMousePos;

    [Tooltip("��ǥ �������� �̵��ϱ� ���� ���� Vector")]
    private Vector3 plusPos;

    [Tooltip("���� ���� ���̽�ƽ ����")]
    private MoveState curPressState;

    [SerializeField]
    [Tooltip("Player ������Ʈ")]
    private Player player;

    private RaycastHit hit;

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

            StartCoroutine(UpdateClickBtn());
        }
    }

    /// <summary>
    /// ���� ���̽�ƽ ��ư ������Ʈ �Լ�
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
