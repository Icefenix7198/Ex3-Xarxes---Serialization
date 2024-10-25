using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Serialization : MonoBehaviour
{
    public enum ActionType
    {
        CREATE_PLAYER,
        MOVE,
        ID
    }

    static MemoryStream stream;

    ClientUDP c_udp;
    public bool isC_udp;

    ServerUDP s_udp;
    public bool isS_udp;

    byte[] bytes;

    //Scripts
    CreatePlayer createPlayer;

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
            if (createPlayer == null && c_udp.passSceneManager.isConnected)
            {
                createPlayer = GameObject.Find("PlayerSpawner").GetComponent<CreatePlayer>();
            }
        }
    }

    public void serializeID(string id)
    {
        ActionType type = ActionType.ID;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(id);
        writer.Write((int)type);

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);
    }

    public void serializeCreatePlayer(string id)
    {
        ActionType type = ActionType.CREATE_PLAYER;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(id);
        writer.Write((int)type);

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);
    }

    public string serializeMovement(string ID, ActionType action, Vector3 movement)
    {
        string id = ID;
        ActionType type = action;
        float[] move = { movement.x, movement.y, movement.z };

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ID);
        writer.Write((int)type);

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

        string ID = reader.ReadString();
        Debug.Log("ID " + ID);

        ActionType action = (ActionType)reader.ReadInt32();
        Debug.Log((int)action);

        switch (action)
        {
            case ActionType.CREATE_PLAYER:
                createPlayer.NewPlayer();
                break;
            case ActionType.MOVE:
                float[] moveList = new float[3];
                for (int i = 0; i < moveList.Length; i++)
                {
                    moveList[i] = reader.ReadInt32();
                }
                Vector3 movement = new Vector3(moveList[0], moveList[1], moveList[2]);
                //Llamar función MovePlayer(int ID, Vector3 movement)
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

        string ID = reader.ReadString();
        ActionType action = (ActionType)reader.ReadInt32();

        Debug.Log("Action Taked!");

        return action;
    }
}