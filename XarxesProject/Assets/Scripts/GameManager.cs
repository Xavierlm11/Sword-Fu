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

    public void ResetPlayersAtStart()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            EraseAllCharacters();
            SpawnPlayers();
        }
       
        // SpawnPlayers();
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

        for( int i = 0; i <  NetworkManager.Instance.activeRoom.party.players.Count; i++)
        {
            if (NetworkManager.Instance.activeRoom.party.players[i].client.nickname == NetworkManager.Instance.GetLocalClient().nickname)
            {
                PlayerManager.Instance.AddPlayer(NetworkManager.Instance.activeRoom.party.players[i], true);
                NetworkManager.Instance.activeRoom.party.players[i].characterModel = (CharacterModel)i + 1;
            }
            else
            {
                PlayerManager.Instance.AddPlayer(NetworkManager.Instance.activeRoom.party.players[i]);
                NetworkManager.Instance.activeRoom.party.players[i].characterModel = (CharacterModel)i + 1;
            }

            if (i <= NetworkManager.Instance.maxPlayers - 1)
            {

                

            }
            else
            {
                NetworkManager.Instance.activeRoom.party.players[i].isSpectator = true;
            }

        }

        SetNicknameInUI();

        PlayerManager.Instance.SpawnCharacters();
    }

    public void SetNicknameInUI()
    {
        for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
        {
            if (PartyManager.Instance.playerCharacterLinks[i].isLocal)
            {
                GameplayManager.Instance.localNicknameText.text = PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname;
            }
        }

    }
}
