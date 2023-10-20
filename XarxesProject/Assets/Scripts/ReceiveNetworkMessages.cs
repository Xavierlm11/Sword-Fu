using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ReceiveNetworkMessages : MonoBehaviour
{
    #region variables

    [SerializeField]
    private TMP_InputField ipField;

    [SerializeField]
    private TMP_InputField messageField;

    [SerializeField]
    private string ip;

    [SerializeField]
    private string message;

    [SerializeField]
    private int maxMessageCharSize;

    [SerializeField]
    private int receivedMessageSize;

    [SerializeField]
    private Socket socket;

     [SerializeField]
    private Socket clientSocket;

    [SerializeField]
    private byte[] receivedDataBuffer;

    [SerializeField]
    private IPEndPoint IpEndPoint;

    [SerializeField]
    private Thread networkThread;

    [SerializeField]
    private Thread messagesThread;

    [SerializeField]
    private SendNetworkMessages sendNetworkMessages;

    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    private void OnEnable()
    {
        Resources.LoadAll("");
        NetworkManager.Call_GoToLobbyScene();
    }

    private void Start()
    {
        //StartReceivingMessages();

        //if (NetworkManager.Instance.transportType == TransportType.TCP)
        //{
        //    socket.Listen(10);
        //    networkThread = new Thread(WaitForClient);
        //    networkThread.Start();

        //    messagesThread = new Thread(WaitForMessages);
        //    messagesThread.Start();
        //}
        //else
        //{
        //    networkThread = new Thread(ReceiveMessage);
        //    networkThread.Start();
        //}
    }

    

    //private void WaitForClient()
    //{
    //    //Blocking
    //    clientSocket = socket.Accept();
    //    IPEndPoint clientIpEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
    //    Debug.Log("Connected with [" + clientIpEndPoint.Address.ToString() + "] at port [" + clientIpEndPoint.Port.ToString() + "]");
    //}

    private void WaitForMessages()
    {
        //while (true)
        //{
        //    if(clientSocket != null)
        //    {
        //        receivedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];
        //        receivedMessageSize = clientSocket.Receive(receivedDataBuffer);

        //        //Blocking
        //        string message = Encoding.ASCII.GetString(receivedDataBuffer, 0, receivedMessageSize);

        //        if (receivedMessageSize == 0)
        //        {
        //            return;
        //        }

        //        Debug.Log("Received message: " + message);



        //        // Env�a un mensaje de vuelta
        //        string replyMessage = "Received your message: " + message;
        //        byte[] replyData = Encoding.ASCII.GetBytes(replyMessage);
        //        clientSocket.Send(replyData, replyData.Length, SocketFlags.None);

        //    }
        //}
    }

    private void ReceiveMessage()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                ReceiveMessage_UDP();
                break;
            case TransportType.TCP:
                //ReceiveMessage_TCP();
                break;
        }
        
    }

    public void ReceiveMessage_UDP()
    {
        //while (true)
        //{
        //    EndPoint Remote = IpEndPoint;
            
        //    receivedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];

        //    receivedMessageSize = socket.ReceiveFrom(receivedDataBuffer, ref Remote);

        //    string message = Encoding.ASCII.GetString(receivedDataBuffer, 0, receivedMessageSize);

        //    string remoteString = Remote.ToString();
        //    int remoteIpIndex = remoteString.IndexOf(':');

        //    if (remoteIpIndex != -1)
        //    {
        //        string remoteIpString = remoteString.Substring(0, remoteIpIndex);
        //        Debug.Log("Received message from " + remoteIpString + ": " + message);
        //    }
        //    else
        //    {
        //        Debug.Log("Received message from " + remoteString + ": " + message);
        //    }
        //}
    }

    //public void ReceiveMessage_TCP()
    //{
    //    EndPoint Remote = IpEndPoint;
    //    socket.Listen(10);
    //    //Blocking
    //    clientSocket = socket.Accept();
    //}

    private void OnDisable()
    {
        ////if(networkThread != null)
        ////{
        ////    networkThread.Abort();
        ////}

        ////if (messagesThread != null)
        ////{
        ////    messagesThread.Abort();
        ////}

        ////if (socket != null)
        ////{
        ////    socket.Close();
        ////}
        

        //clientSocket.Close();

    }
    #endregion
}
