using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameServer
{
    internal class Player
    {
        public int Id;
        public string UserName;

        public Vector3 Position;
        public Quaternion Rotation;

        private float MoveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] Inputs;

        public Player(int _Id, string _UserName, Vector3 _SpawnPosition)
        {
            Id = _Id;
            UserName = _UserName;
            Position = _SpawnPosition;
            Rotation = Quaternion.Identity;

            Inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 _InputDirection = Vector2.Zero;
            if (Inputs[0])
            {
                _InputDirection.Y += 1;
            }
            if (Inputs[1])
            {
                _InputDirection.Y -= 1;
            }
            if (Inputs[2])
            {
                _InputDirection.X -= 1;
            }
            if (Inputs[3])
            {
                _InputDirection.X += 1;
            }

            Move(_InputDirection);
        }

        private void Move(Vector2 _InputDirection)
        {
            Vector3 _MoveDirection = new Vector3(_InputDirection.X, _InputDirection.Y, 1);
            Position += _MoveDirection * MoveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] _Inputs, Quaternion _Rotation)
        {
            Inputs = _Inputs;
            Rotation = _Rotation;
        }
    }
}
