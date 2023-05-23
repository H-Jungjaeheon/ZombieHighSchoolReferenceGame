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
    //클라이언트의 접속을 받아들이기 위한 객체
    CListener client_listener;

    //메시지 수신용 풀
    SocketAsyncEventArgsPool receive_event_args_pool;
    //메시지 전송용 풀
    SocketAsyncEventArgsPool send_event_args_pool;

    //메시지 수진, 전송시 .Net 비동기 소켓에서 사용할 버퍼를 관리하는 객체
    BufferManager buffer_manager;

    //클라이언트의 접속이 이뤄졌을때 작동하는 콜벡 델리게이트
    public delegate void SessionHandle(CUserToken token);
    public SessionHandler session_created_callback { get; set; }
    public void listen(string host, int port, int backlog)
    {
        CListener listener = new CListener();

        //TODO:on_new_client콜백 함수 구현 예정
        listener.callback_on_newclient += on_new_client;
        //매개변수들을 받아 listener 호출 기다림
        listener.start(host, port, backlog);
    }
}

public class CListener
{
    //비동기 Accept를 위한 EventArgs
    SocketAsyncEventArgs accept_args;

    //클라이언트의 접속을 처리할 소켓
    Socket listen_socket;

    //Accept처리의 순서를 제어하기 위한 이벤트 변수
    AutoResetEvent flow_control_event;

    //새로운 클라이언트가 접속했을 때 호출되는 콜백
    public delegate void NewclientHandle(Socket client_socket, object token);
    public NewclientHandle callback_on_newclient = null;

    public CListener()
    {
        this.callback_on_newclient = null;
    }

    public void start(string host, int port, int backlog)
    {
        //새로운 소켓 생성
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
            //소켓에 host정보를 바인딩 시킨뒤 Listen메소드를 호출하여 준비
            this.listen_socket.Bind(endpoint);
            this.listen_socket.Listen(backlog);

            this.accept_args = new SocketAsyncEventArgs();
            this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

            //클라이언트가 들어오기를 기다림
            //비동기 메소드 이므로 블로킹 되지 않고 바로 리턴
            //콜백 메소드를 통해서 접속 통보를 처리
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
        //accept처리 제어를 위해 이벤트 객체를 생성
        this.flow_control_event = new AutoResetEvent(false);

        while(true)
        {
            //SocketAsyncEventArgs를 재사용하기 위해서 null로 변환
            this.accept_args.AcceptSocket = null;

            bool pending = true;
            try
            {
                //비동기 accept를 호출하여 클라이언트의 접속을 수락
                //비동기 메소드 이지만 동기적으로 수행이 완료될 경우가 있으니 리턴값 확인하여 분기
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
            //새로 생긴 소켓을 보관해 놓음
            Socket client_socket = e.AcceptSocket;

            //다음 연결을 수락
            this.flow_control_event.Set();

            //현재 클래스에서는 accept까지의 역할만을 수행하고 클라이언트의 접속 이후의 처리는
            //외부로 넘기기 위해서 콜백 메소드를 호출해 주도록 한다.
            //이유는 소켓 처리부와 컨텐츠 구현부의 분리를 위함
            //컨텐츠 구현부분은 자주 바뀔 가능성이 있지만, 소켓 accept부분은 상대적으로 변경이 적기 때문에

            //양쪽을 분리시키는것이 좋다.
            //또한 클래스 설계 방침에 따라 Listen에 관련된 코드만 존재하도록 하기 위한 이유도 있다.

            if(this.callback_on_newclient != null)
            {
                this.callback_on_newclient(client_socket, e.UserToken);
            }

            return;
        }
        
        else
        {
            //Accept 실패 문구
            Console.WriteLine("Failed to accept client");
        }

        //다음 연결 대기
        this.flow_control_event.Set();
    }
}

//재사용 가능한 SocketAsyncEventArgs 개체 컬렉션
public class SocketAsyncEventArgsPool
{
    Stack<SocketAsyncEventArgs> m_pool;

    //객체 풀을 지정된 크기로 초기화합니다
    //"capacity" 매개 변수는 다음의 최대 개수입니다
    //풀에 저장할 수 있는 SocketAsyncEventArgs 개체
    public SocketAsyncEventArgsPool(int capacity)
    {
        m_pool = new Stack<SocketAsyncEventArgs>(capacity);
    }

    //SocketAsyncEventArg 인스턴스를 풀에 추가합니다
    //"item" 매개 변수는 SocketAsyncEventArgs 인스턴스입니다
    //풀에 추가
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

    //풀에서 SocketAsyncEventArgs 인스턴스를 제거합니다
    //풀에서 제거된 개체를 반환합니다
    public SocketAsyncEventArgs Pop()
    {
        lock(m_pool)
        {
            return m_pool.Pop();
        }
    }

    //풀의 SocketAsyncEventArgs 인스턴스 수입니다
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

    //하나의 거대한 바이트 배열 생성
    public void InitBuffer()
    {
        m_buffer = new byte[m_numBytes];
    }

    /// <summary>
    /// SocketAsyncEventArgs객체에 버퍼 설정
    /// 인덱스 값을 증가시켜 다음 버퍼 위치 지목
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
    /// 사용하지 않는 버퍼 반환
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
