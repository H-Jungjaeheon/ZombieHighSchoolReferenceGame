using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNet
{
    class CNetworkService
    {
        CListener client_listener;

        SocketAsyncEventArgsPool recive_event_args_pool;
        SocketAsyncEventArgsPool send_event_args_pool;

        BufferManager buffer_manager;

        public delegate void SessionHanadler(CUserToken token);
        public SessionHanadler session_created_callback { get; set; }
    }
}
