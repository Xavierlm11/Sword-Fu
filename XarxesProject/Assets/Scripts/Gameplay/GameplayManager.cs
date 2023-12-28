using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    #region variables

    [SerializeField]
    private int rounds = 4;

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

    private List<Scene> scenesList = new List<Scene>();
    #endregion

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
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (i > 1)
            {
                if (SceneManager.GetSceneByBuildIndex(i) != null)
                    scenesList.Add(SceneManager.GetSceneByBuildIndex(i));
                else
                    Debug.Log("failed scene " + i);
            }
        }
        //Debug.Log("amogassss " +  scenesList.Count);
        Debug.Log("Scenes on build: " + SceneManager.sceneCountInBuildSettings);
        //Debug.Log(SceneManager.GetSceneByBuildIndex(0).name);
        levelsCount = SceneManager.sceneCountInBuildSettings;

    }

    // Update is called once per frame
    void Update()
    {

        if (isEndOfRound)
        {
            dtWinScreen += Time.deltaTime;
            if (winScreen.activeSelf == false)
            {
                winScreen.SetActive(true);
            }
            //WinnerText.text = GetWinner().playerInfo.client.nickname + " somehow won";
            WinnerText.text = "PACO " + " somehow won";
            if (dtWinScreen >= timeWinScreen)
            {
                dtWinScreen = 0f;
                winScreen.SetActive(false);
                //if (NetworkManager.Instance.GetLocalClient().isHost)
                //{
                //    LoadNewRandomRound();

                //}
                //else
                //{
                //    LoadNewRound(randomLvl);
                //}
                LoadNewRound(randomLvl);
                isEndOfRound = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetRandomNextLvl();
            LoadNewRandomRound();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (NetworkManager.Instance.GetLocalClient().isHost)
            {
               // winScreen.SetActive(true);
                isEndOfRound = true;
                GetRandomNextLvl();
                UpdateGameplayEveryOne();
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
                UpdateGameplayEveryOne();
            }
            isEndOfRound = true;
            winScreen.SetActive(true);

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
    private int CountPlayerAlive()
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
        return count;
    }

    private void UpdateGameplayEveryOne()
    {
        UpdateGameplay _updateGameplay = new UpdateGameplay(randomLvl, isEndOfRound/*, GetWinner().playerInfo.client.nickname*/);
        _updateGameplay.transferType = TransferType.OnlyClients;
        ConnectionManager.Instance.SerializeToJsonAndSend(_updateGameplay);
    }
}
