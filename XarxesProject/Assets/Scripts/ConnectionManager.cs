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
    public IPEndPoint ipEndPointOfSender;

    [SerializeField]
    private Thread networkThreadToSendData;

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

    //[SerializeField]
    //private int maxTransferedDataSize;

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

    //[SerializeField]
    //public class PartyPlayersInfo
    //{
    //    public string playerName;
    //    public int playerID;
    //    public int playersConected;

    //    public PlayerPositionsInfo playerInfo;

    //}

    private Action dataMethod;


    private MemoryStream stream;
    private BinaryWriter binaryWriter;
    private BinaryReader binaryReader;

    public List<byte[]> dataToSendList = new List<byte[]>();

    public byte[] dataToSend;
    //public byte[] receivedData;
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
        SetTransferedDataSize();

        //Starts being able to receive data
        OpenNewThreat_Receive();

        ////Starts being able to send data
        OpenNewThreat_Send();

    }

    //Creates the socket
    public void SetSocket()
    {
        socket = NetworkManager.StartNetwork();
    }

    public void SetTransferedDataSize()
    {
        transferedDataSize = NetworkManager.Instance.maxTransferedDataSize;
    }

    private void Update()
    {

    }

    private void OpenNewThreat_Send()
    {
        networkThreadToSendData = new Thread(SendNetworkData);
        networkThreadToSendData.Start();
    }

    private void OpenNewThreat_Receive()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                networkThreadToReceiveData = new Thread(ReceiveData_UDP);
                networkThreadToReceiveData.Start();
                break;
        }
    }

    //Main function of receiving data (in a threat)
    public void ReceiveData_UDP()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            ipEndPointToReceive = new IPEndPoint(IPAddress.Any, NetworkManager.Instance.defaultPort);
            socket.Bind(ipEndPointToReceive);

            Debug.Log("Waiting for a client...");

            ipEndPointOfSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = ipEndPointOfSender;

            while (!NetworkManager.Instance.appIsQuitting)
            {
                
                Remote = ipEndPointOfSender;
                byte[] receivedData = new byte[NetworkManager.Instance.maxTransferedDataSize];

                try
                {
                    int recv = socket.ReceiveFrom(receivedData, ref Remote);

                    

                    if (Remote is IPEndPoint remoteEndPoint)
                    {
                        // Accede a la información de la IP y el puerto remotos
                        IPAddress remoteIPAdress = remoteEndPoint.Address;
                        int remotePort = remoteEndPoint.Port;

                        //Debug.LogError("Server receiving using " + remoteEndPoint.Port.ToString());

                        string remoteIP = remoteIPAdress.ToString();

                        UnityMainThreadDispatcher.Instance().Enqueue(() => DeserializeJsonAndReceive(receivedData, recv, remoteIP, remotePort));
                    }

                    //receivedData = null;
                }
                catch
                {
                    //Debug.Log($"Error al recibir datos: {ex.Message}");
                }
            }
        }
        else
        {
            ipEndPointOfSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = ipEndPointOfSender;

            //Por hacer
            while (!NetworkManager.Instance.appIsQuitting)
            {

                Remote = ipEndPointOfSender;
                byte[] receivedData = new byte[NetworkManager.Instance.maxTransferedDataSize];

                //Socket server = new Socket(AddressFamily.InterNetwork,
                //     SocketType.Dgram, ProtocolType.Udp);

                try
                {
                    int recv = socket.ReceiveFrom(receivedData, ref Remote);

                    if (Remote is IPEndPoint remoteEndPoint)
                    {
                        // Accede a la información de la IP y el puerto remotos
                        IPAddress remoteIPAdress = remoteEndPoint.Address;
                        int remotePort = remoteEndPoint.Port;

                        //Debug.LogError("Client receiving using " + remoteEndPoint.Port.ToString());

                        string remoteIP = remoteIPAdress.ToString();

                        UnityMainThreadDispatcher.Instance().Enqueue(() => DeserializeJsonAndReceive(receivedData, recv, remoteIP, remotePort));
                    }
                    
                    //receivedData = null;
                }
                catch
                {
                    //Debug.Log($"Error al recibir datos: {ex.Message}");
                }
                
            }
        }
        
    }

    //The deserialization gets the base class of the transfered data first.
    //This base class only contains a code, which indicates what is the child class
    //Depending of this code, we deserialize the info into the corresponding child class
    //Finally, a function is called in each case, depending on how we want to respond after receiving that type of info
    public void DeserializeJsonAndReceive(byte[] dataReceived, int dataSize, string remoteIP, int remotePort)
    {
        //Debug.Log(NetworkManager.Instance.GetLocalClient().isHost);
        stream = new MemoryStream(dataReceived, 0, dataSize);
        binaryReader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        string json = binaryReader.ReadString();

        GenericSendClass sendClass = new GenericSendClass();
        sendClass = JsonConvert.DeserializeObject<GenericSendClass>(json);

        Debug.Log("Data Received: " + sendClass.sendCode.ToString());

        switch (sendClass.sendCode)
        {
            case SendCode.ConnectionRequest:
                ConnectionRequest connectionRequest = new ConnectionRequest();
                connectionRequest = JsonConvert.DeserializeObject<ConnectionRequest>(json);
                connectionRequest.remoteIP = remoteIP;
                connectionRequest.remotePort = remotePort;
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

            //case SendCode.PartyManager:
            //    PartyPlayersInfo PPI = JsonConvert.DeserializeObject<PartyPlayersInfo>(json);
            //    Receive_PartyPlayersInfo(PPI);
            //    break;

            //case SendCode.SendIdPlayer:
            //    SendIdPlayer sIP = JsonConvert.DeserializeObject<SendIdPlayer>(json);
            //    Receive_SendIdPlayer(sIP);
            //    break;

            case SendCode.ClientListUpdate:
                //ClientListUpdate clientListUpdate = new ClientListUpdate();
                //clientListUpdate = JsonConvert.DeserializeObject<ClientListUpdate>(json);
                ////UpdateClientList(clientListUpdate.clientList);
                break;
            case SendCode.RoomInfoUpdate:
                RoomInfoUpdate roomInfoUpdate = new RoomInfoUpdate();
                roomInfoUpdate = JsonConvert.DeserializeObject<RoomInfoUpdate>(json);
                UpdateRoomInfo(roomInfoUpdate.room);
                break;
        }
    }

    public GenericSendClass DeserializeJsonBasic(byte[] dataReceived, int dataSize)
    {

        stream = new MemoryStream(dataReceived, 0, dataSize);

        binaryReader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        string json = binaryReader.ReadString();

        GenericSendClass sendClass = new GenericSendClass();

        sendClass = JsonConvert.DeserializeObject<GenericSendClass>(json);

        return sendClass;

    }

    //private void Receive_SendIdPlayer(SendIdPlayer sIP)
    //{
    //    if (!NetworkManager.Instance.GetLocalClient().isHost)
    //    {
    //        partyObj.playerID=sIP.playerId;

    //    }
    //}

    public void CreateRoom()
    {
        NetworkManager.Instance.activeRoom = new Room();
        NetworkManager.Instance.activeRoom.clients.Add(NetworkManager.Instance.GetLocalClient());
        NetworkManager.Instance.activeRoom.host = NetworkManager.Instance.GetLocalClient();
    }

    public void Receive_ConnectionRequest(ConnectionRequest connectionRequest)
    {
        if (NetworkManager.Instance.GetLocalClient() == null || !NetworkManager.Instance.GetLocalClient().isHost)
        {
            Debug.Log("Received Message but is not host");
        }
        else if (!NetworkManager.Instance.allowSameIPInRoom && NetworkManager.Instance.activeRoom.clients.Exists(x => x.localIp == connectionRequest.clientRequesting.localIp))
        {
            //NetworkManager.Instance.UpdateRemoteIP(connectionRequest.clientRequesting.localIp);
            //UpdateEndPointToSend();
            //Send_Data(() => ConnectionConfirmation(false, "A client with this IP and Port is already connected"));
            connectionRequest.sender.globalIP = connectionRequest.remoteIP;
            connectionRequest.sender.globalPort = connectionRequest.remotePort;
            ConnectionConfirmation(connectionRequest.remoteIP, connectionRequest.remotePort, connectionRequest.sender, false, "A client with this IP is already connected");
        }
        else if (NetworkManager.Instance.activeRoom.clients.Exists(x => x.nickname == connectionRequest.clientRequesting.nickname))
        {
            //NetworkManager.Instance.UpdateRemoteIP(connectionRequest.clientRequesting.localIp);
            //UpdateEndPointToSend();
            //Send_Data(() => ConnectionConfirmation(false, "A client with this nickname is already connected"));
            connectionRequest.sender.globalIP = connectionRequest.remoteIP;
            connectionRequest.sender.globalPort = connectionRequest.remotePort;
            ConnectionConfirmation(connectionRequest.remoteIP, connectionRequest.remotePort, connectionRequest.sender, false, "A client with this nickname is already connected");
        }
        else
        {
            //NetworkManager.Instance.activeRoom.clients.Add(connectionRequest.clientRequesting);

            //NetworkManager.Instance.UpdateRemoteIP(connectionRequest.clientRequesting.localIp);
            //NetworkManager.Instance.UpdateRemotePort(connectionRequest.clientRequesting.localPort);

            //partyObj.AddPartyPlayer();
            //AddNewPartyPlayer(connectionRequest.clientRequesting);

            //UpdateEndPointToSend();
            //Send_Data(() => ConnectionConfirmation(true, null, NetworkManager.Instance.clients));

            connectionRequest.sender.globalIP = connectionRequest.remoteIP;
            connectionRequest.sender.globalPort = connectionRequest.remotePort;
            NetworkManager.Instance.activeRoom.clients.Add(connectionRequest.sender);
            ConnectionConfirmation(connectionRequest.remoteIP, connectionRequest.remotePort, connectionRequest.sender, true, null );
            //UnityMainThreadDispatcher.Instance().Enqueue(() => SendClientListUpdate());

            //SendClientListUpdate();
            SendRoomInfoUpdate();
        }
    }

    public void SendClientListUpdate()
    {
        ClientListUpdate clientListUpdate = new ClientListUpdate(NetworkManager.Instance.activeRoom.clients);
        SerializeToJsonAndSend(clientListUpdate);
    }
    public void SendRoomInfoUpdate()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            RoomInfoUpdate roomInfoUpdate = new RoomInfoUpdate(NetworkManager.Instance.activeRoom);

            roomInfoUpdate.transferType = TransferType.OnlyClients;
            SerializeToJsonAndSend(roomInfoUpdate);
        }
    }
    
    //public void AddNewPartyPlayer(Client cl) //Add new player to the party and asing their own player Id  
    //{
    //    PartyPlayersInfo newplayer = new PartyPlayersInfo();
    //    int newID = 0;
    //    foreach (PartyPlayersInfo item in partyObj.partyPlayersList)
    //    {
    //        if (item.playerID >= newID)
    //        {
    //            newID = item.playerID + 1;
    //        }
    //    }
    //    newplayer.playerID = newID;
    //    newplayer.playerInfo.playerIp = cl.localIp;
    //    newplayer.playerName = cl.nickname;
    //    partyObj.partyPlayersList.Add(newplayer);
    //    SendIdToPlayer(newplayer);
    //    //aqui falta que le devuelva al player o al host su player id

    //}

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

    //public void SendIdToPlayer(PartyPlayersInfo playersInfo)
    //{
    //    SendIdPlayer sendId = new SendIdPlayer(playersInfo);

    //    SerializeToJsonAndSend(sendId);

    //}

    public void ConnectionConfirmation(string remoteIP, int remotePort, Client sender, bool confirmation, string reason = null)
    {
        ConnectionConfirmation connectionConfirmation = new ConnectionConfirmation(confirmation, reason);
        connectionConfirmation.transferType = TransferType.Custom;
        connectionConfirmation.receivers.Add(sender);
        //connectionConfirmation.remoteIP = remoteIP;
        //connectionConfirmation.remotePort = remotePort;
        //connectionConfirmation.sender.globalPort = remotePort;
        SerializeToJsonAndSend(connectionConfirmation);

    }

    public void Receive_ConnectionConfirmation(ConnectionConfirmation connectionConfirmation)
    {
        if (NetworkManager.Instance.GetLocalClient() != null && NetworkManager.Instance.GetLocalClient().isHost)
        {
            Debug.Log("Received Message but is host");
        }
        else if (connectionConfirmation.acceptedConnection)
        {
            Debug.Log("You are connected to the Server!");
            LobbyManager.Instance.ChangeStage(MenuStage.waitingRoom);
            LobbyManager.Instance.ipText.text = NetworkManager.Instance.remoteIp.ToString();
        }
        else
        {
            Debug.Log("Could not connect to server: " + connectionConfirmation.reasonToDeny);
            //UnityMainThreadDispatcher.Instance().Enqueue(() => LobbyManager.Instance.ChangeStage(LobbyManager.MenuStage.settingClient));
            //UnityMainThreadDispatcher.Instance().Enqueue(() => EndConnections());
        }

        //Client localClient = NetworkManager.Instance.GetLocalClient();
        //localClient.globalPort = connectionConfirmation.sender.globalPort;
    }

    //public void UpdateClientList(List<Client> newClientList)
    //{
    //    if(NetworkManager.Instance.activeRoom == null)
    //    {
    //        NetworkManager.Instance.activeRoom = new Room();
    //    }

    //    NetworkManager.Instance.activeRoom.clients.Clear();
    //    NetworkManager.Instance.activeRoom.clients = newClientList;
    //}

    public void UpdateRoomInfo(Room newRoomInfo)
    {
        NetworkManager.Instance.activeRoom = null;
        NetworkManager.Instance.activeRoom = newRoomInfo;
        LobbyManager.Instance.ipText.text = "Host IP: " + NetworkManager.Instance.activeRoom.host.localIp.ToString();
    }

    public void Receive_DebugMessage(DebugMessage debugMessage)
    {
        Debug.Log("IP: " + debugMessage.senderIp.ToString() + ". Nickname: " + debugMessage.senderNickname.ToString() + ". Message: " + debugMessage.debugMessageText.ToString());
    }


    //public void Receive_PartyPlayersInfo(PartyPlayersInfo ppi)
    //{

    //    // partyObj.partyPlayersList.Add(ppi);

    //}


    //This method creates a custom class that, when arriving to the server,
    //tells them that a client wants to connect to them.
    //After the server checks if the same IP and port is already connected or
    //the nickname is already taken, it sends back a confirmation or denial.

    //The main function that gets custom classes depending on the
    //information you want to send, serialize them and send them.
    public void SerializeToJsonAndSend<T>(T objectToSerialize)
    {

        string json = JsonConvert.SerializeObject(objectToSerialize);

        stream = new MemoryStream();
        binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(json);

        byte[] data = stream.ToArray();
        dataToSendList.Add(data);

        //byte[] data = stream.ToArray();

        //socket.SendTo(data, data.Length, SocketFlags.None, ipEndPointToSend);
    }

    public void SendNetworkData()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                SendNetworkData_UDP();
                break;
        }
    }

    public void SendNetworkData_UDP()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            while (!NetworkManager.Instance.appIsQuitting)
            {

                for (int i = dataToSendList.Count - 1; i >= 0; i--)
                {

                    GenericSendClass dataInfo = new GenericSendClass();

                    if (dataToSendList[i] != null)
                    {
                        dataInfo = DeserializeJsonBasic(dataToSendList[i], dataToSendList[i].Length);

                        Debug.Log("Sending Data: " + dataInfo.sendCode.ToString());

                    }

                    try
                    {
                        

                        switch (dataInfo.transferType)
                        {
                            case TransferType.OnlyClients:
                                dataInfo.receivers.Clear();
                                foreach (Client cl in NetworkManager.Instance.activeRoom.clients)
                                {
                                    if (!cl.isHost)
                                    {
                                        dataInfo.receivers.Add(cl);
                                    }
                                }
                                break;
                            case TransferType.Custom:

                                break;
                            default:
                                dataInfo.receivers.Clear();
                                break;
                        }



                        //if (dataInfo.sender.globalPort == 0)
                        //{
                        //    dataInfo.sender.globalPort = dataInfo.remotePort;
                        //    ipEndPointToSend = new IPEndPoint(
                        //            IPAddress.Parse(dataInfo.remoteIP), dataInfo.remotePort);
                        //    Debug.LogError("Server sending using " + dataInfo.remotePort.ToString());

                        //    socket.SendTo(dataToSendList[i], dataToSendList[i].Length, SocketFlags.None, ipEndPointToSend);

                        //    //}

                        //    dataToSendList.RemoveAt(i);
                        //}
                        //else
                        //{
                        foreach (Client receiver in dataInfo.receivers)
                        {
                            ipEndPointToSend = new IPEndPoint(
                                    IPAddress.Parse(receiver.globalIP), receiver.globalPort);
                            //Debug.LogError("Server sending using " + receiver.globalPort);

                            socket.SendTo(dataToSendList[i], dataToSendList[i].Length, SocketFlags.None, ipEndPointToSend);

                            //}

                            dataToSendList.RemoveAt(i);
                        }
                        //}

                            

                    }
                    catch
                    {
                        // Manejar la excepción
                        //Debug.Log($"Error al enviar datos");
                    }


                }
            }
        }
        else
        {
            ipEndPointToSend = new IPEndPoint(
                            IPAddress.Parse(NetworkManager.Instance.remoteIp), NetworkManager.Instance.defaultPort);

            //Debug.LogError(ipEndPointToSend.Port.ToString());

            while (!NetworkManager.Instance.appIsQuitting)
            {
                

                for(int i = dataToSendList.Count - 1; i >= 0; i--)
                {
                    GenericSendClass dataInfo = new GenericSendClass();

                    if (dataToSendList[i] != null)
                    {
                        Debug.Log("Sending Data: " + dataInfo.sendCode.ToString());

                    }

                    try
                    {
                        socket.SendTo(dataToSendList[i], dataToSendList[i].Length, SocketFlags.None, ipEndPointToSend);
                        dataToSendList.RemoveAt(i);
                        //Debug.LogError("Client sending using " + NetworkManager.Instance.defaultPort.ToString());
                    }
                    catch
                    {
                        // Manejar la excepción
                        //Debug.Log($"Error al enviar datos");
                    }
                    
                }
            }
        }
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
            SetEndPoint(ref ipEndPointToReceive, IPAddress.Parse(NetworkManager.Instance.localIp), NetworkManager.Instance.defaultPort);
            //socket.Connect(ipEndPointToReceive);
        }
    }

    public void UpdateEndPointToSend()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            SetEndPoint(ref ipEndPointToSend, IPAddress.Any, 0);
        }
        else
        {
            SetEndPoint(ref ipEndPointToSend, IPAddress.Any, 0);
        }
        
    }

    public void EndConnections()
    {
        NetworkManager.Instance.DeleteClients();

        if (socket != null)
        {
            socket.Close();
            socket.Dispose();
        }

        if (networkThreadToReceiveData != null)
        {
            networkThreadToReceiveData.Abort();
        }

        if (networkThreadToSendData != null)
        {
            networkThreadToSendData.Abort();
        }
    }

    private void OnDisable()
    {
        EndConnections();
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
