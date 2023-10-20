using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown transportDropdown;

    [SerializeField]
    private TMP_InputField nicknameField;

    [SerializeField]
    private TMP_InputField remoteIpField;

    private void OnEnable()
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

    private bool NicknameIsEmpty()
    {
        return string.IsNullOrEmpty(nicknameField.text);
    }

    private bool RemoteIpIsEmpty()
    {
        return string.IsNullOrEmpty(remoteIpField.text);
    }

    public void OnClick_GoToServerScene()
    {
        if (!NicknameIsEmpty())
        {
            NetworkManager.Instance.hasInitialized = false;
            SceneManager.LoadScene(NetworkManager.Instance.serverSceneName);
        }
    }

    public void OnClick_GoToClientScene()
    {
        if (!NicknameIsEmpty() && !RemoteIpIsEmpty())
        {
            NetworkManager.Instance.hasInitialized = false;
            SceneManager.LoadScene(NetworkManager.Instance.clientSceneName);
        }
    }
}
