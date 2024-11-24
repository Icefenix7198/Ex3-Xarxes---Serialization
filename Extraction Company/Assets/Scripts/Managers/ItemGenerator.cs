using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public enum itemType
    {
        COMMUNE,
        RARE,
        EPIC,
        LEGENDARY,
        NONE
    }

    public struct itemObj
    {
        public itemType type;
        public GameObject obj;
        public int objType;
        public Vector3 pos;
    }

    public int ObjectQuantity = 5;

    [HideInInspector]
    public List<itemObj> allItems;

    public List<GameObject> allObjects;
    public List<Transform> spawnPoints;

    public Transform parentItems;

    Serialization serialization;

    private void Start()
    {
        if (!serialization.isS_udp)
        {
            this.GetComponent<ItemGenerator>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        allItems = new List<itemObj>();

        for (int i = 0; i < ObjectQuantity; i++)
        {
            GameObject tmpObj;
            itemObj tmpItem = new itemObj();

            //itemType type = (itemType)Random.Range(0, (int)itemType.NONE);
            //tmpItem.type = type;

            int randomObject = UnityEngine.Random.Range(0, allObjects.Count - 1);

            tmpObj = Instantiate(allObjects[randomObject], parentItems.transform);

            if (spawnPoints.Count > 0)
            {
                int spawnPoint = UnityEngine.Random.Range(0, spawnPoints.Count - 1);
                tmpObj.transform.position = spawnPoints[spawnPoint].position;
                spawnPoints.RemoveAt(spawnPoint);
            }

            tmpItem.obj = tmpObj;
            tmpItem.objType = randomObject;
            tmpItem.pos = tmpObj.transform.position;

            tmpItem.type = tmpObj.GetComponent<Item>().ItemType;
            tmpObj.GetComponent<Item>().item = tmpItem;

            allItems.Add(tmpItem);
        }
    }

    public void SpawnItems(List<itemObj> items)
    {
        foreach (itemObj item in items)
        {
            GameObject tmpObj;
            tmpObj = Instantiate(allObjects[item.objType], parentItems.transform);
            tmpObj.transform.position = item.pos;
        }

        allItems = items;
    }
}
