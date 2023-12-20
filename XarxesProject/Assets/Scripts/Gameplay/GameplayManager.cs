using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
    private bool isEndOfRound=false;

    [SerializeField]
    private float timeWinScreen = 5.0f;
    [SerializeField]
    private float dtWinScreen = 0f;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if(isEndOfRound)
        {
            dtWinScreen = Time.deltaTime;
            if(dtWinScreen >= timeWinScreen)
            {
                dtWinScreen = 0f;
                winScreen.SetActive(false);
                LoadNewRound();
                isEndOfRound = false;
            }
        }

    }
    public void LoadNewRound()
    {

    }
    public void CheckEndOfRound()
    {
        if(CountPlayerAlive() <=1) 
        {
            winScreen.SetActive(true);
            WinnerText.text = GetWinner().playerInfo.client.nickname + " somehow won";
            isEndOfRound = true;
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
}
