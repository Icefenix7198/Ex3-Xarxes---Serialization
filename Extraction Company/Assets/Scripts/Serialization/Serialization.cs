using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Net;
using System.Net.Sockets;
using static PlayerManager;

public class Serialization : MonoBehaviour
{
    public enum ActionType
    {
        CREATE_PLAYER,
        MOVE,
        ID,
        SPAWN_PLAYERS
    }

    static MemoryStream stream;

    ClientUDP c_udp;
    public bool isC_udp;

    ServerUDP s_udp;
    public bool isS_udp;

    byte[] bytes;

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
        if (isC_udp)
        {
            if (playerManager == null && c_udp.passSceneManager.isConnected)
            {
                playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
            }
        }

        if (isS_udp)
        {
            if (playerManager == null && s_udp.passScene.isConnected)
            {
                playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
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

        Debug.Log("serialized!");
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

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);
    }
    
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

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, lastID);
    }

    public string serializeMovement(string ID, Vector3 movement)
    {
        string id = ID;
        ActionType type = ActionType.MOVE;
        float[] move = { movement.x, movement.y, movement.z };

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)type);
        writer.Write(ID);

        foreach (var i in move)
        {
            writer.Write(i);
        }

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);

        return id;
    }

    public void deserialize(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        ActionType action = (ActionType)reader.ReadInt32();

        switch (action)
        {
            case ActionType.ID:
                string ID0 = reader.ReadString();
                break;
            case ActionType.CREATE_PLAYER:
                string ID1 = reader.ReadString();
                playerManager.NewPlayer(ID1);
                break;
            case ActionType.MOVE:
                string ID2 = reader.ReadString();
                float[] moveList = new float[3];
                for (int i = 0; i < moveList.Length; i++)
                {
                    moveList[i] = reader.ReadSingle();
                }
                Vector3 movement = new Vector3(moveList[0], moveList[1], moveList[2]);
                playerManager.ClientMove(ID2, movement);
                break;
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

    public void SendToServer(byte[] message)
    {
        c_udp.server.SendTo(message, message.Length, SocketFlags.None, c_udp.ipepServer);
    }   
    
    public void SendToClient(byte[] message, string ID)
    {
        s_udp.Send(message, ID);
    }

    public string TakeID(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        ActionType action = (ActionType)reader.ReadInt32();
        string ID = reader.ReadString();

        Debug.Log("ID Taked!");

        return ID;
    }

    public ActionType TakeAction(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        ActionType action = (ActionType)reader.ReadInt32();
        string ID = reader.ReadString();

        Debug.Log("Action Taked!");

        return action;
    }
}