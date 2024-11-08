using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerManager;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPref;
    public ClientUDP c_udp;
    public ServerUDP s_udp;
    public Serialization serialization;
    public GameObject clientParent;
    
    //Boton de crear player (me da TOC que se vea el boton cuando no puede funcionar)
    public GameObject button;

    public struct Player
    {
        public string name;
        public string ID;
        public GameObject playerObj;
        public Rigidbody playerRb;
        public TextMeshProUGUI textID;
    }

    public struct PlayerServer
    {
        public string ID;
        public Vector3 position;
    }

    public Player player;
    List<PlayerServer> playerList = new List<PlayerServer>();

    float movementHorizontal;
    float movementVertical;
    public float speed;
    Vector3 movement;
    float dt;

    bool passedScene = false;

    private void Update()
    {
        if (serialization == null)
        {
            serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        }

        if (c_udp == null && s_udp == null)
        {
            c_udp = GameObject.Find("UDP_Manager").GetComponent<ClientUDP>();
        }

        if (s_udp == null && c_udp == null)
        {
            s_udp = GameObject.Find("UDP_Manager").GetComponent<ServerUDP>();

        }

        if (s_udp != null)
        {
            button.SetActive(false);
        }

        
        
        if(player.playerObj != null && c_udp != null) 
        {
            dt += Time.deltaTime;
            MovePlayer();
            if (dt > 0.0416f) //We only send the info some frames not constantly to reduce the server load
            {
                SendMovement();
                dt = 0;
            }

            
        }
    }

    private void SendMovement()
    {
        if (passedScene)
        {
            if (player.playerObj != null)
            {
                if (Mathf.Abs(player.playerRb.velocity.magnitude) > 0)
                {
                    serialization.serializeMovement(player.ID, player.playerObj.transform.position);
                }
            }
        }
    }

    public void NewPlayer(string playerId)
    {
        //Hacer versión para server! El tiene que crear un player, y mandar a dicho cliente que lo ha creado la lista entera de players para que les haga spawn
        if(c_udp != null)
        {
            if (player.playerObj == null)
            {
                player = new Player();
                player.ID = playerId;

                player.playerObj = Instantiate(playerPref, clientParent.transform);

                player.playerRb = player.playerObj.GetComponent<Rigidbody>();
                player.playerRb.freezeRotation = true;

                player.textID = player.playerObj.GetComponent<TextMeshProUGUI>();
                player.textID.text = playerId;

                Transform child = player.playerObj.transform.GetChild(0);
                child.gameObject.GetComponent<TextMeshPro>().text = serialization.tmpNameClient;

                passedScene = true;
            }
            else
            {
                Player pTemp = new Player();
                pTemp.ID = playerId;

                pTemp.playerObj = Instantiate(playerPref, clientParent.transform);

                pTemp.playerRb = pTemp.playerObj.GetComponent<Rigidbody>();
                pTemp.playerRb.freezeRotation = true;

                pTemp.textID = pTemp.playerObj.GetComponent<TextMeshProUGUI>();
                pTemp.textID.text = playerId;

                PlayerServer pServer = new PlayerServer();
                pServer.ID = pTemp.ID;
                pServer.position = pTemp.playerObj.transform.position;

                playerList.Add(pServer);
            }
        }

        if(s_udp != null)
        {
            player = new Player();
            player.ID = playerId;

            GameObject tmp = Instantiate(playerPref, clientParent.transform);

            player.playerObj = tmp;

            player.playerRb = player.playerObj.GetComponent<Rigidbody>();
            player.playerRb.freezeRotation = true;

            player.textID = player.playerObj.GetComponent<TextMeshProUGUI>();
            player.textID.text = playerId;

            PlayerServer pServer = new PlayerServer();
            pServer.ID = playerId;
            pServer.position = tmp.transform.position;

            playerList.Add(pServer);

            serialization.SendAllPlayers(playerList);
        }

         //UnityEngine.Debug.Log("CreatePlayer!");
    }

    public void CreateNewPlayer()
    {
        //while(player.playerObj == null) { }
        serialization.serializeCreatePlayer(c_udp.clientID);
    }

    public void SpawnAllPlayers(List<PlayerServer> pList)
    {
        foreach (var pServer in pList) 
        {
            GameObject tmpPlayer = Instantiate(playerPref, clientParent.transform);
            tmpPlayer.transform.position = pServer.position;

            Player _player = new Player();
            _player.ID = pServer.ID;

            _player.playerObj = tmpPlayer;

            _player.playerRb = _player.playerObj.GetComponent<Rigidbody>();
            _player.playerRb.freezeRotation = true;

            _player.textID = _player.playerObj.GetComponent<TextMeshProUGUI>();
            _player.textID.text = pServer.ID;
        }
    }

    public void MovePlayer()
    {
        movementHorizontal = Input.GetAxis("Horizontal");
        movementVertical = Input.GetAxis("Vertical");

        if (movementHorizontal != 0 || movementVertical != 0)
        {
            movement = transform.forward * movementVertical + transform.right * movementHorizontal;

            player.playerRb.velocity += movement.normalized * speed * Time.deltaTime;
        }
        else
        {
            player.playerRb.velocity = new Vector3(0, player.playerRb.velocity.y, 0);
        }
    }

    public void ClientMove(string ID, Vector3 moveTo)
    {
        List<GameObject> clientList = new List<GameObject>();

        foreach (Transform child in clientParent.transform) //Cogemos todos los hijos del padre ClientList
        {
            clientList.Add(child.gameObject);

            string idClient = child.gameObject.GetComponent<TextMeshProUGUI>().text;

            if(ID == idClient)
            {
                child.gameObject.transform.position = moveTo;

                List<PlayerServer> tmpPlayers = new List<PlayerServer>(); 

                foreach(var pServer in playerList)
                {
                    PlayerServer pTmp = new PlayerServer();
                    pTmp = pServer;

                    if (pServer.ID == ID)
                    {
                        pTmp.position = moveTo;
                        //playerList.FindIndex(pServer => pServer.Equals(pTmp));
                    }

                    tmpPlayers.Add(pTmp);
                }

                playerList = tmpPlayers;
            }
        }
    }

    public GameObject FindPlayer(string ID)
    {
        foreach (Transform child in clientParent.transform) //Cogemos todos los hijos del padre ClientList
        {
            string idClient = child.gameObject.GetComponent<TextMeshProUGUI>().text;

            if (ID == idClient)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
