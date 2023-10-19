using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown transportDropdown;

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
}
