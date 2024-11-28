using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Interaction : MonoBehaviour
{
    public bool button;

    // Start is called before the first frame update
    void Start()
    {
        button = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        Renderer rend = GetComponent<Renderer>();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(button == true)
            {
                rend.material.SetColor("_Color", Color.red);
                rend.material.SetColor("_EmissionColor", Color.red);
                button = false; 
            }

            else
            {
                rend.material.SetColor("_Color", Color.green);
                rend.material.SetColor("_EmissionColor", Color.green);
                button = true; 
            }
        }
    }
}
