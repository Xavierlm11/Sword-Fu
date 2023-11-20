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
    private TMP_InputField nicknameField;

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


    [SerializeField]
    private GameObject ipObj;

    [SerializeField]
    private GameObject lobbyObj;

    [SerializeField]
    private GameObject waitingObj;

    [SerializeField]
    private GameObject playObj;

    [SerializeField]
    private GameObject setNameObj;

    [SerializeField]
    private GameObject noNameObj;

     [SerializeField]
    private GameObject noIpObj;

    [SerializeField]
    private float popUpMessageTime = 2.0f;
    private float popUpMessageDt = 0.0f;

    public List<GameObject> stagesList;
    private stages stage = stages.lobby;
    public enum stages
    {
        lobby,
        waitingHost,
        selectIp,
        waitingClient,
    }

    #endregion

    #region methods

    private void Start()
    {
        stagesList = new List<GameObject>();
        serverHasConfirmedConnection = false;
        SetTransportType();
        SetInitialValues();
        stagesList.Add(ipObj);
        stagesList.Add(lobbyObj);
        stagesList.Add(waitingObj);
        stagesList.Add(setNameObj);
        stagesList.Add(playObj);
        ChangeStage(stages.lobby);
        
    }
    private void Update()
    {
        if (serverHasConfirmedConnection)
        {
            serverHasConfirmedConnection = false;
            BeTheClient();
        }

        if (isNoName ||isNoIp)
        {
            popUpMessageDt += Time.deltaTime;
            if (popUpMessageDt >= popUpMessageTime)
            {

                if (isNoName)
                {
                    isNoName = false;
                    noNameObj.SetActive(false); 
                }
                else if(isNoIp)
                {
                    isNoIp = false;
                    noIpObj.SetActive(false);
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
                    if (item == setNameObj || item == lobbyObj)
                    {
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
                    if (item == waitingObj ||item==playObj)
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
            case stages.selectIp:
                foreach (GameObject item in stagesList)
                {
                    if (item == ipObj)
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
                    if (item == waitingObj)
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
        noNameObj.SetActive(true);
    }
    private void PopUpNoIp()
    {
        isNoIp = true;
        noIpObj.SetActive(true);
    }



    private bool NicknameIsEmpty()
    {
        return string.IsNullOrEmpty(nicknameField.text);
    }

    private bool RemoteIpIsEmpty()
    {
        return string.IsNullOrEmpty(remoteIpField.text);
    }

    public void OnClick_BeTheServer()
    {
        if (!NicknameIsEmpty())
        {
            BeTheServer();
            ChangeStage(stages.waitingHost);
        }
        else PopUpNoName();
    }

    public void BeTheServer()
    {
        NetworkManager.Instance.hasInitialized = false;
        Client cl = NetworkManager.Instance.CreateClient(nicknameField.text, ConnectionManager.Instance.GetLocalIPv4(), true);
        NetworkManager.Instance.SetLocalClient(cl);

        Debug.Log("You are the server");
        //SceneManager.LoadScene(NetworkManager.Instance.chatSceneName);
    }

    public void BeTheClient()
    {
        NetworkManager.Instance.hasInitialized = false;

        Client cl = NetworkManager.Instance.CreateClient(nicknameField.text, ConnectionManager.Instance.GetLocalIPv4());
        NetworkManager.Instance.SetLocalClient(cl);

        Debug.Log("You are a client");

        //SceneManager.LoadScene(NetworkManager.Instance.chatSceneName);
    }
    public void OnClick_WriteIpClient()
    {
        if (!NicknameIsEmpty() )
        {
            ChangeStage(stages.selectIp);
        }
        else PopUpNoName();
    }
    public void OnClick_BeTheClient()
    {
        if ( !RemoteIpIsEmpty())
        {
            UpdateInfo();
            ConnectToServer();
            ChangeStage(stages.waitingClient);
        }
        else PopUpNoIp();
    }

    private void ConnectToServer()
    {
        ConnectionManager.Instance.Send_Data(ConnectionManager.Instance.ConnectionRequest);
    }

    public void OnClick_GetLocalIPv4()
    {
        remoteIpField.text = ConnectionManager.Instance.GetLocalIPv4();
    }

    private void SetInitialValues()
    {
        nicknameField.text = temp_nickname;
        remoteIpField.text = temp_ip;

        UpdateInfo();
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

    public void UpdateInfo()
    {
        Debug.Log("IP Edited");
        if (!RemoteIpIsEmpty())
        {
            ConnectionManager.Instance.SetRemoteIP(remoteIpField.text);
        }
    }

    #endregion
}
