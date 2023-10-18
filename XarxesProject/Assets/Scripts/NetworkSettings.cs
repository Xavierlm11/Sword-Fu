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
    private bool hasInitialized;

    public static void SetEndPoint(ref IPEndPoint ipEndPoint, IPAddress ipAddess, int port)
    {
        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(“-----”),port);
        ipEndPoint = new IPEndPoint(ipAddess, port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    private void OnEnable()
    {
        hasInitialized = true;
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
        Instance.hasInitialized = false;
        SceneManager.LoadScene(Instance.serverSceneName);
    }

    public static void OnClick_GoToClientScene()
    {
        Instance.hasInitialized = false;
        SceneManager.LoadScene(Instance.clientSceneName);
    }

    public static void Call_GoToLobbyScene()
    {
        if (Instance.hasInitialized)
        {
            SceneManager.LoadScene(Instance.lobbySceneName);
            Instance.hasInitialized = false;
        }
    }

    public static void GoToLobbyScene()
    {
        SceneManager.LoadScene(Instance.lobbySceneName);
    }

    public static void OnClick_GoToLobbyScene()
    {
        Instance.hasInitialized = false;
        GoToLobbyScene();
    }
}
