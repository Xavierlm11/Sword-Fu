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
        NetworkSettings.SetEndPoint(ref IpEndPoint, IPAddress.Parse(ip), NetworkSettings.Instance.port); 
    }

    public void SetMessage()
    {
        message = messageField.text;
    }

    private void Start()
    {

        socket = NetworkSettings.StartNetwork();

        networkThread = new Thread(SendNetworkMessage);

        ipField.text = ip;
        messageField.text = message;

        UpdateInfo();
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
        if (!string.IsNullOrEmpty(ip))
        {
            SetRemoteIP();
        }
    }

    public void SendNetworkMessage()
    {
        switch (NetworkSettings.transportType)
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
        Debug.Log("Sended via UDP: " + message);
    }

    public void SendMessage_TCP()
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.Send(data, data.Length, SocketFlags.None);
        Debug.Log("Sended via TCP: " + message);
    }

    public void Call_SendNetworkMessage()
    {
        networkThread.Start();
    }

    private void OnDisable()//he visto que recomiendan cerrar  todo al fiinal 
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    #endregion
}
