using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCardFromSnapshot : MonoBehaviour
{
    public SnapshotManager snapshotManager;
    public void getCard() // this is just triggered from cardmanager, and just triggers the snapshot to open so the player can find a selection
    {
        snapshotManager.viewSnapshot(true, gameObject);
    }
}
