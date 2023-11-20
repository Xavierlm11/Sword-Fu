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

    [Serializable]
    public class PlayerPositionsInfo
    {
        public string senderIp;
        public string senderNickname;
        public List<PlayerPosition> playerPositions;
    }

    [Serializable]
    public class PlayerPosition
    {
        public string playerName;
        public float positionX;
        public float positionY;
        public float positionZ;
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
        if(Instance == null) 
        { 
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        

    }

    public void StartConnections()
    {
        SetSocket();

        //SetInitialValues();
        StartReceivingMessages();

        OpenNewThreat_Receive();
        OpenNewThreat_Send();
    }

    public void SetSocket()
    {
        socket = NetworkManager.StartNetwork();
    }

    public void StartReceivingMessages()
    {
        transferedDataSize = NetworkManager.Instance.messageMaxBytes;

        SetEndPoint(ref ipEndPointToReceive, IPAddress.Any, NetworkManager.Instance.localPort);
        socket.Bind(ipEndPointToReceive);

        NetworkManager.Instance.UpdateLocalPort(((IPEndPoint)socket.LocalEndPoint).Port);
        LobbyManager.Instance.SetLocalPort();

        Debug.Log("EndPoint set and socket binded");
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
        if (isMessage)
        {
            CallToChat();
        }
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

    public void ReceiveData_UDP()
    {
        while (true)
        {
            EndPoint Remote = ipEndPointToReceive;

            transferedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];

            transferedDataSize = socket.ReceiveFrom(transferedDataBuffer, ref Remote);

            if (transferedDataSize == 0)
            {
                return;
            }

            DeserializeJsonAndReceive(transferedDataBuffer, transferedDataSize);

            //socket.SendTo(transferedDataBuffer, transferedDataSize, SocketFlags.None, Remote);


            //string message = Encoding.ASCII.GetString(transferedDataBuffer, 0, transferedDataSize);

            //string remoteString = Remote.ToString();
            //int remoteIpIndex = remoteString.IndexOf(':');

            //if (remoteIpIndex != -1)
            //{
            //    string remoteIpString = remoteString.Substring(0, remoteIpIndex);
            //    Debug.Log("Received message from " + remoteIpString + ": " + message);
            //}
            //else
            //{
            //    Debug.Log("Received message from " + remoteString + ": " + message);
            //}
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

                if (transferedDataSize == 0)
                {
                    return;
                }

                DeserializeJsonAndReceive(transferedDataBuffer, transferedDataSize);

            }
        }
    }

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


        }
    }

    public void Receive_ConnectionRequest(ConnectionRequest connectionRequest)
    {
        if (NetworkManager.Instance.clients.Exists(x => x.localIp == connectionRequest.clientRequesting.localIp))
        {
            Send_Data(() => ConnectionConfirmation(false, "A client with this IP is already connected"));
        }
        else if (NetworkManager.Instance.clients.Exists(x => x.nickname == connectionRequest.clientRequesting.nickname))
        {
            Send_Data(() => ConnectionConfirmation(false, "A client with this nickname is already connected"));
        }
        else
        {
            NetworkManager.Instance.clients.Add(connectionRequest.clientRequesting);
            Send_Data(() => ConnectionConfirmation(true));
        }
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

    public void ConnectionConfirmation(bool confirmation, string reason = null)
    {
        ConnectionConfirmation connectionRequest = new ConnectionConfirmation(confirmation);

        if (!confirmation)
        {
            connectionRequest.reasonToDeny = reason;
        }

        SerializeToJsonAndSend(connectionRequest);

    }

    public void Receive_ConnectionConfirmation(ConnectionConfirmation connectionConfirmation)
    {
        if (connectionConfirmation.acceptedConnection)
        {
            Debug.Log("You are connected to the Server!");
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
    public void Send_Data(Action method)
    {
        if (networkThreadToSendData.ThreadState != ThreadState.Unstarted)
        {
            OpenNewThreat_Send();
        }

        dataMethod = method;

        networkThreadToSendData.Start();
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

    private void ConnectionRequest_UDP()
    {

        Debug.Log("Sending request");

        //DebugMessage debugMessage = new DebugMessage(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname, "Testing");

        Client client = new Client(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname);

        //ConnectionRequest connectionRequest = new ConnectionRequest(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname);
        ConnectionRequest connectionRequest = new ConnectionRequest(client);

        SerializeToJsonAndSend(connectionRequest);

        Debug.Log("Sent request");

    }

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

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    public static void SetEndPoint(ref IPEndPoint ipEndPoint, IPAddress ipAddess, int port)
    {
        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(“-----”),port);
        ipEndPoint = new IPEndPoint(ipAddess, port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    public void SetRemoteIP(string ip)
    {
        SetEndPoint(ref ipEndPointToSend, IPAddress.Parse(ip), NetworkManager.Instance.serverPort);
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
        // Aquí puedes procesar la información de las posiciones de los jugadores
        // PlayerPositionsInfo.senderIp: IP del remitente
        // PlayerPositionsInfo.senderNickname: Apodo del remitente
        // PlayerPositionsInfo.playerPositions: Lista de posiciones de jugadores

        foreach (var playerPosition in PlayerPositionsInfo.playerPositions)
        {
            // Accede a la información de cada jugador
            // playerPosition.playerName: Nombre del jugador
            // playerPosition.positionX: Posición X del jugador
            // playerPosition.positionY: Posición Y del jugador
            // playerPosition.positionZ: Posición Z del jugador

        }
    }

    #endregion
}
