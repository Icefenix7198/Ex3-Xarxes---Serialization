using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using static Serialization;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using Unity.VisualScripting;

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
        public int order;
        public ActionType action;
    }

    public struct AckMessage
    {
        public Byte[] message;
        public float time;
        public string id;
        public string clientId;
        public ActionType action;
        public bool waitForAck;
        public int order;
    }

    public List<Message> messageBuffer = new List<Message>();
    public List<AckMessage> ack_messageBuffer = new List<AckMessage>();

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
            mainThread.Start();

            Thread sendThread = new Thread(sendMessages);
            sendThread.Start();
        }
        else
        {
            ipAdress.text = "127.0.0.1";
            connected.SetActive(true);
            noConnected.SetActive(false);

            Thread mainThread = new Thread(Send);
            mainThread.Start();

            Thread sendThread = new Thread(sendMessages);
            sendThread.Start();
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

        if(ack_messageBuffer.Count > 0)
        {
            AckMessagesCheck();
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

                if(action == ActionType.ACK)
                {
                    serialization.Deserialize(tempData);
                }
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

    public void sendMessageACK(byte[] text, IPEndPoint ip)
    {
        AckMessage am = new AckMessage();
        byte[] data = text;

        am.id = System.Guid.NewGuid().ToString();
        am.clientId = clientID;
        am.message = text;
        am.time = 0;
        am.order = 0;

        ActionType action = ActionType.NONE;
        action = serialization.ExtractAction(data);

        am.action = action;

        lock (ack_messageBuffer)
        {
            am.order = ack_messageBuffer.Count;

            am.message = serialization.SendAckMessage(text, am.id, am.clientId, am.order);

            ack_messageBuffer.Add(am);
        }
    }

    public void sendMessage(Byte[] text, IPEndPoint ip, int orderAck, ActionType actionAck)
    {
        System.Random r = new System.Random();

        if (((r.Next(0, 100) > lossThreshold) && packetLoss) || !packetLoss) // Don't schedule the message with certain probability
        {
            Message m = new Message();

            m.message = text;
            m.order = orderAck;
            m.action = actionAck;

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
            System.Random r = new System.Random();

            DateTime d = DateTime.Now;
            int i = 0;
            if (messageBuffer.Count > 0)
            {
                List<Message> auxBuffer;
                lock (messageBuffer)
                {
                    auxBuffer = new List<Message>(messageBuffer);
                }
                for (int h = 0; h < messageBuffer.Count; h++)
                {

                    if (messageBuffer[h].time < d)
                    { 
                       server.SendTo(messageBuffer[h].message, messageBuffer[h].message.Length, SocketFlags.None, messageBuffer[h].ip);

                       lock (messageBuffer)
                       {
                           for (int j = 0; j < ack_messageBuffer.Count; j++)
                           {
                               if (ack_messageBuffer[j].message == messageBuffer[i].message)
                               {
                                   AckMessage message = ack_messageBuffer[j];
                                   message.waitForAck = true;

                                   ack_messageBuffer[j] = message;
                               }
                           }

                           messageBuffer.RemoveAt(i);
                       }
                       i--;
                            //string myLog = Encoding.ASCII.GetString(messageBuffer[h].message, 0, messageBuffer[h].message.Length);
                            //Debug.Log("message sent!");
                    }
                    i++;
                }
            }
        }
    }

    void AckMessagesCheck()
    {
        for (int i = 0; i < ack_messageBuffer.Count; i++)
        {
            AckMessage ackMessage = ack_messageBuffer[i];
            ackMessage.time += Time.deltaTime;

            bool alreadyInList = false;

            for (int j = 0; j < messageBuffer.Count; j++)
            {
                if (ack_messageBuffer[i].message == messageBuffer[j].message)
                {
                    alreadyInList = true;
                }
            }

            if (ackMessage.time > 0.1f && !alreadyInList && !ackMessage.waitForAck)
            {
                sendMessage(ackMessage.message, ipepServer, ackMessage.order, ackMessage.action);
                ackMessage.time = 0;
            }

            if (ackMessage.waitForAck && ackMessage.time > 1f) 
            {
                ackMessage.waitForAck = false;
            }

            ack_messageBuffer[i] = ackMessage;
        }
    }

    public void ReciveAck(string id)
    {
        for (int i = 0; i < ack_messageBuffer.Count; i++)
        {
            if (ack_messageBuffer[i].id == id)
            {
                ack_messageBuffer.RemoveAt(i);
            }
        }
    }
}

