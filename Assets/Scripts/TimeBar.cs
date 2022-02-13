using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    public GameController GameControllerRef;

    /// <summary>
    /// callback when timer expires
    /// </summary>
    public Action OnTimerExpired;

    /// <summary>
    /// dynamically filled timer
    /// </summary>
    public Image TimerFiller;

    /// <summary>
    /// show pulse etc 
    /// </summary>
    public Animator TimeEffectAnimator;

    /// <summary>
    /// time left for game
    /// </summary>
    private float timeLeft;

    [Header("Audio Assets")]
    public AudioClip TimerAudioClip;
    public AudioClip TimerFastAudioClip;
    public AudioClip BuzzerAudioClip;
    public AudioSource TimerBarAudio;
    public AudioSource TimerBoostAudio;

    /// <summary>
    /// when timer starts we cache the total duration
    /// </summary>
    private float currTotalDuration;

    /// <summary>
    /// how long it takes to fill up the timer
    /// </summary>
    public float TimerRefillAnimDuration = 0.25f;

    private void Awake()
    {
        TimerFiller.color = Color.green;
        TimerFiller.fillAmount = 1.0f;
    }

    public void StartTimer(float duration)
    {
        StopAllCoroutines();

        currTotalDuration = duration;
        timeLeft = duration;

        StartCoroutine(TimerRoutine());
    }

    /// <summary>
    /// Allow time attack to regain some time
    /// </summary>
    public void BoostTime(float timeBoost)
    {
        timeLeft += timeBoost;
        timeLeft = Mathf.Min(timeLeft, currTotalDuration);

        StartCoroutine(BoostTimeRoutine());
    }

    private IEnumerator BoostTimeRoutine()
    {
        TimeEffectAnimator.Play("TimeAttackBoostFlyOver");

        yield return new WaitForSeconds(1.0f);

        StopAllCoroutines();

        StartCoroutine(TimerRoutine());
    }

    /// <summary>
    /// simple routine helper to animate the progress bar
    /// </summary>
    private IEnumerator RefillTimer()
    {
        float startVal = TimerFiller.fillAmount;
        float finalVal = timeLeft / currTotalDuration;
        TimerBoostAudio.Play();

        for (float tParam = 0.0f; tParam < 1.0f; tParam += Time.deltaTime / TimerRefillAnimDuration)
        {
            TimerFiller.fillAmount = Mathf.Lerp(startVal, finalVal, tParam);
            yield return null;
        }
    }

    /// <summary>
    /// Main countdown timer routine
    /// </summary>
    private IEnumerator TimerRoutine()
    {
        // animate the refilling from current time to target time
        yield return StartCoroutine(RefillTimer());

        TimeEffectAnimator.Play("Idle");

        RefreshTimeBar();

        while (timeLeft >= 0)
        {
            if (!GameControllerRef.GamePaused)
            {
                timeLeft -= Time.deltaTime;
                TimerFiller.fillAmount = timeLeft / currTotalDuration;

                RefreshTimeBar();
            }
            else
            {
                TimerBarAudio.Stop();
                TimerBarAudio.clip = null;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Helper to refresh the visuals of time bar
    /// </summary>
    private void RefreshTimeBar()
    {
        if (timeLeft <= 0.0f)
        {
            TimeEffectAnimator.Play("Idle");
            TimerBarAudio.Stop();
            TimerBarAudio.pitch = 1.0f;
            TimerBarAudio.PlayOneShot(BuzzerAudioClip);
            OnTimerExpired?.Invoke();
            StopAllCoroutines();
        }
        else if (TimerFiller.fillAmount < 0.25f)
        {
            TimerFiller.color = Color.red;
            TimeEffectAnimator.Play("Pulse_Fast");

            if (TimerBarAudio.clip != TimerFastAudioClip)
            {
                TimerBarAudio.clip = TimerFastAudioClip;
                TimerBarAudio.loop = true;
                TimerBarAudio.Play();
            }
        }
        else if (TimerFiller.fillAmount < 0.5f)
        {
            TimerFiller.color = Color.yellow;
            TimeEffectAnimator.Play("Pulse");

            // timer audio
            if (!TimerBarAudio.isPlaying)
            {
                TimerBarAudio.clip = TimerAudioClip;
                TimerBarAudio.loop = true;
                TimerBarAudio.Play();
            }
        }
        else
        {
            TimerFiller.color = Color.green;
            TimerBarAudio.Stop();
        }
    }
}
