using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables

    public static PlayerManager Instance;
    public GameObject playerPrefab;
    public List<GameObject> spawnPoints = new List<GameObject>();
    private List<GameObject> players = new List<GameObject>();

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

    public void UpdatePlayers(ConnectionManager.PlayerPositionsInfo ppi)
    {
        foreach (GameObject item in players)
        {
            if (ppi.playerPositions.playerId == item.GetComponent<PlayerMovement>().playerId)
            {
                Vector3 newpos = new Vector3(ppi.playerPositions.positionX, ppi.playerPositions.positionY, ppi.playerPositions.positionZ);
                item.transform.position = newpos;
                //item.transform.rotation.eulerAngles.y = ppi.playerPositions.rotY;
            }
        }
    }

    private void FixedUpdate()
    {

    }

    //Funcion to spawn players
    public void AddPlayer(string playerName, bool isLocal = false)
    {
        //if (players.Count < numberOfPlayers)
        //{
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.name = playerName;
            newPlayer.GetComponent<PlayerMovement>().isLocal = isLocal;
        //if (isLocal)
        //{
        //    //newPlayer.GetComponent<PlayerMovement>().playerId = partyManagerObj.playerID;
        //}

        players.Add(newPlayer);
        Debug.Log($"{playerName} has Spawned");
        //}
        //else
        //{
        //    Debug.Log("�Ya hay suficientes jugadores en el juego!");
        //}
    }

    //Hace que aparezcan los players en los puntos de spawn por orden
    void AssignSpawnPoints()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i < spawnPoints.Count)
            {
                players[i].transform.position = spawnPoints[i].transform.position;
                Debug.Log($"{players[i].name} aparece en el punto {spawnPoints[i].name}.");
            }
            else
            {
                Debug.Log($"No hay suficientes puntos de reaparici�n para {players[i].name}.");
            }
        }
    }


}