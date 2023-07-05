using System;
using System.Threading;

namespace GameServer
{
    internal class Program
    {
        private static bool IsRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            IsRunning = true;

            Thread _MainThread = new Thread(new ThreadStart(MainThread));
            _MainThread.Start();

            Server.Start(50, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main Thread Stared. Running At {Constants.TICKS_PER_SEC} Ticks Per Second.");
            DateTime _NextLoop = DateTime.Now;

            while(IsRunning)
            {
                while(_NextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _NextLoop = _NextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if(_NextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_NextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}