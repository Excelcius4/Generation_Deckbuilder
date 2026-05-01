using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Sprite[] slides;
    [SerializeField] GameObject slideDisplayer;
    public int slideIndex;
    private bool showingTutorialNow = false;

    void Start()
    {
        slideDisplayer.GetComponent<Image>().sprite = slides[0];
    }
    public void tutorialClicked()
    {
        if (showingTutorialNow)
        {
            gameObject.SetActive(false);
            showingTutorialNow = false;
            return;
        }
        gameObject.SetActive(true);
        showingTutorialNow = true;
    }

    public void leftArrowClicked()
    {
        if (showingTutorialNow)
        {
            if (slideIndex > 0)
            {
                slideIndex -= 1;
            }
            slideDisplayer.GetComponent<Image>().sprite = slides[slideIndex];
        }
    }
    public void rightArrowClicked()
    {
        if (showingTutorialNow)
        {
            if (slideIndex < slides.Length - 1)
            {
                slideIndex += 1;
            }
            slideDisplayer.GetComponent<Image>().sprite = slides[slideIndex];
        }
    }
}
