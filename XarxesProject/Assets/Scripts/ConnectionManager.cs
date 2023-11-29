using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

public class ConnectionManager : MonoBehaviour
{
    #region variables

    public static ConnectionManager Instance;

    [SerializeField]
    public IPEndPoint ipEndPointToReceive;

    [SerializeField]
    public IPEndPoint ipEndPointToSend;

    [SerializeField]
    private Thread networkThreadToSendData;
    [SerializeField]
    private Thread networkThreadToReceiveConnections;
    [SerializeField]
    private Thread networkThreadToReceiveData;

    //public Socket socketToReceive;
    //public Socket socketToSend;
    public Socket socket;
    public Socket connectedClientSocket;

    public bool isMessage;

    [SerializeField]
    private byte[] transferedDataBuffer;

    //[SerializeField]
    //private string transferedData;

    [SerializeField]
    private int transferedDataSize;

    [SerializeField]
    private int maxTransferedDataSize;

    [SerializeField]
    private PartyManager partyObj;

    [Serializable]
    public class PlayerPositionsInfo
    {
        public string playerIp;
        public PlayerPosition playerPositions;
    }

    [Serializable]
    public class PlayerPosition
    {
        public string playerName;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotY;
        public int playerId;
    }

    [SerializeField]
    public class PartyPlayersInfo
    {
        public string playerName;
        public int playerID;
        public int playersConected;

        public PlayerPositionsInfo playerInfo;

    }

    private Action dataMethod;


    private MemoryStream stream;
    private BinaryWriter binaryWriter;
    private BinaryReader binaryReader;

    #endregion

    //______________________________________________________________________________________________

