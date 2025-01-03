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
    public struct Message
    {
       public string id;
       public string clientID;
       public byte[] data;
       public ActionType action;
       public int order;
    }

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

    [SerializeField]
    public Serialization serialization;

    List<string> clientsIdList;
    List<Message> messagesToSent;

    Dictionary<string, List<Message>> messages;
    public Dictionary<string, int> messageToSentNow;

    bool deserializate;
    byte[] tempData;
    
    byte[] tempDataRequest;

    public List<Transform> spawns;

    public struct UserUDP
    {
        public EndPoint Remote;
        public Socket socket;
        public string name;

        public string NetID;
    }

    void Start()
    {
        userSocketsList = new List<UserUDP>();
        clientsIdList = new List<String>();
        messagesToSent = new List<Message>();
        messages = new Dictionary<string, List<Message>>();
        messageToSentNow = new Dictionary<string, int>();
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

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        userSocket = socket;

        Thread newConnection = new Thread(Receive);
        newConnection.Start();

        Thread sendMessages = new Thread(SendMessages);
        sendMessages.Start();
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

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, port);
        EndPoint Remote = (EndPoint)(sender);
        userRemote = Remote;

        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref Remote);

                if (recv == 0)
                {
                    break;
                }

                UserUDP u = new UserUDP();
                u.socket = socket;
                u.Remote = Remote;

                byte[] ogData = data; //Will quit Ack layer
                byte[] ogData3 = data; //Will have ackolegment header

                ogData = serialization.QuitACK(ogData); //Quit layer to ogData

                byte[] ogData1 = ogData;
                byte[] ogData2 = ogData;
                byte[] ogData4 = ogData;

                ActionType action = serialization.ExtractAction(ogData1);

                string clientID = serialization.ExtractID(ogData1);

                string id;
                id = clientID;
                u.NetID = id;

                lock (userSocketsList) //ID -2 means that the message is not send to any player and is for the server.
                {
                    if (!userSocketsList.Contains(u) && id != "-2" && action == ActionType.ID_NAME) //Check if player already exist, if type ID = -2 and if it set name
                    {
                        string name;
                        name = serialization.ExtractName(ogData2);
                        u.name = name;

                        clientsIdList.Add(u.NetID);
                        userSocketsList.Add(u);

                        //If a player over the for allowed tries to connect
                        if (userSocketsList.Count > 4)
                        {
                            serialization.MaxPlayers(u.NetID);

                            clientsIdList.Remove(u.NetID);
                            userSocketsList.Remove(u);
                        }
                    }
                }

                serialization.SendAMessage(ogData3, ogData, u);
            }
            catch
            {

            }
        }
    }

    void MessageSender(byte[] ogData4, string clientID) //Here the proccess of sendig starts
    {
        byte[] ogData = ogData4;
        byte[] ogData1 = ogData4;
        byte[] ogData2 = ogData4;

        ActionType action = serialization.ExtractAction(ogData1);

        if (action == ActionType.DOORS || action == ActionType.REQUEST_ITEMS || action == ActionType.DESTROY_ITEM || action == ActionType.EXTRACTION_TO_SERVER || action == ActionType.REQUEST_MONSTERS) //This is for messages that only need info from the server as an awnser, and not send it to other people
        {
            //Debug.Log("TEMPORAL! Entro en el if de serverUDP");
            serialization.Deserialize(ogData2);
        }
        else
        {
            Thread newConnection = new Thread(() => Send(ogData, clientID));
            newConnection.Start();
        }
    }

    public void Send(byte[] data, string ID = "-1")
    {
        lock (userSocketsList)
        {
            foreach (var scoketsUser in userSocketsList) //We send the data to each client that collected to the sever
            {
                byte[] ogData = data;
                byte[] ogData1 = data;

                ActionType action = serialization.ExtractAction(data);

                if (action == ActionType.SPAWN_PLAYERS) //This case is send to EVERYONE, for specific things.
                {
                    scoketsUser.socket.SendTo(ogData1, ogData1.Length, SocketFlags.None, scoketsUser.Remote);
                }
                else if (action == ActionType.CREATE_PLAYER || action == ActionType.MOVE_SERVER) //This is very specific for creating the player due to the server needing to also save the data
                {
                    if(userSocketsList.Count <= 4)
                    {
                        deserializate = true;
                        tempData = ogData;
                    }
                }
                else if (action == ActionType.ID_NAME || action == ActionType.SPAWN_ITEMS || action == ActionType.CREATE_MONSTER || action == ActionType.MAX_PLAYERS || action == ActionType.UPDATE_MONSTER || action == ActionType.ACK) //This is only to send to the client with the ID
                {
                    if (scoketsUser.NetID == ID)
                    {
                        scoketsUser.socket.SendTo(ogData1, ogData1.Length, SocketFlags.None, scoketsUser.Remote);
                    }
                }
                else
                {
                    if (scoketsUser.NetID != ID) //This is to send everyone excluding the original sender (example the movement action)
                    {
                        scoketsUser.socket.SendTo(ogData1, ogData1.Length, SocketFlags.None, scoketsUser.Remote);
                    }
                }

                if (passScene != null && passScene.firstConnection)
                {
                    passScene.connected = true;
                    passScene.server = true;
                    passScene.serverUDP = true;
                    passScene.firstConnection = false;
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        foreach(var socketsUser in userSocketsList)
        {
            socketsUser.socket.Close();
        }

        socket.Close();
    }

    public void SaveMessages(Message message) //Save the messages to wait in order of execution (drop one if has passed to much time)
    {
        Message m = message;

        if (messages.ContainsKey(message.clientID))
        {
            //int counter = messageToSentNow[message.clientID];
            //counter++;

            messages[message.clientID].Add(m);
            //messageToSentNow[message.clientID] = counter;
        }
        else
        {
            List<Message> mList = new List<Message>();
            mList.Add(m);

            messages.Add(message.clientID, mList);
            messageToSentNow.Add(message.clientID, 0);
        }
    }

    void SendMessages() //Send the messages of the list MessagesToSent
    {
        while (true)
        {
            if (userSocketsList.Count > 0)
            {
                lock (userSocketsList) 
                {
                    foreach (UserUDP u in userSocketsList)
                    {
                        if (u.NetID != null && messages != null)
                        {
                            if (messages.Count > 0)
                            {
                                if (messages.ContainsKey(u.NetID))
                                {
                                    List<Message> mList = new List<Message>();

                                    if (messages.TryGetValue(u.NetID, out mList))
                                    {
                                        for (int i = 0; i < mList.Count; i++)
                                        {
                                            if (mList[i].order == messageToSentNow[u.NetID])
                                            {
                                                messagesToSent.Add(mList[i]);
                                                mList.RemoveAt(i);

                                                //for (int j = 0; j < mList.Count; j++)
                                                //{
                                                //    Message mLess = mList[j];
                                                //    mLess.order--;
                                                //    mList[j] = mLess;
                                                //}

                                                messages[u.NetID] = mList;
                                                messageToSentNow[u.NetID]++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            lock (messagesToSent)
            {
                if (messagesToSent.Count > 0)
                {
                    foreach (var message in messagesToSent)
                    {
                        MessageSender(message.data, message.clientID);
                    }

                    messagesToSent.Clear();
                }
            }
        }
    }
}
