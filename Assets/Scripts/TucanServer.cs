using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class TucanServer
{
    private readonly List<IPEndPoint> m_Clients;
    private readonly TucanPacket m_TempBuffer;
    private readonly UdpClient m_Socket;
    
    private Boolean m_Running;
    
    public TucanServer(
        Int32 port,
        Int32 clientCount = Int32.MaxValue,
        Boolean waitForAll = false)
    {
        m_Clients = new List<IPEndPoint>();
        m_TempBuffer = new TucanPacket();
        m_Socket = new UdpClient(port);

        Handle(clientCount, waitForAll);
    }
    
    public Action<TucanPacket> ReceivePacket { get; set; }
    public Action<IPEndPoint> ClientConnect { get; set; }
    public Action<IPEndPoint> ClientDisconnect { get; set; }

    public void Stop()
    {
        m_Running = false;
        m_Socket.Close();
    }

    private async void Handle(Int32 clientCount, Boolean waitForAll)
    {
        await Task.Run(() =>
        {
            m_Running = true;
            while (m_Running)
            {
                var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                
                Byte[] receiveBuffer;
                try
                {
                    receiveBuffer = m_Socket.Receive(ref clientEndPoint);
                }
                catch
                {
                    continue;
                }
                
                if (receiveBuffer.Length > 0)
                {
                    m_TempBuffer.Clear();
                    m_TempBuffer.WriteBytes(receiveBuffer);
                    ReceivePacket?.Invoke(m_TempBuffer);
                }

                if (!m_Clients.Contains(clientEndPoint))
                {
                    if (m_Clients.Count >= clientCount)
                        continue;
                
                    ClientConnect?.Invoke(clientEndPoint);
                    m_Clients.Add(clientEndPoint);
                    continue;
                }

                if (waitForAll && m_Clients.Count < clientCount)
                    continue;

                foreach (var client in m_Clients.Where(client => !client.Equals(clientEndPoint)))
                {
                    try
                    {
                        m_Socket.Send(receiveBuffer, receiveBuffer.Length, client);
                    }
                    catch
                    {
                        ClientDisconnect?.Invoke(client);
                        m_Clients.Remove(client);
                    }
                }
            }
            m_Socket.Close();
        });
    }
}
