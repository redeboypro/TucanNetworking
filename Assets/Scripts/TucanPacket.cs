using System;
using System.Collections.Generic;
using System.Text;

using Float32 = System.Single;

public sealed class TucanPacket : IDisposable
{
    private const Int32 Int64Size = 8;
    private const Int32 Int32Size = 4;
    private const Int32 Int16Size = 2;
    private const Int32 Float32Size = 4;

    private readonly List<Byte> m_Buffer;
    
    private Int32 m_ReadPtr;
    private Boolean m_Disposed;

    public TucanPacket()
    {
        m_Buffer = new List<Byte>();
    }

    public Int32 BufferSize
    {
        get
        {
            return m_Buffer.Count;
        }
    }

    public Int32 UnreadSize
    {
        get
        {
            return BufferSize - m_ReadPtr;
        }
    }

    public Byte[] ToArray()
    {
        return m_Buffer.ToArray();
    }

    public Byte[] ReadBytes(Int32 size)
    {
        var data = m_Buffer.GetRange(m_ReadPtr, size);
        m_ReadPtr += size;
        return data.ToArray();
    }

    public Int16 ReadInt16()
    {
        return BitConverter.ToInt16(ReadBytes(Int16Size), 0);
    }

    public Int32 ReadInt32()
    {
        return BitConverter.ToInt32(ReadBytes(Int32Size), 0);
    }

    public Int64 ReadInt64()
    {
        return BitConverter.ToInt64(ReadBytes(Int64Size), 0);
    }

    public Float32 ReadFloat32()
    {
        return BitConverter.ToSingle(ReadBytes(4), 0);
    }

    public String ReadString(Encoding encoding)
    {
        var length = ReadInt32();
        return encoding.GetString(ReadBytes(length), 0, length);
    }
    
    public String ReadString()
    {
        return ReadString(Encoding.ASCII);
    }

    public Boolean TryReadBytes(Int32 length, out Byte[] data)
    {
        data = Array.Empty<Byte>();
        
        if (length > UnreadSize)
            return false;

        data = ReadBytes(length);
        return true;
    }

    public Boolean TryReadInt16(out Int16 data)
    {
        data = 0;
        
        if (Int16Size > UnreadSize)
            return false;

        data = ReadInt16();
        return true;
    }

    public Boolean TryReadInt32(out Int32 data)
    {
        data = 0;
        
        if (Int32Size > UnreadSize)
            return false;

        data = ReadInt32();
        return true;
    }

    public Boolean TryReadInt64(out Int64 data)
    {
        data = 0;
        
        if (Int64Size > UnreadSize)
            return false;

        data = ReadInt64();
        return true;
    }

    public Boolean TryReadFloat32(out Float32 data)
    {
        data = 0;
        
        if (Float32Size > UnreadSize)
            return false;

        data = ReadFloat32();
        return true;
    }

    public Boolean TryReadString(Encoding encoding, out String data)
    {
        data = string.Empty;

        if (!TryReadInt32(out var length)) 
            return true;
        
        if (length > UnreadSize)
            return false;

        data = encoding.GetString(ReadBytes(length), 0, length);

        return true;
    }

    public Boolean TryReadString(out String data)
    {
        return TryReadString(Encoding.ASCII, out data);
    }
    
    public void WriteBytes(IEnumerable<Byte> data)
    {
        m_Buffer.AddRange(data);
    }

    public void WriteInt16(Int16 data)
    {
        var bytes = BitConverter.GetBytes(data);
        m_Buffer.AddRange(bytes);
    }

    public void WriteInt32(Int32 data)
    {
        var bytes = BitConverter.GetBytes(data);
        m_Buffer.AddRange(bytes);
    }

    public void WriteInt64(Int64 data)
    {
        var bytes = BitConverter.GetBytes(data);
        m_Buffer.AddRange(bytes);
    }

    public void WriteFloat32(Float32 data)
    {
        var bytes = BitConverter.GetBytes(data);
        m_Buffer.AddRange(bytes);
    }

    public void WriteString(String data)
    {
        if (data == null) 
            return;
        
        WriteInt32(data.Length);
        m_Buffer.AddRange(Encoding.ASCII.GetBytes(data));
    }
    
    public void Clear()
    {
        m_Buffer.Clear();
        m_ReadPtr = 0;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (m_Disposed)
            return;
        
        if (disposing)
            Clear();
        
        m_Disposed = true;
    }

    ~TucanPacket()
    {
        Dispose(false);
    }
}