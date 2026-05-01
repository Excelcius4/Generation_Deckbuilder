using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<GameObject> cards;
    public int playerHealth;
    public int playerMoney;
    public int playerDay;
    public bool isInGameplay;
}
