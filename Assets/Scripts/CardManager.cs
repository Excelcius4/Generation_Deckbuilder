using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardManager : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] int energyCost = 1;
    [SerializeField] bool isAttack = true;
    [SerializeField] int damage = 0;
    [SerializeField] int block = 0;
    public TMP_Text cardText;
    [Header("card states")]
    public bool isExhaust = false;
    public bool isTemporary = false;
    public bool isBeingPlayed = false;
    public bool isBeingSelected = false;
    [Header("snapshot stuff")]
    public int cardInHand = -1;
    [Header("Other Card Abilities")]
    [SerializeField] GetCardFromSnapshot getCardFromSnapshot = null;
    [SerializeField] WeirdDamage weirdDamage = null;
    [SerializeField] DrawCards drawCards = null;
    [SerializeField] AddEnergy addEnergy = null;
    [SerializeField] bool isEnergyFromSnapshot = false;
    [SerializeField] bool takeASnapshot = false;
    [SerializeField] bool canBePlayedOnlyIfTemporary;
    [SerializeField] int addDamageEnemyToPlayer = 0;

    private GameManager gameManager = null;
    public SnapshotManager snapshotManager;
    private ShopManager shopManager = null;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("Gameplay") != null)
        {
            gameManager = GameObject.Find("Gameplay").GetComponent<GameManager>();
        }
        if (GameObject.Find("Shop") != null)
        {
            shopManager = GameObject.Find("Shop").GetComponent<ShopManager>();
        }
        if (GetComponent<GetCardFromSnapshot>() != null)
        {
            GetComponent<GetCardFromSnapshot>().snapshotManager = snapshotManager;
        }
        if (isTemporary)
        {
            cardText.text += "\n" + "Temporary.";
        }
        
    }
    public void PlayCard()
    {
        if (isBeingPlayed && gameManager != null) // for playing in hand in gameplay
        {
            if (gameManager.energy < energyCost)
            {
                return;
            }
            if (GetComponent<GetCardFromSnapshot>() != null)
            {
                if (!snapshotManager.hasTakenSnapshotThisDay)
                {
                    return;
                }
            }
            if (canBePlayedOnlyIfTemporary && !isTemporary)
            {
                return;
            }
            gameManager.changeEnergy(-energyCost);
            if (weirdDamage == null)
            {
                gameManager.addDamage(damage);
            }
            gameManager.addBlock(block);

            // start effects from certain cards
            if (getCardFromSnapshot != null)
            {
                getCardFromSnapshot.getCard();
            }
            if (takeASnapshot)
            {
                snapshotManager.takeSnapshot();
            }
            if (weirdDamage != null)
            {
                if (weirdDamage.isDoubleDamageCurrentToEnemy)
                {
                    gameManager.addDamage(gameManager.playerDamage);
                }
                if (weirdDamage.isDoubleDamageCurrentToPlayer)
                {
                    gameManager.addDamage(gameManager.enemyDamage);
                }
                if (weirdDamage.isDamageAddedFromSnapshot)
                {
                    gameManager.addDamage(snapshotManager.snapPlayerDamage);
                }
            }
            if (addEnergy != null)
            {
                if (!isEnergyFromSnapshot)
                {
                    gameManager.changeEnergy(addEnergy.energyAmount);
                }
                else
                {
                    gameManager.changeEnergy(snapshotManager.snapEnergy);
                    addDamageEnemyToPlayer = gameManager.energy * 2; // currently just for a specific card
                }
            }
            if (drawCards != null)
            {
                for (int i = 0; i < drawCards.cardsDrawn; i++)
                {
                    gameManager.drawCard(); // this is the right format for this method dw
                }
            }

            gameManager.addEnemyDamage(addDamageEnemyToPlayer); // usually wont be used
            // end effects from certain cards

            if (isTemporary || isExhaust) // remove card from deck
            {
                for (int i = 0; i < gameManager.handObjectCards.Count; i++)
                {
                    if (gameManager.handObjectCards[i] == gameObject)
                    {
                        gameManager.exhaustCards.Add(gameManager.handCards[i]);
                        gameManager.exhaustCardsWithTemporary.Add(true); // basically tags this card that it had temporary
                        gameManager.handCards.RemoveAt(i);
                        gameManager.handObjectCards.RemoveAt(i);
                        gameManager.positionHandCards();
                        Destroy(gameObject);
                        return;
                    }
                }
            }
            gameManager.discardCard(gameObject);
        }
        if (isBeingSelected && gameManager != null) // for selecting from snapshot in gameplay
        {
            snapshotManager.sendBackData(gameObject);
        }
        if (isBeingSelected && shopManager != null) // for selecting for removal in shop
        {
            shopManager.removeCard(gameObject);
        }
    }
}
