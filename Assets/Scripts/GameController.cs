using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [System.SerializableAttribute]
    public class LevelInfo
    {
        // 1, 2 or 3
        public int LevelIndex;

        /// <summary>
        ///  if 0 it means unlimited time
        /// </summary>
        public float TimerDuration;

        // how many rounds before moving on
        public int roundsToPlay = 4;

        [Header("Game Params")]
        public int CrossCheckCountMax = 8;
        public int CrossCheckCountMin = 3;

        /// <summary>
        /// harder levels we don't reset timer
        /// </summary>
        public bool ResetTimerEachRound;

        /// <summary>
        /// when player clears this level
        /// </summary>
        public Sprite LevelPassPopupMSG;

        /// <summary>
        /// when player starts the next message
        /// </summary>
        public Sprite LevelStartPopupMSG;

        /// <summary>
        /// special flag for advanced level to enable/disable audio cue
        /// </summary>
        public bool ProvideAudioCue = true;

        /// <summary>
        /// Time attack feature if activated, will be non zero
        /// determines how much time to award player when round is completed
        /// </summary>
        public float TimeAttackRoundBoost = 0.0f;

        /// <summary>
        /// set when level is done
        /// </summary>
        [HideInInspector]
        public bool LevelCompleted = false;
    }

    /// <summary>
    /// to parent the dragged object
    /// </summary>
    public GameObject UIFrontLayerRoot;

    /// <summary>
    /// dynamic swap out the chinese text at each level
    /// </summary>
    public VerticalLayoutGroup LeftPanelRoot;
    public GameObject ItemTypeBorrowedRoot;
    public GameObject ItemTypeGoRoot;

    /// <summary>
    /// all the picture icons
    /// </summary>
    public GridLayoutGroup RightPanelRoot;

    /// <summary>
    /// for left panel
    /// </summary>
    public GameItemUI GameItemCHTextUIPrefab;

    /// <summary>
    /// for right panel
    /// </summary>
    public GameObject GameItemFoodDisplayUIPrefab;

    public DropZone DropZoneRef;

    /// <summary>
    /// all the level info, details on rounds/levels/timers etc etc
    /// </summary>
    public List<LevelInfo> LevelInfoTable = new List<LevelInfo>();

    /// <summary>
    /// controls the timer required for rounds/levels
    /// </summary>
    public TimeBar TimeBarRef;

    /// <summary>
    /// cache the curr level we are playing
    /// </summary>
    private LevelInfo currLevel;

    /// <summary>
    /// load from resources
    /// </summary>
    private List<GameItem> FoodItemTable = new List<GameItem>();

    private List<GameObject> RPanelUIElements = new List<GameObject>();
    private List<GameItemUI> LPanelUIElements = new List<GameItemUI>();

    /// <summary>
    /// To remove when cross check and populated when level starts
    /// </summary>
    //private List<GameItem> foodItemDynamicTable = new List<GameItem>();

    /// <summary>
    /// populates left panel
    /// </summary>
    private List<GameItem> foodItemCrossCheckTable = new List<GameItem>();

    /// <summary>
    /// summary at each successful level or failed level etc etc
    /// </summary>
    public GameSummary GameSummaryRef;

    /// <summary>
    /// rounds left to the next level
    /// </summary>
    private int roundsLeft = 0;

    /// <summary>
    /// to initlialize progress visuals
    /// </summary>
    public LevelProgressBar LevelProgressBarRef;

    public GameItem DraggedItem = null;

    /// <summary>
    /// play when we receive a correct item
    /// </summary>
    public ParticleSystem ValidShopCartPlacementFX;

    public bool GamePaused;

    /// <summary>
    /// to be displayed when answer is incorrect
    /// </summary>
    public Animator CrossX;

    /// <summary>
    /// only 1 customer object that swap out sprites
    /// </summary>
    public Animator CustomerObject;

    public AudioClip CorrectAnsAudioClip;
    public AudioClip WrongAnsAudioClip;

    public CustomerController CustomerControllerRef;

    /// <summary>
    /// Game specific grouping for left panel items
    /// </summary>
    public GameObject BorrowHeader;
    public GameObject GoHeader;

    /// <summary>
    /// when drag is detected store the ID
    /// </summary>
    //public int DragTouchID = -1;

    //public Text DebugText;
    //public Text DebugText2;

    private void Awake()
    {
        Input.multiTouchEnabled = false;

        DropZoneRef.OnRecieveGameItem += ValidateDropZoneGameItem;

        // load all the game items
        FoodItemTable.AddRange(Resources.LoadAll<GameItem>("GameItems"));

        // cache a dynamic table for cross check
        //foodItemDynamicTable = FoodItemTable;

        // set up right grid table (show picture)
        foreach (var item in FoodItemTable)
        {
            var rightPanelElement = Instantiate(GameItemFoodDisplayUIPrefab, RightPanelRoot.transform);
            RPanelUIElements.Add(rightPanelElement);
            rightPanelElement.GetComponentInChildren<GameItemUI>().InitDragIcon(item, this);
        }

        TimeBarRef.OnTimerExpired += OnTimerExpired;

        LevelProgressBarRef.InitProgressBar(LevelInfoTable);

        roundsLeft = 0;
        GamePaused = false;
    }

    //private void Update()
    //{
    //    //string textOutput = string.Empty;

    //    //for (int touchIndex = 0; touchIndex < Input.touchCount; ++touchIndex)
    //    //{
    //    //    textOutput += "TouchIndex : " + touchIndex + "with" + Input.GetTouch(touchIndex).fingerId + Environment.NewLine;
    //    //}

    //    //DebugText.text = "";

    //    ////DebugText.text = textOutput + " Total Touches " + Input.touchCount;
    //    //if (Input.touchSupported)
    //    //{
    //    //    if (Input.touchCount == 0 || Input.touchCount >= 2)
    //    //    {
    //    //        DraggedItem = null;
    //    //        //DebugText.text = "Ending Drag";
    //    //        //RPanelUIElements.ForEach(item => item.GetComponentInChildren<GameItemUI>().OnEndDrag(null));
    //    //    }
    //    //}
    //}

    private void Start()
    {
        // kick start the game
        SetupNextLevel();
    }

    private void OnTimerExpired()
    {
        // times up and still has item
        if (currLevel.TimerDuration != 0.0f && foodItemCrossCheckTable.Count > 0)
        {
            GameEnded();
        }
    }

    /// <summary>
    /// restart curr level
    /// </summary>
    private void SetupNextLevel()
    {
        LevelInfo prevLevel = currLevel;
        bool gameEnded = false;

        if (currLevel == null)
        {
            currLevel = LevelInfoTable[0];
        }
        else
        {
            prevLevel.LevelCompleted = true;

            // go next level!
            LevelInfo nextLevel = LevelInfoTable.Find(item => item.LevelIndex == currLevel.LevelIndex + 1);

            // check if anymore levels
            if (nextLevel != null)
            {
                currLevel = nextLevel;
            }
            else
            {
                gameEnded = true;
            }
        }

        roundsLeft = currLevel.roundsToPlay;

        // hide time bar if level doesn't need time
        TimeBarRef.gameObject.SetActive(currLevel.TimerDuration > 0.0f);

        GamePaused = true;

        if (!gameEnded)
        {
            GameSummaryRef.ShowNextLevel(prevLevel, currLevel, RestartRoundHelper);
        }
        else
        {
            GameSummaryRef.ShowNextLevel(prevLevel, currLevel, GameEnded);
        }
    }

    /// <summary>
    /// reinitialize the next round
    /// </summary>
    private void ResetRoundParams()
    {
        bool playingFirstRound = roundsLeft == currLevel.roundsToPlay;
        if (currLevel.TimerDuration > 0.0f)
        {
            // if we not resetting timer, then we only start timer at first round
            // if we reset each round then we reset all the time
            if (playingFirstRound && !currLevel.ResetTimerEachRound || currLevel.ResetTimerEachRound)
            {
                TimeBarRef.StartTimer(currLevel.TimerDuration);
            }
        }

        if (currLevel.roundsToPlay > 0)
        {
            --roundsLeft;
        }
        else
        {
            // endless
            roundsLeft = -1;
        }

        LPanelUIElements.ForEach(item => Destroy(item.gameObject));
        LPanelUIElements.Clear();

        // prepare the dynamic table again
        //foodItemDynamicTable = FoodItemTable;

        int maxElements = Mathf.Min(FoodItemTable.Count, currLevel.CrossCheckCountMax);
        int minElements = Mathf.Min(currLevel.CrossCheckCountMin, FoodItemTable.Count);

        List<GameItem> dynamicTable = new List<GameItem>();
        dynamicTable.AddRange(FoodItemTable);

        // determine how many to cross check this level, populate foodItemCrossCheckTable
        int currLevelCrossCheckCount = UnityEngine.Random.Range(minElements, maxElements + 1);
        for (int index = 0; index < currLevelCrossCheckCount; ++index)
        {
            int rngIndex = UnityEngine.Random.Range(0, dynamicTable.Count);
            var rngGameItem = dynamicTable[rngIndex];

            // ensure unique item
            dynamicTable.RemoveAt(rngIndex);

            foodItemCrossCheckTable.Add(rngGameItem);
        }

        dynamicTable.Clear();

        // hide at start
        BorrowHeader.SetActive(false);
        GoHeader.SetActive(false);

        // create the left panel UI
        foreach (var item in foodItemCrossCheckTable)
        {
            var clonedUI = Instantiate(GameItemCHTextUIPrefab, LeftPanelRoot.transform);
            LPanelUIElements.Add(clonedUI);

            // set the sibiling index
            if (item.ItemTypeRef == GameItem.ItemType.Borrow)
            {
                BorrowHeader.SetActive(true);

                // sibling 0 is borrow header
                clonedUI.transform.SetSiblingIndex(1);
            }
            else if (item.ItemTypeRef == GameItem.ItemType.Go)
            {
                GoHeader.SetActive(true);

                clonedUI.transform.SetAsLastSibling();
            }

            clonedUI.InitChineseText(item, this);
            clonedUI.ReadAloudBtn.interactable = currLevel.ProvideAudioCue;
        }
    }

    /// <summary>
    /// Restart round or could be the first round
    /// </summary>
    private void RestartRoundHelper()
    {
        //RPanelUIElements.ForEach(item => item.GetComponentInChildren<GameItemUI>().CacheParams());
        StartCoroutine(RoundCompletedRoutine());
    }

    /// <summary>
    /// Display game ended stuff
    /// </summary>
    private void GameEnded()
    {
        GamePaused = true;

        GameSummaryRef.ShowGameOverPopup(RestartGame, currLevel);
    }

    private void RestartGame()
    {
        GamePaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Check if we drag the correct icon
    /// </summary>
    private void ValidateDropZoneGameItem(GameItemUI dragIcon)
    {
        if(GamePaused)
        {
            return;
        }

        if(foodItemCrossCheckTable.Contains(dragIcon.GameItemOwner))
        {
            DropZoneRef.OnDropAccept();

            GetComponent<AudioSource>().PlayOneShot(CorrectAnsAudioClip);

            ValidShopCartPlacementFX.Play();

            //Destroy(dragIcon.gameObject);

            var CHWordUI = LPanelUIElements.Find(item => item.GameItemOwner == dragIcon.GameItemOwner && item.ActiveInCheckList);
            if(CHWordUI)
            {
                CHWordUI.CrossOutCheckListEffect();
            }

            // 1 less to check
            foodItemCrossCheckTable.Remove(dragIcon.GameItemOwner);

            // check end round
            if (foodItemCrossCheckTable.Count == 0)
            {
                // successfully completed a round
                LevelProgressBarRef.AdvanceProgressBar();

                //bool atFinalLevel = LevelInfoTable[LevelInfoTable.Count - 1] == currLevel;

                // check if level ended
                if (roundsLeft <= 0)
                {
                    SetupNextLevel();
                }
                else
                {
                    RestartRoundHelper();
                }
            }
        }
        // incorrect item, reject!
        else
        {
            GetComponent<AudioSource>().PlayOneShot(WrongAnsAudioClip);

            DropZoneRef.OnDragReject();

            CrossX.Play("CrossXShow",0, 0.0f);
        }
    }

    /// <summary>
    /// simple display when round ends
    /// </summary>
    private IEnumerator RoundCompletedRoutine()
    {
        float customerEnterDuration = 1.5f;

        GamePaused = true;

        // Time attack presentation
        if(currLevel.TimeAttackRoundBoost > 0.0f && roundsLeft < currLevel.roundsToPlay)
        {
            GamePaused = true;

            // show time attack boost flyover
            TimeBarRef.BoostTime(currLevel.TimeAttackRoundBoost);

            yield return new WaitForSeconds(TimeBarRef.TimerRefillAnimDuration);
        }

        // if not the first round
        bool playingFirstLevelFirstRound = currLevel.LevelIndex == 1 && roundsLeft == currLevel.roundsToPlay;
        if(!playingFirstLevelFirstRound)
        {
            // fade chat bubble and customer move out of Scene
            CustomerObject.Play("CustomerExit");
            yield return new WaitForSeconds(customerEnterDuration);
        }

        ResetRoundParams();

        // allow time for grid elements to be arranged
        yield return null;

        // move customer in and fade in chat bubble
        CustomerController.CustomerInfo randomCustomerInfo = CustomerControllerRef.GetRandomCustomerInfo();

        DropZoneRef.Display.sprite = randomCustomerInfo.DefaultSprite;

        CustomerObject.Play("CustomerEnter");

        DropZoneRef.CurrCustomerInfo = randomCustomerInfo;

        yield return new WaitForSeconds(customerEnterDuration);

        GamePaused = false;
    }
}
