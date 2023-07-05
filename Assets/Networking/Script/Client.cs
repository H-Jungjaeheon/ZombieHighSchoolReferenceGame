using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int DataBufferSize = 4096;

    public string Ip = "127.0.0.1";
    public int Port = 26950;
    public int MyId = 0;
    public Tcp MyTcp;

    private delegate void PacketHandler(Packet _Packet);
    private static Dictionary<int, PacketHandler> PacketHandlers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        else if (Instance != this)
        {
            Debug.Log("Instance Already Exists, Destroying Object");
            Destroy(this);
        }
    }

    private void Start()
    {
        MyTcp = new Tcp();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        MyTcp.Connect();
    }

    public class Tcp
    {
        public TcpClient Socket;

        private NetworkStream Stream;
        private Packet ReceiveData;
        private byte[] ReceiveBuffer;

        public void Connect()
        {
            Socket = new TcpClient
            {
                ReceiveBufferSize = DataBufferSize,
                SendBufferSize = DataBufferSize
            };

            ReceiveBuffer = new byte[DataBufferSize];
            Socket.BeginConnect(Instance.Ip, Instance.Port, ConnectCallback, null);
        }

        private void ConnectCallback(IAsyncResult _Result)
        {
            Socket.EndConnect(_Result);

            if (!Socket.Connected)
            {
                return;
            }

            Stream = Socket.GetStream();

            ReceiveData = new Packet();

            Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _Result)
        {
            try
            {
                int _ByteLength = Stream.EndRead(_Result);
                if (_ByteLength <= 0)
                {
                    //TODO: 클라이언트 접속 종료
                    return;
                }

                byte[] _Data = new byte[_ByteLength];
                Array.Copy(ReceiveBuffer, _Data, _ByteLength);

                ReceiveData.Reset(HandleData(_Data));
                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
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
                        PacketHandlers[_PacketId](_Packet);
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

    private void InitializeClientData()
    {
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome }
        };
        Debug.Log("Initialized Packet");
    }
}
