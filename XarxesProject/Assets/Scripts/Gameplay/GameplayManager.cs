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
    public bool isEndOfRound = false;

    [SerializeField]
    private float timeWinScreen = 5.0f;

    [SerializeField]
    private float dtWinScreen = 0f;

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
    public bool isPaused = false;

    [SerializeField]
    public bool canPaused = true;

    [SerializeField]
    private List<GameObject> winsList = new List<GameObject>();

    [SerializeField]
    private List<GameObject> leaderPlayersObjList = new List<GameObject>();

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
        //for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        //{
        //    if (i > 1)
        //    {
        //        if (SceneManager.GetSceneByBuildIndex(i) != null)
        //            scenesList.Add(SceneManager.GetSceneByBuildIndex(i));
        //        else
        //            Debug.Log("failed scene " + i);
        //    }
        //}
        //Debug.Log("amogassss " +  scenesList.Count);
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
            case roundPhase.Starting:
                {
                    roundState = roundPhase.InGame;
                    CountPlayerAlive();
                }
                break;
            case roundPhase.InGame:
                {
                    if (isEndOfRound)
                    {
                        roundState = roundPhase.Ending;

                    }



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

                    if (Input.GetKeyUp(KeyCode.U))
                    {
                        CountPlayerAlive();
                    }

                    //if (Input.GetKeyDown(KeyCode.L))
                    //{
                    //    GetRandomNextLvl();
                    //    LoadNewRandomRound();
                    //}
                    if (Input.GetKeyDown(KeyCode.K))
                    {
                        if (NetworkManager.Instance.GetLocalClient().isHost)
                        {
                            // winScreen.SetActive(true);
                            isEndOfRound = true;
                            GetRandomNextLvl();
                            UpdateGameplayEveryOneHost();
                        }
                    }
                }
                break;
            case roundPhase.Ending:
                {
                    dtWinScreen += Time.deltaTime;
                    if (winScreen.activeSelf == false)
                    {
                        PlayerCharacterLink winnerLink = GetWinner();
                        if (winnerLink != null)
                        {
                            WinnerText.text = winnerLink.playerInfo.client.nickname + " somehow won";
                            winnerLink.playerCharacter.playerMovement.wins++;
                        }
                        else
                        {
                            WinnerText.text = "Someone" + " somehow won";
                        }



                        UpdateLeaderboard();
                        winScreen.SetActive(true);

                    }


                    if (dtWinScreen >= timeWinScreen)
                    {
                        dtWinScreen = 0f;
                        winScreen.SetActive(false);



                        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
                        {
                            if (player.playerCharacter.playerMovement.wins < rounds)
                            {
                                LoadNewRound(randomLvl);
                                roundState = roundPhase.Starting;

                            }
                            else
                            {
                                roundState = roundPhase.EndGame;
                            }
                        }
                        roundState = roundPhase.Starting;
                        isEndOfRound = false;
                    }
                }
                break;
            case roundPhase.EndGame:
                {

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

    public void UpdateLeaderboard()
    {
        int count = 0;

        foreach (PlayerCharacterLink player in PartyManager.Instance.playerCharacterLinks)
        {

            if (player != null)
            {
                for (int i = 0; i < player.playerCharacter.playerMovement.wins; i++)
                {

                    if (winsList[count].transform.childCount > player.playerCharacter.playerMovement.wins)
                    {
                        winsList[count].transform.GetChild(i).GetComponent<RawImage>().color = Color.yellow;
                    }
                }

                //foreach (GameObject stars in winsList)
                //{

                //}
                //for (int i = 0; i < winsList.Count; i++)
                //{
                //    winsList[i].transform.GetChild()
                //}
                //for (int j = 0; j < count; j++)
                //{
                //    for (int i = 0; i < player.playerCharacter.playerMovement.wins; i++)
                //    {
                //        winsList[j].transform.GetChild(i).GetComponent<RawImage>().color = Color.yellow;
                //    } 
                //}

                //if (player.playerCharacter.playerMovement)
                //{

                //}
                count++;
            }
        }
    }
    public void LoadNewRound(int lvlIndex)
    {

        SceneManager.LoadScene(lvlIndex);
        Debug.Log("Loaded Level: " + lvlIndex);

    }
    public void LoadNewRandomRound()
    {
        //for (int i = 0; i < SceneManager.; i++)
        //{
        //    SceneManager.GetSceneByName("Level" + "i%");
        //}


        //lastLevelIndex = randomLvl;
        SceneManager.LoadScene(randomLvl);
        Debug.Log("Loaded Level: " + randomLvl);
        // Debug.Log("lvl index Level: " + startLevelsIndex);
    }

    private int GetRandomNextLvl()
    {
        while (randomLvl == lastLevelIndex)
        {
            randomLvl = Random.Range(startLevelsIndex, levelsCount);
        }
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
            //winScreen.SetActive(true);

        }
    }
    private PlayerCharacterLink GetWinner()
    {
        PlayerCharacterLink link = null;


        foreach (PlayerCharacterLink item in PartyManager.Instance.playerCharacterLinks)
        {
            if (item != null)
            {

                if (item.playerCharacter.playerMovement.isAlive)
                {

                    return item;
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
            if (item != null)
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

    private void UpdateGameplayEveryOneHost()
    {
        UpdateGameplayHost _updateGameplay = new UpdateGameplayHost(randomLvl, isEndOfRound);
        _updateGameplay.transferType = TransferType.OnlyClients;
        ConnectionManager.Instance.SerializeToJsonAndSend(_updateGameplay);
    }
    private void UpdateGameplayEveryOne()
    {
        UpdateGameplay _updateGameplay = new UpdateGameplay(isPaused);
        _updateGameplay.transferType = TransferType.AllExceptLocal;
        ConnectionManager.Instance.SerializeToJsonAndSend(_updateGameplay);
    }
}
