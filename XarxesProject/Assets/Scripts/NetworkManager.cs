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
        hasToCheckTargets = true;
    }

    public SendCode sendCode;
    public Client sender;
    public string remoteIP;
    public int remotePort;
    public List<Client> receivers = new List<Client>();
    public TransferType transferType;
    public bool hasToCheckTargets;

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
    
    public void CheckTargets()
    {
        hasToCheckTargets = false;
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

public class StartGame : GenericSendClass
{
    public StartGame()
    {

    }

    public StartGame(bool isNotJsonConversion) : base(true)
    {
        sendCode = SendCode.StartGame;
    }

}

public class PlayerTransform : GenericSendClass
{

    public PlayerTransform()
    {

    }

    public PlayerTransform(bool isNotJsonConversion) : base(true)
    {
        sendCode = SendCode.PlayerTransform;
    }

    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public PlayerInfo player;

}

public class FallSword : GenericSendClass
{

    public FallSword()
    {

    }

    public FallSword(Vector3 pos) : base(true)
    {
        positionX = pos.x;
        positionY = pos.y;
        positionZ = pos.z;

        sendCode = SendCode.FallSword;
    }

    public float positionX;
    public float positionY;
    public float positionZ;

    public PlayerInfo player;

}

public class CollectFallSword : GenericSendClass
{

    public CollectFallSword()
    {

    }

    public CollectFallSword(bool isNotJsonConversion) : base(true)
    {
        sendCode = SendCode.CollectFallSword;
    }

    public PlayerInfo playerWhoCollect;
    public PlayerInfo ownerOfSword;
}

public class SwordTransform : GenericSendClass
{

    public SwordTransform()
    {

    }

    public SwordTransform(bool isNotJsonConversion) : base(true)
    {
        sendCode = SendCode.SwordTransform;
    }

    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public PlayerInfo player;
}

public class MeleeAttack : GenericSendClass
{
    public MeleeAttack()
    {

    }

    public MeleeAttack(PlayerInfo playerInfo) : base(true)
    {
        player = playerInfo;
        sendCode = SendCode.MeleeAttack;
    }

    public PlayerInfo player;

}

public class DistanceAttack : GenericSendClass
{
    public DistanceAttack()
    {

    }

    public DistanceAttack(PlayerInfo playerInfo) : base(true)
    {
        player = playerInfo;
        sendCode = SendCode.DistanceAttack;
    }

    public PlayerInfo player;

}

public class UpdateParty : GenericSendClass
{
    public Party party;

    public UpdateParty()
    {

    }

    public UpdateParty(Party updatedParty) : base(true)
    {
        party = updatedParty;
        sendCode = SendCode.UpdateParty;
    }

}

public class UpdateGameplayHost : GenericSendClass
{
    public int nextLvl;
    public bool isEnd;
    public bool isRestart;
    public bool isLvlSync;
    public int lvlIndex;
    //public string winnerName;

    public UpdateGameplayHost()
    {

    }
    public UpdateGameplayHost(int nextLvl, bool isEnd, bool isRestart, bool isLvlSync, int lvlIndex) : base(true)
    {
        this.nextLvl = nextLvl;
        this.isEnd = isEnd;
        this.isRestart = isRestart;
        this.isLvlSync = isLvlSync;
        this.lvlIndex = lvlIndex;
        sendCode = SendCode.UpdateGameplayHost;
    }   
}

public class UpdateGameplay : GenericSendClass
{
    
    public bool isPaused;
    public bool isDead;
    public bool isChangePhase;

    public int playerId;

    public UpdateGameplay()
    {

    }
    public UpdateGameplay(bool isPaused, int playerId, bool isDead, bool isChangePhase) : base(true)
    {
        sendCode = SendCode.UpdateGameplay;
        this.isPaused = isPaused;
        this.playerId = playerId;

        this.isDead = isDead;
        this.isChangePhase = isChangePhase;
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

    //public int networkID;

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
    PlayerTransform,
    PartyManager,
    SendIdPlayer,
    ClientListUpdate,
    RoomInfoUpdate,
    StartGame,
    UpdateParty,
    UpdateGameplayHost,
    UpdateGameplay,
    MeleeAttack,
    DistanceAttack,
    SwordTransform,
    FallSword,
    CollectFallSword
}

public enum TransferType
{
    AllClients,
    OnlyClients,
    Host,
    AllExceptLocal,
    Custom
}

public class Room
{
    public Room()
    {

    }

    public Client host;
    public List<Client> clients = new List<Client>();
    public Party party;
}

//public class PlayerTransformInfo
//{
//    public PlayerTransformInfo()
//    {

//    }

//    public float positionX;
//    public float positionY;
//    public float positionZ;
//    public float rotY;

//    public PlayerInfo player;

//}

public enum InterpolationMode
{
    None,
    Lerp,
    Slerp,
    SmoothStep,
}

public enum CharacterModel
{
    None,
    BigHead,
    Gothic,
    Granny,
    CapBoy
}

public class PlayerInfo
{
    public PlayerInfo()
    {

    }

    public PlayerInfo(Client cl)
    {
        client = cl;
    }

    public Client client;
    public CharacterModel characterModel;
    public bool isSpectator = false;
}

public class Party
{
    public Party()
    {

    }


    public List<PlayerInfo> players = new List<PlayerInfo>();
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

    public bool useNetworkUpdateInterval;
    public float networkUpdateInterval;

    public InterpolationMode movementInterpolation;
    public InterpolationMode rotationInterpolation;

    public int maxPlayers;

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

    public PlayerInfo GetLocalPlayer()
    {
        if (activeRoom != null && activeRoom.party != null && activeRoom.party.players != null)
        {
            foreach (PlayerInfo pl in activeRoom.party.players)
            {
                if (pl.client.nickname == GetLocalClient().nickname)
                {
                    return pl;
                }
            }
        }
        return null;
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
