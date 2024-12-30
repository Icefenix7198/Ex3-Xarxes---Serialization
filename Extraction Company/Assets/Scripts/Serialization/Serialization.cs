using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using static ItemGenerator;
using static PlayerManager;
using static ServerUDP;

public class Serialization : MonoBehaviour
{
    public enum ActionType
    {
        CREATE_PLAYER, //Create Player for new Client
        MOVE_SERVER,
        MOVE_CLIENT,//Move all players positions
        ID_NAME,
        SPAWN_PLAYERS, //Create player in scene for other clients
        CREATE_MONSTER,
        REQUEST_MONSTERS, //When spawns player create all current monsters
        UPDATE_MONSTER, //Send position and target to monster dummy
        SPAWN_ITEMS,
        REQUEST_ITEMS,
        DESTROY_ITEM,
        EXTRACTION_TO_SERVER,
        EXTRACTION_TO_CLIENT,
        MAX_PLAYERS,
        DOORS,
        WIN,
        DEATH,
        ACK,
        NONE
    }

    public struct ItemToDestroy
    {
        public string item;
        public string playerID;
    }

    static MemoryStream stream;

    ClientUDP c_udp;
    public bool isC_udp;

    ServerUDP s_udp;
    public bool isS_udp;
    public string tmpNameClient;

    byte[] bytes; //Messages serialized that are sent
    byte[] chainData; //WIP: All the bytes to send each update, where more than one serialized action is found, separated by a ";;"

    //Scripts
    PlayerManager playerManager;
    ItemGenerator itemManager;
    ExtractionManager extractionManager;
    MonsterManager monsterManager;
    Door_Manager buttonInteraction; 

    public GameObject maxPlayers;

    ItemToDestroy itemToDestroy;
    bool itemDestroy = false;

    private void Start()
    {
        if (isC_udp)
        {
            c_udp = new ClientUDP();
            c_udp = this.GetComponent<ClientUDP>();
        }

        if (isS_udp)
        {
            s_udp = new ServerUDP();
            s_udp = this.GetComponent<ServerUDP>();
        }
    }

    private void Update()
    {
        //On each frame we check if we connected to assign the player manager. Once we have it assigned we stop trying
        if (isC_udp)
        {
            if (playerManager == null && c_udp.passSceneManager.isConnected)
            {
                GameObject tmp = GameObject.Find("PlayerSpawner");

                if (tmp != null)
                {
                    playerManager = tmp.GetComponent<PlayerManager>();
                }
            }

            if (itemManager == null && c_udp.passSceneManager.isConnected)
            {
                GameObject tmp = GameObject.Find("ItemsManager");

                if (tmp != null)
                {
                    itemManager = GameObject.Find("ItemsManager").GetComponent<ItemGenerator>();
                }
            }

            if (extractionManager == null && c_udp.passSceneManager.isConnected)
            {
                GameObject tmp = GameObject.Find("ExtractionManager");

                if (tmp != null)
                {
                    extractionManager = GameObject.Find("ExtractionManager").GetComponent<ExtractionManager>();
                }
            }

            if (buttonInteraction == null && c_udp.passSceneManager.isConnected)
            {
                GameObject tmp = GameObject.Find("DoorManager");

                if (tmp != null)
                {
                    buttonInteraction = GameObject.Find("DoorManager").GetComponent<Door_Manager>();
                }
            }

            if (monsterManager == null && c_udp.passSceneManager.isConnected)
            {
                GameObject tmp = GameObject.Find("MonsterManager");

                if (tmp != null)
                {
                    monsterManager = GameObject.Find("MonsterManager").GetComponent<MonsterManager>();
                }
            }

        }

        if (isS_udp)
        {
            if (playerManager == null && s_udp.passScene.isConnected)
            {
                GameObject tmp = GameObject.Find("PlayerSpawner");

                if (tmp != null)
                {
                    playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
                }
            }

            if (itemManager == null && s_udp.passScene.isConnected)
            {
                GameObject tmp = GameObject.Find("ItemsManager");

                if (tmp != null)
                {
                    itemManager = GameObject.Find("ItemsManager").GetComponent<ItemGenerator>();
                }
            }

            if (extractionManager == null && s_udp.passScene.isConnected)
            {
                GameObject tmp = GameObject.Find("ExtractionManager");

                if (tmp != null)
                {
                    extractionManager = GameObject.Find("ExtractionManager").GetComponent<ExtractionManager>();
                }
            }

            if (monsterManager == null && s_udp.passScene.isConnected)
            {
                GameObject tmp = GameObject.Find("MonsterManager");

                if (tmp != null)
                {
                    monsterManager = GameObject.Find("MonsterManager").GetComponent<MonsterManager>();
                }
            }

            if (buttonInteraction == null && s_udp.passScene.isConnected)
            {
                GameObject tmp = GameObject.Find("DoorManager");

                if (tmp != null)
                {
                    buttonInteraction = GameObject.Find("DoorManager").GetComponent<Door_Manager>();
                }
            }

        }

        if (itemDestroy)
        {
            itemManager.DestroyItem(itemToDestroy.item, itemToDestroy.playerID);
            itemDestroy = false;
        }
    }
    public void serializeIDandName(string id, string name)
    {
        ActionType type = ActionType.ID_NAME;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(id);
        writer.Write(name);

        bytes = stream.ToArray();

        Send(bytes, id);
    }

