using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowText : MonoBehaviour
{
    public GameObject text;
    void Start()
    {
       text.SetActive(false);
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        text.SetActive(true); 
    }

    private void OnTriggerExit(Collider other)
    {
        text.SetActive(false);
    }
}
