using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PassSceneManager : MonoBehaviour
{
    public bool connected;
    public bool isConnected;
    public bool firstConnection;
    public GameObject serverObject;

    public bool server = false;
    public bool client = false;

    public bool serverTCP = false;
    public bool serverUDP = false;

    public bool clientTCP = false;
    public bool clientUDP = false;

    // Start is called before the first frame update
    void Start()
    {
        connected = false;
        isConnected = false;
        firstConnection = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            serverObject = this.gameObject;
            DontDestroyOnLoad(serverObject);

            SceneManager.LoadScene("Lobby");
            isConnected = true;
            connected = false;
        }
    }
}
