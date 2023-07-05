using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class Client
    {
        public static int DataBufferSize = 4096;

        public int MyId;
        public Tcp MyTcp;

        public Client(int _ClientId)
        {
            MyId = _ClientId;
            MyTcp = new Tcp(MyId);
        }
        
        public class Tcp
        {
            public TcpClient Socket;

            private readonly int Id;
            private NetworkStream Stream;
            private byte[] ReceiveBuffer;

            public Tcp(int _Id)
            {
                Id = _Id;
            }

            public void Connect(TcpClient _Socket)
            {
                Socket = _Socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                Stream = Socket.GetStream();

                ReceiveBuffer = new byte[DataBufferSize];

                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallBack, null);

                ServerSend.Welcome(Id, "Welcome To The Server");
            }

            public void SendData(Packet _Packet)
            {
                try
                {
                    if(Socket != null)
                    {
                        Stream.BeginWrite(_Packet.ToArray(), 0, _Packet.Length(), null, null);
                    }
                }
                catch(Exception _Ex)
                {
                    Console.WriteLine($"Error Sending Data To Player {Id} Via Tcp {_Ex}");
                }
            }

            private void ReceiveCallBack(IAsyncResult _Result)
            {
                try
                {
                    int _ByteLength = Stream.EndRead(_Result);
                    if(_ByteLength <= 0)
                    {
                        // TODO: 클라이언트 접속 종료
                        return;
                    }

                    byte[] _Data = new byte[_ByteLength];
                    Array.Copy(ReceiveBuffer, _Data, _ByteLength);
                }
                catch (Exception _Ex)
                {
                    Console.WriteLine($"Error Receiving TCP Data: {_Ex}");
                    //TODO: 클라이언트 접속 종료
                }
            }
        }
    }
}
