using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class GameLogic
    {
        public static void Update()
        {
            foreach (Client _Client in Server.Clients.Values)
            {
                if(_Client.MyPlayer != null)
                {
                    _Client.MyPlayer.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
