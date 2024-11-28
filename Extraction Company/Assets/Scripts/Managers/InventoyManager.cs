using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemGenerator;

public class InventoyManager : MonoBehaviour
{
    List<itemObj> items;
    public int maxInventory = 4;
    Serialization serialization;
    PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
        items = new List<itemObj>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Item" && Input.GetKeyDown(KeyCode.E))
        {
            itemObj tmpItem = new itemObj();
            tmpItem.obj = other.gameObject;
            tmpItem.type = other.GetComponent<Item>().item.type;
            tmpItem.ID = other.GetComponent<Item>().item.ID;

            if(items.Count < maxInventory)
            {
                items.Add(tmpItem);
                serialization.SendDestroyItem(tmpItem, playerManager.player.ID);
                Destroy(other.gameObject);
            }
        }
    }
}
