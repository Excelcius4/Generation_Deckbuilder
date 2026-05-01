using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Popup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject[] popups;
    [SerializeField] float timeTillPopup;

    private bool isPopupTriggered = false;
    private float popupTriggerTimeNow = -1f;
    private bool isPopupActive = false;

    void Update()
    {
        if (popupTriggerTimeNow > 0 && !isPopupActive)
        {
            if (Time.time > popupTriggerTimeNow + timeTillPopup)
            {
                foreach (GameObject popup in popups)
                {
                    popup.SetActive(true);
                }
                isPopupActive = true;
                popupTriggerTimeNow = -1f;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPopupTriggered)
        {
            isPopupTriggered = true;
            popupTriggerTimeNow = Time.time;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (GameObject popup in popups)
        {
            popup.SetActive(false);
        }
        isPopupTriggered = false;
        popupTriggerTimeNow = -1f;
        isPopupActive = false;
    }
}
