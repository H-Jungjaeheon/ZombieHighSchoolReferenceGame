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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        else if(Instance != this)
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
        MyTcp.Connect();
    }

    public class Tcp
    {
        public TcpClient Socket;

        private NetworkStream Stream;
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

            if(!Socket.Connected)
            {
                return;
            }

            Stream = Socket.GetStream();

            Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _Result)
        {
            try
            {
                int _ByteLength = Stream.EndRead(_Result);
                if(_ByteLength <= 0 ) 
                {
                    //TODO: 클라이언트 접속 종료
                    return;
                }

                byte[]_Data = new byte[_ByteLength];
                Array.Copy(ReceiveBuffer, _Data, _ByteLength);

                //TODO: Handle Data
                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch(Exception e)
            {
                //TODO: 클라이언트 접속 종료
            }
        }
    }
}
