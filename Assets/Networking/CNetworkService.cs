using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class CNetworkService
{
    // 클라이언트의 접속을 받아들이기 위한 객체
    CListener client_listener;

    // 메시지 수신, 전송시 필요한 오브젝트
    SocketAsyncEventArgsPool receive_event_args_pool;
    SocketAsyncEventArgsPool send_event_args_pool;

    //메시지 수진, 전송시 .Net 비동기 소켓에서 사용할 버퍼를 관리하는 객체
    BufferManager buffer_manager;

    //클라이언트의 접속이 이뤄졌을때 작동하는 콜벡 델리게이트
    public delegate void SessionHandle(CUserToken token);
    public SessionHandler session_created_callback { get; set; }
}

public class CListener
{

}

public class SocketAsyncEventArgsPool
{

}

public class BufferManager
{

}

public class SessionHandler
{

}

public class CUserToken
{

}


