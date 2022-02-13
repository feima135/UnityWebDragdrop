using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control visual aspect of round/level progression
/// </summary>
public class LevelProgressBar : MonoBehaviour
{
    /// <summary>
    /// Dynamically create the star levels
    /// </summary>
    public Image LevelMilestoneIconPrefab;

    /// <summary>
    /// the part that stretches as rounds progresses
    /// </summary>
    public Image DynamicProgressBar;

    /// <summary>
    /// house all the level milestone icons here
    /// </summary>
    public GameObject LevelMilestonesRoot;

    /// <summary>
    /// dynamically create the level milestone icons (stars)
    /// </summary>
    private List<Image> levelMilestoneIcons = new List<Image>();

    /// <summary>
    /// calculate and cache the total number of rounds
    /// </summary>
    private int totalNumRounds = 0;

    public void InitProgressBar(List<GameController.LevelInfo> levelInfoTable)
    {
        // tabulate the total rounds, each levels has a certain num rounds
        // given total rounds we can stretch the UI image after normalized
        foreach(var level in levelInfoTable)
        {
            totalNumRounds += level.roundsToPlay;
            var clonedMilestoneIcon = Instantiate(LevelMilestoneIconPrefab, LevelMilestonesRoot.transform);
            levelMilestoneIcons.Add(clonedMilestoneIcon);

            clonedMilestoneIcon.gameObject.SetActive(true);

            // hide the star icon initially
            clonedMilestoneIcon.transform.GetChild(0).gameObject.SetActive(false);
        }

        // determine how far along x to place the star icon
        // star icon depicts milestone of a level
        float xPos = 0;
        float hackOffSet = -15.0f;
        foreach (var level in levelInfoTable)
        {
            xPos += (float)level.roundsToPlay / totalNumRounds * GetComponent<RectTransform>().sizeDelta.x;
            levelMilestoneIcons[level.LevelIndex - 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos + hackOffSet, 0);
        }

        DynamicProgressBar.fillAmount = 0.0f;
    }

    /// <summary>
    /// once flyover is done, we show the relevant star
    /// </summary>
    public void ActivateLevelMilestoneIcon(int targetLevelIndex)
    {
        GetComponent<AudioSource>().Play();
        levelMilestoneIcons[targetLevelIndex - 1].gameObject.SetActive(true);
        levelMilestoneIcons[targetLevelIndex - 1].transform.GetChild(0).gameObject.SetActive(true);
    }

    /// <summary>
    /// moving along the progress bar, after player completes a round
    /// </summary>
    public void AdvanceProgressBar()
    {
        StopAllCoroutines();

        // we have completed 1 round
        float stepInc = 1.0f / totalNumRounds;
        float targetTParam = DynamicProgressBar.fillAmount + stepInc;

        StartCoroutine(AnimateProgressBar(targetTParam));
    }

    /// <summary>
    /// simple routine helper to animate the progress bar
    /// </summary>
    private IEnumerator AnimateProgressBar(float targetParam)
    {
        float startVal = DynamicProgressBar.fillAmount;
        float endVal = targetParam;

        float animDuration = 0.5f;
        for(float tParam = 0.0f; tParam < 1.0f; tParam += Time.deltaTime / animDuration)
        {
            DynamicProgressBar.fillAmount = Mathf.Lerp(startVal, endVal, tParam);
            yield return null;
        }
    }
}
