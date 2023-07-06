using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public Udp MyUdp;

        public Client(int _ClientId)
        {
            MyId = _ClientId;
            MyTcp = new Tcp(MyId);
            MyUdp = new Udp(MyId);
        }
        
        public class Tcp
        {
            public TcpClient Socket;

            private readonly int Id;
            private NetworkStream Stream;
            private Packet ReceiveData;
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

                ReceiveData = new Packet();
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

                    ReceiveData.Reset(HandleData(_Data));
                    Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallBack, null);
                }
                catch (Exception _Ex)
                {
                    Console.WriteLine($"Error Receiving TCP Data: {_Ex}");
                    //TODO: 클라이언트 접속 종료
                }
            }

            private bool HandleData(byte[] _Data)
            {
                int _PacketLength = 0;

                ReceiveData.SetBytes(_Data);

                if (ReceiveData.UnreadLength() >= 4)
                {
                    _PacketLength = ReceiveData.ReadInt();
                    if (_PacketLength <= 0)
                    {
                        return true;
                    }
                }

                while (_PacketLength > 0 && _PacketLength <= ReceiveData.UnreadLength())
                {
                    byte[] _PacketBytes = ReceiveData.ReadBytes(_PacketLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _Packet = new Packet(_PacketBytes))
                        {
                            int _PacketId = _Packet.ReadInt();
                            Server.PacketHandlers[_PacketId](Id, _Packet);
                        }
                    });

                    _PacketLength = 0;
                    if (ReceiveData.UnreadLength() >= 4)
                    {
                        _PacketLength = ReceiveData.ReadInt();
                        if (_PacketLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_PacketLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public class Udp
        {
            public IPEndPoint EndPoint;

            private int Id;

            public Udp(int _Id)
            {
                Id = _Id;
            }

            public void Connect(IPEndPoint _EndPoint)
            {
                EndPoint = _EndPoint;
            }

            public void SendData(Packet _Packet)
            {
                Server.SendUdpData(EndPoint, _Packet);
            }

            public void HandleData(Packet _PacketData)
            {
                int _PacketLength = _PacketData.ReadInt();
                byte[] _PacketBytes = _PacketData.ReadBytes(_PacketLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _Packet = new Packet(_PacketBytes))
                    {
                        int _PacketId = _Packet.ReadInt();
                        Server.PacketHandlers[_PacketId](Id, _Packet);
                    }
                })
            }
        }   
    }
}
