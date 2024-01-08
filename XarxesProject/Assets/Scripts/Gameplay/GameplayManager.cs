using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class GameplayManager : MonoBehaviour
{
    #region variables

    [SerializeField]
    private int rounds = 3;

    [SerializeField]
    private int currentRound = 1;

    [SerializeField]
    private GameObject winScreen;

    [SerializeField]
    private TMP_Text WinnerText;

    [SerializeField]
    private TMP_Text EndGameWinnerText;

    [SerializeField]
    public bool isEndOfRound = false;

    [SerializeField]
    private float timeWinScreen = 5.0f;

    [SerializeField]
    private float dtWinScreen = 0f;

    [SerializeField]
    private float timeEndGame = 5.0f;

    [SerializeField]
    private float dtEndGame = 0f;

    [SerializeField]
    private float timeIsAlive = 5.0f;

    [SerializeField]
    private float dtIsAlive = 0f;

    [SerializeField]
    private int levelsCount = 0;

    [SerializeField]
    private int startLevelsIndex = 1;

    [SerializeField]
    private int lastLevelIndex = 1;

    [SerializeField]
    public int randomLvl = 1;

    [SerializeField]
    private int playerAlive = 0;

    // private List<Scene> scenesList = new List<Scene>();

    [SerializeField]
    private GameObject leaderboardScreen;

    [SerializeField]
    private GameObject pauseScreen;

    [SerializeField]
    private GameObject endGameScreen;

    [SerializeField]
    private GameObject winEndGameScreen;

    [SerializeField]
    public bool isPaused = false;

    [SerializeField]
    public bool canPaused = true;

    [SerializeField]
    public bool isRoundZero = true;

    [SerializeField]
    public bool smoWon = false;

    [SerializeField]
    private List<GameObject> winsList = new List<GameObject>();

    [SerializeField]
    private List<GameObject> leaderPlayersObjList = new List<GameObject>();

    [SerializeField]
    private int[] winsPlayers = { 0, 0, 0, 0 };

    private bool isF1 = false;

    public bool isChangPhase = false;
    public bool isRecivePhase = false;
    public int winnerId = -1;
    public TextMeshProUGUI localNicknameText;

    #endregion

    public enum roundPhase
    {

        StartGame,
        Starting,
        InGame,
        Ending,
        EndGame,

    }
    private roundPhase roundState = roundPhase.Starting;
    public static GameplayManager Instance;

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
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Scenes on build: " + SceneManager.sceneCountInBuildSettings);

        levelsCount = SceneManager.sceneCountInBuildSettings;

    }

    // Update is called once per frame
    void Update()
    {
        switch (roundState)
        {
            case roundPhase.StartGame:
                {
                    roundState = roundPhase.Starting;
                }
                break;
            //Inizilice the players and set the round
            case roundPhase.Starting:
                {
                    isChangPhase = isRecivePhase = false;
                    winnerId = -1;  
                    CountPlayerAlive();
                    if (!isRoundZero)
                        PlayerManager.Instance.SpawnCharacters();

                    AssingPlayerId();
                    AssingPlayerWins();
                    roundState = roundPhase.InGame;

                }
                break;
            //What happens during the game
            case roundPhase.InGame:
                {

                    if (isEndOfRound)
                    {
                        roundState = roundPhase.Ending;
                        if (!isRecivePhase) isChangPhase = true;
                    }


                    //pause the game for everyone but only can renaude who pause the game
                    if (Input.GetKeyDown(KeyCode.Escape) && canPaused)
                    {

                        if (!isPaused)
                        {
                            isPaused = true;
                        }
                        else
                        {
                            isPaused = false;
                        }
                        PauseTheGame();
                        UpdateGameplayEveryOne();
                    }

                    //Show the wins and names of everyone 
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        leaderboardScreen.SetActive(true);
                        if (!leaderPlayersObjList[0].activeSelf)
                        {
                            for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
                            {
                                if (!leaderPlayersObjList[i].activeSelf)
                                {
                                    leaderPlayersObjList[i].SetActive(true);
                                    leaderPlayersObjList[i].GetComponent<TMP_Text>().text = PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname;
                                }
                            }
                        }

                    }
                    if (Input.GetKeyUp(KeyCode.Tab))
                    {
                        leaderboardScreen.SetActive(false);
                    }

                    //if (Input.GetKeyUp(KeyCode.U))
                    //{
                    //    CountPlayerAlive();
                    //}

                    // Debug kay in case someone is async sync everyone in the same escene
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        if (NetworkManager.Instance.GetLocalClient().isHost)
                        {
                            isF1 = true;
                            isEndOfRound = true;
                            GetRandomNextLvl();
                            UpdateGameplayEveryOneHost();
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        if (NetworkManager.Instance.GetLocalClient().isHost)
                        {
                            RestartGame(); 
                        }
                    }
                        dtIsAlive += Time.deltaTime;
                    if (dtIsAlive >= timeIsAlive)
                    {
                        dtIsAlive = 0f;
                        CheckEndOfRound();
                    }

                }
                break;
            case roundPhase.Ending:
                {
                    dtWinScreen += Time.deltaTime;
                    if (winScreen.activeSelf == false)
                    {
                        PlayerCharacterLink winnerLink = null;
                        if (!isRecivePhase && CountPlayerAlive() <= 1) winnerLink = GetWinner();
                        else winnerLink = GetWinner(true, winnerId);

                        if (winnerLink != null)
                        {
                            WinnerText.text = winnerLink.playerInfo.client.nickname + " somehow won";
                            winnerId = winnerLink.playerCharacter.playerMovement.playerId;
                            if (!isF1) winnerLink.playerCharacter.playerMovement.wins++;
                            else isF1 = false;
                            //winnerLink.playerCharacter.playerMovement.wins=3;
                        }
                        else
                        {
                            WinnerText.text = "Someone" + " somehow won";
                        }



                        UpdateLeaderboard();
                        if (!isRecivePhase && CountPlayerAlive()<=1)
                            UpdateGameplayEveryOne(winnerId, false, true);

                        winScreen.SetActive(true);

                    }


                    if (dtWinScreen >= timeWinScreen)
                    {
                        dtWinScreen = 0f;
                        if (winScreen.activeSelf) winScreen.SetActive(false);


                        //checks if someone alredy won 3 times and loaad next round or end the game
                        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
                        {
                            if (player.playerCharacter.playerMovement.wins < rounds && !smoWon)
                            {
                                LoadNewRound(randomLvl);
                                GameManager.Instance.EraseAllCharacters();
                                roundState = roundPhase.Starting;

                            }
                            else
                            {
                                if (!winEndGameScreen.activeSelf)
                                {
                                    winEndGameScreen.SetActive(true);
                                }
                                PlayerCharacterLink winnerLink = GetWinner();
                                if (winnerLink != null)
                                {
                                    EndGameWinnerText.text = winnerLink.playerInfo.client.nickname + " won the game Congratulations!";

                                }
                                else
                                {
                                    EndGameWinnerText.text = "¡Someone" + " won the game Congratulations!";
                                }
                                roundState = roundPhase.EndGame;
                                smoWon = true;
                            }
                        }

                        isEndOfRound = false;

                    }
                    isRoundZero = false;
                }
                break;
            case roundPhase.EndGame:
                {

                    if (dtEndGame >= timeEndGame)
                    {
                        if (winEndGameScreen.activeSelf)
                            winEndGameScreen.SetActive(false);

                        if (!leaderboardScreen.activeSelf)
                        {
                            leaderboardScreen.SetActive(true);
                            if (!leaderPlayersObjList[0].activeSelf)
                            {
                                for (int i = 0; i < PartyManager.Instance.playerCharacterLinks.Count; i++)
                                {
                                    if (!leaderPlayersObjList[i].activeSelf)
                                    {
                                        leaderPlayersObjList[i].SetActive(true);
                                        leaderPlayersObjList[i].GetComponent<TMP_Text>().text = PartyManager.Instance.playerCharacterLinks[i].playerInfo.client.nickname;
                                    }
                                }
                            }
                            GameManager.Instance.EraseAllCharacters();
                        }
                        if (!endGameScreen.activeSelf && NetworkManager.Instance.GetLocalClient().isHost)
                        {
                            endGameScreen.SetActive(true);
                        }

                    }
                    else
                    {

                        dtEndGame += Time.deltaTime;
                    }

                }
                break;
        }



    }

    public void PauseTheGame()
    {
        if (isPaused)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    //assing the wins for the players at start of the round
    public void AssingPlayerWins()
    {
        int count = 0;
        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        {

            if (player != null && player.playerCharacter != null && player.playerCharacter.playerMovement != null)
            {
                player.playerCharacter.playerMovement.wins = winsPlayers[count];

                count++;
            }
        }
    }
    public void AssingPlayerId()
    {
        int count = 0;
        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        {

            if (player != null && player.playerCharacter != null && player.playerCharacter.playerMovement != null)
            {
                player.playerCharacter.playerMovement.playerId = count;

                count++;
            }
        }
    }
    public void UpdatePLayersAlive(int playerId)
    {


        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        {

            if (player != null && player.playerCharacter != null && player.playerCharacter.playerMovement != null)
            {
                if (player.playerCharacter.gameObject.activeSelf && player.playerCharacter.playerMovement.playerId == playerId && player.playerCharacter.playerMovement.isAlive  )
                {
                    Debug.Log("ESTA MUERTOOOOOaaaaa " + player.playerCharacter.playerMovement.isAlive);
                    player.playerCharacter.playerMovement.ReceiveDamage(20);
                    Debug.Log("PLAYER IDDDDDDD" +  playerId);
                }
            }
        }
    }
    public void UpdateLeaderboard()
    {
        int count = 0;

        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        {

            if (player != null && player.playerCharacter != null && player.playerCharacter.playerMovement != null)
            {
                for (int i = 0; i < player.playerCharacter.playerMovement.wins; i++)
                {

                    if (winsList[count].transform.childCount >= player.playerCharacter.playerMovement.wins)
                    {
                        winsList[count].transform.GetChild(i).GetComponent<RawImage>().color = Color.yellow;
                        winsPlayers[count] = player.playerCharacter.playerMovement.wins;
                    }
                }

                count++;
            }
        }
    }
    public void RestartGame()
    {
        if (NetworkManager.Instance.GetLocalClient().isHost)
            UpdateGameplayEveryOneHost(true);

        leaderboardScreen.SetActive(false);
        endGameScreen.SetActive(false);
        smoWon = false;
        ResetLeaderboard();
        dtEndGame = 0f;
        LoadNewRound(randomLvl);
        roundState = roundPhase.StartGame;
    }

    private void ResetLeaderboard()
    {
        int count = 0;

        //foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        //{

        //    if (player != null && player.playerCharacter != null && player.playerCharacter.playerMovement != null)
        //    {
        //        for (int i = 0; i < player.playerCharacter.playerMovement.wins; i++)
        //        {

        //            if (winsList[count].transform.childCount >= player.playerCharacter.playerMovement.wins)
        //            {
        //                winsList[count].transform.GetChild(i).GetComponent<RawImage>().color = Color.gray;
        //            }
        //        }
        //        winsPlayers[count] = player.playerCharacter.playerMovement.wins = 0;

        //        count++;
        //    }
        //}
        for (int i = 0; i < winsPlayers.Length; i++)
        {
            winsPlayers[i] = 0;
            //if (winsList[i]!=null && winsList[i].transform.childCount >= i && winsList[i].transform.GetChild(i)!=null)
            //{
            //}
            for (int j = 0; j < winsList[i].transform.childCount; j++)
            {
                winsList[i].transform.GetChild(j).GetComponent<RawImage>().color = Color.gray;

            }
            count++;
        }
    }

    public void LoadNewRound(int lvlIndex)
    {

        SceneManager.LoadScene(lvlIndex);
        Debug.Log("Loaded Level: " + lvlIndex);

    }
    public void LoadNewRandomRound()
    {

        SceneManager.LoadScene(randomLvl);
        Debug.Log("Loaded Level: " + randomLvl);

    }

    private int GetRandomNextLvl()
    {
        while (randomLvl == lastLevelIndex)
        {
            randomLvl = Random.Range(startLevelsIndex, levelsCount);
        }
        Debug.Log("NEXT LEVEL IS: " + randomLvl);
        lastLevelIndex = randomLvl;
        return randomLvl;
    }

    public void CheckEndOfRound()
    {
        if (CountPlayerAlive() <= 1)
        {

            if (NetworkManager.Instance.GetLocalClient().isHost)
            {
                GetRandomNextLvl();
                UpdateGameplayEveryOneHost();
            }
            isEndOfRound = true;


        }
    }
    private PlayerCharacterLink GetWinner(bool isReceivePhase = false, int playerId = -1)
    {
        PlayerCharacterLink link = null;


        foreach (PlayerCharacterLink item in PartyManager.Instance.playerCharacterLinks)
        {
            if (item != null && item.playerCharacter != null && item.playerCharacter.playerMovement != null)
            {

                if (!isReceivePhase)
                {
                    if (item.playerCharacter.playerMovement.isAlive)
                    {

                        return item;
                    }
                }
                else
                {
                    if (item.playerCharacter.playerMovement.playerId == playerId)
                    {

                        return item;
                    }
                }
            }
        }

        return link;
    }
    public int CountPlayerAlive()
    {
        int count = 0;
        foreach (PlayerCharacterLink item in PartyManager.Instance.playerCharacterLinks)
        {
            if (item != null && item.playerCharacter != null && item.playerCharacter.playerMovement != null)
            {

                if (item.playerCharacter.playerMovement.isAlive)
                {
                    count++;
                }
            }
        }
        Debug.Log("Players alive: " + count);
        playerAlive = count;
        return count;
    }
    //updates the gameplay for everyone except the host
    private void UpdateGameplayEveryOneHost(bool isRestart = false)
    {
        UpdateGameplayHost _updateGameplay = new UpdateGameplayHost(randomLvl, isEndOfRound, isRestart);
        _updateGameplay.transferType = TransferType.OnlyClients;
        ConnectionManager.Instance.SerializeToJsonAndSend(_updateGameplay);
    }
    //updates the gameplay for everyone except for yourself
    public void UpdateGameplayEveryOne(int playerId = -1, bool isDead = false, bool isChangingPhase = false)
    {
        UpdateGameplay _updateGameplay = new UpdateGameplay(isPaused, playerId, isDead, isChangingPhase);
        _updateGameplay.transferType = TransferType.AllExceptLocal;
        ConnectionManager.Instance.SerializeToJsonAndSend(_updateGameplay);
    }
}
