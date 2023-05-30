using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
                this.listen_Socket.Bind(endpoint);
                this.listen_Socket.Listen(backlog);

                this.accept_args = new SocketAsyncEventArgs();
                this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

                this.listen_Socket.AcceptAsync(this.accept_args);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
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
