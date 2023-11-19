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
    private string temp_nickname;

    [SerializeField]
    private string temp_ip;

    [SerializeField]
    public bool serverHasConfirmedConnection;

    #endregion

    #region methods

    private void Start()
    {
        serverHasConfirmedConnection = false;
        SetTransportType();
        SetInitialValues();
    }
    private void Update()
    {
        if (serverHasConfirmedConnection)
        {
            serverHasConfirmedConnection = false;
            BeTheClient();
        }
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
        }
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

    public void OnClick_BeTheClient()
    {
        if (!NicknameIsEmpty() && !RemoteIpIsEmpty())
        {
            UpdateInfo();
            ConnectToServer();
        }
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
