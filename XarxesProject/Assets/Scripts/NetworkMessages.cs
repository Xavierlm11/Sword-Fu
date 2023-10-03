using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NetworkMessages : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField ipField;

    [SerializeField]
    private TMP_InputField messageField;

    [SerializeField]
    private string ip;

    [SerializeField]
    private string message;

    public void SetRemoteIP()
    {
        ip = ipField.text;
    }

    public void SetMessage()
    {
        message = messageField.text;
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
        SetRemoteIP();
        SendNetworkMessage();
    }

    public void SendNetworkMessage()
    {
        UpdateInfo();
    }
}
