using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("setup stuff")]
    [SerializeField] List<GameObject> startingDeck;
    [SerializeField] int startingMoney;
    [Header("buttons")]
    [SerializeField] GameObject continueButton;
    [SerializeField] GameObject newGameButton;
    [SerializeField] GameObject settingsButton;

    private bool hasContinueGame = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("SaveData", "") != "")
        {
            hasContinueGame = true;
        }
        else
        {
            continueButton.GetComponent<Button>().interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void continueClicked()
    {
        if (hasContinueGame)
        {
            string json = PlayerPrefs.GetString("SaveData", "");
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data.isInGameplay)
            {
                SceneManager.LoadScene("Gameplay");
            }
            else
            {
                SceneManager.LoadScene("Shop");
            }
        }
    }
    public void newGameClicked()
    {
        SaveData data = new SaveData
        {
            cards = startingDeck,
            playerHealth = 100,
            playerMoney = startingMoney,
            playerDay = 1,
            isInGameplay = true
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Gameplay");
    }
    public void tutorialClicked()
    {

    }
}
