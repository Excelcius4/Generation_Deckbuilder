using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("top bar")]
    [SerializeField] TMP_Text playerHealth;
    [SerializeField] TMP_Text playerMoney;
    [SerializeField] TMP_Text playerDay;
    [Header("Buttons")]
    [SerializeField] GameObject cardOffer1;
    [SerializeField] GameObject cardOffer2;
    [SerializeField] GameObject cardOffer3;
    [SerializeField] GameObject cardOffer4;
    [SerializeField] GameObject healOffer;
    [SerializeField] GameObject removalOffer;
    [SerializeField] GameObject rerollOffer;
    [SerializeField] GameObject nextDayOffer;
    [Header("pricing text")]
    [SerializeField] TMP_Text cardOffer1Price;
    [SerializeField] TMP_Text cardOffer2Price;
    [SerializeField] TMP_Text cardOffer3Price;
    [SerializeField] TMP_Text cardOffer4Price;
    [SerializeField] TMP_Text healOfferPrice;
    [SerializeField] TMP_Text removalOfferPrice;
    [SerializeField] TMP_Text rerollOfferPrice;
    [Header("cards on offer")]
    [SerializeField] List<GameObject> commonCards;
    [SerializeField] List<GameObject> uncommonCards;
    [SerializeField] List<GameObject> rareCards;
    [Header("other stats")]
    [SerializeField] int healAmount;
    [Header("chance for card rarities")]
    [SerializeField] int commomChance;
    [SerializeField] int uncommonChance;
    [SerializeField] int rareChance;
    [Header("pricing")]
    [SerializeField] int commonBasePrice;
    [SerializeField] int commonDeviation;
    [SerializeField] int uncommonBasePrice;
    [SerializeField] int uncommonDeviation;
    [SerializeField] int rareBasePrice;
    [SerializeField] int rareDeviation;
    [SerializeField] int removalPrice;
    [SerializeField] int healPrice;
    [SerializeField] int healIncrement;
    [SerializeField] int rerollPrice;
    [SerializeField] int rerollIncrement;
    [Header("view deck")]
    [SerializeField] GameObject viewDeckOverlay;
    [SerializeField] GameObject viewRemovalOverlay;
    [SerializeField] float cardViewingXSpacingMin;
    [SerializeField] float cardViewingXSpacingMed;
    [SerializeField] float cardViewingXSpacingMax;
    [SerializeField] float cardViewingYSpacingMed;
    [SerializeField] float cardViewingYSpacingMax;
    [SerializeField] float medScale;
    [SerializeField] float maxScale;

    private List<GameObject> cardsOnSale = new List<GameObject>();
    private List<int> cardsOnSalePrices = new List<int>();

    private List<int> cardsBought = new List<int>();

    private int currentHealPrice = 0;
    private int currentRerollPrice = 0;
    private bool hasRemoved = false;

    private List<GameObject> currentPlayerCards;
    private int currentPlayerHealth;
    private int currentPlayerMoney;
    private int currentPlayerDay;
    private bool currentPlayerState;

    private bool gamePaused = false;

    void Start()
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log(json);
        currentPlayerCards = data.cards;
        currentPlayerHealth = data.playerHealth;
        currentPlayerMoney = data.playerMoney;
        currentPlayerDay = data.playerDay;
        currentPlayerState = data.isInGameplay;

        playerHealth.text = "" + currentPlayerHealth;
        playerMoney.text = "$" + currentPlayerMoney;
        playerDay.text = "" + currentPlayerDay;

        addShopOffers(false);
    }

    public void addShopOffers(bool isRerolling)
    {
        cardsOnSale.Clear();
        cardsOnSalePrices.Clear();

        if (!isRerolling)
        {
            currentHealPrice = healPrice;
            healOfferPrice.text = "" + currentHealPrice;

            currentRerollPrice = rerollPrice;
            rerollOfferPrice.text = "" + currentRerollPrice;

            removalOfferPrice.text = "" + removalPrice;
        }
        
        for (int i = 0; i < 4; i++) // adding cards
        {
            int chosenRarity = Random.Range(1, 101);
            GameObject tempCard;
            int tempPrice;
            while (true) // trying to ignore duplicates
            {
                if (chosenRarity <= commomChance)
                {
                    tempCard = commonCards[Random.Range(0, commonCards.Count)];
                    tempPrice = commonBasePrice + Random.Range(-commonDeviation, commonDeviation);
                }
                else if (chosenRarity <= uncommonChance + commomChance)
                {
                    tempCard = uncommonCards[Random.Range(0, uncommonCards.Count)];
                    tempPrice = uncommonBasePrice + Random.Range(-uncommonDeviation, uncommonDeviation);
                }
                else
                {
                    tempCard = rareCards[Random.Range(0, rareCards.Count)];
                    tempPrice = rareBasePrice + Random.Range(-rareDeviation, rareDeviation);
                }
                if (!cardsOnSale.Contains(tempCard))
                {
                    break;
                } 
            }
            if (cardsBought.Contains(i)) // if already bought the card there
            {
                cardsOnSale.Add(null);
                cardsOnSalePrices.Add(0);
            }
            else
            {
                cardsOnSale.Add(tempCard);
                cardsOnSalePrices.Add(tempPrice);
            }
        }
        if (cardsOnSale[0] != null)
        {
            GameObject tempCard = Instantiate(cardsOnSale[0], cardOffer1.transform);
            tempCard.GetComponent<Button>().enabled = false;
            tempCard.GetComponent<CardManager>().enabled = false;
            tempCard.GetComponent<CardMovement>().enabled = false;
            cardOffer1Price.text = "$" + cardsOnSalePrices[0];
        }
        if (cardsOnSale[1] != null)
        {
            GameObject tempCard = Instantiate(cardsOnSale[1], cardOffer2.transform);
            tempCard.GetComponent<Button>().enabled = false;
            tempCard.GetComponent<CardManager>().enabled = false;
            tempCard.GetComponent<CardMovement>().enabled = false;
            cardOffer2Price.text = "$" + cardsOnSalePrices[1];
        }
        if (cardsOnSale[2] != null)
        {
            GameObject tempCard = Instantiate(cardsOnSale[2], cardOffer3.transform);
            tempCard.GetComponent<Button>().enabled = false;
            tempCard.GetComponent<CardManager>().enabled = false;
            tempCard.GetComponent<CardMovement>().enabled = false;
            cardOffer3Price.text = "$" + cardsOnSalePrices[2];
        }
        if (cardsOnSale[3] != null)
        {
            GameObject tempCard = Instantiate(cardsOnSale[3], cardOffer4.transform);
            tempCard.GetComponent<Button>().enabled = false;
            tempCard.GetComponent<CardManager>().enabled = false;
            tempCard.GetComponent<CardMovement>().enabled = false;
            cardOffer4Price.text = "$" + cardsOnSalePrices[3];
        }     
    }

    public void viewDeck(bool isForRemoval)
    {
        if (gamePaused && !isForRemoval) // if already paused
        {
            viewDeckOverlay.SetActive(false);
            gamePaused = false;
            foreach (Transform child in viewDeckOverlay.transform)
            {
                Destroy(child.gameObject);
            }
            return;
        }
        else if (gamePaused && isForRemoval)
        {
            viewRemovalOverlay.SetActive(false);
            gamePaused = false;
            foreach (Transform child in viewRemovalOverlay.transform)
            {
                Destroy(child.gameObject);
            }
            return;
        }
        if (!isForRemoval)
        {
            viewDeckOverlay.SetActive(true);
        }
        else
        {
            viewRemovalOverlay.SetActive(true);
        }
        gamePaused = true;
        List<GameObject> cardsUsed = new List<GameObject>();
        for (int i = 0; i < currentPlayerCards.Count; i++)
        {
            cardsUsed.Add(currentPlayerCards[i]);
        }

        if (cardsUsed.Count < 8)
        {
            for (int i = 0; i < cardsUsed.Count; i++)
            {
                GameObject newCard;
                if (!isForRemoval)
                {
                    newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                    newCard.GetComponent<CardManager>().enabled = false;
                }
                else
                {
                    newCard = Instantiate(cardsUsed[i], viewRemovalOverlay.transform);
                    newCard.GetComponent<CardManager>().isBeingSelected = true;
                }
                newCard.GetComponent<CardMovement>().enabled = false;
                RectTransform rt = newCard.GetComponent<RectTransform>();
                float cardWidth = rt.rect.width;

                // Center the whole hand around the origin
                float totalWidth = cardsUsed.Count * cardWidth + (cardsUsed.Count - 1) * cardViewingXSpacingMin;
                float startX = -totalWidth / 2f + cardWidth / 2f;

                newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * (cardWidth + cardViewingXSpacingMin), 0);
            }
            return;
        }
        if (cardsUsed.Count < 15)
        {
            for (int i = 0; i < cardsUsed.Count; i++)
            {
                GameObject newCard;
                if (!isForRemoval)
                {
                    newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                    newCard.GetComponent<CardManager>().enabled = false;
                }
                else
                {
                    newCard = Instantiate(cardsUsed[i], viewRemovalOverlay.transform);
                    newCard.GetComponent<CardManager>().isBeingSelected = true;
                }
                newCard.GetComponent<CardMovement>().enabled = false;
                RectTransform rt = newCard.GetComponent<RectTransform>();
                rt.localScale = new Vector3(medScale, medScale, 1);
                float cardWidth = rt.rect.width * rt.localScale.x;

                // Center the whole hand around the origin
                float totalWidth = 7 * cardWidth + (7 - 1) * cardViewingXSpacingMed;
                float startX = -totalWidth / 2f + cardWidth / 2f;

                if (i < 7)
                {
                    newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * (cardWidth + cardViewingXSpacingMed), +cardViewingYSpacingMed);
                }
                else
                {
                    newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + (i - 7) * (cardWidth + cardViewingXSpacingMed), -cardViewingYSpacingMed);
                }
            }
            return;
        }
        if (cardsUsed.Count < 22)
        {
            for (int i = 0; i < cardsUsed.Count; i++)
            {
                GameObject newCard;
                if (!isForRemoval)
                {
                    newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                    newCard.GetComponent<CardManager>().enabled = false;
                }
                else
                {
                    newCard = Instantiate(cardsUsed[i], viewRemovalOverlay.transform);
                    newCard.GetComponent<CardManager>().isBeingSelected = true;
                }
                newCard.GetComponent<CardMovement>().enabled = false;
                RectTransform rt = newCard.GetComponent<RectTransform>();
                rt.localScale = new Vector3(maxScale, maxScale, 1);
                float cardWidth = rt.rect.width * rt.localScale.x;

                // Center the whole hand around the origin
                float totalWidth = 7 * cardWidth + (7 - 1) * cardViewingXSpacingMax;
                float startX = -totalWidth / 2f + cardWidth / 2f;

                if (i < 7)
                {
                    newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * (cardWidth + cardViewingXSpacingMax), +cardViewingYSpacingMax);
                }
                else if (i < 14)
                {
                    newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + (i - 7) * (cardWidth + cardViewingXSpacingMax), 0);
                }
                else
                {
                    newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + (i - 14) * (cardWidth + cardViewingXSpacingMax), -cardViewingYSpacingMax);
                }
            }
        }
    }

    public void buyCardOffer1()
    {
        if (cardsOnSale[0] != null && currentPlayerMoney >= cardsOnSalePrices[0])
        {
            currentPlayerCards.Add(cardsOnSale[0]);
            currentPlayerMoney -= cardsOnSalePrices[0];
            updateStats();

            foreach (Transform child in cardOffer1.transform)
            {
                if (child.gameObject.name != "PriceOption1")
                {
                    Destroy(child.gameObject);
                }
            }
            cardOffer1Price.text = "X";
            cardsBought.Add(0);

            playerMoney.text = "$" + currentPlayerMoney;
        }
    }
    public void buyCardOffer2()
    {
        if (cardsOnSale[1] != null && currentPlayerMoney >= cardsOnSalePrices[1])
        {
            currentPlayerCards.Add(cardsOnSale[1]);
            currentPlayerMoney -= cardsOnSalePrices[1];
            updateStats();

            foreach (Transform child in cardOffer2.transform)
            {
                if (child.gameObject.name != "PriceOption2")
                {
                    Destroy(child.gameObject);
                }
            }
            cardOffer2Price.text = "X";
            cardsBought.Add(1);

            playerMoney.text = "$" + currentPlayerMoney;
        }
    }
    public void buyCardOffer3()
    {
        if (cardsOnSale[2] != null && currentPlayerMoney >= cardsOnSalePrices[2])
        {
            currentPlayerCards.Add(cardsOnSale[2]);
            currentPlayerMoney -= cardsOnSalePrices[2];
            updateStats();

            foreach (Transform child in cardOffer3.transform)
            {
                if (child.gameObject.name != "PriceOption3")
                {
                    Destroy(child.gameObject);
                }
            }
            cardOffer3Price.text = "X";
            cardsBought.Add(2);

            playerMoney.text = "$" + currentPlayerMoney;
        }
    }
    public void buyCardOffer4()
    {
        if (cardsOnSale[3] != null && currentPlayerMoney >= cardsOnSalePrices[3])
        {
            currentPlayerCards.Add(cardsOnSale[3]);
            currentPlayerMoney -= cardsOnSalePrices[3];
            updateStats();

            foreach (Transform child in cardOffer4.transform)
            {
                if (child.gameObject.name != "PriceOption4")
                {
                    Destroy(child.gameObject);
                }
            }
            cardOffer4Price.text = "X";
            cardsBought.Add(3);

            playerMoney.text = "$" + currentPlayerMoney;
        }
    }

    public void reroll()
    {
        if (currentPlayerMoney >= currentRerollPrice)
        {
            currentPlayerMoney -= currentRerollPrice;
            updateStats();

            addShopOffers(true);

            currentRerollPrice += rerollIncrement;
            rerollOfferPrice.text = "" + currentRerollPrice;

            playerMoney.text = "$" + currentPlayerMoney;
        }
    }
    public void heal()
    {
        if (currentPlayerMoney >= currentHealPrice)
        {
            currentPlayerHealth += healAmount;
            if (currentPlayerHealth > 100)
            {
                currentPlayerHealth = 100;
            }
            currentPlayerMoney -= currentHealPrice;
            updateStats();

            currentHealPrice += healIncrement;
            healOfferPrice.text = "" + currentHealPrice;

            playerHealth.text = "" + currentPlayerHealth;
            playerMoney.text = "$" + currentPlayerMoney;
        }
    }
    public void removal()
    {
        if (!hasRemoved && currentPlayerMoney >= rerollPrice)
        {
            currentPlayerMoney -= rerollPrice;
            updateStats();

            hasRemoved = true;
            removalOfferPrice.text = "X";

            playerMoney.text = "$" + currentPlayerMoney;

            viewDeck(true); // opening the viewing for removal
        }
    }
    public void removeCard(GameObject card)
    {
        for (int i = 0; i < currentPlayerCards.Count; i++)
        {
            if (currentPlayerCards[i].gameObject.name + "(Clone)" == card.gameObject.name) // this is a super jank way to check names, but it does work
            {
                currentPlayerCards.RemoveAt(i);
                break;
            }
        }

        updateStats();

        viewDeck(true); // close the viewing for removal
    }
    public void nextDay()
    {
        currentPlayerDay += 1;
        currentPlayerState = true;
        updateStats();
        SceneManager.LoadScene("Gameplay");
    }
    public void GoToHome()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void updateStats()
    {
        SaveData data = new SaveData
        {
            cards = currentPlayerCards,
            playerHealth = currentPlayerHealth,
            playerMoney = currentPlayerMoney,
            playerDay = currentPlayerDay,
            isInGameplay = currentPlayerState
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }
}
