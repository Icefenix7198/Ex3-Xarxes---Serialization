using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatDisplacement : MonoBehaviour
{
    public GameObject chatObj;
    public TextMeshProUGUI chatText;
    string lastChatText;

    // Start is called before the first frame update
    void Start()
    {
        chatText = chatObj.GetComponent<TextMeshProUGUI>();
        lastChatText = chatText.text;
    }

    // Update is called once per frame
    void Update()
    {
        if (chatText.text != lastChatText)
        {
            chatObj.transform.position = new Vector3(chatObj.transform.position.x, chatObj.transform.position.y + 20f, chatObj.transform.position.z);
            lastChatText = chatText.text;
        }
    }
}
