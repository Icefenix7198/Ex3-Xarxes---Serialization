using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemGenerator;
using TMPro;

public class InventoyManager : MonoBehaviour
{
    List<itemObj> items;
    public int maxInventory = 4;
    Serialization serialization;
    PlayerManager playerManager;

    public int priceCommon = 10;
    public int priceRare = 30;
    public int priceEpic = 50;
    public int priceLegendary = 100;

    int totalMoneyCarring = 0;
    int totalMoneySaved = 0;

    public TMP_Text textMoneyCarring;
    public TMP_Text textMoneySaved;

    // Start is called before the first frame update
    void Start()
    {
        serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
        textMoneyCarring = GameObject.Find("MoneyCarring_Number").GetComponent<TMP_Text>();
        textMoneySaved = GameObject.Find("MoneySaved_Number").GetComponent<TMP_Text>();

        textMoneyCarring.text = "0";
        textMoneySaved.text = "0";

        items = new List<itemObj>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Item" && Input.GetKeyDown(KeyCode.E))
        {
            itemObj tmpItem = new itemObj();
            tmpItem.obj = other.gameObject;

            tmpItem.type = other.gameObject.GetComponent<Item>().item.type;

            tmpItem.ID = other.GetComponent<Item>().item.ID;

            if (items.Count < maxInventory)
            {
                items.Add(tmpItem);
                serialization.SendDestroyItem(tmpItem, playerManager.player.ID);
                Destroy(other.gameObject);

                switch (tmpItem.type)
                {
                    case itemType.COMMUNE:
                        totalMoneyCarring += priceCommon;
                        break;
                    case itemType.RARE:
                        totalMoneyCarring += priceRare;
                        break;
                    case itemType.EPIC:
                        totalMoneyCarring += priceEpic;
                        break;
                    case itemType.LEGENDARY:
                        totalMoneyCarring += priceLegendary;
                        break;
                    default:
                        break;
                }

                textMoneyCarring.text = totalMoneyCarring.ToString();
            }
        }

        if (other.tag == "Extraction" && Input.GetKey(KeyCode.E))
        {
            items.Clear();
            int quantity = int.Parse(textMoneyCarring.text) + int.Parse(textMoneySaved.text);
            textMoneySaved.text = quantity.ToString();
            textMoneyCarring.text = "0";

            SendExtraction();
        }
    }

    void SendExtraction()
    {
        serialization.SendExtraction(int.Parse(textMoneySaved.text), playerManager.player.ID);
    }
}
