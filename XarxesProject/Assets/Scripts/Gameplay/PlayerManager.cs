using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables

    public static PlayerManager Instance;
    public GameObject playerPrefab;
    public List<GameObject> spawnPoints = new List<GameObject>();

    public int numberOfPlayers = 2;

    private ConnectionManager connectionManager;
    private PartyManager partyManagerObj;

    void Start()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // connectionManager = FindObjectOfType<ConnectionManager>();


        //Llama a la funcion de crear un nuevo player segun el numero de players que le hayas asignado

        partyManagerObj = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();


        //foreach (ConnectionManager.PartyPlayersInfo item in partyManagerObj.partyPlayersList)
        //{
        //    if (item.playerID == partyManagerObj.playerID)
        //    {

        //        AddPlayer(item.playerName, true);
        //    }
        //    else
        //    {
        //        AddPlayer(item.playerName);

        //    }
        //}
        //for (int i = 0; i < numberOfPlayers; i++)
        //{
        //    AddPlayer("Player " + (i + 1));
        //}

        //AssignSpawnPoints();


    }

    private void Update()
    {
        //foreach (ConnectionManager.PartyPlayersInfo item in partyManagerObj.partyPlayersList)
        //{
        //    if (item.playerID != partyManagerObj.playerID)
        //    {


        //    }
        //}
    }

    //public void UpdatePlayerPositions(ConnectionManager.PlayerPositionsInfo ppi)
    //{
    //    foreach (GameObject item in players)
    //    {
    //        if (ppi.playerPositions.playerId == item.GetComponent<PlayerMovement>().playerId)
    //        {
    //            Vector3 newpos = new Vector3(ppi.playerPositions.positionX, ppi.playerPositions.positionY, ppi.playerPositions.positionZ);
    //            item.transform.position = newpos;
    //            //item.transform.rotation.eulerAngles.y = ppi.playerPositions.rotY;
    //        }
    //    }
    //}

    public void UpdatePlayerPosition(PlayerTransform ppi)
    {
        //foreach (GameObject item in players)
        //{
        //    if (ppi.playerPositions.playerId == item.GetComponent<PlayerMovement>().playerId)
        //    {
        //        Vector3 newpos = new Vector3(ppi.playerPositions.positionX, ppi.playerPositions.positionY, ppi.playerPositions.positionZ);
        //        item.transform.position = newpos;
        //        //item.transform.rotation.eulerAngles.y = ppi.playerPositions.rotY;
        //    }
        //}
    }

    private void FixedUpdate()
    {

    }

    //Function to spawn players
    public void AddPlayer(PlayerInfo playerInfo, bool isLocal = false)
    {
        //if (players.Count < numberOfPlayers)
        //{

        PlayerCharacterLink newLink = new PlayerCharacterLink();
        

        GameObject newPlayerCharacterObj = Instantiate(playerPrefab);
        newPlayerCharacterObj.name = playerInfo.client.nickname;

        newLink.isLocal = isLocal;
        newLink.playerCharacter = newPlayerCharacterObj.GetComponent<PlayerCharacter>();
        newLink.playerCharacter.characterObject = newLink.playerCharacter.gameObject;
        newLink.playerCharacter.characterObject.SetActive(false);
        newLink.playerCharacter.characterLink = newLink;
        newLink.playerInfo = playerInfo;

        //if (isLocal)
        //{
        //    //newPlayer.GetComponent<PlayerMovement>().playerId = partyManagerObj.playerID;
        //}


        PartyManager.Instance.playerCharacterLinks.Add(newLink);

        Debug.Log($"{playerInfo.client.nickname} has Spawned");
        //}
        //else
        //{
        //    Debug.Log("¡Ya hay suficientes jugadores en el juego!");
        //}
    }

    //Hace que aparezcan los players en los puntos de spawn por orden
    public void AssignSpawnPoints()
    {
        for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
        {
            if (i < spawnPoints.Count)
            {
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject.transform.position = spawnPoints[i].transform.position;
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject.SetActive(true);
                Debug.Log($"{PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname} aparece en el punto {spawnPoints[i].name}.");
            }
            else
            {
                Debug.Log($"No hay suficientes puntos de reaparición para {PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname}.");
            }
        }
    }


}