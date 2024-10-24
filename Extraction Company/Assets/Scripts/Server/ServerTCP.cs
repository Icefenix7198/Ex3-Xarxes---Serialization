using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ServerTCP : MonoBehaviour
{
    Socket socket;
    Thread mainThread = null;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    public string serverText;

    public PassSceneManager passScene;

    int port = 9050;
    string serverIP = "127.0.0.1";
    public string serverName = "DefaultName";
    byte[] ping;

    IPEndPoint ipep;

    public GameObject connected;
    public GameObject noConnected;
    public GameObject ConnectedUDP;
    bool enableServer = false;

    public User userSocket;
    List<User> userSocketsList;
    public TMP_InputField insertNameServer;

    public struct User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        userSocketsList = new List<User>();
        passScene = GetComponent<PassSceneManager>();

        if (UItextObj != null)
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();

        ipep = new IPEndPoint(IPAddress.Any, port);
    }


    void Update()
    {
        if (UItextObj != null)
            UItext.text = serverText;

        if (Input.GetKeyDown(KeyCode.T) && !enableServer && !ConnectedUDP.active)
        {
            startServer();
            enableServer = true;
        }
    }


    public void startServer()
    {
        serverName = insertNameServer.text;

        if (serverName == "")
        {
            serverName = "DefaultServer";
        }

        serverText = "Starting " + serverName + " TCP Server...";

        connected.SetActive(true);
        noConnected.SetActive(false);
        //TO DO 1
        //Create and bind the socket
        //Any IP that wants to connect to the port 9050 with TCP, will communicate with this socket
        //Don't forget to set the socket in listening mode

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipep);
        socket.Listen(10);

        //TO DO 3
        //TIme to check for connections, start a thread using CheckNewConnections
        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
    }

    void CheckNewConnections()
    {
        while(true)
        {
            User newUser = new User();
            newUser.name = "";
            //TO DO 3
            //TCP makes it so easy to manage conections, so we are going
            //to put it to use
            //Accept any incoming clients and store them in this user.
            //When accepting, we can now store a copy of our server socket
            //who has established a communication between a
            //local endpoint (server) and the remote endpoint(client)
            //If you want to check their ports and adresses, you can acces
            //the socket's RemoteEndpoint and LocalEndPoint
            //try printing them on the console

            newUser.socket = socket.Accept();//accept the socket
            IPEndPoint clientep = (IPEndPoint)socket.LocalEndPoint;
            serverText = serverText + "\n"+ "Connected with " + clientep.Address.ToString() + " at port " + clientep.Port.ToString();

            userSocketsList.Add(newUser);

           //TO DO 5
           //For every client, we call a new thread to receive their messages. 
           //Here we have to send our user as a parameter so we can use it's socket.
           Thread newConnection = new Thread(() => Receive(newUser));
           newConnection.Start();
        }
        //This users could be stored in the future on a list
        //in case you want to manage your connections

    }

    void Receive(User user) //ENVIAR A TODOS LOS SOCKET!
    {
        //TO DO 5
        //Create an infinite loop to start receiving messages for this user
        //You'll have to use the socket function receive to be able to get them.
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            data = new byte[1024];
            recv = user.socket.Receive(data);

            if (recv == 0)
                break;
            else
            {
                if (Encoding.ASCII.GetString(data, 0, recv) != "PING" && serverText != Encoding.ASCII.GetString(data, 0, recv))
                {
                    serverText = "\n" + Encoding.ASCII.GetString(data, 0, recv);
                }
            }

            string message = serverText;
            Debug.Log(message);

            //TO DO 6
            //We'll send a ping back every time a message is received
            //Start another thread to send a message, same parameters as this one.
            Thread answer = new Thread(() => Send(user, message));
            answer.Start();
        }
    }

    //TO DO 6
    //Now, we'll use this user socket to send a "ping".
    //Just call the socket's send function and encode the string.
    public void Send(User user, string data = "PING")
    {
        foreach (var userSocket in userSocketsList)
        {
            if (passScene.firstConnection)
            {
                data += "\n" + "Server: " + serverName;
            }

            ping = Encoding.ASCII.GetBytes(data);

            if (ping != null)
            {
                userSocket.socket.Send(ping);
            }

            if (passScene != null && passScene.firstConnection)
            {
                Debug.Log("PassScene");
                passScene.connected = true;
                passScene.server = true;
                passScene.serverTCP = true;
                passScene.firstConnection = false;
            }
        }

        serverText = data;
    }
}
