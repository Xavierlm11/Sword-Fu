using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCharacterLink
{
    public bool isLocal;
    public PlayerInfo playerInfo;
    public PlayerCharacter playerCharacter;
}

public class PartyManager : MonoBehaviour
{

    //public int playerID = 0;
    //private int partyCapacity = 4;

    //public List<ConnectionManager.PartyPlayersInfo> partyPlayersList = new List<ConnectionManager.PartyPlayersInfo>();
    //public List<ConnectionManager.PartyPlayersInfo> partyPlayersList = new List<ConnectionManager.PartyPlayersInfo>();

    //public GameObject player1;

    // public GameObject player2;

    private List<PlayerCharacterLink> playerCharacterLinks = new List<PlayerCharacterLink>();

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public PlayerInfo GetLocalPlayerCharacter()
    {

        return null;
    }

    public PlayerCharacterLink GetLocalPlayerCharacterLink()
    {
        foreach (PlayerCharacterLink plCharLink in playerCharacterLinks)
        {
            //if (plCharLink.playerCharacter.is)
            //{

            //}
        }

        return null;
    }

    //public PlayerInfo GetLocalPlayer()
    //{

    //    return null;
    //}

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
