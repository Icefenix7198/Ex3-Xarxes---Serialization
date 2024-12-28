using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using static Serialization;
using System;
using System.Collections.Generic;

public class ClientUDP : MonoBehaviour
{
    public Socket server;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    public TMP_InputField ipAdress;
    public string clientText;

    public int port = 9050;
    public string clientName = "DefaultClient";
    public string address = "127.0.0.1";
    public string clientID;

    bool deserializate;
    byte[] tempData; 

    public GameObject connected;
    public GameObject noConnected;

    [SerializeField]
    public PassSceneManager passSceneManager;

    public TMP_InputField insertNameClient;
    public IPEndPoint ipepServer; 
    Serialization serialization;

    public bool jitter = true;
    public bool packetLoss = true;
    public int minJitt = 0;
    public int maxJitt = 800;
    public int lossThreshold = 90;
    public struct Message
    {
        public Byte[] message;
        public DateTime time;
        public UInt32 id;
        public IPEndPoint ip;

    }

    public List<Message> messageBuffer = new List<Message>();

    // Start is called before the first frame update
    void Start()
    {
        if (UItextObj != null)
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();

        passSceneManager = GetComponent<PassSceneManager>();
        serialization = GetComponent<Serialization>();
        deserializate = false;
    }
    public void StartClient()
    {
        clientName = insertNameClient.text;

        if (clientName == "")
        {
            clientName = "DefaultClient";
        }

        if (ipAdress.text != "")
        {
            connected.SetActive(true);
            noConnected.SetActive(false);

            Thread mainThread = new Thread(Send);
            Thread sendThread = new Thread(sendMessages);
            mainThread.Start();
            sendThread.Start();
        }
        else
        {
            ipAdress.text = "127.0.0.1";
            connected.SetActive(true);
            noConnected.SetActive(false);

            Thread mainThread = new Thread(Send);
            mainThread.Start();
        }

        address = ipAdress.text;
    }

    void Update()
    {
        if (UItextObj != null)
            UItext.text = clientText;

        if (deserializate)
        {
            serialization.Deserialize(tempData);
            deserializate = false;
        }
    }

    public void Send()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAdress.text), port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipepServer = ipep;

        byte[] data = new byte[1024];

        string handshake = "HANDSHAKE";
        data = Encoding.ASCII.GetBytes(handshake);

        if (!passSceneManager.isConnected)
        {
            string id = System.Guid.NewGuid().ToString();
            clientID = id;
            serialization.serializeIDandName(id, clientName);
        }
        Thread receive = new Thread(Receive);
        receive.Start();
    }

    void Receive()
    {
        while (true)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ipAdress.text), port);
            EndPoint Remote = (EndPoint)(sender);
            byte[] data = new byte[1024];
            int recv = 0;

            try
            {
                recv = server.ReceiveFrom(data, ref Remote);
            }
            catch
            {
                //If there is no message to recive do nothing instead of break
            }

            ActionType action = ActionType.NONE;

            if (recv != 0)
            {
                deserializate = true;
                tempData = data;

               action = serialization.ExtractAction(data);
            }

            if (recv != 0 && passSceneManager.firstConnection && action != ActionType.MAX_PLAYERS)
            {
                passSceneManager.connected = true;
                passSceneManager.client = true;
                passSceneManager.clientUDP = true;
                passSceneManager.firstConnection = false;
            }
        }
    }

    public void sendMessage(Byte[] text, IPEndPoint ip)
    {
        System.Random r = new System.Random();
        if (((r.Next(0, 100) > lossThreshold) && packetLoss) || !packetLoss) // Don't schedule the message with certain probability
        {
            Message m = new Message();
            m.message = text;
            if (jitter)
            {
                m.time = DateTime.Now.AddMilliseconds(r.Next(minJitt, maxJitt)); // delay the message sending according to parameters
            }
            else
            {
                m.time = DateTime.Now;
            }
            m.id = 0;
            m.ip = ip;
            lock (messageBuffer)
            {
                messageBuffer.Add(m);
            }
            Debug.Log(m.time.ToString());
        }

    }
    //Run this always in a separate Thread, to send the delayed messages
    void sendMessages()
    {
        Debug.Log("really sending..");
        while (true)
        {
            DateTime d = DateTime.Now;
            int i = 0;
            if (messageBuffer.Count > 0)
            {
                List<Message> auxBuffer;
                lock (messageBuffer)
                {
                    auxBuffer = new List<Message>(messageBuffer);
                }
                foreach (var m in auxBuffer)
                {
                    if (m.time < d)
                    {
                        server.SendTo(m.message, m.message.Length, SocketFlags.None, m.ip);
                        lock (messageBuffer)
                        {
                            messageBuffer.RemoveAt(i);
                        }
                        i--;
                        string myLog = Encoding.ASCII.GetString(m.message, 0, m.message.Length);
                        //Debug.Log("message sent!");
                    }
                    i++;
                }
            }
        }
    }
}

