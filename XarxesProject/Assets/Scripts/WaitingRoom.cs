using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRoom : MonoBehaviour
{
    public GameObject startGame;

    void OnEnable()
    {
        if (NetworkManager.Instance.GetLocalClient() != null && NetworkManager.Instance.GetLocalClient().isHost)
        {
            startGame.SetActive(true);
        }
        else
        {
            startGame.SetActive(false);
        }
    }

}
