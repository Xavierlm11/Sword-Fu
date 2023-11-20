using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Server
{

}

//public class Message
//{
//    public Client client;
//    public string messageText;
//}

#region send classes
public class GenericSendClass
{
    public SendCode sendCode;
}
public class DebugMessage : GenericSendClass
{
    public DebugMessage()
    {
        
    }
    public DebugMessage(string ip, string nickname, string message)
    {
        senderIp = ip;
        senderNickname = nickname;
        debugMessageText = message;
        sendCode = SendCode.DebugMessage;
    }
    public string senderIp;
    public string senderNickname;
    public string debugMessageText;
}

//public class ConnectionRequest : GenericSendClass
//{
//    public ConnectionRequest()
//    {

//    }
//    public ConnectionRequest(string ip, string nickname)
//    {
//        senderIp = ip;
//        senderNickname = nickname;
//        sendCode = SendCode.ConnectionRequest;
//    }
//    public string senderIp;
//    public string senderNickname;
//}

public class ConnectionRequest : GenericSendClass
{
    public ConnectionRequest()
    {

    }
    public ConnectionRequest(Client client)
    {
        clientRequesting = client;
        sendCode = SendCode.ConnectionRequest;
    }
    public Client clientRequesting;
}

public class ConnectionConfirmation : GenericSendClass
{
    public ConnectionConfirmation()
    {

    }
    public ConnectionConfirmation(bool accepted)
    {
        acceptedConnection = accepted;
        sendCode = SendCode.ConnectionConfirmation;
    }

    public bool acceptedConnection;
    public string reasonToDeny;
}

#endregion

public class Client
{
    public bool isHost;
    public string nickname;
    public string localIp;

    public Client()
    {

    }
    public Client(string ip, string nick, bool host = false)
    {
        nickname = nick;
        localIp = ip; 
        isHost = host;
    }
}

public enum TransportType
{
    UDP,
    TCP
}

public enum SendCode
{
    ConnectionRequest,
    ConnectionConfirmation,
    DebugMessage,
    PlayerPositions
}

[CreateAssetMenu(fileName = "NetworkManager", menuName = "ScriptableObjects/NetworkManager")]
public class NetworkManager : SingletonScriptableObject<NetworkManager>
{
    #region variables

    public string udpString;
    public string tcpString;

    public string lobbySceneName;
    public string serverSceneName;
    public string clientSceneName;
    public string chatSceneName;

    [SerializeField]
    private TransportType _transportType;

    public TransportType transportType
    {
        get
        {
            return _transportType;
        }

        private set
        {
            _transportType = value;
            ConnectionManager.Instance.SetSocket();
        }
    }
        

    public int port;
    public int messageMaxBytes;
    public bool hasInitialized;

    [SerializeField]
    private Client localClient;

    [SerializeField]
    public List<Client> clients = new List<Client>();


    #endregion

    //__________________________________________________________________________

    #region methods

    public void SetLocalClient(Client cl)
    {
        localClient = cl;
    }

    public Client GetLocalClient()
    {
        return localClient;
    }

    public void OnClick_ChangeTransportType(TMP_Dropdown dropdown)
    {
        switch (dropdown.options[dropdown.value].text)
        {
            case "UDP":
                ChangeTransportType(TransportType.UDP);
                break;
            case "TCP":
                ChangeTransportType(TransportType.TCP);
                break;
        }
    }

    private void ChangeTransportType(TransportType newTransport)
    {
        if (transportType != newTransport)
        {
            transportType = newTransport;
        }
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

    public Client CreateClient(string nick, string ip, bool isHost = false)
    {
        Client newClient = new Client(nick, ip, isHost);
        return newClient;
    }

    #endregion
}
