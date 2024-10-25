using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayer : MonoBehaviour
{
    public GameObject player;
    public ClientUDP c_udp;
    public Serialization serialization;

    private void Update()
    {
        if(c_udp == null)
        {
            c_udp = GameObject.Find("UDP_ClientManager").GetComponent<ClientUDP>();
        }

        if(serialization == null)
        {
            serialization = GameObject.Find("UDP_ClientManager").GetComponent<Serialization>();
        }
    }

    public void NewPlayer()
    {
        Debug.Log("CreatePlayer!");

        Instantiate(player, this.transform);
    }

    public void CreateNewPlayer()
    {
        serialization.serializeCreatePlayer(c_udp.clientID);
    }
}
