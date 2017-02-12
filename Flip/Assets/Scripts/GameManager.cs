using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    // Singleton instance
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public GameObject mainUI;
    public GameObject pauseUI;
    public GameObject bottleCountSprite;
    public GameObject bottlePlusSprite;
    public AudioClip startSound;
    public AudioClip winSound;
    public int currentRound;
    public int numberOfRounds = 5;
    public bool winState;
    public bool roundActive;
    public int bottleCount = 10;
    public int requiredFlips = 3;
    public int flipsLanded = 0;

    // UI object names
    private string NEXT_ROUND_BUTTON = "NextRoundButton";
    private string WIN_TEXT = "WinText";
    private string ROUND_NUMBER_TEXT = "RoundNumberText";
    private string FLIP_TEXT = "FlipText";
    private string BOTTLE_COUNT_TEXT = "BottleCountText";
    private string FLIPS_LANDED_TEXT = "FlipsLandedText";
    private string LOSE_TEXT = "LoseText";
    private string RETRY_BUTTON = "RetryButton";
    private string PLUS = "Plus";
    private string GAME_OVER_ELEMENTS = "GameOverElements";

    private bool setupRound;
    private float setupTimer = 2.5f;
    private float currSetupTime = 0;
    private int initBottleCount;
    private int initRequiredFlips;
    private float plusTimer = 1f;
    private float currPlusTime = 0.1f;
    private bool paused;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        currentRound = 1;
        initBottleCount = bottleCount;
        initRequiredFlips = requiredFlips;
    }
    void Start()
    { 
        StartRound();
    }
	void Update ()
    {
        if (setupRound)
        {
            Setup();
        }

        if (currPlusTime > 0)
        {
            currPlusTime -= Time.deltaTime;
        }
        else if (currPlusTime <= 0)
        {
            DisplayUIObject(PLUS, false);
            bottlePlusSprite.SetActive(false);
        }
	}

    public void WinState()
    {
        AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position, 0.5f);

        winState = true;

        if (currentRound == numberOfRounds)
        {
            GameOver();
            return;
        }

        SetUIText(WIN_TEXT, "Round " + currentRound + " Complete!");
        DisplayUIObject(WIN_TEXT, true);
        DisplayUIObject(NEXT_ROUND_BUTTON, true);
    }
    public void ResetState()
    {
        winState = false;
        bottleCount = initBottleCount;
        requiredFlips = initRequiredFlips;
        flipsLanded = 0;

        GameObject.FindObjectOfType<BottleController>().Reset();

        DisplayUIObject(LOSE_TEXT, false);
        DisplayUIObject(RETRY_BUTTON, false);

        StartRound();
    }
    public void StartRound()
    {
        setupRound = true;
        roundActive = false;
        currSetupTime = setupTimer;

        // Round number display
        SetUIText(ROUND_NUMBER_TEXT, "Round " + currentRound);
        DisplayUIObject(ROUND_NUMBER_TEXT, true);

        // Bottle count display
        SetUIText(BOTTLE_COUNT_TEXT, bottleCount.ToString());

        // Flips landed display
        SetUIText(FLIPS_LANDED_TEXT, flipsLanded.ToString() + " / " + requiredFlips.ToString());
    }
    public void NextRound()
    {
        currentRound++;

        GameObject.FindObjectOfType<BottleController>().ResetOnButton();

        DisplayUIObject(NEXT_ROUND_BUTTON, false);
        DisplayUIObject(WIN_TEXT, false);

        ResetState();
        IncreaseRoundDifficulty();
        StartRound();
    }
    public void LoseState()
    {
        roundActive = false;
        DisplayUIObject(LOSE_TEXT, true);
        DisplayUIObject(RETRY_BUTTON, true);
    }
    public void StartOver()
    {
        initBottleCount = 10;
        initRequiredFlips = 3;

        DisplayUIObject(GAME_OVER_ELEMENTS, false);
        DisplayUIObject(BOTTLE_COUNT_TEXT, true);
        DisplayUIObject(FLIPS_LANDED_TEXT, true);
        bottleCountSprite.SetActive(true);

        ResetState();
    }
    public void GameOver()
    {
        roundActive = false;
        winState = true;

        foreach (Transform child in mainUI.transform)
        {
            GameObject obj = child.gameObject;

            // Hide all UI except for GameOverElements
            if (obj.name != GAME_OVER_ELEMENTS)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }

        bottleCountSprite.SetActive(false);
    }
    public void PauseGame()
    {
        paused = true;
        Time.timeScale = 0;
        pauseUI.SetActive(true);
    }
    public void ContinueGame()
    {
        paused = false;
        Time.timeScale = 1;
        pauseUI.SetActive(false);
    }
    private void Setup()
    {
        if (currSetupTime > 0)
        {
            currSetupTime -= Time.deltaTime;
        }
        else
        {
            currSetupTime = 0;
            setupRound = false;
            roundActive = true;
            DisplayUIObject(FLIP_TEXT, false);
        }

        if (currSetupTime < 1f && currSetupTime > 0)
        {
            if (!GetUIObject(FLIP_TEXT).activeSelf)
            {
                DisplayUIObject(ROUND_NUMBER_TEXT, false);
                DisplayUIObject(FLIP_TEXT, true);
                AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position, 0.5f);
            }
        }
    }
    public void DecrementBottleCount()
    {
        bottleCount--;
        SetUIText(BOTTLE_COUNT_TEXT, bottleCount.ToString());
    }
    public void IncrementBottleCount()
    {
        bottleCount++;
        SetUIText(BOTTLE_COUNT_TEXT, bottleCount.ToString());
        DisplayUIObject(PLUS, true);
        bottlePlusSprite.SetActive(true);
        currPlusTime = plusTimer;
    }
    public void IncrementFlipsLanded()
    {
        flipsLanded++;
        SetUIText(FLIPS_LANDED_TEXT, flipsLanded.ToString() + " / " + requiredFlips.ToString());
    }
    private void IncreaseRoundDifficulty()
    {
        requiredFlips = initRequiredFlips + 2;
        initRequiredFlips = requiredFlips;

        // Only increase bottle count on odd number rounds
        if (currentRound % 2 != 0)
        {
            bottleCount = initBottleCount + 1;
            initBottleCount = bottleCount;
        }
    }
    private void DisplayUIObject(string name, bool display)
    {
        foreach (Transform child in mainUI.transform)
        {
            GameObject obj = child.gameObject;

            if (obj.name == name)
            {
                if (display)
                {
                    obj.SetActive(true);
                }
                else
                {
                    obj.SetActive(false);
                }
                break;
            }
        }
    }
    private void SetUIText(string name, string text)
    {
        foreach (Transform child in mainUI.transform)
        {
            GameObject obj = child.gameObject;

            if (obj.name == name)
            {
                obj.GetComponent<Text>().text = text;
            }
        }
    }
    private GameObject GetUIObject(string name)
    {
        foreach (Transform child in mainUI.transform)
        {
            GameObject obj = child.gameObject;

            if (obj.name == name)
            {
                return obj;
            }
        }

        return null;
    }

}
