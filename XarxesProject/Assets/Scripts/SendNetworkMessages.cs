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
    private TMP_InputField nicknameField;

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

    [SerializeField]
    private GameObject chatLogObj;

    [SerializeField]
    private ReceiveNetworkMessages receiveNetworkMessages;

    private ChatLog _chatLog;

    private bool isMessage = false;
    private byte[] receivedDataBuffer;
    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    private void OnEnable()
    {
        Resources.LoadAll("");
        NetworkManager.Call_GoToLobbyScene();
    }

    public void SetRemoteIP()
    {
        ip = ipField.text;
        NetworkManager.SetEndPoint(ref IpEndPoint, IPAddress.Parse(ip), NetworkManager.Instance.port);
    }

    public void SetMessage()
    {
        message = messageField.text;
    }

    public void SetNickname()
    {
        NetworkManager.Instance.SetNickname(nicknameField.text);
    }

    private void OpenNewThreat()
    {
        networkThread = new Thread(SendNetworkMessage);
    }

    private void Start()
    {
        socket = NetworkManager.StartNetwork();

        OpenNewThreat();

        ipField.text = ip;
        messageField.text = message;
        nicknameField.text = NetworkManager.Instance.GetNickname();

        _chatLog = chatLogObj.GetComponent<ChatLog>();

        UpdateInfo();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateInfo();
        }
        if (isMessage) CallToChat(); 
    }

    private void UpdateInfo()
    {
        if (!string.IsNullOrEmpty(ip))
        {
            SetRemoteIP();
        }
        if (!string.IsNullOrEmpty(message))
        {
            SetMessage();
        }
    }

    public void SendNetworkMessage()
    {
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                SendMessage_UDP();
                break;

            case TransportType.TCP:
                SendMessage_TCP();
                break;
        }

        ReceiveResponse();

    }

    public void SendMessage_UDP()
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.SendTo(data, data.Length, SocketFlags.None, IpEndPoint);
        Debug.Log("Sended via UDP: " + message);
        isMessage = true;
        
        networkThread.Interrupt();

    }

    void CallToChat()
    {
        _chatLog.LogMessageToChat(NetworkManager.Instance.GetNickname(), message);
        isMessage = false;
    }

    public void SendMessage_TCP()
    {
        if (NetworkManager.Instance.transportType == TransportType.TCP)
        {
            try
            {
                socket.Connect(IpEndPoint);
            }
            catch (SocketException error)
            {
                Debug.Log("Unable to connect to server. Error: " + error.ToString());
                return;
            }
        }

        if (!socket.Connected)
        {
            Debug.Log("Socket is not connected to server");
            return;
        }

        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.Send(data, data.Length, SocketFlags.None);

        Debug.Log("Sended via TCP: " + message);
        isMessage = true;
    }

    private void ReceiveResponse()
    {
        // Espera la respuesta del receptor y muestra en la consola
        if (socket != null)
        {
            receivedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];
            int receivedSize = socket.Receive(receivedDataBuffer);
            string responseMessage = Encoding.ASCII.GetString(receivedDataBuffer, 0, receivedSize);
            Debug.Log("Received response: " + responseMessage);
        }
    }

    public void Call_SendNetworkMessage()
    {
        if (networkThread.ThreadState != ThreadState.Unstarted)
        {
            OpenNewThreat();
        }

        networkThread.Start();
    }

    private void OnDisable()
    {
        if (socket != null)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
    }
    #endregion
}
