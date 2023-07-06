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

        public Player(int _Id, string _UserName, Vector3 _SpawnPosition)
        {
            Id = _Id;
            UserName = _UserName;
            Position = _SpawnPosition;
            Rotation = Quaternion.Identity;
        }
    }
}
