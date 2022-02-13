using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameController;

/// <summary>
/// In charge of showing stars etc etc tabbing score
/// </summary>
public class GameSummary : MonoBehaviour
{
    /// <summary>
    /// generic popup display sprite
    /// </summary>
    public Image DisplayContent;

    /// <summary>
    /// to show msg box pop in and out
    /// </summary>
    public Animator PopupAnimator;

    /// <summary>
    /// A generic read cfm proceed btn
    /// </summary>
    public Button ReadCfmBtn;

    /// <summary>
    /// Generic game fail msg sprite
    /// </summary>
    public Sprite GameOverMSG;

    /// <summary>
    /// cache the read OK response
    /// </summary>
    private Action postReadCfmActionCallbackCache;

    /// <summary>
    /// fly off to the progress bar
    /// </summary>
    public Animator FlyingStarIcon;

    /// <summary>
    /// activate the level progress milestones icons
    /// </summary>
    public LevelProgressBar LevelProgressBarRef;

    /// <summary>
    /// fireworks effects controller
    /// </summary>
    public FireworksFX FireworksFXController;

    /// <summary>
    /// to be shown at end of game
    /// </summary>
    public Animator FinalRatingRoot;

    public AudioClip LevelPass;
    public AudioClip LevelFail;

    private void Awake()
    {
        FinalRatingRoot.gameObject.SetActive(false);
        FlyingStarIcon.gameObject.SetActive(false);

        ReadCfmBtn.onClick.AddListener(OnReadCfmBtnClicked);
    }

    /// <summary>
    /// when read cfm button is clicked
    /// </summary>
    private void OnReadCfmBtnClicked()
    {
        PopupAnimator.Play("PopOut");

        postReadCfmActionCallbackCache?.Invoke();

        postReadCfmActionCallbackCache = null;
    }

    /// <summary>
    /// Summary MSG to show details passing this level and going on to the next
    /// </summary>
    public void ShowNextLevel(LevelInfo prevLevel, LevelInfo currLevel, Action postReadCfmActionCallback)
    {
        StartCoroutine(ShowNextLevelRoutine(prevLevel, currLevel, postReadCfmActionCallback));
    }

    /// <summary>
    /// Simple routine to show prelevel pass message and next level msg
    /// </summary>
    private IEnumerator ShowNextLevelRoutine(LevelInfo prevLevel, LevelInfo currLevel, Action postReadCfmActionCallback)
    {
        postReadCfmActionCallbackCache = postReadCfmActionCallback;

        ///////////////////////////
        // Show prev Level Pass MSG
        ///////////////////////////
        if (prevLevel != null)
        {
            PopupAnimator.Play("PopIn");
            DisplayContent.sprite = prevLevel.LevelPassPopupMSG;
            FlyingStarIcon.gameObject.SetActive(false);
            ReadCfmBtn.gameObject.SetActive(false);

            // play cheer audio
            GetComponent<AudioSource>().PlayOneShot(LevelPass);

            // play fireworks effects
            FireworksFXController.PlayFireworks();

            yield return new WaitForSeconds(1.5f);

            // pop in the flying star
            FlyingStarIcon.gameObject.SetActive(true);

            // hold star before flying off
            yield return new WaitForSeconds(1.5f);

            // play the actual star fly off animation
            //FlyingStarIcon.Play("StarPopAndFly_" + prevLevel.LevelIndex);
            FlyingStarIcon.Play("StarPopAndFly_0");

            // flying duration
            yield return new WaitForSeconds(1.5f);

            LevelProgressBarRef.ActivateLevelMilestoneIcon(prevLevel.LevelIndex);

            // hold briefly
            yield return new WaitForSeconds(1.5f);

            // pop out to prepare for next msg
            PopupAnimator.Play("PopOut");
            yield return new WaitForSeconds(1.0f);

            FlyingStarIcon.gameObject.SetActive(false);

            FireworksFXController.StopFireworks();

            GetComponent<AudioSource>().Stop();
        }

        ///////////////////////////
        // Show next level start MSG, if there is a next level
        ///////////////////////////
        if (prevLevel != currLevel)
        {
            PopupAnimator.Play("PopIn");
            DisplayContent.sprite = currLevel.LevelStartPopupMSG;
        }
        else
        {
            postReadCfmActionCallbackCache?.Invoke();
        }

        ReadCfmBtn.gameObject.SetActive(true);
    }

    /// <summary>
    /// show game over with restart option
    /// </summary>
    public void ShowGameOverPopup(Action postReadCfmActionCallback, LevelInfo currLevel)
    {
        StartCoroutine(ShowGameOverRoutine(postReadCfmActionCallback, currLevel));
    }

    /// <summary>
    /// Simple routine to show game over
    /// </summary>
    private IEnumerator ShowGameOverRoutine(Action postReadCfmActionCallback, LevelInfo currLevel)
    {
        ReadCfmBtn.gameObject.SetActive(false);

        postReadCfmActionCallbackCache = postReadCfmActionCallback;
        FlyingStarIcon.gameObject.SetActive(false);

        PopupAnimator.Play("PopIn");
        DisplayContent.sprite = GameOverMSG;

        yield return new WaitForSeconds(1.0f);

        if (currLevel.LevelIndex > 1)
        {
            GetComponent<AudioSource>().PlayOneShot(LevelPass);

            FireworksFXController.PlayFireworks();

            FinalRatingRoot.gameObject.SetActive(true);

            string finalRevealAnimName = "FinalReveal_";

            int levelIndexAttained = currLevel.LevelCompleted ? currLevel.LevelIndex : currLevel.LevelIndex - 1;
            FinalRatingRoot.Play(finalRevealAnimName + levelIndexAttained);

            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(LevelFail);
        }

        ReadCfmBtn.gameObject.SetActive(true);
    }
}
