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

    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    private void OnEnable()
    {
        Resources.LoadAll("");
    }

    private void Start()
    {
        receivedMessageSize = NetworkSettings.Instance.messageMaxBytes;

        socket = NetworkSettings.StartNetwork();
        NetworkSettings.SetEndPoint(ref IpEndPoint, IPAddress.Any, NetworkSettings.Instance.port);
        socket.Bind(IpEndPoint);

        networkThread = new Thread(ReceiveMessage);
        networkThread.Start();
    }


    public void Update()
    {
        
    }

    private void ReceiveMessage()
    {
        switch (NetworkSettings.Instance.transportType)
        {
            case TransportType.UDP:
                ReceiveMessage_UDP();
                break;
            case TransportType.TCP:
                ReceiveMessage_TCP();
                break;
        }
        
    }

    public void ReceiveMessage_UDP()
    {
        while (true)
        {
            EndPoint Remote = IpEndPoint;
            
            receivedDataBuffer = new byte[NetworkSettings.Instance.messageMaxBytes];

            receivedMessageSize = socket.ReceiveFrom(receivedDataBuffer, ref Remote);

            string message = Encoding.ASCII.GetString(receivedDataBuffer, 0, receivedMessageSize);



            Debug.Log("Received message from " + Remote.ToString() + ": " + message);

        }
    }

    public void ReceiveMessage_TCP()
    {
        EndPoint Remote = IpEndPoint;
        socket.Listen(10);
        clientSocket = socket.Accept(); // accept es una funcion blocking

        while (true)
        {
            receivedDataBuffer = new byte[NetworkSettings.Instance.messageMaxBytes];
            // IPEndPoint clientep = (IPEndPoint)clientSocket.RemoteEndPoint;
            receivedMessageSize = socket.ReceiveFrom(receivedDataBuffer, ref Remote); 
        }

    }

    private void OnDisable()
    {
        if (socket.Connected)
        {
            clientSocket.Close();
            socket.Close();
        }
    }
    #endregion
}
