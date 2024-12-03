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
        public string ID;
    }

    public struct itemGameObj
    {
        public GameObject obj;
        public string ID;
    }

    public int ObjectQuantity = 5;

    [HideInInspector]
    public List<itemObj> allItems;
    public List<itemGameObj> allItemsGameobjects;

    public List<GameObject> allObjects;
    public List<Transform> spawnPoints;

    public Transform parentItems;

    public PlayerManager playerManager;
    public bool action = true;

    // Update is called once per frame
    void Update()
    {
        if (playerManager != null && action)
        {
            if(playerManager.s_udp != null)
            {
                allItems = new List<itemObj>();
                allItemsGameobjects = new List<itemGameObj>();

                for (int i = 0; i < ObjectQuantity; i++)
                {
                    GameObject tmpObj;
                    itemObj tmpItem = new itemObj();
                    itemGameObj tmpGameObj = new itemGameObj();

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
                    tmpItem.ID = System.Guid.NewGuid().ToString();

                    tmpGameObj.ID = tmpItem.ID;
                    tmpGameObj.obj = tmpObj;
                    allItemsGameobjects.Add(tmpGameObj);

                    tmpItem.type = tmpObj.GetComponent<Item>().ItemType;
                    tmpObj.GetComponent<Item>().item = tmpItem;

                    allItems.Add(tmpItem);
                }

                action = false;
            }
        }
    }

    public void SpawnItems(List<itemObj> items)
    {
        List<itemObj> tmpsItems = new List<itemObj>();

        foreach (itemObj item in items)
        {
            GameObject tmpObj;
            tmpObj = Instantiate(allObjects[item.objType], parentItems.transform);
            tmpObj.transform.position = item.pos;
            tmpObj.GetComponent<Item>().item = item;
            tmpObj.GetComponent<Item>().item.obj = tmpObj;
            tmpsItems.Add(tmpObj.GetComponent<Item>().item);
        }

        allItems = tmpsItems;
    }

    public void DestroyItem(string itemID, string idPlayer = "-1")
    {
        itemObj itemToDestroy = new itemObj();

        foreach (var item in allItems)
        {
            if (item.ID == itemID)
            {
                if(playerManager.s_udp != null)
                {
                    playerManager.serialization.SendDestroyItem(item, idPlayer);
                }

                Destroy(item.obj);
                itemToDestroy = item;
            }
        }

        allItems.Remove(itemToDestroy);
    }
}
