using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chat;
    public TextMeshProUGUI chatText;
    public PassSceneManager passSceneManager;

    public ServerTCP s_tcp;
    public ServerUDP s_udp;

    public ClientTCP c_tcp;
    public ClientUDP c_udp;

    // Start is called before the first frame update
    void Start()
    {
        passSceneManager = GetComponent<PassSceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (chat != null && chatText != null)
        //{
        //    if (passSceneManager.server)
        //    {
        //        if (passSceneManager.serverTCP)
        //        {
        //            chatText.text = s_tcp.serverText;
        //        }
        //        else if (passSceneManager.serverUDP)
        //        {
        //            chatText.text = s_udp.serverText;
        //        }
        //    }
        //    else if (passSceneManager.client)
        //    {
        //        if (passSceneManager.clientTCP)
        //        {
        //            chatText.text = c_tcp.clientText;
        //        }
        //        else if (passSceneManager.clientUDP)
        //        {
        //            chatText.text = c_udp.clientText;
        //        }
        //    }

        //    if (Input.GetKeyDown(KeyCode.Return) && chat.text != "")
        //    {
        //        if (passSceneManager.server)
        //        {
        //            if (passSceneManager.serverTCP)
        //            {
        //                s_tcp.Send(s_tcp.userSocket, s_tcp.serverText + "\n" + s_tcp.serverName + ": " + chat.text);
        //            }
        //            else if (passSceneManager.serverUDP)
        //            {
        //                s_udp.Send(s_udp.serverText + "\n" + s_udp.serverName + ": " + chat.text);
        //            }
        //        }

        //        if (passSceneManager.client)
        //        {
        //            if (passSceneManager.clientTCP)
        //            {
        //                c_tcp.server.Send(Encoding.ASCII.GetBytes(c_tcp.clientText + "\n" + c_tcp.clientName + ": " + chat.text));
        //            }
        //            else if (passSceneManager.clientUDP)
        //            {
        //                c_udp.server.SendTo(Encoding.ASCII.GetBytes(c_udp.clientText + "\n" + c_udp.clientName + ": " + chat.text), Encoding.ASCII.GetBytes(c_udp.clientText + "\n" + c_udp.clientName + ": " + chat.text).Length, SocketFlags.None, c_udp.ipepServer);
        //            }
        //        }
        //    }
        //}
        //else if (passSceneManager.isConnected)
        //{
        //    GameObject chatObj = GameObject.Find("Chat");
        //    GameObject textObj = GameObject.Find("TextChat");

        //    chat = chatObj.GetComponent<TMP_InputField>();
        //    chatText = textObj.GetComponent<TextMeshProUGUI>();
        //}
    }
}
