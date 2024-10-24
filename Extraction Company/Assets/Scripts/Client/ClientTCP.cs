using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    public string clientText;
    public TMP_InputField ipAdress;
    public Socket server;

    int port = 9050;
    string address = "127.0.0.1";
    public string clientName = "DefaultClient";

    public GameObject connected;
    public GameObject noConnected;
    public GameObject ConnectedUDP;
    bool enableServer = false;

    public PassSceneManager passSceneManager;
    public TMP_InputField insertNameServer;

    // Start is called before the first frame update
    void Start()
    {
        if (UItextObj != null)
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();

        passSceneManager = GetComponent<PassSceneManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if (UItextObj != null)
            UItext.text = clientText;
    }

    public void StartClient()
    {
        clientName = insertNameServer.text;

        if (clientName == "")
        {
            clientName = "DefaultClient";
        }

        if (ipAdress.text != "")
        {
            connected.SetActive(true);
            noConnected.SetActive(false);

            Thread connect = new Thread(Connect);
            connect.Start();
        }
        else
        {
            ipAdress.text = "127.0.0.1";
            connected.SetActive(true);
            noConnected.SetActive(false);

            Thread connect = new Thread(Connect);
            connect.Start();
        }
    }
    void Connect()
    {
        //TO DO 2
        //Create the server endpoint so we can try to connect to it.
        //You'll need the server's IP and the port we binded it to before
        //Also, initialize our server socket.
        //When calling connect and succeeding, our server socket will create a
        //connection between this endpoint and the server's endpoint

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAdress.text), port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(ipep);


        //TO DO 4
        //With an established connection, we want to send a message so the server aacknowledges us
        //Start the Send Thread
        Thread sendThread = new Thread(Send);
        sendThread.Start();
        //TO DO 7

        //If the client wants to receive messages, it will have to start another thread. Call Receive()
        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }
    void Send()
    {
        //TO DO 4
        //Using the socket that stores the connection between the 2 endpoints, call the TCP send function with
        //an encoded message
        byte[] data;

        data = Encoding.ASCII.GetBytes(clientName);

        server.Send(data);
    }

    //TO DO 7
    //Similar to what we already did with the server, we have to call the Receive() method from the socket.
    void Receive()
    {
        while (true)
        {
            byte[] data = new byte[1024];
            int recv = 0;

            try
            {
                recv = server.Receive(data);
            }
            catch
            {
                //
            }

            if (recv != 0)
            {
                clientText = "\n" + Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log(clientText);
            }

            if (recv != 0 && passSceneManager.firstConnection)
            {
                Debug.Log("Pass Scene");
                passSceneManager.connected = true;
                passSceneManager.client = true;
                passSceneManager.clientTCP = true;
                passSceneManager.firstConnection = false;
            }
        }
    }

}
