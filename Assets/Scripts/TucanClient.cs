using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

using Float32 = System.Single;

public class TucanClient : MonoBehaviour
{
    private readonly TucanPacket m_SendBuffer, m_ReceiveBuffer;
    private readonly UdpClient m_Socket;
    private readonly IPEndPoint m_ServerEndPoint;

    private bool m_Running;

    public TucanClient(String address, Int32 port)
    {
        m_SendBuffer = new TucanPacket();
        m_ReceiveBuffer = new TucanPacket();
        
        m_Socket = new UdpClient();
        m_ServerEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        
        m_Socket.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        
        Send();
        Handle();
    }

    public Action<TucanPacket> ReceivePacket { get; set; }

    public UdpClient GetSocket()
    {
        return m_Socket;
    }
    
    public IPEndPoint GetServerEndPoint()
    {
        return m_ServerEndPoint;
    }

    public Int32 GetBufferSize()
    {
        return m_SendBuffer.BufferSize;
    }

    public void WriteBytesToBuffer(IEnumerable<Byte> data)
    {
        m_SendBuffer.WriteBytes(data);
    }

    public void WriteInt16ToBuffer(Int16 data)
    {
        m_SendBuffer.WriteInt16(data);
    }

    public void WriteInt32ToBuffer(Int32 data)
    {
        m_SendBuffer.WriteInt32(data);
    }

    public void WriteInt64ToBuffer(Int64 data)
    {
        m_SendBuffer.WriteInt64(data);
    }

    public void WriteFloat32ToBuffer(Float32 data)
    {
        m_SendBuffer.WriteFloat32(data);
    }

    public void WriteStringToBuffer(String data)
    {
        m_SendBuffer.WriteString(data);
    }

    public void ClearBuffer()
    {
        m_SendBuffer.Clear();
    }

    public void Send()
    {
        var buffer = m_SendBuffer.ToArray();
        m_Socket.Send(buffer, buffer.Length, m_ServerEndPoint);
    }

    public void Disconnect()
    {
        m_Running = false;
        m_Socket.Close();
    }

    private async void Handle()
    {
        await Task.Run(() =>
        {
            m_Running = true;
            while (m_Running)
            {
                var serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveBytes = m_Socket.Receive(ref serverEndPoint);

                m_ReceiveBuffer.Clear();
                m_ReceiveBuffer.WriteBytes(receiveBytes);
                
                ReceivePacket?.Invoke(m_ReceiveBuffer);
            }
        });
    }
}
