using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Net;
using System.Net.Sockets;
using static PlayerManager;
using System.Diagnostics;

public class Serialization : MonoBehaviour
{
    public enum ActionType
    {
        CREATE_PLAYER, //Create Player for new Client
        MOVE_SERVER,
        MOVE_CLIENT,//Move all players positions
        ID,
        SPAWN_PLAYERS, //Create player in scene for other clients
        NONE
    }

    static MemoryStream stream;

    ClientUDP c_udp;
    public bool isC_udp;

    ServerUDP s_udp;
    public bool isS_udp;

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

                if(tmp != null)
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
    public void serializeID(string id)
    {
        ActionType type = ActionType.ID;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(id);

        UnityEngine.Debug.Log("Assign ID serialized!");
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

         UnityEngine.Debug.Log("Create player serialized!");
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

        UnityEngine.Debug.Log("Send all players serialized!");
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
        
        if(isS_udp)
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

        UnityEngine.Debug.Log("Movement was serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);

        return id;
    }

    public void Deserialize(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        try 
        {
            ActionType action = (ActionType)reader.ReadInt32();

            switch (action)
            {
                case ActionType.ID:
                    {
                        string ID = reader.ReadString();
                        break;
                    }

                case ActionType.CREATE_PLAYER:
                    {
                        string ID = reader.ReadString();
                        playerManager.NewPlayer(ID);
                        break;
                    }
                case ActionType.MOVE_SERVER:
                    {
                        string ID = reader.ReadString();
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
                        string ID = reader.ReadString();
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
        }
        catch 
        {
            UnityEngine.Debug.Log("Data was corrupted during deserialization");
        }
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
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(sizeof(int), SeekOrigin.Begin);

        string ID = "-2";

        try
        {
            //ActionType action = (ActionType)reader.ReadInt32(); //We exctract the action to have next the ID and be able to read it.
            ID = reader.ReadString();
        }
        catch
        {
            UnityEngine.Debug.LogWarning("Id couldn't be taken");
        }

        UnityEngine.Debug.Log("ID Taked! It was:" + ID);

        return ID; //If return -2 ID == error taking ID
    }

    //Takes bytes of data and extracts the first bits of information to return the first action type of the string.
    public ActionType ExtractAction(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        ActionType action = ActionType.NONE;

        try
        {
            action = (ActionType)reader.ReadInt32();
            UnityEngine.Debug.Log("Action Taked! It was: " + action);
        }
        catch
        {
            UnityEngine.Debug.LogWarning("Action could not be catched!");
        }

        return action;
    }

    public byte[] AddToSerializeChain(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string chain = reader.ToString();
         UnityEngine.Debug.Log(chain);

        chain += ";;";

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(chain);

        return stream.ToArray();
    }
}