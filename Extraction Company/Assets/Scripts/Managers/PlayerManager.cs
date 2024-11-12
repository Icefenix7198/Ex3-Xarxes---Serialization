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
    public GameObject mainCam;
    
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
        public Vector3 position;
        public Quaternion rotation;
        public string ID;
        public string name;
    }

    public struct PlayerToUpdate
    {
        public Vector3 actualPosition;
        public Vector3 futurePosition;
        public Quaternion actualRotation;
        public Quaternion futureRotation;
        public GameObject gameObject;
    }

    public Player player;
    List<PlayerServer> playerList = new List<PlayerServer>();

    float movementHorizontal;
    float movementVertical;
    public float speed;
    Vector3 movement;
    float dt;

    PlayerToUpdate moveUpdatePlayer = new PlayerToUpdate();
    bool hasMoved = false;

    public Transform[] spawnPositions;

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
            //MovePlayer();
            if (dt > 0.0418f) //We only send the info some frames not constantly to reduce the server load
            {
                SendMovement();
                dt = 0;
            }
        }

        if (hasMoved)
        {
            UpdatePositionAndRotation();
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
                    serialization.serializeMovement(player.ID, player.playerObj.transform.position, player.playerObj.transform.rotation);
                }
            }
        }
    }

    public void NewPlayer(string playerId, string playerName = "Player",int playerNum = 4) //Player num is used for determining spawn positions for clients
    {
        //Hacer versi�n para server! El tiene que crear un player, y mandar a dicho cliente que lo ha creado la lista entera de players para que les haga spawn
        if(c_udp != null)
        {
            if (player.playerObj == null) //The client doesn't have a player yet.
            {
                player = new Player();
                player.ID = playerId;

                player.playerObj = Instantiate(playerPref, clientParent.transform); //If we don't recive what player it is we spawn on default position

                player.playerObj.transform.position = playerNum >= 4 ? clientParent.transform.position : spawnPositions[playerNum].position;

                player.playerRb = player.playerObj.GetComponent<Rigidbody>();
                player.playerRb.freezeRotation = true;

                player.playerObj.GetComponent<PlayerMovement>().enabled = true;

                player.textID = player.playerObj.GetComponent<TextMeshProUGUI>();
                player.textID.text = playerId;

                Transform child = player.playerObj.transform.GetChild(0);
                child.gameObject.GetComponent<TextMeshPro>().text = playerName;
                
                Transform cam = player.playerObj.transform.GetChild(1);
                cam.gameObject.SetActive(true);

                if (mainCam != null)
                {
                    mainCam.SetActive(false);
                }

                passedScene = true;
            }
            else //If the client already has a player spawns the new player in its position to be able to see it.
            {
                Player pTemp = new Player();
                pTemp.ID = playerId;

                pTemp.playerObj = Instantiate(playerPref, clientParent.transform);

                pTemp.playerObj.transform.position = playerNum >= 4 ? clientParent.transform.position : spawnPositions[playerNum+1].position;

                pTemp.playerRb = pTemp.playerObj.GetComponent<Rigidbody>();
                pTemp.playerRb.freezeRotation = true;

                pTemp.textID = pTemp.playerObj.GetComponent<TextMeshProUGUI>();
                pTemp.textID.text = playerId;

                Transform child = pTemp.playerObj.transform.GetChild(0);
                child.gameObject.GetComponent<TextMeshPro>().text = playerName;

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
            player.playerObj.transform.position = spawnPositions[playerList.Count].position;

            player.playerRb = player.playerObj.GetComponent<Rigidbody>();
            player.playerRb.freezeRotation = true;

            player.textID = player.playerObj.GetComponent<TextMeshProUGUI>();
            player.textID.text = playerId;

            Transform child = player.playerObj.transform.GetChild(0);
            child.gameObject.GetComponent<TextMeshPro>().text = playerName;

            PlayerServer pServer = new PlayerServer();
            pServer.ID = playerId;
            pServer.position = tmp.transform.position;
            pServer.name = playerName;

            playerList.Add(pServer);

            serialization.SendAllPlayers(playerList);
        }

         //UnityEngine.Debug.Log("CreatePlayer!");
    }

    public void CreateNewPlayer() //This is called by the button CREATE PLAYER.
    {
        //while(player.playerObj == null) { }
        serialization.serializeCreatePlayer(c_udp.clientID, serialization.tmpNameClient, playerList.Count);
    }

    public void SpawnAllPlayers(List<PlayerServer> pList)
    {
        foreach (var pServer in pList) 
        {
            GameObject tmpPlayer = Instantiate(playerPref, clientParent.transform);
            tmpPlayer.transform.position = pServer.position;
            tmpPlayer.transform.rotation = pServer.rotation;

            Player _player = new Player();
            _player.ID = pServer.ID;

            _player.playerObj = tmpPlayer;

            _player.playerRb = _player.playerObj.GetComponent<Rigidbody>();
            _player.playerRb.freezeRotation = true;

            _player.textID = _player.playerObj.GetComponent<TextMeshProUGUI>();
            _player.textID.text = pServer.ID;

            Transform child = _player.playerObj.transform.GetChild(0);
            child.gameObject.GetComponent<TextMeshPro>().text = pServer.name;
        }
    }

    public void UpdatePositionAndRotation()
    {
        if (moveUpdatePlayer.gameObject.transform.position != moveUpdatePlayer.futurePosition)
        {
            moveUpdatePlayer.gameObject.transform.position = Vector3.Lerp(moveUpdatePlayer.gameObject.transform.position, moveUpdatePlayer.futurePosition, 100 * Time.deltaTime);
            hasMoved = true;
        }

        if (moveUpdatePlayer.gameObject.transform.rotation != moveUpdatePlayer.futureRotation)
        {
            moveUpdatePlayer.gameObject.transform.rotation = Quaternion.Lerp(moveUpdatePlayer.gameObject.transform.rotation, moveUpdatePlayer.futureRotation, 100 * Time.deltaTime);
            hasMoved = true;
        }

        if(moveUpdatePlayer.gameObject.transform.rotation != moveUpdatePlayer.futureRotation && moveUpdatePlayer.gameObject.transform.position != moveUpdatePlayer.futurePosition)
        {
            hasMoved = false;
        }
    }

    public void ClientMove(string ID, Vector3 moveTo, Quaternion rotation)
    {
        List<GameObject> clientList = new List<GameObject>();

        foreach (Transform child in clientParent.transform)
        {
            clientList.Add(child.gameObject);

            string idClient = child.gameObject.GetComponent<TextMeshProUGUI>().text;

            if(ID == idClient)
            {
                moveUpdatePlayer.gameObject = child.gameObject;
                hasMoved = true;
                moveUpdatePlayer.actualPosition = child.gameObject.transform.position;
                moveUpdatePlayer.actualRotation = child.gameObject.transform.rotation;
                moveUpdatePlayer.futurePosition = moveTo;
                moveUpdatePlayer.futureRotation = rotation;

                List<PlayerServer> tmpPlayers = new List<PlayerServer>(); 

                foreach(var pServer in playerList)
                {
                    PlayerServer pTmp = new PlayerServer();
                    pTmp = pServer;

                    if (pServer.ID == ID)
                    {
                        pTmp.position = moveTo;
                        pTmp.rotation = rotation;
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
