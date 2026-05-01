using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SnapshotManager : MonoBehaviour
{
    [Header("snapshot vars")]
    public List<GameObject> snapPlayerCards;
    public int snapPlayerDamage;
    private int snapEnemyHealth;
    public int snapEnemyDamage;
    public int snapEnergy;
    public int snapPlayerHealth;
    private int snapPlayerTurn;

    private int currentPlayerMoney;
    private int currentPlayerDay;

    [Header("fight stats")]
    [SerializeField] TMP_Text playerHealthText;
    [SerializeField] TMP_Text playerMoneyText;
    [SerializeField] TMP_Text playerDayText;
    [SerializeField] TMP_Text playerDamageText;
    [SerializeField] TMP_Text enemyHealthText;
    [SerializeField] TMP_Text enemyDamageText;
    [SerializeField] TMP_Text energyText;
    [SerializeField] TMP_Text turnNumberText;

    [Header("dimensions of card area")]
    [SerializeField] float cardAreaXOrigin;
    [SerializeField] float cardAreaYOrigin;
    [SerializeField] float cardAreaWidth;
    [SerializeField] float cardXSpacing;
    [SerializeField] float cardAreaChooseYOffset;
    [SerializeField] GameObject cardAreaObject;

    [Header("viewing stuff")]
    [SerializeField] GameObject viewDeckButton;
    [SerializeField] GameObject viewDrawButton;
    [SerializeField] GameObject viewDiscardButton;
    [SerializeField] GameObject viewExhaustButton;
    [SerializeField] GameObject viewSnapshotButton;
    [SerializeField] GameObject takeSnapshotButton;

    [Header("background animation")]
    [SerializeField] Image background;
    [SerializeField] Color32 startingColor;
    [SerializeField] float hueLow;
    [SerializeField] float hueHigh;
    [SerializeField] float speed;
    private float hueIndex;
    private bool hueIsGoingHigh = true;

    [Header("other")]
    [SerializeField] GameManager gameManager;
    public bool hasTakenSnapshotThisDay = false;
    public bool hasTakenSnapshotThisTurn = false;

    private bool isViewingSnapshot = false;
    private bool hasObjectLookingForData = false;
    void Start()
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        currentPlayerMoney = data.playerMoney;
        currentPlayerDay = data.playerDay;

        hueIndex = Random.Range(hueLow, hueHigh);
    }

    void Update()
    {
        if (isViewingSnapshot)
        {
            if (hueIsGoingHigh)
            {
                hueIndex += speed * Time.deltaTime;
                if (hueIndex >= hueHigh)
                {
                    hueIsGoingHigh = false;
                }
            }
            else
            {
                hueIndex -= speed * Time.deltaTime;
                if (hueIndex <= hueLow)
                {
                    hueIsGoingHigh = true;
                }
            }

            Color.RGBToHSV(startingColor, out _, out float saturation, out float vibrance);
            background.color = Color.HSVToRGB(hueIndex / 360f, saturation, vibrance);
        }
    }

    public void viewSnapshot() // for the button
    {
        if (isViewingSnapshot)
        {
            isViewingSnapshot = false;
            gameObject.SetActive(false);
            viewDeckButton.GetComponent<Button>().interactable = true;
            viewDrawButton.GetComponent<Button>().interactable = true;
            viewDiscardButton.GetComponent<Button>().interactable = true;
            viewExhaustButton.GetComponent<Button>().interactable = true;
            if (!hasTakenSnapshotThisTurn)
            {
                takeSnapshotButton.GetComponent<Button>().interactable = true;
            }

            foreach (Transform child in cardAreaObject.transform)
            {
                Destroy(child.gameObject);
            }
            return;
        }
        if (snapPlayerCards == null)
        {
            return;
        }

        isViewingSnapshot = true;
        gameObject.SetActive(true);
        viewDeckButton.GetComponent<Button>().interactable = false;
        viewDrawButton.GetComponent<Button>().interactable = false;
        viewDiscardButton.GetComponent<Button>().interactable = false;
        viewExhaustButton.GetComponent<Button>().interactable = false;
        takeSnapshotButton.GetComponent<Button>().interactable = false;

        playerHealthText.text = "" + snapPlayerHealth; // this is the update visuals method
        playerMoneyText.text = "$" + currentPlayerMoney;
        playerDayText.text = "" + currentPlayerDay;
        playerDamageText.text = "" + snapPlayerDamage;
        enemyHealthText.text = "" + snapEnemyHealth;
        enemyDamageText.text = "" + snapEnemyDamage;
        energyText.text = "" + snapEnergy;
        turnNumberText.text = "" + snapPlayerTurn;

        for (int i = 0; i < snapPlayerCards.Count; i++) // this is the position hand cards method
        {
            RectTransform rt = snapPlayerCards[i].GetComponent<RectTransform>();
            float cardWidth = rt.rect.width;

            // Center the whole hand around the origin
            float totalWidth = snapPlayerCards.Count * cardWidth + (snapPlayerCards.Count - 1) * cardXSpacing;
            float startX = cardAreaXOrigin - totalWidth / 2f + cardWidth / 2f;

            GameObject tempCard = Instantiate(snapPlayerCards[i], cardAreaObject.transform);
            tempCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * (cardWidth + cardXSpacing), cardAreaYOrigin);
            tempCard.GetComponent<CardManager>().isBeingSelected = true;
            tempCard.GetComponent<CardManager>().cardInHand = i;
        }
    }

    public void viewSnapshot(bool isLookingForData, GameObject seeker) // for objects looking for data
    {
        if (isViewingSnapshot)
        {
            isViewingSnapshot = false;
            gameObject.SetActive(false);
            viewDeckButton.GetComponent<Button>().interactable = true;
            viewDrawButton.GetComponent<Button>().interactable = true;
            viewDiscardButton.GetComponent<Button>().interactable = true;
            viewExhaustButton.GetComponent<Button>().interactable = true;
            viewSnapshotButton.GetComponent<Button>().interactable = true;
            if (!hasTakenSnapshotThisTurn)
            {
                takeSnapshotButton.GetComponent<Button>().interactable = true;
            }
            
            foreach (Transform child in cardAreaObject.transform)
            {
                Destroy(child.gameObject);
            }
            return;
        }
        if (snapPlayerCards == null)
        {
            return;
        }
        if (isLookingForData)
        {
            hasObjectLookingForData = true;
        }

        isViewingSnapshot = true;
        gameObject.SetActive(true);
        viewDeckButton.GetComponent<Button>().interactable = false;
        viewDrawButton.GetComponent<Button>().interactable = false;
        viewDiscardButton.GetComponent<Button>().interactable = false;
        viewExhaustButton.GetComponent<Button>().interactable = false;
        viewSnapshotButton.GetComponent<Button>().interactable = false;
        takeSnapshotButton.GetComponent<Button>().interactable = false;

        playerHealthText.text = "" + snapPlayerHealth; // this is the update visuals method
        playerMoneyText.text = "$" + currentPlayerMoney;
        playerDayText.text = "" + currentPlayerDay;
        playerDamageText.text = "" + snapPlayerDamage;
        enemyHealthText.text = "" + snapEnemyHealth;
        enemyDamageText.text = "" + snapEnemyDamage;
        energyText.text = "" + snapEnergy;
        turnNumberText.text = "" + snapPlayerTurn;

        for (int i = 0; i < snapPlayerCards.Count; i++) // this is the position hand cards method
        {
            RectTransform rt = snapPlayerCards[i].GetComponent<RectTransform>();
            float cardWidth = rt.rect.width;

            // Center the whole hand around the origin
            float totalWidth = snapPlayerCards.Count * cardWidth + (snapPlayerCards.Count - 1) * cardXSpacing;
            float startX = cardAreaXOrigin - totalWidth / 2f + cardWidth / 2f;

            GameObject tempCard = Instantiate(snapPlayerCards[i], cardAreaObject.transform);
            tempCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * (cardWidth + cardXSpacing), cardAreaYOrigin);
            tempCard.GetComponent<CardManager>().isBeingSelected = true;
            tempCard.GetComponent<CardManager>().snapshotManager = this;
            tempCard.GetComponent<CardManager>().cardInHand = i;
        }
    }

    public void takeSnapshot()
    {
        hasTakenSnapshotThisDay = true;
        hasTakenSnapshotThisTurn = true;
        viewSnapshotButton.GetComponent<Button>().interactable = true;
        takeSnapshotButton.GetComponent<Button>().interactable = false;
        snapPlayerCards.Clear();
        for (int i = 0; i < gameManager.handCards.Count; i++)
        {
            snapPlayerCards.Add(gameManager.handCards[i]);
        }

        snapPlayerDamage = gameManager.playerDamage;
        snapEnemyHealth = gameManager.enemyHealth;
        snapEnemyDamage = gameManager.enemyDamage;
        snapEnergy = gameManager.energy;
        snapPlayerHealth = gameManager.playerHealth;
        snapPlayerTurn = gameManager.turnNumber;
    }

    public void nullifySnapshot()
    {
        snapPlayerCards = null;
        snapPlayerDamage = 0;
        snapEnemyHealth = 0;
        snapEnemyDamage = 0;
        snapEnergy = 0;
        snapPlayerHealth = 0;
        snapPlayerTurn = 0;
    }
    public void sendBackData(GameObject card) // this is triggered from card chosen in snapshot, and adds a copy of the card selected to hand
    {
        if (hasObjectLookingForData)
        {
            GameObject cardSelected = new GameObject();
            for (int i = 0; i < snapPlayerCards.Count; i++)
            {
                if (snapPlayerCards[i].name + "(Clone)" == card.name)
                {
                    cardSelected = snapPlayerCards[i];
                    break;
                }
            }

            gameManager.drawCard(cardSelected, true);

            viewSnapshot(false, null); // close view
            hasObjectLookingForData = false;
        }
    }
}
