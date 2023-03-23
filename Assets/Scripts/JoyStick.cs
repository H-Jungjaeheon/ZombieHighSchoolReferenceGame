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
        if (player.curState != CurState.Move)
        {
            player.curState = CurState.Move;

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
        while (player.curState == CurState.Move)
        {

            yield return null;
        }
    }
}
