using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using static Serialization;
using System;

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

            if (recv != 0)
            {
                deserializate = true;
                tempData = data;
            }

            if (recv != 0 && passSceneManager.firstConnection)
            {
                passSceneManager.connected = true;
                passSceneManager.client = true;
                passSceneManager.clientUDP = true;
                passSceneManager.firstConnection = false;
            }
        }
    }
}

