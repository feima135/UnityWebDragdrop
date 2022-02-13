using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    public Action<GameItemUI> OnRecieveGameItem;

    /// <summary>
    /// main displayicon
    /// </summary>
    public Image Display;

    /// <summary>
    /// for better drag drop feedback
    /// </summary>
    public Outline BorderDisplay;

    /// <summary>
    /// populated when new customer enters
    /// </summary>
    public CustomerController.CustomerInfo CurrCustomerInfo;

    void Awake()
    {
        BorderDisplay.enabled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnRecieveGameItem?.Invoke(eventData.pointerDrag.GetComponent<GameItemUI>());
    }

    public void OnDropAccept()
    {
        StartCoroutine(AcceptFXRoutine());
    }

    private IEnumerator AcceptFXRoutine()
    {
        Display.sprite = CurrCustomerInfo.AcceptSprite;

        yield return new WaitForSeconds(1.2f);

        Display.sprite = CurrCustomerInfo.DefaultSprite;
    }

    public void OnDragReject()
    {
        StartCoroutine(RejectFXRoutine());
    }

    private IEnumerator RejectFXRoutine()
    {
        BorderDisplay.enabled = true;
        BorderDisplay.effectColor = Color.red;
        Display.sprite = CurrCustomerInfo.RejectSprite;

        yield return new WaitForSeconds(1.2f);

        Display.sprite = CurrCustomerInfo.DefaultSprite;
        BorderDisplay.enabled = false;
    }

    /// <summary>
    /// When other dragged object is hovering over me
    /// </summary>
    public void OnDragHoverEnter()
    {
        Display.sprite = CurrCustomerInfo.DefaultSprite;
        BorderDisplay.enabled = true;
        BorderDisplay.effectColor = Color.cyan;
    }

    /// <summary>
    /// no more dragged object hovering over me
    /// </summary>
    public void OnDragHoverExit()
    {
        BorderDisplay.enabled = false;
        BorderDisplay.effectColor = Color.white;
    }
}