    #region methods

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        
    }

    //This is called when the user tries to create/join a room. It connects the player to the network
    public void StartConnections()
    {
        //Sets the maximum transfered data size
        StartReceivingMessages();

        //Starts being able to receive data
        OpenNewThreat_Receive();
        //Starts being able to send data
        OpenNewThreat_Send();
    }

    //Creates the socket
    public void SetSocket()
    {
        socket = NetworkManager.StartNetwork();
    }

    public void StartReceivingMessages()
    {
        transferedDataSize = NetworkManager.Instance.messageMaxBytes;

        //Debug.Log("EndPoint set and socket binded");
    }

    private void WaitForClient()
    {
        //Blocking
        connectedClientSocket = socket.Accept();
        IPEndPoint clientIpEndPoint = (IPEndPoint)connectedClientSocket.RemoteEndPoint;
        Debug.Log("Connected with [" + clientIpEndPoint.Address.ToString() + "] at port [" + clientIpEndPoint.Port.ToString() + "]");
    }

    private void Update()
    {
        //if (isMessage)
        //{
        //    CallToChat();
        //}
    }

    private void CallToChat()
    {
        isMessage = false;
    }

    private void OpenNewThreat_Send()
    {
        networkThreadToSendData = new Thread(SendNetworkData);
    }

    private void OpenNewThreat_Receive()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                networkThreadToReceiveData = new Thread(ReceiveData_UDP);
                networkThreadToReceiveData.Start();
                break;

            case TransportType.TCP:
                socket.Listen(10);
                networkThreadToReceiveConnections = new Thread(WaitForClient);
                networkThreadToReceiveConnections.Start();

                networkThreadToReceiveData = new Thread(ReceiveData_TCP);
                networkThreadToReceiveData.Start();
                break;
        }
    }

    //Main function of receiving data (in a threat)
    public void ReceiveData_UDP()
    {
        while (true)
        {
            EndPoint Remote = ipEndPointToReceive;

            transferedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];

            transferedDataSize = socket.ReceiveFrom(transferedDataBuffer, ref Remote);

            if (transferedDataSize != 0)
            {
                DeserializeJsonAndReceive(transferedDataBuffer, transferedDataSize);
            }
        }
    }

    public void ReceiveData_TCP()
    {
        while (true)
        {
            if (connectedClientSocket != null)
            {
                transferedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];
                transferedDataSize = connectedClientSocket.Receive(transferedDataBuffer);

                ////Blocking
                //string message = Encoding.ASCII.GetString(transferedDataBuffer, 0, transferedDataSize);

                if (transferedDataSize != 0)
                {
                    DeserializeJsonAndReceive(transferedDataBuffer, transferedDataSize);
                }
            }
        }
    }

    //The deserialization gets the base class of the transfered data first.
    //This base class only contains a code, which indicates what is the child class
    //Depending of this code, we deserialize the info into the corresponding child class
    //Finally, a function is called in each case, depending on how we want to respond after receiving that type of info
    public void DeserializeJsonAndReceive(byte[] dataReceived, int dataSize)
    {
        stream = new MemoryStream(dataReceived, 0, dataSize);
        binaryReader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        string json = binaryReader.ReadString();

        GenericSendClass sendClass = new GenericSendClass();
        sendClass = JsonConvert.DeserializeObject<GenericSendClass>(json);

        switch (sendClass.sendCode)
        {
            case SendCode.ConnectionRequest:
                ConnectionRequest connectionRequest = new ConnectionRequest();
                connectionRequest = JsonConvert.DeserializeObject<ConnectionRequest>(json);
                Receive_ConnectionRequest(connectionRequest);
                break;

            case SendCode.ConnectionConfirmation:
                ConnectionConfirmation connectionConfirmation = new ConnectionConfirmation();
                connectionConfirmation = JsonConvert.DeserializeObject<ConnectionConfirmation>(json);
                Receive_ConnectionConfirmation(connectionConfirmation);
                break;

            case SendCode.DebugMessage:
                DebugMessage debugMessage = new DebugMessage();
                debugMessage = JsonConvert.DeserializeObject<DebugMessage>(json);
                Receive_DebugMessage(debugMessage);
                break;

            case SendCode.PlayerPositions:
                PlayerPositionsInfo PlayerPositionsInfo = JsonConvert.DeserializeObject<PlayerPositionsInfo>(json);
                Receive_PlayerPositions(PlayerPositionsInfo);
                break;

            case SendCode.PartyManager:
                PartyPlayersInfo PPI = JsonConvert.DeserializeObject<PartyPlayersInfo>(json);
                Receive_PartyPlayersInfo(PPI);
                break;

            case SendCode.SendIdPlayer:
                SendIdPlayer sIP = JsonConvert.DeserializeObject<SendIdPlayer>(json);
                Receive_SendIdPlayer(sIP);
                break;
        }
    }

    private void Receive_SendIdPlayer(SendIdPlayer sIP)
    {
        if (!NetworkManager.Instance.GetLocalClient().isHost)
        {
            partyObj.playerID=sIP.playerId;

        }
    }

    public void Receive_ConnectionRequest(ConnectionRequest connectionRequest)
    {
        if (NetworkManager.Instance.GetLocalClient() == null || !NetworkManager.Instance.GetLocalClient().isHost)
        {
            Debug.Log("Received Message but is not host");
        }
        else if (NetworkManager.Instance.clients.Exists(x => x.localIp == connectionRequest.clientRequesting.localIp && x.localPort == connectionRequest.clientRequesting.localPort))
        {
            Send_Data(() => ConnectionConfirmation(false, "A client with this IP and Port is already connected"));
        }
        else if (NetworkManager.Instance.clients.Exists(x => x.nickname == connectionRequest.clientRequesting.nickname))
        {
            Send_Data(() => ConnectionConfirmation(false, "A client with this nickname is already connected"));
        }
        else
        {
            NetworkManager.Instance.clients.Add(connectionRequest.clientRequesting);

            NetworkManager.Instance.UpdateRemoteIP(connectionRequest.clientRequesting.localIp);
            //NetworkManager.Instance.UpdateRemotePort(connectionRequest.clientRequesting.localPort);

            //partyObj.AddPartyPlayer();
            //AddNewPartyPlayer(connectionRequest.clientRequesting);
            UpdateEndPointToSend();
            Send_Data(() => ConnectionConfirmation(true, null, NetworkManager.Instance.clients));
        }
    }

    public void AddNewPartyPlayer(Client cl) //Add new player to the party and asing their own player Id  
    {
        PartyPlayersInfo newplayer = new PartyPlayersInfo();
        int newID = 0;
        foreach (PartyPlayersInfo item in partyObj.partyPlayersList)
        {
            if (item.playerID >= newID)
            {
                newID = item.playerID + 1;
            }
        }
        newplayer.playerID = newID;
        newplayer.playerInfo.playerIp = cl.localIp;
        newplayer.playerName = cl.nickname;
        partyObj.partyPlayersList.Add(newplayer);
        SendIdToPlayer(newplayer);
        //aqui falta que le devuelva al player o al host su player id

    }

    //public void Receive_ConnectionRequest(ConnectionRequest connectionRequest)
    //{
    //    if (NetworkManager.Instance.clients.Exists(x => x.localIp == connectionRequest.senderIp))
    //    {
    //        Send_Data(() => ConnectionConfirmation(false, "A client with this IP is already connected"));
    //    }
    //    else if (NetworkManager.Instance.clients.Exists(x => x.nickname == connectionRequest.senderNickname))
    //    {
    //        Send_Data(() => ConnectionConfirmation(false, "A client with this nickname is already connected"));
    //    }
    //    else
    //    {
    //        //NetworkManager.Instance.clients.Add(connectionRequest.clientRequesting);
    //        Send_Data(() => ConnectionConfirmation(true));
    //    }
    //}

    public void SendIdToPlayer(PartyPlayersInfo playersInfo)
    {
        SendIdPlayer sendId = new SendIdPlayer(playersInfo);

        SerializeToJsonAndSend(sendId);

    }

    public void ConnectionConfirmation(bool confirmation, string reason = null, List<Client> clientList = null)
    {
        ConnectionConfirmation connectionRequest = new ConnectionConfirmation(confirmation, reason, clientList);

        SerializeToJsonAndSend(connectionRequest);

    }

    public void Receive_ConnectionConfirmation(ConnectionConfirmation connectionConfirmation)
    {
        if (connectionConfirmation.acceptedConnection)
        {
            Debug.Log("You are connected to the Server!");
            LobbyManager.Instance.ChangeStage(LobbyManager.stages.waitingClient);
        }
        else
        {
            Debug.Log("Could not connect to server: " + connectionConfirmation.reasonToDeny);
        }
    }

    public void Receive_DebugMessage(DebugMessage debugMessage)
    {
        Debug.Log("IP: " + debugMessage.senderIp.ToString() + ". Nickname: " + debugMessage.senderNickname.ToString() + ". Message: " + debugMessage.debugMessageText.ToString());
    }

    //This is the main function to send data. 
    //You call this function when you want to interact with other clients.
    //This interaction depends on the method that you pass as a parameter,
    //which will be called in another threat. 
    //The method usually consists con creating a custom class, serialize it and send it.
    public void Send_Data(Action method)
    {
        if (networkThreadToSendData.ThreadState != ThreadState.Unstarted)
        {
            OpenNewThreat_Send();
        }

        dataMethod = method;

        networkThreadToSendData.Start();
    }

    public void Receive_PartyPlayersInfo(PartyPlayersInfo ppi)
    {

        // partyObj.partyPlayersList.Add(ppi);

    }
    //public void Send_ConnectionRequest()
    //{
    //    if (networkThreadToSendData.ThreadState != ThreadState.Unstarted)
    //    {
    //        OpenNewThreat_Send();
    //    }

    //    dataMethod = ConnectionConfirmationRequest;
    //    networkThreadToSendData.Start();
    //}

    public void ConnectionRequest()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                ConnectionRequest_UDP();
                break;

            case TransportType.TCP:
                ConnectionRequest_TCP();
                break;
        }
    }


    //This method creates a custom class that, when arriving to the server,
    //tells them that a client wants to connect to them.
    //After the server checks if the same IP and port is already connected or
    //the nickname is already taken, it sends back a confirmation or denial.
    private void ConnectionRequest_UDP()
    {
        Debug.Log("Sending request");

        //DebugMessage debugMessage = new DebugMessage(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname, "Testing");

        //Client client = new Client(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname);

        //ConnectionRequest connectionRequest = new ConnectionRequest(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname);




        if (NetworkManager.Instance.GetLocalClient() == null)
        {
            Debug.Log("There is no local client");
            return;
        }

        ConnectionRequest connectionRequest = new ConnectionRequest(NetworkManager.Instance.GetLocalClient());

        SerializeToJsonAndSend(connectionRequest);

        Debug.Log("Sent request");
    }

    //The main function that gets custom classes depending on the
    //information you want to send, serialize them and send them.
    public void SerializeToJsonAndSend<T>(T objectToSerialize)
    {

        string json = JsonConvert.SerializeObject(objectToSerialize);

        stream = new MemoryStream();
        binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(json);

        byte[] data = stream.ToArray();

        socket.SendTo(data, data.Length, SocketFlags.None, ipEndPointToSend);
    }

    public void SendDebugMessage()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                SendDebugMessage_UDP();
                break;

            case TransportType.TCP:
                SendDebugMessage_TCP();
                break;
        }
    }

    private void SendDebugMessage_UDP()
    {
        //byte[] data = Encoding.ASCII.GetBytes(debugMessage);

        //ConnectionManager.Instance.socket.SendTo(data, data.Length, SocketFlags.None, ConnectionManager.Instance.ipEndPointToSend);
        //Debug.Log("Sended via UDP: " + debugMessage);
        //ConnectionManager.Instance.isMessage = true;
    }

    private void SendDebugMessage_TCP()
    {
        //byte[] data = Encoding.ASCII.GetBytes(debugMessage);

        //ConnectionManager.Instance.socket.Send(data, data.Length, SocketFlags.None);

        //Debug.Log("Sended via TCP: " + debugMessage);

        //ConnectionManager.Instance.isMessage = true;
    }

    private void ConnectionRequest_TCP()
    {
        LobbyManager.Instance.serverHasConfirmedConnection = true;
    }

    public void SendNetworkData()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                SendNetworkData_UDP();
                break;

            case TransportType.TCP:
                SendNetworkData_TCP();
                break;
        }
    }

    public void SendNetworkData_UDP()
    {
        dataMethod();
        networkThreadToSendData.Interrupt();
    }

    public void SendNetworkData_TCP()
    {
        try
        {
            socket.Connect(ipEndPointToSend);
        }
        catch (SocketException error)
        {
            Debug.Log("Unable to connect to server. Error: " + error.ToString());
            return;
        }

        if (!socket.Connected)
        {
            Debug.Log("Socket is not connected to server");
            return;
        }

        dataMethod();
    }

    //Gets the local IP of this computer. Useful for testing the application on
    //two instances on the same computer, which act as a server and as a client.
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    public void SetEndPoint(ref IPEndPoint ipEndPoint, IPAddress ipAddess, int port)
    {
        ipEndPoint = new IPEndPoint(ipAddess, port);
    }

    //Update the EndPoints to receive and send information
    //These depends on the parameters that are set in the menus
    //and stored in the NetworkManager
    public void UpdateEndPoints()
    {
        UpdateEndPointToReceive();
        UpdateEndPointToSend();
    }

    public void UpdateEndPointToReceive()
    {
        
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            SetEndPoint(ref ipEndPointToReceive, IPAddress.Any, NetworkManager.Instance.defaultPort);
            socket.Bind(ipEndPointToReceive);
        }
        else
        {
            SetEndPoint(ref ipEndPointToReceive, IPAddress.Parse(NetworkManager.Instance.remoteIp), NetworkManager.Instance.defaultPort);
            socket.Connect(ipEndPointToReceive);
        }
    }

    public void UpdateEndPointToSend()
    {
        SetEndPoint(ref ipEndPointToSend, IPAddress.Parse(NetworkManager.Instance.remoteIp), NetworkManager.Instance.defaultPort);
    }


    private void OnDisable()
    {
        //Reset info
        NetworkManager.Instance.clients.Clear();

        //Close to Send
        if (socket != null)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        //Close to Receive
        if (networkThreadToReceiveConnections != null)
        {
            networkThreadToReceiveConnections.Abort();
        }

        if (networkThreadToReceiveData != null)
        {
            networkThreadToReceiveData.Abort();
        }

    }

    public void Receive_PlayerPositions(PlayerPositionsInfo PlayerPositionsInfo)
    {
        //Aquí puedes procesar la información de las posiciones de los jugadores

        PlayerManager.Instance.UpdatePlayers(PlayerPositionsInfo);
        //foreach (PlayerPositionsInfo playerPosition in PlayerPositionsInfo)
        //{
           
            
        //}
    }

    #endregion
}
