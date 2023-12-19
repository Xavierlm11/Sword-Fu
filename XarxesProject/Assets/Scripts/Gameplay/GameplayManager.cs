using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    private bool isEndOfRound=false;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadNewRound()
    {

    }
    public void CheckEndOfRound()
    {
        if(CountPlayerAlive() <=1) 
        { 
            isEndOfRound = true;
        }
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
