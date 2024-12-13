using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Interaction : MonoBehaviour
{
    public bool button;
    public List<Animator> animator;

    public bool buttonA;
    public bool buttonB;
    public bool buttonC;

    // Start is called before the first frame update
    void Start()
    {
        button = false;
        buttonA = false;
        buttonB = false;
        buttonC = false;
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
            if(this.CompareTag("GroupA"))
            {
                if (buttonA == true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    buttonA = false;

                }
                else if (buttonA == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    buttonA = true;
                }
            }

            if (this.CompareTag("GroupB"))
            {
                if (buttonB== true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    buttonB = false;

                }
                else if (buttonB == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    buttonB = true;
                }
            }

            if (this.CompareTag("GroupC"))
            {
                if (buttonC == true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    buttonC = false;

                }
                else if (buttonC == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    buttonC = true;
                }
            }


            for (int i = 0; i < animator.Count; i++) {

                if (animator[i].CompareTag("GroupA")) { 
                    animator[i].SetBool("Button", buttonA); 
                }

                else
                {
                    animator[i].SetBool("Button", !buttonA); 
                }

                if (animator[i].CompareTag("GroupB"))
                {
                    animator[i].SetBool("Button", buttonB);
                }

                else
                {
                    animator[i].SetBool("Button", !buttonB);
                }

                if (animator[i].CompareTag("GroupC"))
                {
                    animator[i].SetBool("Button", buttonC);
                }

                else
                {
                    animator[i].SetBool("Button", !buttonC);
                }

            }
        }
    }
}
