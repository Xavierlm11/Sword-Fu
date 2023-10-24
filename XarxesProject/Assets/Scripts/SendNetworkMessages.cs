using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

public class SendNetworkMessages : MonoBehaviour
{
    #region variables

    //[SerializeField]
    //private TMP_InputField ipField;

    [SerializeField]
    private TMP_InputField messageField;

    //[SerializeField]
    //private TMP_InputField nicknameField;

    //[SerializeField]
    //private string ip;

    [SerializeField]
    private string debugMessage;

    [SerializeField]
    private int maxDebugMessageCharSize;

    [SerializeField]
    private int receivedMessageSize;

    //[SerializeField]
    //private Socket socket;

    //[SerializeField]
    //private IPEndPoint IpEndPoint;

    [SerializeField]
    private Thread networkThread;

    [SerializeField]
    private GameObject chatLogObj;

    [SerializeField]
    private ReceiveNetworkMessages receiveNetworkMessages;

    private ChatLog chatLog;

    //private bool isMessage = false;
    private byte[] receivedDataBuffer;
    #endregion

    //-----------------------------------------------------------------------------------------------------------

    #region code

    private void OnEnable()
    {
        Resources.LoadAll("");
        NetworkManager.Call_GoToLobbyScene();
    }

    //public void SetRemoteIP()
    //{
    //    ip = ipField.text;
    //    NetworkManager.SetEndPoint(ref IpEndPoint, IPAddress.Parse(ip), NetworkManager.Instance.port);
    //}

    public void SetMessage()
    {
        debugMessage = messageField.text;
    }

    //public void SetNickname()
    //{
    //    NetworkManager.Instance.SetNickname(nicknameField.text);
    //}

    //private void OpenNewThreat()
    //{
    //    //networkThread = new Thread(SendNetworkMessage);
    //}

    //private void SetInitualValues()
    //{
    //    //ipField.text = ip;
    //    messageField.text = message;
    //    //nicknameField.text = NetworkManager.Instance.GetNickname();

    //    //UpdateInfo();
    //}

    private void Start()
    {
        //socket = NetworkManager.StartNetwork();

        //SetInitualValues();

        //OpenNewThreat();
        chatLog = chatLogObj.GetComponent<ChatLog>();
    }

    public void Update()
    {
        //if (isMessage) CallToChat(); 
    }

    //public void UpdateInfo()
    //{
    //    //if (!string.IsNullOrEmpty(ip))
    //    //{
    //    //    SetRemoteIP();
    //    //}
    //    if (!string.IsNullOrEmpty(message))
    //    {
    //        SetMessage();
    //    }
    //    if (!string.IsNullOrEmpty(NetworkManager.Instance.GetNickname()))
    //    {
    //        SetNickname();
    //    }
    //}

    void CallToChat()
    {
        chatLog.LogMessageToChat(NetworkManager.Instance.GetLocalClient().nickname, debugMessage);
        //isMessage = false;
    }

    private void ReceiveResponse()
    {
        // Espera la respuesta del receptor y muestra en la consola
        //if (socket != null)
        //{
        //    receivedDataBuffer = new byte[NetworkManager.Instance.messageMaxBytes];
        //    int receivedSize = socket.Receive(receivedDataBuffer);
        //    string responseMessage = Encoding.ASCII.GetString(receivedDataBuffer, 0, receivedSize);
        //    Debug.Log("Received response: " + responseMessage);
        //}
    }

    public void Call_SendNetworkMessage()
    {
        //if (networkThread.ThreadState != ThreadState.Unstarted)
        //{
        //    OpenNewThreat();
        //}

        //networkThread.Start();
    }

    private void OnDisable()
    {
        //if (socket != null)
        //{
        //    if (socket.Connected)
        //    {
        //        socket.Shutdown(SocketShutdown.Both);
        //        socket.Close();
        //    }
        //}
    }

    //public void OnClick_GetLocalIPv4()
    //{
    //    ipField.text = NetworkManager.Instance.GetLocalIPv4();
    //    UpdateInfo();
    //}

    

    //private void SendDebugMessage_UDP()
    //{
    //    byte[] data = Encoding.ASCII.GetBytes(debugMessage);

    //    ConnectionManager.Instance.socket.SendTo(data, data.Length, SocketFlags.None, ConnectionManager.Instance.ipEndPointToSend);
    //    Debug.Log("Sended via UDP: " + debugMessage);
    //    ConnectionManager.Instance.isMessage = true;
    //}

    //private void SendDebugMessage_TCP()
    //{
    //    byte[] data = Encoding.ASCII.GetBytes(debugMessage);

    //    ConnectionManager.Instance.socket.Send(data, data.Length, SocketFlags.None);

    //    Debug.Log("Sended via TCP: " + debugMessage);

    //    ConnectionManager.Instance.isMessage = true;
    //}

    #endregion
}
