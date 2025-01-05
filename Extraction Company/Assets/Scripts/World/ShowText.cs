using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowText : MonoBehaviour
{
    public GameObject text;
    TMP_Text text_Press;

    void Start()
    {
        text = GameObject.Find("Press_E");

        text_Press = text.GetComponent<TMP_Text>();

        text_Press.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if(text_Press == null) 
        {
            text = GameObject.Find("Press_E");

            text_Press = text.GetComponent<TMP_Text>();
        }
        text_Press.text = "Press 'E' to interact";
    }

    private void OnTriggerExit(Collider other)
    {
        if (text_Press == null)
        {
            text = GameObject.Find("Press_E");

            text_Press = text.GetComponent<TMP_Text>();
        }
        text_Press.text = "";
    }
}
