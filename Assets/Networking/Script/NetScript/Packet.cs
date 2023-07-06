using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameServer
{
    /// <summary>서버에서 클라이언트로 전송한다.</summary>
    public enum ServerPackets
    {
        welcome = 1,
        spawnPlayer,
        playerPosition,
        playerRotation
    }

    /// <summary>클라이언트에서 서버로 전송한다.</summary>
    public enum ClientPackets
    {
        welcomeReceived = 1,
        playerMovement,
    }

    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        /// <summary>새 빈 패킷을 만든다(ID 없음).</summary>
        public Packet()
        {
            buffer = new List<byte>(); // 버퍼 초기화
            readPos = 0; // 읽는 위치 0으로 설정
        }

        /// <summary>지정된 ID로 새 패킷을 생성, 송신에 사용된다.</summary>
        /// <param name="_id">패킷 ID</param>
        public Packet(int _id)
        {
            buffer = new List<byte>(); // 버퍼 초기화
            readPos = 0; // 읽는 위치 0으로 설정

            Write(_id); // 버퍼에 패킷 Id 작성
        }

        /// <summary>데이터를 읽을 수 있는 패킷을 생성, 송신에 사용된다.</summary>
        /// <param name="_data">패킷에 추가할 Byte</param>
        public Packet(byte[] _data)
        {
            buffer = new List<byte>(); // 버퍼 초기화
            readPos = 0; // 읽는 위치 0으로 설정

            SetBytes(_data);
        }

        #region Functions
        /// <summary>패킷의 내용 설정, 읽을 수 있도록 준비한다.</summary>
        /// <param name="_data">패킷에 추가할 Byte</param>
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        /// <summary>버퍼 시작 부분에 패킷 내용의 길이를 삽입한다.</summary>
        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // 버퍼 처음에 패킷의 바이트 길이를 삽입.
        }

        /// <summary>버퍼의 시작 부분에 주어진 int를 삽입한다.</summary>
        /// <param name="_value">삽입할 Int</param>
        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value)); // 버퍼 처음에 바이트로 변환된 int 삽입
        }

        /// <summary>패킷의 내용을 배열 형식으로 가져온다.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>패킷의 길이를 가져온다.</summary>
        public int Length()
        {
            return buffer.Count; // 버퍼의 길이를 리턴
        }

        /// <summary>패킷에 포함된 읽지 않은 데이터의 길이를 가져온다.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // 남은 길이 반환(읽지 않음)
        }

        /// <summary>재사용할 수 있도록 패킷 인스턴스를 재설정한다.</summary>
        /// <param name="_shouldReset">패킷을 재설정할지 여부</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear(); // 버퍼 지움
                readableBuffer = null;
                readPos = 0; // 읽을 위치 초기화
            }
            else
            {
                readPos -= 4; // 마지막으로 읽은 int를 "읽지 않음"
            }
        }
        #endregion

        #region Write Data
        /// <summary>패킷에 Byte형 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Byte</param>
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }
        /// <summary>패킷에 Byte Array형 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Byte Array</param>
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }
        /// <summary>패킷에 Short형 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Short.</param>
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>패킷에 Int형 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Int.</param>
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>패킷에 Long형 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Long.</param>
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>패킷에 Float 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Float.</param>
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>패킷에 Bool 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 Bool.</param>
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>패킷에 String 데이터를 추가한다.</summary>
        /// <param name="_value">추가 할 String.</param>
        public void Write(string _value)
        {
            Write(_value.Length); // 패킷에 문자열 길이 추가
            buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // 문자열 자체 추가
        }
        /// <summary>
        /// 패킷에 Vector3 데이터를 추가한다.
        /// </summary>
        /// <param name="_value"> 추가 할 Vector3</param>
        public void Write(Vector3 _value)
        {
            //패킷에 X, Y, Z값을 개별 저장
            Write(_value.x);
            Write(_value.y);
            Write(_value.z);
        }
        /// <summary>
        /// 패킷에 Quaternion 데이터를 추가한다.
        /// </summary>
        /// <param name="_value">추가 할 Quaternion</param>
        public void Write(Quaternion _value)
        {
            //패킷에 X, Y, Z, W값을 개별 저장
            Write(_value.x);
            Write(_value.y);
            Write(_value.z);
            Write(_value.w);
        }
        #endregion

        #region Read Data
        /// <summary>패킷에서 Byte를 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                byte _value = readableBuffer[readPos]; // readPos의 위치에서 바이트 가져오기
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += 1; // readPos를 1 증가
                }
                return _value; // Byte를 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>패킷에서 ByteArray를 읽는다.</summary>
        /// <param name="_length">바이트 배열의 길이</param>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                byte[] _value = buffer.GetRange(readPos, _length).ToArray(); // _length 범위의 readPos 위치에서 바이트 가져오기
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += _length; // readPos를 _length만큼 증가
                }
                return _value; // ByteArray를 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>패킷에서 short를 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                short _value = BitConverter.ToInt16(readableBuffer, readPos); // 바이트를 short로 변환
                if (_moveReadPos)
                {
                    // _moveReadPos가 true이고 읽지 않은 바이트가 있는 경우
                    readPos += 2; // readPos를 2 증가
                }
                return _value; // Short를 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>패킷에서 int를 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                int _value = BitConverter.ToInt32(readableBuffer, readPos); // Byte를 Int로 변환
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += 4; // readPos를 4 증가
                }
                return _value; // Int를 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>패킷에서 long을 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                long _value = BitConverter.ToInt64(readableBuffer, readPos); // 바이트를 long으로 변환
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += 8; // readPos를 8 증가
                }
                return _value; // Long 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>패킷에서 Float을 읽습니다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                float _value = BitConverter.ToSingle(readableBuffer, readPos); // 바이트를 Float로 변환
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += 4; // readPos를 4 증가
                }
                return _value; // Float 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>패킷에서 Bool을 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // 읽지 않은 바이트가 있는 경우
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos); // 바이트를 Bool로 변환
                if (_moveReadPos)
                {
                    // _moveReadPos가 true인 경우
                    readPos += 1; // readPos를 1 증가
                }
                return _value; // Bool 반환
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>패킷에서 String을 읽는다.</summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); // 문자열의 길이 구하기
                string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); // Byte를 String로 변환
                if (_moveReadPos && _value.Length > 0)
                {
                    // _moveReadPos가 참이면 문자열이 비어 있지 않습니다.
                    readPos += _length; // 문자열 길이만큼 readPos 증가
                }
                return _value; // 문자열을 반환
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        /// <summary>
        /// 패킷에서 Vector3를 읽는다.
        /// </summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public Vector3 ReadVector3(bool _moveReadPos = true)
        {
            return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        /// <summary>
        /// 패킷에서 Quaternion를 읽는다.
        /// </summary>
        /// <param name="_moveReadPos">버퍼의 읽기 위치를 이동할지 여부</param>
        public Quaternion ReadQuaternion(bool _moveReadPos = true)
        {
            return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }
        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}