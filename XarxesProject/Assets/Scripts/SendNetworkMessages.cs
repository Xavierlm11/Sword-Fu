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
    private NetworkSettings networkSettings;

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
    private IPEndPoint IpEndPoint;

    [SerializeField]
    private Thread networkThread;

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
        port = networkSettings.port;

        StartNetwork();

        networkThread = new Thread(SendNetworkMessage);
    }

    public void StartNetwork()
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
        IpEndPoint = new IPEndPoint(IPAddress.Parse("10.0.103.34"), port);
        //IpEndPoint = new IPEndPoint(IPAddress.Any, port);
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateInfo();
        }
    }

    private void UpdateInfo()
    {
        //SetRemoteIP();
        //if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(message))
        //{
        //    Call_SendNetworkMessage();
        //}
    }

    public void SendNetworkMessage()
    {
        switch (transportType)
        {
            case TransportType.UDP:
                SendMessage_UDP();
                break;

            case TransportType.TCP:
                SendMessage_TCP();
                break;
        }
    }

    

    public void SendMessage_UDP()
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.SendTo(data, data.Length, SocketFlags.None, IpEndPoint);
        Debug.Log("Sended: " + message);
    }

    public void SendMessage_TCP()
    {
        
    }

    public void Call_SendNetworkMessage()
    {
        networkThread.Start();
    }
    #endregion
}
