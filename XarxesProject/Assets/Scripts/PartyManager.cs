using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{

    public int playerID = 0;
    //private int partyCapacity = 4;

    //public List<ConnectionManager.PartyPlayersInfo> partyPlayersList = new List<ConnectionManager.PartyPlayersInfo>();
    //public List<ConnectionManager.PartyPlayersInfo> partyPlayersList = new List<ConnectionManager.PartyPlayersInfo>();

    //public GameObject player1;

   // public GameObject player2;



    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void AddPartyPlayer(ConnectionManager.PartyPlayersInfo partyPlayer)
    //{
    //    int newID = 0;
    //    //partyPlayersList.Sort();
    //    foreach (ConnectionManager.PartyPlayersInfo item in partyPlayersList)
    //    {
    //        if (item.playerID >= newID)
    //        {
    //            newID += 1;
    //        }
    //    }

    //    partyPlayer.playerID = newID;
    //    partyPlayersList.Add(partyPlayer);
    //}
}
