using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum TransportType
{
    UDP,
    TCP
}

[CreateAssetMenu(fileName = "NetworkSettings", menuName = "ScriptableObjects/NetworkSettings")]
public class NetworkSettings : SingletonScriptableObject<NetworkSettings>
{
    public static TransportType transportType;

    public int port;
    public int messageMaxBytes;

    public static void SetEndPoint(ref IPEndPoint ipEndPoint, IPAddress ipAddess, int port)
    {
        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(“-----”),port);
        ipEndPoint = new IPEndPoint(ipAddess, port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    private static Socket StartNetwork_UDP()
    {
        return new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);
    }

    private static Socket StartNetwork_TCP()
    {
        return new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
    }

    public static Socket StartNetwork()
    {
        Socket socket = null;

        switch (transportType)
        {
            case TransportType.UDP:
                socket = StartNetwork_UDP();
                break;
            case TransportType.TCP:
                socket = StartNetwork_TCP();
                break;
        }

        return socket;

    }
}
