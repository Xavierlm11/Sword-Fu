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
    private EndPoint endPoint;

    [SerializeField]
    private IPEndPoint IpEndPoint;

    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    public void SetRemoteIP()
    {
        ip = ipField.text;
    }

    public void SetMessage()
    {
        message = messageField.text;
    }

    private void Start()
    {
        StartNetwork();
        Thread networkThread = new Thread(SendNetworkMessage);
        networkThread.Start();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateInfo();
        }
    }

    private void UpdateInfo()
    {
        SetRemoteIP();
        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(message))
        {
            SendNetworkMessage();
        }
    }

    public void SendNetworkMessage()
    {
        switch (transportType)
        {
            case TransportType.UDP:
                SendMessage_Udp();
                break;

            case TransportType.TCP:
                SendMessage_Tcp();
                break;
        }
    }

    public void StartNetwork()
    {
        socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(“-----”),port);
        IpEndPoint = new IPEndPoint(IPAddress.Parse("10.0.103.34"), port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);

    }

    public void SendMessage_Udp()
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.SendTo(data, data.Length, SocketFlags.None, IpEndPoint);
    }

    public void SendMessage_Tcp()
    {
        Socket newSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
    }

    #endregion
}
