using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<GameObject> spawnPoints = new List<GameObject>();
    private List<GameObject> players = new List<GameObject>();

    public int numberOfPlayers = 4; // Puedes ajustar esto desde el editor de Unity

    void Start()
    {
        // Simulación de agregar jugadores al inicio del juego
        for (int i = 0; i < numberOfPlayers; i++)
        {
            AddPlayer("Player " + (i + 1));
        }

        AssignSpawnPoints();
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
}