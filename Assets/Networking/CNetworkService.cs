using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class CNetworkService
{
    //Ŭ���̾�Ʈ�� ������ �޾Ƶ��̱� ���� ��ü
    CListener client_listener;

    //�޽��� ���ſ� Ǯ
    SocketAsyncEventArgsPool receive_event_args_pool;
    //�޽��� ���ۿ� Ǯ
    SocketAsyncEventArgsPool send_event_args_pool;

    //�޽��� ����, ���۽� .Net �񵿱� ���Ͽ��� ����� ���۸� �����ϴ� ��ü
    BufferManager buffer_manager;

    //Ŭ���̾�Ʈ�� ������ �̷������� �۵��ϴ� �ݺ� ��������Ʈ
    public delegate void SessionHandle(CUserToken token);
    public SessionHandler session_created_callback { get; set; }
    public void listen(string host, int port, int backlog)
    {
        CListener listener = new CListener();

        //TODO:on_new_client�ݹ� �Լ� ���� ����
        listener.callback_on_newclient += on_new_client;
        //�Ű��������� �޾� listener ȣ�� ��ٸ�
        listener.start(host, port, backlog);
    }
}

public class CListener
{
    //�񵿱� Accept�� ���� EventArgs
    SocketAsyncEventArgs accept_args;

    //Ŭ���̾�Ʈ�� ������ ó���� ����
    Socket listen_socket;

    //Acceptó���� ������ �����ϱ� ���� �̺�Ʈ ����
    AutoResetEvent flow_control_event;

    //���ο� Ŭ���̾�Ʈ�� �������� �� ȣ��Ǵ� �ݹ�
    public delegate void NewclientHandle(Socket client_socket, object token);
    public NewclientHandle callback_on_newclient = null;

    public CListener()
    {
        this.callback_on_newclient = null;
    }

    public void start(string host, int port, int backlog)
    {
        //���ο� ���� ����
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
            //���Ͽ� host������ ���ε� ��Ų�� Listen�޼ҵ带 ȣ���Ͽ� �غ�
            this.listen_socket.Bind(endpoint);
            this.listen_socket.Listen(backlog);

            this.accept_args = new SocketAsyncEventArgs();
            this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

            //Ŭ���̾�Ʈ�� �����⸦ ��ٸ�
            //�񵿱� �޼ҵ� �̹Ƿ� ���ŷ ���� �ʰ� �ٷ� ����
            //�ݹ� �޼ҵ带 ���ؼ� ���� �뺸�� ó��
            Thread listen_thread = new Thread(do_listen);
            listen_thread.Start();
        }

        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    void do_listen()
    {
        //acceptó�� ��� ���� �̺�Ʈ ��ü�� ����
        this.flow_control_event = new AutoResetEvent(false);

        while(true)
        {
            //SocketAsyncEventArgs�� �����ϱ� ���ؼ� null�� ��ȯ
            this.accept_args.AcceptSocket = null;

            bool pending = true;
            try
            {
                //�񵿱� accept�� ȣ���Ͽ� Ŭ���̾�Ʈ�� ������ ����
                //�񵿱� �޼ҵ� ������ ���������� ������ �Ϸ�� ��찡 ������ ���ϰ� Ȯ���Ͽ� �б�
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
            //���� ���� ������ ������ ����
            Socket client_socket = e.AcceptSocket;

            //���� ������ ����
            this.flow_control_event.Set();

            //���� Ŭ���������� accept������ ���Ҹ��� �����ϰ� Ŭ���̾�Ʈ�� ���� ������ ó����
            //�ܺη� �ѱ�� ���ؼ� �ݹ� �޼ҵ带 ȣ���� �ֵ��� �Ѵ�.
            //������ ���� ó���ο� ������ �������� �и��� ����
            //������ �����κ��� ���� �ٲ� ���ɼ��� ������, ���� accept�κ��� ��������� ������ ���� ������

            //������ �и���Ű�°��� ����.
            //���� Ŭ���� ���� ��ħ�� ���� Listen�� ���õ� �ڵ常 �����ϵ��� �ϱ� ���� ������ �ִ�.

            if(this.callback_on_newclient != null)
            {
                this.callback_on_newclient(client_socket, e.UserToken);
            }

            return;
        }
        
        else
        {
            //Accept ���� ����
            Console.WriteLine("Failed to accept client");
        }

        //���� ���� ���
        this.flow_control_event.Set();
    }
}

//���� ������ SocketAsyncEventArgs ��ü �÷���
public class SocketAsyncEventArgsPool
{
    Stack<SocketAsyncEventArgs> m_pool;

    //��ü Ǯ�� ������ ũ��� �ʱ�ȭ�մϴ�
    //"capacity" �Ű� ������ ������ �ִ� �����Դϴ�
    //Ǯ�� ������ �� �ִ� SocketAsyncEventArgs ��ü
    public SocketAsyncEventArgsPool(int capacity)
    {
        m_pool = new Stack<SocketAsyncEventArgs>(capacity);
    }

    //SocketAsyncEventArg �ν��Ͻ��� Ǯ�� �߰��մϴ�
    //"item" �Ű� ������ SocketAsyncEventArgs �ν��Ͻ��Դϴ�
    //Ǯ�� �߰�
    public void Push(SocketAsyncEventArgs item)
    {
        if(item == null)
        {
            throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool Cannot be null");
        }

        lock (m_pool)
        {
            m_pool.Push(item);
        }
    }

    //Ǯ���� SocketAsyncEventArgs �ν��Ͻ��� �����մϴ�
    //Ǯ���� ���ŵ� ��ü�� ��ȯ�մϴ�
    public SocketAsyncEventArgs Pop()
    {
        lock(m_pool)
        {
            return m_pool.Pop();
        }
    }

    //Ǯ�� SocketAsyncEventArgs �ν��Ͻ� ���Դϴ�
    public int Count
    {
        get { return m_pool.Count; }
    }
}

internal class BufferManager
{
    int m_numBytes;
    byte[] m_buffer;
    Stack<int> m_freeIndexPool;
    int m_currentIndex;
    int m_BufferSize;

    public BufferManager(int totalBytes, int bufferSize)
    {
        m_numBytes = totalBytes;
        m_currentIndex = 0;
        m_BufferSize = bufferSize;
        m_freeIndexPool = new Stack<int>();
    }

    //�ϳ��� �Ŵ��� ����Ʈ �迭 ����
    public void InitBuffer()
    {
        m_buffer = new byte[m_numBytes];
    }

    /// <summary>
    /// SocketAsyncEventArgs��ü�� ���� ����
    /// �ε��� ���� �������� ���� ���� ��ġ ����
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool SetBuffer(SocketAsyncEventArgs args)
    {
        if(m_freeIndexPool.Count > 0)
        {
            args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_BufferSize);
        }

        else
        {
            if((m_numBytes = m_BufferSize) < m_currentIndex)
            {
                return false;
            }

            args.SetBuffer(m_buffer, m_currentIndex, m_BufferSize);
            m_currentIndex += m_BufferSize;
        }
        return true;
    }

    /// <summary>
    /// ������� �ʴ� ���� ��ȯ
    /// </summary>
    /// <param name="args"></param>
    public void FreeBuffer(SocketAsyncEventArgs args)
    {
        m_freeIndexPool.Push(args.Offset);
        args.SetBuffer(null, 0, 0);
    }
}

public class SessionHandler
{

}
