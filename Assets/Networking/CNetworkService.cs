using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class CNetworkService
{
    // Ŭ���̾�Ʈ�� ������ �޾Ƶ��̱� ���� ��ü
    CListener client_listener;

    // �޽��� ����, ���۽� �ʿ��� ������Ʈ
    SocketAsyncEventArgsPool receive_event_args_pool;
    SocketAsyncEventArgsPool send_event_args_pool;

    //�޽��� ����, ���۽� .Net �񵿱� ���Ͽ��� ����� ���۸� �����ϴ� ��ü
    BufferManager buffer_manager;

    //Ŭ���̾�Ʈ�� ������ �̷������� �۵��ϴ� �ݺ� ��������Ʈ
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


