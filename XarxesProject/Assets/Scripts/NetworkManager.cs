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
    public GenericSendClass()
    {

    }

    public GenericSendClass(bool isNotJsonConversion)
    {
        sender = NetworkManager.Instance.GetLocalClient();
    }

    public SendCode sendCode;
    public Client sender;
    public string remoteIP;
    public int remotePort;
    public List<Client> receivers = new List<Client>();
    public TransferType transferType;

    public void SetReceivers(TransferType transferType)
    {
        switch (transferType)
        {
            case TransferType.AllClients:
                break;
            case TransferType.OnlyClients:
                break;
            case TransferType.Host:
                break;
            case TransferType.Custom:
                break;
        }
    }
}
public class DebugMessage : GenericSendClass
{
    public DebugMessage()
    {
        
    }

    public DebugMessage(string ip, string nickname, string message) : base(true)
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

public class ConnectionRequest : GenericSendClass
{
    public ConnectionRequest()
    {

    }

    public ConnectionRequest(Client client) : base(true)
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

    public ConnectionConfirmation(bool accepted, string reason) : base(true)
    {
        acceptedConnection = accepted;
        reasonToDeny = reason;
        sendCode = SendCode.ConnectionConfirmation;
    }

    public bool acceptedConnection;
    public string reasonToDeny;
}

public class ClientListUpdate : GenericSendClass
{
    public ClientListUpdate()
    {

    }

    public ClientListUpdate(List<Client> clients) : base(true)
    {
        clientList = clients;
        sendCode = SendCode.ClientListUpdate;
    }

    public List<Client> clientList;
}

public class RoomInfoUpdate : GenericSendClass
{
    public RoomInfoUpdate()
    {

    }

    public RoomInfoUpdate(Room roomToUpdate) : base(true)
    {
        room = roomToUpdate;
        sendCode = SendCode.RoomInfoUpdate;
    }

    public Room room;
}

//public class SendIdPlayer : GenericSendClass
//{
//    public int playerId =2;
//   // public string playerIp;
//    public SendIdPlayer()
//    {

//    }
//    public SendIdPlayer(ConnectionManager.PartyPlayersInfo ppi)
//    {
//        playerId = ppi.playerID;
//        sendCode = SendCode.SendIdPlayer;
//        //playerIp = ppi.playerInfo.playerIp;
//    }

//}

public class SendTransPlayer : GenericSendClass
{
    public Vector3 pos;
    public float angRot;
    public int playerId;
   // public string playerIp;
    public SendTransPlayer()
    {

    }

    public SendTransPlayer(ConnectionManager.PlayerPositionsInfo ppi) : base(true)
    {
        ConnectionManager.PlayerPosition newPos = ppi.playerPositions;
        pos = new Vector3(newPos.positionX,newPos.positionY,newPos.positionZ);
        angRot = newPos.rotY;
        playerId = newPos.playerId;
        sendCode = SendCode.PlayerPositions;
        //playerIp = ppi.playerInfo.playerIp;
    }
    
}


#endregion

public class Client
{
    public bool isHost;
    public string nickname;
    public string localIp;
    public int localPort;

    public string globalIP;
    public int globalPort;

    public Client()
    {

    }

    public Client(string ip, string nick, int port, bool host = false)
    {
        nickname = nick;
        localIp = ip;
        localPort = port;
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
    PlayerPositions,
    PartyManager,
    SendIdPlayer,
    ClientListUpdate,
    RoomInfoUpdate
}

public enum TransferType
{
    AllClients,
    OnlyClients,
    Host,
    Custom
}

public class Room
{
    public Room()
    {

    }

    public Client host;
    public List<Client> clients = new List<Client>();
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

    //public int defaultServerPort;
    //public int defaultClientPort;

    public int defaultPort;
    public int localPort;
    //public int remotePort;

    public string localIp;
    public string remoteIp;

    public int maxTransferedDataSize;
    public bool hasInitialized;

    [SerializeField]
    private Client localClient;

    public Room activeRoom;

    //[SerializeField]
    //public List<Client> clients = new List<Client>();

    public bool appIsQuitting;

    public bool allowSameIPInRoom;

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

    //public void UpdateLocalPort(int newPort)
    //{
    //    localPort = newPort;
    //}

    //public void UpdateRemotePort(int newPort)
    //{
    //    remotePort = newPort;
    //}

    public void UpdateRemoteIP(string newIp)
    {
        ////Debug.Log("IP Edited");
        //if (!RemoteIpIsEmpty())
        //{
        //    ConnectionManager.Instance.SetRemoteIP(remoteIpField.text);
        //}
        remoteIp = newIp;
    }

    public void UpdateLocatIP(string newIp)
    {
        localIp = newIp;
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

    public Client CreateClient(string nick, string ip, int port, bool isHost = false)
    {
        Client newClient = new Client(ip, nick, port, isHost);
        return newClient;
    }

    public void DeleteClients()
    {
        if (activeRoom != null)
        {
            for (int i = activeRoom.clients.Count - 1; i >= 0; i--)
            {
                activeRoom.clients[i] = null;
            }

            activeRoom.clients.Clear();
        }

        localClient = null;
    }

    #endregion
}
