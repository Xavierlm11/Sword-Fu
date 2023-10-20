using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    #region variables

    public static ConnectionManager Instance;

    [SerializeField]
    private IPEndPoint IpEndPoint;

    [SerializeField]
    private Thread networkThread;

    #endregion

    #region methods

    private void OnEnable()
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

    private void OpenNewThreat()
    {
       // networkThread = new Thread(SendNetworkMessage);
    }

    public void SetRemoteIP(string ip)
    {
        NetworkManager.SetEndPoint(ref IpEndPoint, IPAddress.Parse(ip), NetworkManager.Instance.port);
    }

    #endregion
}
