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
        isMessage = false;
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

    private void StartReceivingMessages()
    {
        transferedDataSize = NetworkManager.Instance.messageMaxBytes;

        SetEndPoint(ref ipEndPointToReceive, IPAddress.Any, NetworkManager.Instance.port);
        socket.Bind(ipEndPointToReceive);

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

                //Debug.Log("Received message: " + message);



                ////// Env�a un mensaje de vuelta
                ////string replyMessage = "Received your message: " + message;
                ////byte[] replyData = Encoding.ASCII.GetBytes(replyMessage);
                ////connectedClientSocket.Send(replyData, replyData.Length, SocketFlags.None);

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
        sendClass = JsonUtility.FromJson<GenericSendClass>(json);

        switch (sendClass.sendCode)
        {
            case SendCode.ConnectionConfirmation:

                break;

            case SendCode.DebugMessage:
                DebugMessage debugMessage = new DebugMessage();
                debugMessage = JsonUtility.FromJson<DebugMessage>(json);
                Receive_DebugMessage(debugMessage);
                break;
        }
    }

    public void Receive_DebugMessage(DebugMessage debugMessage)
    {
        Debug.Log("IP: " + debugMessage.senderIp.ToString() + ". Nickname: " + debugMessage.senderNickname.ToString() + ". Message: " + debugMessage.debugMessageText.ToString());
    }

    public void Call_ConnectionConfirmation()
    {
        if (networkThreadToSendData.ThreadState != ThreadState.Unstarted)
        {
            OpenNewThreat_Send();
        }

        dataMethod = ConnectionConfirmationRequest;
        networkThreadToSendData.Start();
    }

    private void ConnectionConfirmationRequest()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                ConnectionConfirmationRequest_UDP();
                break;

            case TransportType.TCP:
                ConnectionConfirmationRequest_TCP();
                break;
        }
    }

    private void ConnectionConfirmationRequest_UDP()
    {
        //SendDebugMessage();

        Debug.Log("Sending request");

        DebugMessage debugMessage = new DebugMessage(NetworkManager.Instance.GetLocalClient().localIp, NetworkManager.Instance.GetLocalClient().nickname, "Testing");

        SerializeToJsonAndSend(debugMessage);

        Debug.Log("Sent request");

    }

    public void SerializeToJsonAndSend<T>(T objectToSerialize)
    {
        string json = JsonUtility.ToJson(objectToSerialize);
        stream = new MemoryStream();
        binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(json);

        byte[] data = stream.ToArray();//Encoding.ASCII.GetBytes(json);

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

    private void ConnectionConfirmationRequest_TCP()
    {
        LobbyManager.Instance.serverHasConfirmedConnection = true;
    }

    public void Call_SendNetworkData()
    {
        if (networkThreadToSendData.ThreadState != ThreadState.Unstarted)
        {
            OpenNewThreat_Send();
        }

        dataMethod = SendNetworkData;
        networkThreadToSendData.Start();
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
        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(�-----�),port);
        ipEndPoint = new IPEndPoint(ipAddess, port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    public void SetRemoteIP(string ip)
    {
        SetEndPoint(ref ipEndPointToSend, IPAddress.Parse(ip), NetworkManager.Instance.port);
    }

    private void OnDisable()
    {
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

    #endregion
}
