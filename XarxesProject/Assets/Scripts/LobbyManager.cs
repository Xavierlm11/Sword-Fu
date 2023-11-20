using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    #region variables

    public static LobbyManager Instance;

    [SerializeField]
    private TMP_Dropdown transportDropdown;

    [SerializeField]
    private TMP_InputField client_nicknameField;

    [SerializeField]
    private TMP_InputField host_nicknameField;

    [SerializeField]
    private TMP_InputField remoteIpField;

    [SerializeField]
    private TMP_Text titleIp;

    [SerializeField]
    private string temp_nickname;

    [SerializeField]
    private string temp_ip;

    [SerializeField]
    public bool serverHasConfirmedConnection;

    private bool isNoName = false;
    private bool isNoIp = false;

    #region menus
    [SerializeField]
    private GameObject settingClientMenu;

    [SerializeField]
    private GameObject settingHostMenu;

    [SerializeField]
    private GameObject offlineMenu;

    [SerializeField]
    private GameObject waitingRoomMenu;

    [SerializeField]
    private GameObject startGameButton;

    [SerializeField]
    private GameObject noNicknamePopUp;

    [SerializeField]
    private GameObject noIpPopUp;

    [SerializeField]
    private TextMeshProUGUI ipText;

    [SerializeField]
    private TMP_InputField host_localPortField;

    [SerializeField]
    private TMP_InputField client_localPortField;

    [SerializeField]
    private TMP_InputField client_serverPortField;

    [SerializeField]
    private GameObject noPortPopUp;

    #endregion

    [SerializeField]
    private float popUpMessageTime = 2.0f;
    private float popUpMessageDt = 0.0f;

    public List<GameObject> stagesList;
    private stages stage = stages.lobby;
    public enum stages
    {
        lobby,
        settingHost,
        waitingHost,
        settingClient,
        waitingClient,
    }

    #endregion

    #region methods

    private void Awake()
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
        stagesList = new List<GameObject>();
        serverHasConfirmedConnection = false;
        SetTransportType();
        SetInitialValues();
        stagesList.Add(settingClientMenu);
        stagesList.Add(settingHostMenu);
        stagesList.Add(offlineMenu);
        stagesList.Add(waitingRoomMenu);
        stagesList.Add(startGameButton);
        ChangeStage(stages.lobby);
        
    }
    private void Update()
    {
        if (serverHasConfirmedConnection)
        {
            serverHasConfirmedConnection = false;
            BeTheClient();
        }

        if (isNoName || isNoIp)
        {
            popUpMessageDt += Time.deltaTime;

            if (popUpMessageDt >= popUpMessageTime)
            {

                if (isNoName)
                {
                    isNoName = false;
                    noNicknamePopUp.SetActive(false); 
                }
                else if(isNoIp)
                {
                    isNoIp = false;
                    noIpPopUp.SetActive(false);
                }
                popUpMessageDt = 0.0f;
            }
        }
    }

    private void ChangeStage(stages newStage)
    {
        stage = newStage;

        switch (stage)
        {
            case stages.lobby:
                
                foreach (GameObject item in stagesList)
                {
                    if (item == offlineMenu)
                    {
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }
                break;

            case stages.settingHost:
                foreach (GameObject item in stagesList)
                {
                    if (item == settingHostMenu)
                    {
                        titleIp.text = ConnectionManager.Instance.GetLocalIPv4();
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }
                break;

            case stages.waitingHost:
                foreach (GameObject item in stagesList)
                {
                    if (item == waitingRoomMenu || item==startGameButton)
                    {
                        titleIp.text = ConnectionManager.Instance.GetLocalIPv4();
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }
                break;
            case stages.settingClient:
                foreach (GameObject item in stagesList)
                {
                    if (item == settingClientMenu)
                    {
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }
                break;
            case stages.waitingClient:
                foreach (GameObject item in stagesList)
                {
                    if (item == waitingRoomMenu)
                    {
                        titleIp.text = remoteIpField.text;
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }
                break;
        }
    }

    private void PopUpNoName()
    {
        isNoName = true;
        noNicknamePopUp.SetActive(true);
    }
    private void PopUpNoIp()
    {
        isNoIp = true;
        noIpPopUp.SetActive(true);
    }



    private bool Host_NicknameIsEmpty()
    {
        return string.IsNullOrEmpty(host_nicknameField.text);
    }

    private bool Client_NicknameIsEmpty()
    {
        return string.IsNullOrEmpty(client_nicknameField.text);
    }

    private bool RemoteIpIsEmpty()
    {
        return string.IsNullOrEmpty(remoteIpField.text);
    }

    public void UpdateRemoteIp()
    {
        NetworkManager.Instance.UpdateRemoteIP(remoteIpField.text);
    }

    public void UpdateLocalIp()
    {
        NetworkManager.Instance.UpdateLocatIP(ConnectionManager.Instance.GetLocalIPv4());
    }

    public void OnClick_BeTheServer()
    {
        ChangeStage(stages.settingHost);
        OnClick_GetLocalIPv4();
        host_localPortField.text = NetworkManager.Instance.localPort.ToString();
        UpdateLocalIp();
    }

    public void OnClick_BeTheClient()
    {
        ChangeStage(stages.settingClient);
        client_localPortField.text = NetworkManager.Instance.localPort.ToString();
        client_serverPortField.text = NetworkManager.Instance.remotePort.ToString();
        UpdateLocalIp();
    }

    public void SetLocalPort()
    {
        host_localPortField.text = NetworkManager.Instance.localPort.ToString();
        client_localPortField.text = NetworkManager.Instance.localPort.ToString();
    }

    public void OnClick_LocalDefaultPort()
    {
        int defPort = NetworkManager.Instance.defaultPort;
        host_localPortField.text = defPort.ToString();
        client_localPortField.text = defPort.ToString();
        NetworkManager.Instance.UpdateLocalPort(defPort);
    }

    public void OnClick_ServerDefaultPort()
    {
        int defPort = NetworkManager.Instance.defaultPort;
        client_serverPortField.text = defPort.ToString();
        NetworkManager.Instance.UpdateRemotePort(defPort);
    }

    public void OnClick_CreateRoom()
    {
        if (RemoteIpIsEmpty())
        {
            PopUpNoIp();
        }
        else if (Host_NicknameIsEmpty())
        {
            PopUpNoName();
        }
        else
        {
            BeTheServer();
            ConnectionManager.Instance.StartConnections();

            ChangeStage(stages.waitingHost);
        }
    }

    public void OnClick_AutoPort()
    {
        NetworkManager.Instance.localPort = 0;
        ConnectionManager.Instance.SetSocket();
        ConnectionManager.Instance.StartReceivingMessages();
    }

    public void OnClick_JoinHost()
    {
        if (Client_NicknameIsEmpty())
        {
            PopUpNoName();
        }
        else if (RemoteIpIsEmpty())
        {
            PopUpNoIp();
        }
        else
        {
            BeTheClient();
            ConnectionManager.Instance.UpdateEndPointToSend();
            //UpdateRemoteIP();
            ConnectionManager.Instance.StartConnections();
            ChangeStage(stages.waitingHost);

            ConnectToServer();
        }


    }

    public void BeTheServer()
    {
        NetworkManager.Instance.hasInitialized = false;
        Client cl = NetworkManager.Instance.CreateClient(host_nicknameField.text, NetworkManager.Instance.localIp, NetworkManager.Instance.localPort, true);
        NetworkManager.Instance.SetLocalClient(cl);
        NetworkManager.Instance.clients.Add(cl);

        Debug.Log("You are the server");
        //SceneManager.LoadScene(NetworkManager.Instance.chatSceneName);
    }

    public void BeTheClient()
    {
        NetworkManager.Instance.hasInitialized = false;

        Client cl = NetworkManager.Instance.CreateClient(client_nicknameField.text, NetworkManager.Instance.localIp, NetworkManager.Instance.localPort);
        NetworkManager.Instance.SetLocalClient(cl);

        Debug.Log("You are a client");

        //SceneManager.LoadScene(NetworkManager.Instance.chatSceneName);
    }

    private void ConnectToServer()
    {
        ConnectionManager.Instance.Send_Data(ConnectionManager.Instance.ConnectionRequest);
    }

    public void OnClick_GetLocalIPv4()
    {
        remoteIpField.text = ConnectionManager.Instance.GetLocalIPv4();
        ipText.text = ConnectionManager.Instance.GetLocalIPv4();
    }

    private void SetInitialValues()
    {
        client_nicknameField.text = temp_nickname;
        host_nicknameField.text = temp_nickname;
        remoteIpField.text = temp_ip;
    }

    private void SetTransportType()
    {
        string transportString = "";

        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                transportString = NetworkManager.Instance.udpString;
                break;

            case TransportType.TCP:
                transportString = NetworkManager.Instance.tcpString;
                break;

        }

        for (int i = 0; i < transportDropdown.options.Count; i++)
        {
            if (transportDropdown.options[i].text == transportString)
            {
                transportDropdown.value = i;
            }
        }
    }

    #endregion
}
