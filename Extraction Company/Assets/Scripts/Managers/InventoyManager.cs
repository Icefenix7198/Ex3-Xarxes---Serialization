using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemGenerator;

public class InventoyManager : MonoBehaviour
{
    List<itemObj> items;

    // Start is called before the first frame update
    void Start()
    {
        items = new List<itemObj>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            itemObj tmpItem = new itemObj();
            tmpItem.obj = other.gameObject;
            tmpItem.type = other.GetComponent<Item>().item.type;

            if(items.Count < 4)
            {
                items.Add(tmpItem);
                Destroy(other.gameObject);
            }
        }
    }
}
