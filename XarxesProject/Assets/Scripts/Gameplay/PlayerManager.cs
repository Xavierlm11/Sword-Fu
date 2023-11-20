using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<GameObject> spawnPoints = new List<GameObject>();
    private List<GameObject> players = new List<GameObject>();

    public int numberOfPlayers = 4;

    private ConnectionManager connectionManager;

    void Start()
    {
        connectionManager = FindObjectOfType<ConnectionManager>();

        

        for (int i = 0; i < numberOfPlayers; i++)
        {
            AddPlayer("Player " + (i + 1));
        }

        AssignSpawnPoints();

        StartCoroutine(SendPlayerPositionsToServer());
    }

    void AddPlayer(string playerName)
    {
        if (players.Count < numberOfPlayers)
        {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.name = playerName;
            players.Add(newPlayer);
            Debug.Log($"{playerName} se ha unido al juego.");
        }
        else
        {
            Debug.Log("¡Ya hay suficientes jugadores en el juego!");
        }
    }

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

    IEnumerator SendPlayerPositionsToServer()
    {
        while (true)
        {
            string message = "PlayerPositions,";

            foreach (GameObject player in players)
            {
                Vector3 position = player.transform.position;
                message += $"{player.name},{position.x},{position.y},{position.z},{player.transform.rotation.eulerAngles.y},";
            }

            connectionManager.Send_Data(() => connectionManager.SerializeToJsonAndSend(message));

           
        }
    }
}