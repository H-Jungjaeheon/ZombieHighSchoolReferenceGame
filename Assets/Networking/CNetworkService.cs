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
            this.listen_socket.AcceptAsync(this.accept_args);
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


