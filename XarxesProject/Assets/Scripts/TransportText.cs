using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransportText : MonoBehaviour
{
    private void OnEnable()
    {
        string transport = "";
        switch (NetworkManager.Instance.transportType)
        {
            case TransportType.UDP:
                transport = "UDP";
                break;
            case TransportType.TCP:
                transport = "TCP";
                break;
        }
        GetComponent<TextMeshProUGUI>().text = "Using " + transport;
    }
}
