using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void Update()
    {
        #region Shoot
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ClientSend.PlayerShoot(Vector3.up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ClientSend.PlayerShoot(Vector3.down);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ClientSend.PlayerShoot(Vector3.right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ClientSend.PlayerShoot(Vector3.left);
        }
        #endregion

        #region Throw
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Throw Start");
            ClientSend.PlayerThrowItem(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
        }
        #endregion
    }

    private void SendInputToServer()
    {
        bool[] _Inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
        };

        ClientSend.PlayerMovement(_Inputs);
    }
}
