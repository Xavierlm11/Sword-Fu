using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
public class SocketUdp : MonoBehaviour
{
    private Socket newSocket;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void SocketConnect(string ipString)
    {
        newSocket = new Socket(AddressFamily.InterNetwork,
        SocketType.Dgram,ProtocolType.Udp);

        //IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        byte[] data = Encoding.ASCII.GetBytes(ipString);
        //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(data), port);
    }
}
