using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("fight stats")]
    [SerializeField] TMP_Text playerHealthText;
    [SerializeField] TMP_Text playerMoneyText;
    [SerializeField] TMP_Text playerDayText;
    [SerializeField] TMP_Text playerDamageText;
    [SerializeField] TMP_Text enemyHealthText;
    [SerializeField] TMP_Text enemyDamageText;
    [SerializeField] TMP_Text energyText;
    [SerializeField] TMP_Text turnNumberText;
    public int playerHealth;
    public int playerMoney;
    public int playerDay;
    public int playerDamage;
    public int enemyHealth;
    public int enemyDamage;
    public int energy;
    public int turnNumber;

    [Header("enemy behavior")]
    [SerializeField] int[] enemyDamageScaling;
    [SerializeField] int[] enemyDamageDeviation;
    [SerializeField] int[] enemyHealthScaling;

    [Header("lists for cards")]
    public List<GameObject> handCards;
    public List<GameObject> handObjectCards;
    public List<GameObject> drawCards;
    public List<GameObject> discardCards;
    public List<GameObject> deckCards;
    public List<GameObject> exhaustCards;
    public List<bool> exhaustCardsWithTemporary; //does the card in the exhaust pile have temporary

    [Header("dimensions of card area")]
    [SerializeField] float cardAreaXOrigin;
    [SerializeField] float cardAreaYOrigin;
    [SerializeField] float cardAreaWidth;
    [SerializeField] float cardXSpacing;
    [SerializeField] float cardAreaChooseYOffset;
    public GameObject cardAreaObject;

    [Header("viewing deck stuff")]
    [SerializeField] GameObject viewDeckButton;
    [SerializeField] GameObject viewDrawButton;
    [SerializeField] GameObject viewDiscardButton;
    [SerializeField] GameObject viewExhaustButton;
    [SerializeField] GameObject viewDeckOverlay;
    [SerializeField] GameObject viewEndCombatOverlay;
    [SerializeField] GameObject viewSettingsOverlay;
    [SerializeField] float cardViewingXSpacingMin;
    [SerializeField] float cardViewingXSpacingMed;
    [SerializeField] float cardViewingXSpacingMax;
    [SerializeField] float cardViewingYSpacingMed;
    [SerializeField] float cardViewingYSpacingMax;
    [SerializeField] float medScale;
    [SerializeField] float maxScale;
    private bool gamePaused;
    private int pauseReason; // 0 is view draw, 1 is view discard, 2 is view deck, 3 is view settings, 4 is view exhaust

    [Header("snapshot stuff")]
    [SerializeField] GameObject takeSnapshotButton;
    [SerializeField] GameObject viewSnapshotOverlay;
    [SerializeField] SnapshotManager snapshotManager;

    [Header("animations")]
    [SerializeField] float cardSpeed;
    [SerializeField] float multiCardDrawSpeed;
    [SerializeField] float damageEffectsSpeed;
    [SerializeField] float endToStartTurnSpeed;

    [Header("end of combat stuff")]
    [SerializeField] TMP_Text statusText;
    [SerializeField] GameObject payText;
    [SerializeField] GameObject[] payOutReasons;
    [SerializeField] GameObject payoutTotalText;
    [SerializeField] GameObject[] gameOverReasons;
    [SerializeField] GameObject continueToShopButton;
    [SerializeField] GameObject continueToHomeButton;
    [SerializeField] Vector2 payoutStartPos;
    [SerializeField] float payoutSpacing;
    [SerializeField] float payoutIntervalsSpeed;
    private bool hasTakenDamage = false;
    
    void Start()
    {
        setUpCombat();
        shuffleDeck();
        startTurn();
    }

    private void setUpCombat()
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log(json);
        for (int i = 0; i < data.cards.Count; i++)
        {
            deckCards.Add(data.cards[i]);
            drawCards.Add(data.cards[i]);
        }
        playerHealth = data.playerHealth;
        playerMoney = data.playerMoney;
        playerDay = data.playerDay;
        enemyHealth = enemyHealthScaling[playerDay - 1];
        updateVisuals();
    }
    private void updateVisuals()
    {
        playerHealthText.text = "" + playerHealth;
        playerMoneyText.text = "$" + playerMoney;
        playerDayText.text = "" + playerDay;
        playerDamageText.text = "" + playerDamage;
        enemyHealthText.text = "" + enemyHealth;
        enemyDamageText.text = "" + enemyDamage;
        energyText.text = "" + energy;
        turnNumberText.text = "" + turnNumber;
    }
    public void positionHandCards()
    {
        for (int i = 0; i < handObjectCards.Count; i++)
        {
            RectTransform rt = handObjectCards[i].GetComponent<RectTransform>();
            float cardWidth = rt.rect.width;

            // Center the whole hand around the origin
            float totalWidth = handObjectCards.Count * cardWidth + (handObjectCards.Count - 1) * cardXSpacing;
            float startX = cardAreaXOrigin - totalWidth / 2f + cardWidth / 2f;

            handObjectCards[i].GetComponent<CardMovement>().targetPos = new Vector2(startX + i * (cardWidth + cardXSpacing), cardAreaYOrigin);
        }
    }
    
    IEnumerator startTurnCoroutine()
    {
        takeSnapshotButton.GetComponent<Button>().interactable = true;
        for (int i = 0; i < 5; i++)
        {
            drawCard();
            yield return new WaitForSeconds(multiCardDrawSpeed);
        }
        energy = 3;
        turnNumber += 1;
        enemyDamage = enemyDamageScaling[playerDay - 1] + Random.Range(-enemyDamageDeviation[playerDay - 1], enemyDamageDeviation[playerDay - 1]);
        updateVisuals();
        yield return new WaitForSeconds(damageEffectsSpeed);
    }
    public void startTurn()
    {
        StartCoroutine(startTurnCoroutine());
    }
    IEnumerator endTurnCoroutine()
    {
        for (int i = handObjectCards.Count - 1; i >= 0; i--)
        {
            discardCard(handObjectCards[i]);
            yield return new WaitForSeconds(multiCardDrawSpeed);
        }
        enemyHealth -= playerDamage;
        if (enemyHealth < 0)
        {
            enemyHealth = 0;
        }
        playerDamage = 0;
        updateVisuals();
        yield return new WaitForSeconds(damageEffectsSpeed);
        if (enemyHealth <= 0) // killed the enemy
        {
            StartCoroutine(endCombat());
            yield break;
        }
        if (enemyDamage > 0)
        {
            hasTakenDamage = true;
        }
        playerHealth -= enemyDamage;
        if (playerHealth < 0)
        {
            playerHealth = 0;
        }
        enemyDamage = 0;
        updateVisuals();
        yield return new WaitForSeconds(damageEffectsSpeed);
        if (turnNumber >= 5 || playerHealth <= 0)
        {
            StartCoroutine(gameOver());
            yield break;
        }
        snapshotManager.hasTakenSnapshotThisTurn = false;
        yield return new WaitForSeconds(endToStartTurnSpeed);
        startTurn();
    }
    public void endTurn()
    {
        StartCoroutine(endTurnCoroutine());
    }
    public void drawCard()
    {
        if (drawCards.Count == 0) // if need to reshuffle
        {
            for (int i = 0; i < discardCards.Count; i++)
            {
                drawCards.Add(discardCards[i]);
            }
            discardCards.Clear();
            shuffleDeck();
            if (drawCards.Count == 0) // if entire deck already in hand
            {
                return;
            }
            if (handCards.Count >= 7) // max hand size
            {
                return;
            }
        }
        GameObject card = drawCards[drawCards.Count - 1];
        drawCards.Remove(card);

        GameObject cardClone = Instantiate(card, cardAreaObject.transform);
        cardClone.GetComponent<RectTransform>().anchoredPosition = new Vector2(-cardAreaWidth / 2, cardAreaYOrigin);
        CardMovement movement = cardClone.GetComponent<CardMovement>();
        movement.cardAreaYOrigin = cardAreaYOrigin; // giving our card some movement variables
        movement.cardAreaChooseYOffset = cardAreaChooseYOffset;
        movement.cardSpeed = cardSpeed;
        CardManager cardManager = cardClone.GetComponent<CardManager>();
        cardManager.isBeingPlayed = true;
        cardManager.snapshotManager = viewSnapshotOverlay.GetComponent<SnapshotManager>();
        handCards.Add(card);
        handObjectCards.Add(cardClone);

        positionHandCards();
    }
    public void drawCard(GameObject card, bool isTemporary) // this is if you specifically want a card added to hand, with mods
    {
        GameObject cardClone = Instantiate(card, cardAreaObject.transform);
        cardClone.GetComponent<RectTransform>().anchoredPosition = new Vector2(-cardAreaWidth / 2, cardAreaYOrigin);
        CardMovement movement = cardClone.GetComponent<CardMovement>();
        movement.cardAreaYOrigin = cardAreaYOrigin; // giving our card some movement variables
        movement.cardAreaChooseYOffset = cardAreaChooseYOffset;
        movement.cardSpeed = cardSpeed;
        CardManager cardManager = cardClone.GetComponent<CardManager>();
        cardManager.isBeingPlayed = true;
        cardManager.snapshotManager = viewSnapshotOverlay.GetComponent<SnapshotManager>();
        cardManager.isTemporary = isTemporary;
        handCards.Add(card);
        handObjectCards.Add(cardClone);

        positionHandCards();
    }
    public void discardCard() // this is only for the testing button
    {
        if (handCards.Count == 0) // card discard if no hand
        {
            return;
        }

        GameObject card = handCards[Random.Range(0, handCards.Count)];
        int index = handCards.IndexOf(card);

        handCards.RemoveAt(index);
        discardCards.Add(card);

        Destroy(handObjectCards[index]);
        handObjectCards.RemoveAt(index);   

        positionHandCards();
    }
    public void discardCard(GameObject tempCard)
    {
        int index = handObjectCards.IndexOf(tempCard);
        GameObject card = handCards[index];

        handCards.RemoveAt(index);
        discardCards.Add(card);

        Destroy(handObjectCards[index]);
        handObjectCards.RemoveAt(index);        

        positionHandCards();
    }
    public void shuffleDeck()
    {
        List<GameObject> tempCards = new List<GameObject>();
        for (int i = 0; i < drawCards.Count; i++)
        {
            GameObject chosenCard = drawCards[Random.Range(0, drawCards.Count)];
            drawCards.Remove(chosenCard);
            tempCards.Add(chosenCard);
            i--;
        }
        drawCards = tempCards;
    }

    public void viewDeck(int type) // 0 is viewing draw pile, 1 is viewing discard, 2 is viewing deck, 3 is viewing exhaust
    {
        if (gamePaused) // if already paused
        {
            if (type != pauseReason)
            {
                return;
            }
            viewDeckOverlay.SetActive(false);
            viewDeckButton.GetComponent<Button>().interactable = true;
            viewDrawButton.GetComponent<Button>().interactable = true;
            viewDiscardButton.GetComponent<Button>().interactable = true;
            viewExhaustButton.GetComponent<Button>().interactable = true;
            gamePaused = false;
            foreach (Transform child in viewDeckOverlay.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < handObjectCards.Count; i++)
            {
                handObjectCards[i].GetComponent<CardMovement>().gamePaused = false;
            }
            shuffleDeck();
            return;
        }
        viewDeckOverlay.SetActive(true);
        if (type == 0)
        {
            viewDeckButton.GetComponent<Button>().interactable = false;
            viewDrawButton.GetComponent<Button>().interactable = true;
            viewDiscardButton.GetComponent<Button>().interactable = false;
            viewExhaustButton.GetComponent<Button>().interactable = false;
        }
        else if (type == 1)
        {
            viewDeckButton.GetComponent<Button>().interactable = false;
            viewDrawButton.GetComponent<Button>().interactable = false;
            viewDiscardButton.GetComponent<Button>().interactable = true;
            viewExhaustButton.GetComponent<Button>().interactable = false;
        }
        else if (type == 2)
        {
            viewDeckButton.GetComponent<Button>().interactable = true;
            viewDrawButton.GetComponent<Button>().interactable = false;
            viewDiscardButton.GetComponent<Button>().interactable = false;
            viewExhaustButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            viewDeckButton.GetComponent<Button>().interactable = false;
            viewDrawButton.GetComponent<Button>().interactable = false;
            viewDiscardButton.GetComponent<Button>().interactable = false;
            viewExhaustButton.GetComponent<Button>().interactable = true;
        }
        gamePaused = true;
        pauseReason = type;
        for (int i = 0; i < handObjectCards.Count; i++)
        {
            handObjectCards[i].GetComponent<CardMovement>().gamePaused = true;
        }
        List<GameObject> cardsUsed = new List<GameObject>();
        if (type == 0)
        {
            for (int i = 0; i < drawCards.Count; i++)
            {
                cardsUsed.Add(drawCards[i]);
            }
        }
        else if (type == 1)
        {
            for (int i = 0; i < discardCards.Count; i++)
            {
                cardsUsed.Add(discardCards[i]);
            }
        }
        else if (type == 2)
        {
            for (int i = 0; i < deckCards.Count; i++)
            {
                cardsUsed.Add(deckCards[i]);
            }
        }
        else if (type == 3)
        {
            for (int i = 0; i < exhaustCards.Count; i++)
            {
                cardsUsed.Add(exhaustCards[i]);
            }
        }

        if (cardsUsed.Count < 8)
        {
            for (int i = 0; i < cardsUsed.Count; i++)
            {
                GameObject newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                if (type == 3)
                {
                    if (exhaustCardsWithTemporary[i]) // staple on temporary for exhaust pile
                    {
                        newCard.GetComponent<CardManager>().cardText.text += "\n" + "Temporary.";
                    }
                }
                newCard.GetComponent<CardManager>().enabled = false;
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
                GameObject newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                if (type == 3)
                {
                    if (exhaustCardsWithTemporary[i]) // staple on temporary for exhaust pile
                    {
                        newCard.GetComponent<CardManager>().cardText.text += "\n" + "Temporary.";
                    }
                }
                newCard.GetComponent<CardManager>().enabled = false;
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
                GameObject newCard = Instantiate(cardsUsed[i], viewDeckOverlay.transform);
                if (type == 3)
                {
                    if (exhaustCardsWithTemporary[i]) // staple on temporary for exhaust pile
                    {
                        newCard.GetComponent<CardManager>().cardText.text += "\n" + "Temporary.";
                    }
                }
                newCard.GetComponent<CardManager>().enabled = false;
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
    public void addDamage(int damage)
    {
        playerDamage += damage;
        updateVisuals();
    }
    public void changeEnergy(int change)
    {
        energy += change;
        updateVisuals();
    }
    public void addBlock(int block)
    {
        enemyDamage -= block;
        if (enemyDamage < 0)
        {
            enemyDamage = 0;
        }
        updateVisuals();
    }
    public void addEnemyDamage(int damage)
    {
        enemyDamage += damage;
        updateVisuals();
    }
    IEnumerator endCombat()
    {
        gamePaused = true;
        for (int i = 0; i < handObjectCards.Count; i++)
        {
            handObjectCards[i].GetComponent<CardMovement>().gamePaused = true;
        }
        viewEndCombatOverlay.SetActive(true);
        statusText.text = "You made our city" + "\n" + "live another day!" + "\n" + "Here's your payout:";
        List<GameObject> moneyReasons = new List<GameObject>();
        int totalPayout = 0;
        if (turnNumber < 4)
        {
            moneyReasons.Add(payOutReasons[0]);
            totalPayout += 5;
        }
        if (!hasTakenDamage)
        {
            moneyReasons.Add(payOutReasons[1]);
            totalPayout += 7;
        }
        if (playerMoney < 15)
        {
            moneyReasons.Add(payOutReasons[2]);
            totalPayout += 3;
        }
        moneyReasons.Add(payOutReasons[3]);
        totalPayout += 15;
        payoutTotalText.SetActive(true);
        for (int i = 0; i < moneyReasons.Count; i++)
        {
            GameObject tempReason = Instantiate(moneyReasons[i], payText.transform);
            tempReason.SetActive(true);
            tempReason.GetComponent<RectTransform>().anchoredPosition = new Vector2(payoutStartPos.x, payoutStartPos.y + (payoutSpacing * i));

            payoutTotalText.GetComponent<TMP_Text>().text = "Total: $" + totalPayout;
            playerMoney += totalPayout;
            updateVisuals();
            yield return new WaitForSeconds(payoutIntervalsSpeed);
        }
        continueToShopButton.SetActive(true);
    }
    IEnumerator gameOver()
    {
        gamePaused = true;
        for (int i = 0; i < handObjectCards.Count; i++)
        {
            handObjectCards[i].GetComponent<CardMovement>().gamePaused = true;
        }
        viewEndCombatOverlay.SetActive(true);
        statusText.text = "Game Over";
        for (int i = 0; i < gameOverReasons.Length; i++)
        {
            gameOverReasons[i].SetActive(true);
            yield return new WaitForSeconds(payoutIntervalsSpeed);
        }
        PlayerPrefs.SetString("SaveData", null);
        PlayerPrefs.Save();
        continueToHomeButton.SetActive(true);
    }
    public void goToShop()
    {
        SaveData data = new SaveData
        {
            cards = deckCards,
            playerHealth = this.playerHealth,
            playerMoney = this.playerMoney,
            playerDay = this.playerDay,
            isInGameplay = false
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        viewSnapshotOverlay.GetComponent<SnapshotManager>().nullifySnapshot();
        Debug.Log(json);
        SceneManager.LoadScene("Shop");
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
