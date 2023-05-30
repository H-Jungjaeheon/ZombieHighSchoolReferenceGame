using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FreeNet
{
    class CListener
    {
        //비동기 Accept를 위한 EventArgs
        SocketAsyncEventArgs accept_args;

        //클라이언트 접속을 처리할 소켓
        Socket listen_Socket;

        //Accept 처리의 순서를 제어하기 위한 이벤트 변수
        AutoResetEvent flow_control_event;

        //새로운 클라이언트가 접속했을 때 호출되는 델리게이트 
        public delegate void NewClientHandle(Socket client_socket, object token);
        public NewClientHandle callback_on_newclient;

        public CListener()
        {
            this.callback_on_newclient = null;
        }

        public void start(string host, int port, int backlog)
        {
            //소켓을 생성
            this.listen_Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IPAddress address;
            if(host == "0.0.0.0")
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
                //소켓에 host 정보를 바인딩시킨 뒤 Listen 메서드 호출후 대기
                this.listen_Socket.Bind(endpoint);
                this.listen_Socket.Listen(backlog);

                this.accept_args = new SocketAsyncEventArgs();
                this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);
                
                //클라이언트가 들어오기를 대기
                //비동기 매서드 이므로 블로킹 되지 않고 바로 리턴
                //콜백 매서드 통해 접속 통보를 받음
                Thread listen_thread = new Thread(do_listen);
                listen_thread.Start();
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
        }

        void do_listen()
        {
            //accept 처리 제어를 위해 이벤트 객체 생성
            this.flow_control_event = new AutoResetEvent(false);

            while(true)
            {
                //SocketAsyncEventArgs를 재사용하기 위해 null 상태로 변환
                this.accept_args.AcceptSocket = null;

                bool pending = true;
                try
                {
                    //비동기 accept를 호출하여 클라이언트의 접속 수락
                    //비동기 메서드이지만 동기적으로 수행이 완료될 경우도 있으므로
                    //리턴값을 확인하여 분기 처리
                    pending = listen_Socket.AcceptAsync(this.accept_args);
                }
                catch(Exception e) 
                {
                    continue;
                }

                //즉시 완료(리턴값 == false)가 되면
                //이벤트가 발생하지 않으므로 콜백 매서드를 직접 호출해야 함
                //pending 상태라면 비동기 요청이 들어간 상태라는 뜻이며 콜백 메서드를 기다림
                if(!pending)
                {
                    on_accept_completed(null, this.accept_args);
                }

                //클라이언트 접속 처리가 완료되면 이벤트 객체의 신호를 전달받아 다시 루프 수행
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
                //TODO:Accept 실패 처리
                //Console.WriteLine("Failed to accept client.");
            }

            this.flow_control_event.Set();
        }
    }
}
