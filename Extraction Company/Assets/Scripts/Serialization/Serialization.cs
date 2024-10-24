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
        MOVE
    }

    static MemoryStream stream;
    public ClientUDP c_udp;
    public ServerUDP s_udp;

    byte[] bytes;

    public void serializeCreatePlayer(int ID, ActionType action)
    {
        int id = ID;
        ActionType type = action;

        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ID);
        writer.Write((int)type);

        Debug.Log("serialized!");
        bytes = stream.ToArray();

        Send(bytes, id);
    }

    public void serializeMovement(int ID, ActionType action, Vector3 movement)
    {
        int id = ID;
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
    }

    public void deserialize(byte[] message)
    {
        stream = new MemoryStream();
        stream.Write(message, 0, message.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        int ID = reader.ReadInt32();
        Debug.Log("ID " + ID.ToString());

        ActionType action = (ActionType)reader.ReadInt32();
        Debug.Log((int)action);

        switch (action)
        {
            case ActionType.CREATE_PLAYER:
                //Llamar función CreatePlayer(int ID)
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

        private void Send(byte[] message, int id)
    {
        if (c_udp != null)
        {
            SendToServer(bytes);
        }

        if (s_udp != null)
        {
            SendToClient(bytes, id);
        }
    }

    public void SendToServer(byte[] message)
    {
        c_udp.server.SendTo(message, message.Length, SocketFlags.None, c_udp.ipepServer);
    }   
    
    public void SendToClient(byte[] message, int ID)
    {
        s_udp.Send(message, ID);
    }
}