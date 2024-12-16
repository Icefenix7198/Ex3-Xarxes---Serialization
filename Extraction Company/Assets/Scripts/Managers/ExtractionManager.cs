using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExtractionManager : MonoBehaviour
{
    [SerializeField]
    public Dictionary<string, int> extractions_ofPlayers;

    [SerializeField]
    public Dictionary<string, int> extractions;

    [SerializeField]
    public List<string> player_Names;

    [SerializeField]
    public List<int> player_Numbers;

    public List<TMP_Text> player_Numbers_UI;
    public List<TMP_Text> player_Names_UI;

    ServerUDP server;

    int playerCount = 0;

    public GameObject winText;
    public GameObject winnerPlayer;

    // Start is called before the first frame update
    void Start()
    {
        extractions_ofPlayers = new Dictionary<string, int>();
        extractions = new Dictionary<string, int>();

        player_Numbers = new List<int>();
        player_Names = new List<string>();

        if (server == null)
        {
            server = GameObject.Find("UDP_Manager").GetComponent<ServerUDP>();
        }
        winnerPlayer.SetActive(false);
        winText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (player_Names.Count > 0)
        {
            for (int i = 0; i < player_Names.Count; i++)
            {
                player_Names_UI[i].text = player_Names[i];
                player_Numbers_UI[i].text = player_Numbers[i].ToString();
            }
        }
    }

    public void SaveExtraction(string ID, int extraction)
    {
        if (extractions_ofPlayers.ContainsKey(ID))
        {
            extractions_ofPlayers[ID] = extraction;

            if(server != null)
            {
                if(server.userSocketsList.Count > 0)
                {
                    foreach (var user in server.userSocketsList)
                    {
                        if(user.NetID == ID)
                        {
                            extractions[user.name] = extraction;

                            for(int i = 0; i < player_Names.Count; i++)
                            {
                                if (player_Names[i] == user.name)
                                {
                                    player_Numbers[i] = extraction;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            extractions_ofPlayers.Add(ID, extraction);

            if (server != null)
            {
                if (server.userSocketsList.Count > 0)
                {
                    foreach (var user in server.userSocketsList)
                    {
                        if (user.NetID == ID)
                        {
                            extractions.Add(user.name, extraction);

                            if (playerCount < 4)
                            {
                                player_Names.Add(user.name);
                                player_Numbers.Add(extraction);

                                playerCount++;
                            }
                        }
                    }
                }
            }
        }

        server.serialization.SendExtraction(player_Names, player_Numbers);
    }
    public void Losse(string name)
    {
        winnerPlayer.GetComponent<TMP_Text>().text = name;
        winText.GetComponent<TMP_Text>().color = Color.red;
        winnerPlayer.SetActive(true);
        winText.SetActive(true);
    }
}
