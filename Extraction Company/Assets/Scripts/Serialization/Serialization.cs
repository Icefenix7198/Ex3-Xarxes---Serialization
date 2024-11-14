using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using static PlayerManager;

public class Serialization : MonoBehaviour
{
    public enum ActionType
    {
        CREATE_PLAYER, //Create Player for new Client
        MOVE_SERVER,
        MOVE_CLIENT,//Move all players positions
        ID_NAME,
        SPAWN_PLAYERS, //Create player in scene for other clients
        NONE
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

    public string serializeMovement(string ID, Vector3 movement, Quaternion rotation)
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

        bytes = stream.ToArray();

        Send(bytes, id);

        return id;
    }

    public int Deserialize(byte[] message)
    {
        try
        {
            stream = new MemoryStream();
            stream.Write(message, 0, message.Length);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

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

                            playerManager.ClientMove(ID, movement, rotation); //Move te player in the server

                            GameObject playerMoved = playerManager.FindPlayer(ID); //Search the player who has moved

                            if (playerMoved != null) //Check is not null
                            {
                                serializeMovement(ID, playerMoved.transform.position, playerMoved.transform.rotation);
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

                            playerManager.ClientMove(ID, movement, rotaiton);

                            //Size of the 3 floats together of movement + 4 floats of rotation
                            binaryLength = sizeof(float) * 7;
                            break;
                        }
                    case ActionType.SPAWN_PLAYERS: 
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

                        if (playerManager.player.playerObj == null && pList.Count > 1)
                        {
                            pList.RemoveAt(lengthSize - 1);
                            playerManager.SpawnAllPlayers(pList);
                        }

                        playerManager.NewPlayer(idTmp, lastName, lengthSize-1);

                        //Length of the binary: Int (4) of TotalLength + sizeOfTheThings (he calculated during the process of reading)
                        binaryLength = 4 + totalLength;
                        break;
                    default:
                        break;
                }

                //We return the length of an int (Action), the ID length (as each character is 1 byte) and the specefic size of each parameters
                return 4 + ID.Length + binaryLength;
            }
            catch
            {
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
        c_udp.server.SendTo(message, message.Length, SocketFlags.None, c_udp.ipepServer);
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

    //Takes bytes of data and extracts the first bits of information to return the first action type of the string.
    public ActionType ExtractAction(byte[] message)
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
}