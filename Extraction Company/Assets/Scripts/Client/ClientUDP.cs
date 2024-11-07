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
        ////TO DO 2
        ////Unlike with TCP, we don't "connect" first,
        ////we are going to send a message to establish our communication so we need an endpoint
        ////We need the server's IP and the port we've binded it to before
        ////Again, initialize the socket
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAdress.text), port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipepServer = ipep;

        ////TO DO 2.1 
        ////Send the Handshake to the server's endpoint.
        ////This time, our UDP socket doesn't have it, so we have to pass it
        ////as a parameter on it's SendTo() method

        byte[] data = new byte[1024];

        string handshake = "HANDSHAKE";
        data = Encoding.ASCII.GetBytes(handshake);

        if (!passSceneManager.isConnected)
        {
            string id = System.Guid.NewGuid().ToString();
            clientID = id;
            serialization.serializeIDandName(id, clientName);
        }

        //server.SendTo(data, data.Length, SocketFlags.None, ipep);

        ////TO DO 5
        ////We'll wait for a server response,
        ////so you can already start the receive thread
        Thread receive = new Thread(Receive);
        receive.Start();
    }

    //TO DO 5
    //Same as in the server, in this case the remote is a bit useless
    //since we already know it's the server who's communicating with us
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

                if (passSceneManager.isConnected == false)
                {
                    //clientText = ("Message received from {0}: " + Remote.ToString());
                }

                //clientText = "\n" + Encoding.ASCII.GetString(data, 0, recv);
                // UnityEngine.Debug.Log(clientText);
            }

            if (recv != 0 && passSceneManager.firstConnection)
            {
                 UnityEngine.Debug.Log("Pass Scene");
                passSceneManager.connected = true;
                passSceneManager.client = true;
                passSceneManager.clientUDP = true;
                passSceneManager.firstConnection = false;

                //Hacer CreatePlayer!
            }
        }
    }

    //private void OnApplicationQuit()
    //{
    //    server.Close();
    //}
}

