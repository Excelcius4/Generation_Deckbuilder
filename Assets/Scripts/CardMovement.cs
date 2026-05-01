using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor.UI;

public class CardMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float cardAreaYOrigin;
    public float cardAreaChooseYOffset;
    public float cardSpeed;
    public Vector2 targetPos;
    public bool gamePaused;

    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }
    public void Update()
    {
        if (gamePaused)
        {
            return;
        }
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, cardSpeed * Time.deltaTime);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPos.y = cardAreaYOrigin + cardAreaChooseYOffset;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPos.y = cardAreaYOrigin;
    }
}
