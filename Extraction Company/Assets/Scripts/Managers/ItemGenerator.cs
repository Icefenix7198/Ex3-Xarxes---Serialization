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
    }

    public int ObjectQuantity = 5;

    List<itemObj> allItems;
    public List<GameObject> allObjects;
    public List<Transform> spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        allItems = new List<itemObj>();
        
        for (int i = 0; i < ObjectQuantity; i++)
        {
            GameObject tmpObj = new GameObject();
            itemObj tmpItem = new itemObj();

            itemType type = (itemType)Random.Range(0, (int)itemType.NONE);
            tmpItem.type = type;

            int randomObject = Random.Range(0, allObjects.Count - 1);

            tmpObj = Instantiate(allObjects[randomObject]);
            tmpObj.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count - 1)].position;
            tmpItem.obj = tmpObj;

            tmpObj.GetComponent<Item>().item = tmpItem;

            allItems.Add(tmpItem);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