    public void serializeCreatePlayer(string id, string name, int numPlayers = -1)
    {
        ActionType type = ActionType.CREATE_PLAYER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(id);
        writer.Write(name);
        writer.Write(numPlayers);

        bytes = stream.ToArray();

        Send(bytes, id);
    }

    //Tell all current existing player
    public void SendAllPlayers(List<PlayerServer> playerList)
    {
        ActionType type = ActionType.SPAWN_PLAYERS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        string lastID = "-1";

        writer.Write((int)type);
        writer.Write(playerList.Count);

        foreach (var a in playerList) //It stores ID = ....., name = ...... move = [x, y, z], ID....
        {
            writer.Write(a.ID);
            writer.Write(a.name);

            float[] move = { a.position.x, a.position.y, a.position.z };
            float[] rotation = { a.rotation.x, a.rotation.y, a.rotation.z, a.rotation.w };

            foreach (var m in move)
            {
                writer.Write(m);
            }

            foreach (var r in rotation)
            {
                writer.Write(r);
            }

            lastID = a.ID;
        }

        bytes = stream.ToArray();

        Send(bytes, lastID);
    }

    public string serializeMovement(string ID, Vector3 movement, Quaternion rotation, bool run)
    {
        string id = ID;
        ActionType type = ActionType.NONE;

        if (isC_udp)
        {
            type = ActionType.MOVE_SERVER;
        }

        if (isS_udp)
        {
            type = ActionType.MOVE_CLIENT;
        }

        float[] move = { movement.x, movement.y, movement.z };
        float[] rot = { rotation.x, rotation.y, rotation.z, rotation.w };

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(ID);

        foreach (var i in move)
        {
            writer.Write(i);
        }

        foreach (var r in rot)
        {
            writer.Write(r);
        }

        writer.Write(run);

        bytes = stream.ToArray();

        Send(bytes, id);

        return id;
    }

    public string serializeCreateMonster(string id, Vector2 position, int monsterType = 0)
    {
        ActionType type = ActionType.CREATE_MONSTER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);

        writer.Write(1);


        writer.Write(monsterType);

        float[] pos = { position.x, position.y };

        for (int i = 0; i < 2; i++)
        {
            writer.Write(pos[i]);
        }


        Send(bytes, id);

