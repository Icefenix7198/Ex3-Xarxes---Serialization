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

    byte[] bytes; //ERIC: Esto maybe seria mas correcto escrito como Data
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

        //UnityEngine.Debug.Log("Assign ID serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);
    }

    public void serializeCreatePlayer(string id)
    {
        ActionType type = ActionType.CREATE_PLAYER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(id);

        //UnityEngine.Debug.Log("Create player serialized!");
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

        foreach (var a in playerList) //Se guarda ID = ..... move = [x, y, z], ID....
        {
            writer.Write(a.ID);

            float[] move = { a.position.x, a.position.y, a.position.z };

            foreach (var b in move)
            {
                writer.Write(b);
            }

            lastID = a.ID;
        }

        //UnityEngine.Debug.Log("Send all players serialized!");
        bytes = stream.ToArray();

        Send(bytes, lastID);
    }

    public string serializeMovement(string ID, Vector3 movement)
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

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(ID);

        foreach (var i in move)
        {
            writer.Write(i);
        }

        //UnityEngine.Debug.Log("Movement was serialized!");
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
                switch (action)
                {
                    case ActionType.ID_NAME:
                        {
                            ID = reader.ReadString();
                            tmpNameClient = reader.ReadString();
                            break;
                        }

                    case ActionType.CREATE_PLAYER:
                        {
                            ID = reader.ReadString();
                            playerManager.NewPlayer(ID);
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
                            playerManager.ClientMove(ID, movement); //Movemos el player en el server.

                            GameObject playerMoved = playerManager.FindPlayer(ID); //Buscamos el player que se ha movido

                            if (playerMoved != null) //revisar que el player sea distinto de null para saber que existe
                            {
                                serializeMovement(ID, playerMoved.transform.position);
                            }
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
                            playerManager.ClientMove(ID, movement);
                            break;
                        }
                    case ActionType.SPAWN_PLAYERS:
                        int lenghtSize = reader.ReadInt32();

                        List<PlayerServer> pList = new List<PlayerServer>();

                        PlayerServer pServer = new PlayerServer();

                        for (int i = 0; i < lenghtSize; i++)
                        {
                            pServer.ID = reader.ReadString();

                            float[] moveList1 = new float[3];
                            for (int a = 0; a < moveList1.Length; a++)
                            {
                                moveList1[a] = reader.ReadSingle();
                            }

                            pServer.position = new Vector3(moveList1[0], moveList1[1], moveList1[2]);
                            pList.Add(pServer);
                        }

                        string idTmp = pServer.ID;

                        if (playerManager.player.playerObj == null && pList.Count > 1)
                        {
                            pList.RemoveAt(lenghtSize - 1);
                            playerManager.SpawnAllPlayers(pList);
                        }

                        playerManager.NewPlayer(idTmp);
                        break;
                    default:
                        break;
                }

                //We return the length of an int (Action), the ID length (as each character is 1 byte) and the specefic size of each parameters
                return 4 + ID.Length + 0; //WIP
            }
            catch
            {
                //UnityEngine.Debug.Log("Data was corrupted during deserialization");
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
                //ActionType action = (ActionType)reader.ReadInt32(); //We exctract the action to have next the ID and be able to read it.
                ID = reader.ReadString();
            }
            catch
            {
                //UnityEngine.Debug.LogWarning("Id couldn't be taken");
            }

            //UnityEngine.Debug.Log("ID Taked! It was:" + ID);


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
                //UnityEngine.Debug.Log("Action Taked! It was: " + action);
            }
            catch
            {
                //UnityEngine.Debug.LogWarning("Action could not be catched!");
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

        //stream = new MemoryStream();
        //BinaryWriter writer = new BinaryWriter(stream);
        //writer.Write(";;");

        //separator = stream.ToArray(); //A byte of ;; is [2][59][59]

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