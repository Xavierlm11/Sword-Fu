using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables
    public GameObject playerPrefab;
    public List<GameObject> spawnPoints = new List<GameObject>();
    private List<GameObject> players = new List<GameObject>();

    public int numberOfPlayers = 2;

    private ConnectionManager connectionManager;
    private PartyManager partyManagerObj;

    void Start()
    {
        connectionManager = FindObjectOfType<ConnectionManager>();


        //Llama ala funcion de crear un nuevo player segun el numero de players que le hayas asignado

        partyManagerObj = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();


        foreach (ConnectionManager.PartyPlayersInfo item in partyManagerObj.partyPlayersList)
        {
            if (item.playerID == partyManagerObj.playerID)
            {

                AddPlayer(item.playerName, true);
            }
            else
            {
                AddPlayer(item.playerName);

            }
        }
        //for (int i = 0; i < numberOfPlayers; i++)
        //{
        //    AddPlayer("Player " + (i + 1));
        //}

        AssignSpawnPoints();

        
    }

    private void Update()
    {
        foreach (ConnectionManager.PartyPlayersInfo item in partyManagerObj.partyPlayersList)
        {
            if (item.playerID != partyManagerObj.playerID)
            {


            }
        }
    }

    private void FixedUpdate()
    {

    }


    void AddPlayer(string playerName, bool isLocal = false)
    //Funcion que añade un player

    {
        if (players.Count < numberOfPlayers)
        {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.name = playerName;
            newPlayer.GetComponent<PlayerMovement>().isLocal = isLocal;
            if (isLocal) newPlayer.GetComponent<PlayerMovement>().playerId = partyManagerObj.playerID;

            players.Add(newPlayer);
            Debug.Log($"{playerName} se ha unido al juego.");
        }
        else
        {
            Debug.Log("¡Ya hay suficientes jugadores en el juego!");
        }
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
                Debug.Log($"No hay suficientes puntos de reaparición para {players[i].name}.");
            }
        }
    }

    
}