        return id;
    }

    public void SendMonsters(List<GameObject> listMonsters, string ID)
    {
        if (listMonsters.Count > 0) 
        {
            ActionType type = ActionType.CREATE_MONSTER;

            stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((int)type);

            writer.Write(listMonsters.Count);

            foreach (GameObject mon in monsterManager.GetExistingMonsterList())
            {
                writer.Write(monsterManager.GetMonsterID(mon));

                float[] pos = { mon.transform.position.x, mon.transform.position.z };

                for (int i = 0; i < 2; i++)
                {
                    writer.Write(pos[i]);
                }
            }

            bytes = stream.ToArray();

            Send(bytes, ID);
        }        
    }

    public void SerializeUpdateMonster(string ID,int monsterIndex,Vector2 monPosition, Vector3 monTarget)
    {
        ActionType type = ActionType.UPDATE_MONSTER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        //Action Serialized
        writer.Write((int)type);

        //Write player target
        writer.Write(ID);

        //Write list position of currentMonsterList
        writer.Write(monsterIndex);


        float[] pos = { monPosition.x, monPosition.y };

        for (int i = 0; i < 2; i++)
        {
            writer.Write(pos[i]);
        }

        float[] target = { monTarget.x, monTarget.y, monTarget.z };

        for (int i = 0; i < 3; i++)
        {
            writer.Write(target[i]);
        }

        bytes = stream.ToArray();

        Send(bytes, ID);
    }

    public void SendItems(List<itemObj> items, string ID)
    {
        ActionType type = ActionType.SPAWN_ITEMS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);

        writer.Write(items.Count);

        foreach (var item in items)
        {
            writer.Write(item.ID);
            writer.Write(item.objType);
            writer.Write((int)item.type);

            float[] pos = { item.pos.x, item.pos.y, item.pos.z };

            for (int i = 0; i < 3; i++)
            {
                writer.Write(pos[i]);
            }
        }

        bytes = stream.ToArray();

        Send(bytes, ID);
    } 
    
    public void SendDestroyItem(itemObj item, string ID)
    {
        ActionType type = ActionType.DESTROY_ITEM;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);
        writer.Write(ID);

        writer.Write(item.ID);

        bytes = stream.ToArray();

        Send(bytes, ID);
    }

    public void SendExtraction(int extraction, string ID)
    {
        ActionType type = ActionType.EXTRACTION_TO_SERVER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);
        writer.Write(ID);

        writer.Write(extraction);

        bytes = stream.ToArray();

        Send(bytes, ID);
    }
    
    public void SendExtraction(List<string> names, List<int> extractions)
    {
        ActionType type = ActionType.EXTRACTION_TO_CLIENT;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);

        writer.Write(names.Count);

        foreach (string name in names)
        {
            writer.Write(name);
        }

        writer.Write(extractions.Count);

        foreach (int extraction in extractions)
        {
            writer.Write(extraction);
        }

        bytes = stream.ToArray();

        Send(bytes, "-2");
    }
    
    public void MaxPlayers(string ID)
    {
        ActionType type = ActionType.MAX_PLAYERS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);

        bytes = stream.ToArray();

        Send(bytes, ID);
    }

    public void WinCondition(string name, string ID)
    {
        ActionType type = ActionType.WIN;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);
        writer.Write(ID);

        writer.Write(name);

        bytes = stream.ToArray();

        Send(bytes, ID);
    } 
    
    public void SendDeathPlayer(string ID)
    {
        ActionType type = ActionType.DEATH;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);
        writer.Write(ID);

        bytes = stream.ToArray();

        Send(bytes, ID);
    }

    public void SendDoors(string door, string ID = "-2")
    {
        ActionType type = ActionType.DOORS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((int)type);
        writer.Write(ID);
        writer.Write(door);

        bytes = stream.ToArray();

        Send(bytes, ID);
    }

    public byte[] SendAckMessage(byte[] data, string id, string clientID)
    {
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(id);
        writer.Write(clientID);
        writer.Write(data);

        bytes = stream.ToArray();

        return bytes;
    }

    public string ReturnAckMessage(byte[] data, UserUDP u)
    {
        stream = new MemoryStream();
        stream.Write(data, 0, data.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string id = reader.ReadString();
        string clientID = reader.ReadString();

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        ActionType type = ActionType.ACK;

        writer.Write((int)type);
        writer.Write(id);

        bytes = stream.ToArray();

        u.socket.SendTo(bytes, bytes.Length, SocketFlags.None, u.Remote);

        return clientID;
    }

    public int Deserialize(byte[] message)
    {
        try
        {
            stream = new MemoryStream();
            stream.Write(message, 0, message.Length);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (isS_udp)
            {
                string id_ACK = reader.ReadString();
                string clientID_ACK = reader.ReadString();
            }

            try
            {
                ActionType action = (ActionType)reader.ReadInt32();
                string ID = "";
                int binaryLength = 0;

                switch (action)
                {
                    case ActionType.ID_NAME:
                        {
                            ID = reader.ReadString();
                            tmpNameClient = reader.ReadString();

                            //Length of the name we send
                            binaryLength = tmpNameClient.Length;
                            break;
                        }
                    case ActionType.CREATE_PLAYER:
                        {
                            ID = reader.ReadString();
                            string playerName = reader.ReadString();
                            int numPlayer = -1;

                            int temp = reader.ReadInt32(); //We read the player number sent by create player

                            if(temp<4 && temp >= 0) //We check data isn't corrupted
                            {
                                numPlayer = temp;
                            }

                            playerManager.NewPlayer(ID, playerName, numPlayer);

                            binaryLength = playerName.Length + 4;
                            break;
                        }
                    case ActionType.MOVE_SERVER:
                        {
                            ID = reader.ReadString();
                            float[] moveList = new float[3];
                            for (int i = 0; i < moveList.Length; i++)
                            {
                                moveList[i] = reader.ReadSingle();
                            }

                            Vector3 movement = new Vector3(moveList[0], moveList[1], moveList[2]);

                            float[] rotList = new float[4];
                            for (int i = 0; i < rotList.Length; i++)
                            {
                                rotList[i] = reader.ReadSingle();
                            }

                            Quaternion rotation = new Quaternion(rotList[0], rotList[1], rotList[2], rotList[3]);

                            bool run = reader.ReadBoolean();

                            playerManager.ClientMove(ID, movement, rotation, run); //Move te player in the server

                            GameObject playerMoved = playerManager.FindPlayer(ID); //Search the player who has moved

                            if (playerMoved != null) //Check is not null
                            {

                                serializeMovement(ID, playerMoved.transform.position, playerMoved.transform.rotation, run);
                            }

                            //Size of the 3 floats together of movement + 4 floats of rotation
                            binaryLength = sizeof(float) * 7;
                            break;
                        }
                    case ActionType.MOVE_CLIENT:
                        {
                            ID = reader.ReadString();
                            float[] moveList = new float[3];
                            for (int i = 0; i < moveList.Length; i++)
                            {
                                moveList[i] = reader.ReadSingle();
                            }

                            Vector3 movement = new Vector3(moveList[0], moveList[1], moveList[2]);

                            float[] rotateList = new float[4];
                            for (int i = 0; i < rotateList.Length; i++)
                            {
                                rotateList[i] = reader.ReadSingle();
                            }
                            Quaternion rotaiton = new Quaternion(rotateList[0], rotateList[1], rotateList[2], rotateList[3]);

                            bool run = reader.ReadBoolean();

                            playerManager.ClientMove(ID, movement, rotaiton, run);

                            //Size of the 3 floats together of movement + 4 floats of rotation
                            binaryLength = sizeof(float) * 7;
                            break;
                        }
                    case ActionType.SPAWN_PLAYERS:
                        {
                            int lengthSize = reader.ReadInt32();

                            List<PlayerServer> pList = new List<PlayerServer>();

                            PlayerServer pServer = new PlayerServer();

                            int totalLength = 0; //Binary length of all the things inside the server

                            for (int i = 0; i < lengthSize; i++)
                            {
                                pServer.ID = reader.ReadString();
                                pServer.name = reader.ReadString();

                                totalLength += pServer.ID.Length; //Length ID, as we dont assign ID and stays as "" it doesn't have a length in the final

                                float[] moveList1 = new float[3];
                                for (int a = 0; a < moveList1.Length; a++)
                                {
                                    moveList1[a] = reader.ReadSingle();
                                }
                                totalLength += sizeof(float) * 3; //Length of the move vector

                                pServer.position = new Vector3(moveList1[0], moveList1[1], moveList1[2]);

                                float[] rotation = new float[4];
                                for (int a = 0; a < rotation.Length; a++)
                                {
                                    rotation[a] = reader.ReadSingle();
                                }
                                totalLength += sizeof(float) * 4; //Length of the rotation quaternion

                                pServer.rotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);

                                pList.Add(pServer);
                            }

                            string idTmp = pServer.ID;
                            string lastName = pServer.name;

                            if (!playerManager.button.activeInHierarchy)
                            {
                                if (playerManager.player.playerObj == null && pList.Count > 1)
                                {
                                    pList.RemoveAt(lengthSize - 1);
                                    playerManager.SpawnAllPlayers(pList);
                                }

                                playerManager.NewPlayer(idTmp, lastName, lengthSize - 1);
                            }

                            //Length of the binary: Int (4) of TotalLength + sizeOfTheThings (he calculated during the process of reading)
                            binaryLength = 4 + totalLength;
                            break;
                        }
                    case ActionType.CREATE_MONSTER:
                        {
                            int length = reader.ReadInt32();

                            for (int i = 0; i < length; i++)
                            {
                                int typeMonster = reader.ReadInt32();

                                float[] pos = new float[2];
                                for (int b = 0; b < pos.Length; b++)
                                {
                                    pos[b] = reader.ReadSingle();
                                }

                                Vector2 monPosition = new Vector2(pos[0], pos[1]);

                                monsterManager.SpawnEnemy(typeMonster, monPosition,true);

                            }

                            //Size of the 3 floats together of movement + 4 floats of rotation
                            binaryLength = sizeof(float) * 7;
                            break;
                        }
                    case ActionType.REQUEST_MONSTERS: //For when the player spaws after the game already started
                        {
                            ID = reader.ReadString();

                            //Debug.Log("TEMPORAL! Entro en deserialize RequestMonsters, list size was:" + monsterManager.GetExistingMonsterList() + " and id is: " + ID);
                            SendMonsters(monsterManager.GetExistingMonsterList(), ID);
                            break;
                        }
                    case ActionType.UPDATE_MONSTER: //For when the player spaws after the game already started
                        {
                            ID = reader.ReadString();

                            int monsterListIndex = reader.ReadInt32();

                            float[] pos = new float[2]; //World position
                            for (int b = 0; b < pos.Length; b++)
                            {
                                pos[b] = reader.ReadSingle();
                            }

                            Vector2 monPosition = new Vector2(pos[0], pos[1]);

                            float[] target = new float[3]; //Target Monster
                            for (int t = 0; t < target.Length; t++)
                            {
                                target[t] = reader.ReadSingle();
                            }

                            Vector3 monTarget = new Vector3(target[0], pos[1], target[2]);

                            monsterManager.UpdateClientMonster(monsterListIndex, monPosition, monTarget);
                            break;
                        }
                    case ActionType.SPAWN_ITEMS:
                        {
                            List<itemObj> items = new List<itemObj>();

                            int length = reader.ReadInt32();

                            for (int i = 0; i < length; i++)
                            {
                                string id = reader.ReadString();
                                int objType = reader.ReadInt32();
                                int Type = reader.ReadInt32();

                                float[] pos = new float[3];
                                for (int b = 0; b < pos.Length; b++)
                                {
                                    pos[b] = reader.ReadSingle();
                                }

                                Vector3 movement = new Vector3(pos[0], pos[1], pos[2]);

                                itemObj item = new itemObj();
                                item.pos = movement;
                                item.objType = objType;
                                item.type = (itemType)Type;
                                item.ID = id;

                                items.Add(item);
                            }

                            itemManager.SpawnItems(items);

                            //Size of the 3 floats together of movement + 4 floats of rotation
                            binaryLength = sizeof(float) * 7;
                            break;
                        }
                    case ActionType.REQUEST_ITEMS:
                        {
                            ID = reader.ReadString();

                            SendItems(itemManager.allItems, ID);
                            break;
                        }
                    case ActionType.DESTROY_ITEM:
                        {
                            ID = reader.ReadString();
                            string itemID = reader.ReadString();

                            itemToDestroy.item = itemID;
                            itemToDestroy.playerID = ID;
                            itemDestroy = true;
                            break;
                        }
                    case ActionType.EXTRACTION_TO_SERVER:
                        {
                            ID = reader.ReadString();
                            int extraction = reader.ReadInt32();

                            extractionManager.SaveExtraction(ID, extraction);
                            break;
                        }
                    case ActionType.EXTRACTION_TO_CLIENT:
                        {
                            List<string> names = new List<string>();
                            List<int> extractions = new List<int>();

                            int nameCount = reader.ReadInt32();
                            for(int i = 0; i < nameCount; i++)
                            {
                                names.Add(reader.ReadString());
                            }

                            int extractionCount = reader.ReadInt32();
                            for (int i = 0; i < extractionCount; i++)
                            {
                                extractions.Add(reader.ReadInt32());
                            }

                            extractionManager.player_Names = names;
                            extractionManager.player_Numbers = extractions;

                            break;
                        }
                    case ActionType.MAX_PLAYERS:
                        {
                            MaxPlayers();
                            break;
                        }
                    case ActionType.DOORS:
                        {
                            ID = reader.ReadString();
                            string door = reader.ReadString();

                            if (isS_udp)
                            {
                                SendDoors(door, ID);
                            }

                            buttonInteraction.DoorOpen(door);

                            break;
                        }
                    case ActionType.WIN:
                        {
                            ID = reader.ReadString();
                            string name = reader.ReadString();

                            extractionManager.Losse(name);
                            break;
                        }
                    case ActionType.DEATH:
                        {
                            ID = reader.ReadString();

                            playerManager.ClientDeath(ID);
                            break;
                        }
                    case ActionType.ACK:
                        {
                            string id = reader.ReadString();

                            c_udp.ReciveAck(id);
                            break;
                        }
                    default:
                        break;
                }

                //We return the length of an int (Action), the ID length (as each character is 1 byte) and the specefic size of each parameters
                return 4 + ID.Length + binaryLength;
            }
             catch
            {
                int a = reader.ReadInt32();
                if(a < (int)ActionType.NONE) 
                {
                    ActionType action = (ActionType)reader.ReadInt32();
                    Debug.LogWarning("Failed deseralization of action:" + action);
                }
                else 
                {
                    Debug.LogWarning("Failed deseralization of posible action:" + a);
                }                
            }
        }
        catch
        {

        }

        return -1; //An error ocurred. WIP: Tengo que pensar/ver que devuelvo aqui.
    }

    //The send is generic as SendToServer only requieres bytes of data and just ignores the ID as the server don't have one.
    private void Send(byte[] message, string id)
    {
        if (isC_udp)
        {
            SendToServer(bytes);
        }

        if (isS_udp)
        {
            SendToClient(bytes, id);
        }
    }

    //Send from the Client to the server
    public void SendToServer(byte[] message)
    {
        c_udp.sendMessageACK(message, c_udp.ipepServer);
        //c_udp.server.SendTo(message, message.Length, SocketFlags.None, c_udp.ipepServer); //Send messages no Jitter and Packet Lost
    }

    //Send from the Server to the Client
    public void SendToClient(byte[] message, string ID)
    {
        s_udp.Send(message, ID);
    }

    public string ExtractID(byte[] message)
    {
        string ID = "-2";

        try
        {
            stream = new MemoryStream();
            stream.Write(message, 0, message.Length);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(sizeof(int), SeekOrigin.Begin);

            try
            {
                string id = reader.ReadString();
                string clientID = reader.ReadString();

                ID = reader.ReadString();
            }
            catch
            {
            }
        }
        catch
        {

        }

        return ID; //If return -2 ID == error taking ID
    }

    public string ExtractName(byte[] message)
    {
        string ID = "-2";
        string name = "Default";

        try
        {
            stream = new MemoryStream();
            stream.Write(message, 0, message.Length);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                string id = reader.ReadString();
                string clientID = reader.ReadString();

                ActionType tmpAction = (ActionType)reader.ReadInt32();
                string tmpID = reader.ReadString();
                name = reader.ReadString();
            }
            catch
            {
            }
        }
        catch
        {

        }

        return name; //If return -2 ID == error taking ID
    }

    //Takes bytes of data and extracts the first bits of information to return the first action type of the string.
    public ActionType ExtractAction(byte[] message, bool ack = false)
    {
        ActionType action = ActionType.NONE;

        try
        {
            stream = new MemoryStream();
            stream.Write(message, 0, message.Length);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);


            try
            {
                if (ack)
                {
                    string id = reader.ReadString();
                    string clientID = reader.ReadString();
                }

                action = (ActionType)reader.ReadInt32();
            }
            catch
            {
            }

        }
        catch
        {

        }

        return action;
    }

    public byte[] AddToSerializeChain(byte[] chain , byte[] message) //Old byte array, new btye array to add after it.
    {
        byte[] separator = new byte[] { 2, 59, 59 }; //This is the equivalent of serializing ";;" as a string, its written in this way to make the process faster

        byte[] rv = new byte[chain.Length + message.Length + separator.Length]; //New byte with old chain + message + separators

        //Code extracted from Stackoverflow, just combines the 3 arrays into one (rv)
        System.Buffer.BlockCopy(chain, 0, rv, 0, chain.Length);
        System.Buffer.BlockCopy(message, 0, rv, chain.Length, message.Length);
        System.Buffer.BlockCopy(separator, 0, rv, chain.Length + message.Length, separator.Length);

        return rv;
    }

    public void DeseralizeLongChain(byte[] message)
    {
        int index = 0;
        while (index < message.Length - 4) //The -4 is to take into account the ";;", maybe they become none necesary but for now I will keep using the ";;" separator
        {
            byte[] toSend = new byte[message.Length - index];
            System.Buffer.BlockCopy(message, index, toSend, 0, toSend.Length);
            index += Deserialize(toSend);
            if (message[index + 1] == 2 && message[index + 2] == 59 && message[index + 3] == 59) //Check we have separator, we check the bytes values as its faster.
            {
                index += 4; //Although the size of the separator is only 3, due to the index finishing in the last position of the chain prior to the separator we need to move 1 extra position to be over the separator and 3 more to move away from it
                Debug.Log("Current index is:" + index);
            }
            else
            {
                Debug.Log("Error in chain");
                break; //Maybe an idea here is instead of a break activate a protocol to ignore the unsucesful chain and seek the next message to deserialize.
            }
        }
    }

    public void RequestItems(string ID)
    {
        ActionType type = ActionType.REQUEST_ITEMS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(ID);

        bytes = stream.ToArray();

        Send(bytes, "-2");
    }

    public void RequestMonsters(string ID)
    {
        ActionType type = ActionType.REQUEST_MONSTERS;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(ID);

        bytes = stream.ToArray();

        Send(bytes, "-2"); //This message is send only from clients to the server
    }

    public void MaxPlayers()
    {
        if(maxPlayers != null)
        {
            maxPlayers.gameObject.SetActive(true);
        }
    }

    public byte[] QuitACK(byte[] message)
    {
        byte[] data;

        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string id = reader.ReadString();
        string clientID = reader.ReadString();
        data = reader.ReadBytes(message.Length);

        return data;
    }
}