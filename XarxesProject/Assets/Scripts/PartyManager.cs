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

    public List<PlayerCharacterLink> playerCharacterLinks = new List<PlayerCharacterLink>();

    public static PartyManager Instance;

    public float sendTimer;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        sendTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!NetworkManager.Instance.GetLocalClient().isHost)
        //{
        //    return;
        //}

        foreach (PlayerCharacterLink link in playerCharacterLinks)
        {
            if (link.isLocal && link.playerCharacter != null && link.playerCharacter.playerMovement != null)
            {
                if (link.playerCharacter.playerMovement.canSendSynchronizationData)
                {
                    SendLocalPlayerPosition(NetworkManager.Instance.useNetworkUpdateInterval);
                }
            }
        }





    }

    public void SendLocalPlayerPosition(bool useInterval)
    {
        sendTimer += Time.deltaTime;

        bool sendPositions = false;

        if (!useInterval)
        {
            sendPositions = true;
        }
        else if (sendTimer >= NetworkManager.Instance.networkUpdateInterval)
        {
            sendPositions = true;
        }

        if(!sendPositions)
        {
            return;
        }

        foreach (PlayerCharacterLink link in playerCharacterLinks)
        {
            if (link.isLocal && link.playerCharacter.characterObject != null)
            {
                PlayerTransform playerTransform = new PlayerTransform(true);
                playerTransform.player = link.playerInfo;

                //playerTransform.position = new float[]
                //{
                //link.playerCharacter.characterObject.transform.position.x,
                //link.playerCharacter.characterObject.transform.position.y,
                //link.playerCharacter.characterObject.transform.position.z
                //};

                playerTransform.positionX = link.playerCharacter.characterObject.transform.position.x;
                playerTransform.positionY = link.playerCharacter.characterObject.transform.position.y;
                playerTransform.positionZ = link.playerCharacter.characterObject.transform.position.z;

                playerTransform.rotationX = link.playerCharacter.characterObject.transform.rotation.eulerAngles.x;
                playerTransform.rotationY = link.playerCharacter.characterObject.transform.rotation.eulerAngles.y;
                playerTransform.rotationZ = link.playerCharacter.characterObject.transform.rotation.eulerAngles.z;

                playerTransform.transferType = TransferType.AllExceptLocal;

                ConnectionManager.Instance.SerializeToJsonAndSend(playerTransform);

                
            }
        }

        sendTimer = 0;
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
