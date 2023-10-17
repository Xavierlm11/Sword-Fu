using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SendNetworkMessages : MonoBehaviour
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

    public enum TransportType
    {
        UDP,
        TCP
    }

    [SerializeField]
    private TransportType transportType;

    [SerializeField]
    private int port;

    [SerializeField]
    private int maxMessageCharSize;

    [SerializeField]
    private int receivedMessageSize;

    [SerializeField]
    private Socket socket;

    [SerializeField]
    private byte[] receivedDataBuffer;

    [SerializeField]
    private IPEndPoint IpEndPoint;

    [SerializeField]
    private Thread networkThread;

    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    private void Start()
    {
        StartNetwork();

        networkThread = new Thread(ReceiveMessage);
        networkThread.Start();
    }

    private void StartNetwork()
    {
        switch (transportType)
        {
            case TransportType.UDP:
                StartNetwork_UDP();
                break;
            case TransportType.TCP:
                StartNetwork_TCP();
                break;
        }

        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(“-----”),port);
        IpEndPoint = new IPEndPoint(IPAddress.Any, 9050);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);

        socket.Bind(IpEndPoint);

    }

    public void StartNetwork_UDP()
    {
        socket = new Socket(
             AddressFamily.InterNetwork,
             SocketType.Dgram,
             ProtocolType.Udp);
    }

    public void StartNetwork_TCP()
    {
        socket = new Socket(
           AddressFamily.InterNetwork,
           SocketType.Stream,
           ProtocolType.Tcp);
    }

    public void Update()
    {
        
    }

    private void ReceiveMessage()
    {
        switch (transportType)
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
            Debug.Log("Funca: " + receivedMessageSize.ToString());
            receivedDataBuffer = new byte[1024];
            receivedMessageSize = socket.ReceiveFrom(receivedDataBuffer, ref Remote);
            if (receivedMessageSize > 0)
            {
                
            }
            
        }
        

        
        
    }

    public void ReceiveMessage_TCP()
    {

    }

    #endregion
}
