using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TransportType
{
    UDP,
    TCP
}

[CreateAssetMenu(fileName = "NetworkSettings", menuName = "ScriptableObjects/NetworkSettings")]
public class NetworkSettings : SingletonScriptableObject<NetworkSettings>
{
    public string lobbySceneName;
    public string serverSceneName;
    public string clientSceneName;

    public TransportType transportType;

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

        switch (Instance.transportType)
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

    public static void OnClick_GoToServerScene()
    {
        SceneManager.LoadScene(Instance.serverSceneName);
    }

    public static void OnClick_GoToClientScene()
    {
        SceneManager.LoadScene(Instance.clientSceneName);
    }

    public static void OnClick_GoToLobbyScene()
    {
        SceneManager.LoadScene(Instance.lobbySceneName);
    }
}
