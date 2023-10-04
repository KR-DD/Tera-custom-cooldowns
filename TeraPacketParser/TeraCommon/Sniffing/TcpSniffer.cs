﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using PacketDotNet;

namespace TeraPacketParser.TeraCommon.Sniffing;

public class TcpSniffer
{
    readonly ConcurrentDictionary<ConnectionId, TcpConnection> _connections = new();

    readonly object _lock = new();
    readonly string? _snifferType;
    public TcpSniffer(IpSniffer ipSniffer)
    {
        ipSniffer.PacketReceived += Receive;
        _snifferType = ipSniffer.GetType().FullName;
        //Task.Run(()=>ParsePacketsLoop());
    }


    public event Action<TcpConnection?>? NewConnection;
    public event Action<TcpConnection?>? EndConnection;

    protected void OnNewConnection(TcpConnection connection)
    {
        var handler = NewConnection;
        handler?.Invoke(connection);
    }
    protected void OnEndConnection(TcpConnection? connection)
    {
        var handler = EndConnection;
        handler?.Invoke(connection);
    }

    internal void RemoveConnection(TcpConnection connection)
    {
        if (_connections.ContainsKey(connection.ConnectionId))
            _connections.TryRemove(connection.ConnectionId, out _);
    }

    //private void ParsePacketsLoop()
    //{
    //    while (true)
    //    {
    //        QPacket toProcess;
    //        if (_buffer.TryDequeue(out toProcess))
    //            toProcess.Connection.HandleTcpReceived(toProcess.SequenceNumber, toProcess.Packet);
    //        else System.Threading.Thread.Sleep(1);
    //    }
    //}

    void Receive(IPv4Packet ipData)
    {
        var tcpPacket = ipData.PayloadPacket as TcpPacket;
        if (tcpPacket == null || tcpPacket.DataOffset*4 > ipData.PayloadLength) return;
        //if (tcpPacket.Checksum!=0 && !tcpPacket.ValidTCPChecksum) return;
        var isFirstPacket = tcpPacket.Synchronize;
        var connectionId = new ConnectionId(ipData.SourceAddress, tcpPacket.SourcePort, ipData.DestinationAddress,
            tcpPacket.DestinationPort);


        TcpConnection? connection;
        bool isInterestingConnection;
        if (isFirstPacket)
        {
            connection = new TcpConnection(connectionId, tcpPacket.SequenceNumber, RemoveConnection, _snifferType);
            OnNewConnection(connection);
            isInterestingConnection = connection.HasSubscribers;
            if (!isInterestingConnection) return;
            _connections[connectionId] = connection;
            Debug.Assert(tcpPacket.PayloadData.Length == 0);
        }
        else
        {
            isInterestingConnection = _connections.TryGetValue(connectionId, out connection);
            if (!isInterestingConnection) return;
            byte[] payload;
            try { payload = tcpPacket.PayloadData; } catch { return; }
            //_buffer.Enqueue(new QPacket(connection, tcpPacket.SequenceNumber, tcpPacket.Payload));
            lock (_lock)
            {
                if (tcpPacket.Finished|| tcpPacket.Reset) {OnEndConnection(connection); return;}
                connection?.HandleTcpReceived(tcpPacket.SequenceNumber, payload);
            }
            //if (!string.IsNullOrEmpty(TcpLogFile))
            //    File.AppendAllText(TcpLogFile,
            //        string.Format("{0} {1}+{4} | {2} {3}+{4} ACK {5} ({6})\r\n",
            //            connection.CurrentSequenceNumber, tcpPacket.SequenceNumber, connection.BytesReceived,
            //            connection.SequenceNumberToBytesReceived(tcpPacket.SequenceNumber),
            //            tcpPacket.Payload.Count, tcpPacket.AcknowledgementNumber,
            //            connection.BufferedPacketDescription));
        }
    }
}