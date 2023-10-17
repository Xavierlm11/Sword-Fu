using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkSettings", menuName = "ScriptableObjects/NetworkSettings")]
public class NetworkSettings : ScriptableObject
{
    public int port;
    public int messageMaxBytes;
}
