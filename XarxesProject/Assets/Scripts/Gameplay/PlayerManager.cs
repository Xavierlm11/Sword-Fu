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
    }


    //Adds players base data structure
    public void AddPlayer(PlayerInfo playerInfo, bool isLocal = false)
    {

        PlayerCharacterLink newLink = new PlayerCharacterLink();

        newLink.isLocal = isLocal;
        newLink.playerInfo = playerInfo;

        PartyManager.Instance.playerCharacterLinks.Add(newLink);

    }

    public void GetSpawns()
    {
        spawnPoints.Clear();
        spawnPoints = SpawnManager.Instance.spawnPoints;
    }

    public void SpawnCharacters()
    {
        GetSpawns();

        for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
        {
            if (i < spawnPoints.Count)
            {
                GameObject newPlayerCharacterObj = Instantiate(playerPrefab);
                newPlayerCharacterObj.name = PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname;

                PartyManager.Instance.playerCharacterLinks[i].playerCharacter = newPlayerCharacterObj.GetComponent<PlayerCharacter>();
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject = PartyManager.Instance.playerCharacterLinks[i].playerCharacter.gameObject;
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterLink = PartyManager.Instance.playerCharacterLinks[i];
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject.transform.position = spawnPoints[i].transform.position;
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.playerMovement = PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject.GetComponent<PlayerMovement>();
                PartyManager.Instance.playerCharacterLinks[i].playerCharacter.playerMovement.canSendSynchronizationData = true;

                Debug.Log($"[{PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname}] spawns on the point [{spawnPoints[i].name}].");
            }
            else
            {
                Debug.Log($"There is no space left for [{PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname}].");
            }
        }
    }


}