using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerManager;
using static System.Net.Mime.MediaTypeNames;
using TMPro;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPref;
    public ClientUDP c_udp;
    public ServerUDP s_udp;
    public Serialization serialization;
    public ExtractionManager extractionManager;
    public GameObject clientParent;
    public GameObject mainCam;
    
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

        public Queue<Vector3> positions;
        public Queue<Quaternion> rotations;
    }

    public Player player;
    public List<PlayerServer> playerList = new List<PlayerServer>(); //Temporalmente publica

    public float speed;
    float dt;

    float waitToUpdate = 0.0416f; //Times that waits until sending data and for lerp things
    float dtInterpolate = 0;
    public float smoothness = 0.05f;

    List<PlayerToUpdate> movedPlayers = new List<PlayerToUpdate>();

    public Transform[] spawnPositions;

    bool passedScene = false;

    public List<TMP_Text> player_Names_UI;
    int playerCountName = 1;

    [System.Obsolete]
    private void Update()
    {
        if (s_udp == null && c_udp == null)
        {
            s_udp = GameObject.Find("UDP_Manager").GetComponent<ServerUDP>();
        }

        if (serialization == null)
        {
            serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        }

        if (c_udp == null && s_udp == null)
        {
            c_udp = GameObject.Find("UDP_Manager").GetComponent<ClientUDP>();
        }

        if (s_udp != null)
        {
            button.SetActive(false);
        }

        if(player.playerObj != null && c_udp != null) 
        {
            dt += Time.deltaTime;

            if (dt > waitToUpdate) //Random.RandomRange(0.0200f, 0.600f//We only send the info some frames not constantly to reduce the server load
            {
                SendMovement();
                dt = 0;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            UnityEngine.Application.Quit();
        }

        UpdatePositionAndRotation();
    }

    private void SendMovement()
    {
        if (passedScene)
        {
            if (player.playerObj != null)
            {
                if (Mathf.Abs(player.playerRb.velocity.magnitude) > 0.05 || Input.GetAxisRaw("Mouse X") != 0)
                {
                    serialization.serializeMovement(player.ID, player.playerObj.transform.position, player.playerObj.transform.rotation);
                }
            }
        }
    }

    public void NewPlayer(string playerId, string playerName = "Player",int playerNum = 4) //Player num is used for determining spawn positions for clients
    {
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
                player.playerObj.GetComponent<InventoyManager>().enabled = true;

                player.textID = player.playerObj.GetComponent<TextMeshProUGUI>();
                player.textID.text = playerId;

                Transform child = player.playerObj.transform.GetChild(0);
                child.gameObject.GetComponent<TextMeshPro>().text = playerName;
                player_Names_UI[0].text = playerName;

                Transform cam = player.playerObj.transform.GetChild(1);
                cam.gameObject.SetActive(true);

                if (mainCam != null)
                {
                    mainCam.SetActive(false);
                }

                serialization.RequestItems(playerId);

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
                player_Names_UI[playerCountName].text = playerName;

                PlayerServer pServer = new PlayerServer();
                pServer.ID = pTemp.ID;
                pServer.position = pTemp.playerObj.transform.position;

                playerList.Add(pServer);
                playerCountName++;
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
            player_Names_UI[playerCountName - 1].text = playerName;
            extractionManager.extractions_ofPlayers.Add(playerId, 0);
            extractionManager.extractions.Add(playerName, 0);
            extractionManager.player_Names.Add(playerName);
            extractionManager.player_Numbers.Add(0);

            PlayerServer pServer = new PlayerServer();
            pServer.ID = playerId;
            pServer.position = tmp.transform.position;
            pServer.name = playerName;

            playerList.Add(pServer);

            serialization.SendAllPlayers(playerList);
            playerCountName++;
        }
    }

    public void CreateNewPlayer() //This is called by the button CREATE PLAYER.
    {
        serialization.serializeCreatePlayer(c_udp.clientID, serialization.tmpNameClient, playerList.Count);
    }

    public void SpawnAllPlayers(List<PlayerServer> pList)
    {
        int nameCount = 1;

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
            player_Names_UI[nameCount].text = pServer.name;

            nameCount++;
        }
    }

    public void UpdatePositionAndRotation()
    {
        if(dtInterpolate > waitToUpdate)
        {
            dtInterpolate = 0;
        }

        Vector3 vel = new Vector3();

        foreach(var movedPlayer in movedPlayers)
        {
            Rigidbody rbPlayer = movedPlayer.gameObject.GetComponent<Rigidbody>();
            PlayerMovement pMovement = movedPlayer.gameObject.GetComponent<PlayerMovement>();

            if (movedPlayers.Count > 0)
            {
                if (movedPlayer.positions.Count > 0) 
                {
                    if (movedPlayer.gameObject.transform.position != movedPlayer.positions.Peek())
                    {
                        float dist = Mathf.Abs((movedPlayer.gameObject.transform.position - movedPlayer.positions.Peek()).magnitude);

                        if (dist > 0.1f && dist < 0.7f)
                        {
                            pMovement.animator.SetFloat("Speed", 0.2f);
                            pMovement.animator.SetBool("Run", false);
                        }
                        else if(dist > 0.7f)
                        {
                            pMovement.animator.SetBool("Run", true);
                        }
                        else
                        {
                            pMovement.animator.SetFloat("Speed", 0f);
                            pMovement.animator.SetBool("Run", false);
                        }

                        Vector3 moveTo = movedPlayer.positions.Peek();
                        //movedPlayer.gameObject.transform.position = Vector3.SmoothDamp(movedPlayer.gameObject.transform.position, moveTo, ref vel, smoothness);
                        movedPlayer.gameObject.transform.position = Vector3.Lerp(movedPlayer.gameObject.transform.position, moveTo, dtInterpolate / 0.2f);
                    }
                    else
                    {
                        if (movedPlayers.Count > 0)
                        {
                            movedPlayer.positions.Dequeue();
                            movedPlayers.FindIndex(movedPlayers => movedPlayers.Equals(movedPlayer));
                        }
                    }
                }

                if (movedPlayer.rotations.Count > 0)
                {
                    if (movedPlayer.gameObject.transform.rotation != movedPlayer.rotations.Peek())
                    {
                        Quaternion rotationTo = movedPlayer.rotations.Peek();
                        movedPlayer.gameObject.transform.rotation = Quaternion.Lerp(movedPlayer.gameObject.transform.rotation, rotationTo, dtInterpolate / 0.1f);
                    }
                    else
                    {
                        if (movedPlayers.Count > 0)
                        {
                            movedPlayer.rotations.Dequeue();
                            movedPlayers.FindIndex(movedPlayers => movedPlayers.Equals(movedPlayer));
                        }
                    }
                }
            }
        }

        dtInterpolate += Time.deltaTime;
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
                PlayerToUpdate movedPlayer = new PlayerToUpdate();

                movedPlayer.gameObject = child.gameObject;
                movedPlayer.actualPosition = child.gameObject.transform.position;
                movedPlayer.actualRotation = child.gameObject.transform.rotation;
                movedPlayer.positions = new Queue<Vector3>();
                movedPlayer.rotations = new Queue<Quaternion>();
                movedPlayer.positions.Enqueue(moveTo);
                movedPlayer.rotations.Enqueue(rotation);

                bool exist = false;

                for(int i = 0; i < movedPlayers.Count; i++) 
                {
                    if(movedPlayers[i].gameObject == movedPlayer.gameObject) 
                    {
                        movedPlayers[i] = movedPlayer;
                        exist = true;

                    }
                }

                if (!exist)
                {
                    movedPlayers.Add(movedPlayer);
                }

                List<PlayerServer> tmpPlayers = new List<PlayerServer>(); 

                foreach(var pServer in playerList)
                {
                    PlayerServer pTmp = new PlayerServer();
                    pTmp = pServer;

                    if (pServer.ID == ID)
                    {
                        pTmp.position = moveTo;
                        pTmp.rotation = rotation;
                    }

                    tmpPlayers.Add(pTmp);
                }

                playerList = tmpPlayers;
            }
        }
    }

    public GameObject FindPlayer(string ID)
    {
        foreach (Transform child in clientParent.transform) //We take all the childrens from the father ClientList
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
