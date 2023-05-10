using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

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
    SocketAsyncEventArgs accept_args;

    Socket listen_socket;

    AutoResetEvent flow_control_event;

    public delegate void NewclientHandle(Socket client_socket, object token);
    public NewclientHandle callback_on_newclient = null;

    public CListener()
    {
        this.callback_on_newclient = null;
    }

    public void start(string host, int port, int backlog)
    {
        this.listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPAddress address;
        if(host == "0,0,0,0")
        {
            address = IPAddress.Any;
        }
        else
        {
            address = IPAddress.Parse(host);
        }
        IPEndPoint endpoint = new IPEndPoint(address, port);

        try
        {
            this.listen_socket.Bind(endpoint);
            this.listen_socket.Listen(backlog);

            this.accept_args = new SocketAsyncEventArgs();
            this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

            this.listen_socket.AcceptAsync(this.accept_args);
        }

        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    void do_listen()
    {
        this.flow_control_event = new AutoResetEvent(false);

        while(true)
        {
            this.accept_args.AcceptSocket = null;

            bool pending = true;
            try
            {
                pending = listen_socket.AcceptAsync(this.accept_args);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            if(!pending)
            {
                on_accept_completed(null, this.accept_args);
            }

            this.flow_control_event.WaitOne();
        }
    }

    void on_accept_completed(object sender, SocketAsyncEventArgs e)
    {
        if(e.SocketError == SocketError.Success)
        {
            Socket client_socket = e.AcceptSocket;

            this.flow_control_event.Set();

            if(this.callback_on_newclient != null)
            {
                this.callback_on_newclient(client_socket, e.UserToken);
            }

            return;
        }
        
        else
        {
            Console.WriteLine("Failed to accept client");
        }

        this.flow_control_event.Set();
    }
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


