using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Generic;
using System;
using static Serialization;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    public string serverText;

    public PassSceneManager passScene;

    int port = 9050;
    public string serverName = "DefaultName";

    public GameObject connected;
    public GameObject noConnected;

    public Socket userSocket;
    public EndPoint userRemote;

    public List<UserUDP> userSocketsList;
    public TMP_InputField insertNameServer;
    Serialization serialization;

    List<string> clientsIdList;

    bool deserializate;
    byte[] tempData;

    public struct UserUDP
    {
        public EndPoint Remote;
        public Socket socket;

        public string NetID;
    }

    void Start()
    {
        userSocketsList = new List<UserUDP>();
        clientsIdList = new List<String>();
        passScene = GetComponent<PassSceneManager>();

        if(UItextObj != null)
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();

        DontDestroyOnLoad(this.gameObject);
        serialization = GetComponent<Serialization>();

        startServer();
    }
    public void startServer()
    {
        serverName = insertNameServer.text;

        if (serverName == "")
        {
            serverName = "DefaultServer";
        }

        serverText = "Starting " + serverName + " UDP Server...";

        connected.SetActive(true);
        noConnected.SetActive(false);

        ////TO DO 1
        ////UDP doesn't keep track of our connections like TCP
        ////This means that we "can only" reply to other endpoints,
        ////since we don't know where or who they are
        ////We want any UDP connection that wants to communicate with 9050 port to send it to our socket.
        ////So as with TCP, we create a socket and bind it to the 9050 port. 

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        userSocket = socket;

        ////TO DO 3
        ////Our client is sending a handshake, the server has to be able to recieve it
        ////It's time to call the Receive thread
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Update()
    {
        if (UItextObj != null)
            UItext.text = serverText;

        if (deserializate)
        {
            serialization.Deserialize(tempData);
            deserializate = false;
        }
    }
 
    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];

        serverText = serverText + "\n" + "Waiting for new Client...";

        //Loop the whole process, and start receiveing messages directed to our socket
        //(the one we binded to a port before)
        //When using socket.ReceiveFrom, be sure send our remote as a reference so we can keep
        //this adress (the client) and reply to it on TO DO 4

        //TO DO 3
        //We don't know who may be comunicating with this server, so we have to create an
        //endpoint with any address and an IpEndpoint from it to reply to it later.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, port);
        EndPoint Remote = (EndPoint)(sender);
        userRemote = Remote;

        while (true)
        {
            recv = socket.ReceiveFrom(data, ref Remote);

            if (recv == 0)
            {
                break;
            }

            //if (Encoding.ASCII.GetString(data, 0, recv) != "PING" && serverText != Encoding.ASCII.GetString(data, 0, recv))
            //{
            //    serverText = "\n" + Encoding.ASCII.GetString(data, 0, recv);
            //}

             UnityEngine.Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

            UserUDP u = new UserUDP();
            u.socket = socket;
            u.Remote = Remote;

            string id;
            id = serialization.ExtractID(data);
            u.NetID = id;

            if (!userSocketsList.Contains(u))
            {
                clientsIdList.Add(u.NetID);
                userSocketsList.Add(u);
            }

             UnityEngine.Debug.Log(serverText);

            //TO DO 4
            //When our UDP server receives a message from a random remote, it has to send a ping,
            //Call a send thread
            Thread newConnection = new Thread(() => Send(data, u.NetID));
            newConnection.Start();
        }
    }

    public void Send(byte[] data, string ID = "-1")
    {
        foreach (var scoketsUser in userSocketsList) //We send the data to each client that collected to the sever
        {
            //TO DO 4
            //Use socket.SendTo to send a ping using the remote we stored earlier.
            //byte[] data = new byte[1024];

            //if (passScene.firstConnection == true)
            //{
            //    message = "\n" + "Server: " + serverName;
            //}

            ActionType action =  serialization.ExtractAction(data);


            if (action == ActionType.ID || action == ActionType.SPAWN_PLAYERS)
            {
                scoketsUser.socket.SendTo(data, data.Length, SocketFlags.None, scoketsUser.Remote);
            }
            else if (action == ActionType.CREATE_PLAYER)
            {
                deserializate = true;
                tempData = data;
            }
            else
            {
                if (scoketsUser.NetID != ID) //Este es para mandar a todo el mundo menos él
                {
                    scoketsUser.socket.SendTo(data, data.Length, SocketFlags.None, scoketsUser.Remote);
                }
            }

            if (passScene != null && passScene.firstConnection)
            {
                 UnityEngine.Debug.Log("PassScene");
                passScene.connected = true;
                passScene.server = true;
                passScene.serverUDP = true;
                passScene.firstConnection = false;
            }
        }
    }

    private void OnApplicationQuit()
    {
        foreach(var socketsUser in userSocketsList)
        {
            socketsUser.socket.Close();
        }
    }
}
