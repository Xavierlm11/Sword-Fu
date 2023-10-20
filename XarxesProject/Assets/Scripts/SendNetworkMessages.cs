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
    private TMP_InputField nickNameField;

    [SerializeField]
    private string ip;

    [SerializeField]
    private string message;
    
    [SerializeField]
    private string nickName;

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

    private ChatLog _chatLog;

    private bool isMessage = false;
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

    public void SetNickName()
    {
        nickName = nickNameField.text;
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
        nickNameField.text = nickName;

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

        _chatLog.LogMessageToChat(nickName + ": " + message);//para hacer que el mensaje se vea en el chat
        //no se xq no me deja usar la funcion por algo del transform y thread
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
        _chatLog.LogMessageToChat(nickName + ": " + message);//para hacer que el mensaje se vea en el chat

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
