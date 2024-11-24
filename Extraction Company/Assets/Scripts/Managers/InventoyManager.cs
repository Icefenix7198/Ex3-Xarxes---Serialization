using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemGenerator;

public class InventoyManager : MonoBehaviour
{
    List<itemObj> items;
    public int maxInventory = 4;

    // Start is called before the first frame update
    void Start()
    {
        items = new List<itemObj>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Item" && Input.GetKeyDown(KeyCode.E))
        {
            itemObj tmpItem = new itemObj();
            tmpItem.obj = other.gameObject;
            tmpItem.type = other.GetComponent<Item>().item.type;

            if(items.Count < maxInventory)
            {
                items.Add(tmpItem);
                Destroy(other.gameObject);
            }
        }
    }
}
