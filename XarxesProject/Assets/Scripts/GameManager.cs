using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerSpawnPoint;

    public static GameManager Instance;

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

    private void Start()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
        {
            CreateParty();
        }
    }

    public void ResetCharacters()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            EraseAllCharacters();
            PlayerManager.Instance.SpawnCharacters();
        }
    }

    public void CreateParty()
    {
        NetworkManager.Instance.activeRoom.party = new Party();

        foreach (Client cl in NetworkManager.Instance.activeRoom.clients)
        {
            PlayerInfo newPlayer = new PlayerInfo(cl);
            NetworkManager.Instance.activeRoom.party.players.Add(newPlayer);
        }

        UpdateParty updateParty = new UpdateParty(NetworkManager.Instance.activeRoom.party);
        updateParty.transferType = TransferType.AllClients;
        ConnectionManager.Instance.SerializeToJsonAndSend(updateParty);
    }

    public void EraseAllCharacters()
    {
        for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
        {
            Destroy(PartyManager.Instance.playerCharacterLinks[i].playerCharacter.characterObject);
        }
    }

    public void SpawnPlayers()
    {

        foreach (PlayerInfo pl in NetworkManager.Instance.activeRoom.party.players)
        {
            if (pl.client.nickname == NetworkManager.Instance.GetLocalClient().nickname)
            {
                PlayerManager.Instance.AddPlayer(pl, true);
            }
            else
            {
                PlayerManager.Instance.AddPlayer(pl);
            }
            
        }

        PlayerManager.Instance.SpawnCharacters();
    }
}
