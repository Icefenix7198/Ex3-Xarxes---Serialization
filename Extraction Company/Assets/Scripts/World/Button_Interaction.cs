using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Interaction : MonoBehaviour
{
    Door_Manager doorManager;
    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        doorManager = GameObject.Find("DoorManager").GetComponent<Door_Manager>();
        rend = GetComponent<Renderer>();

        if (this.CompareTag("GroupA"))
        {
            if (doorManager.buttonA == false)
            {
                rend.material.SetColor("_Color", Color.red);
                rend.material.SetColor("_EmissionColor", Color.red);
            }

            else
            {
                rend.material.SetColor("_Color", Color.green);
                rend.material.SetColor("_EmissionColor", Color.green);
            }
        }

        if (this.CompareTag("GroupB"))
        {
            if (doorManager.buttonB == false)
            {
                rend.material.SetColor("_Color", Color.red);
                rend.material.SetColor("_EmissionColor", Color.red);
            }

            else
            {
                rend.material.SetColor("_Color", Color.green);
                rend.material.SetColor("_EmissionColor", Color.green);
            }
        }

        if (this.CompareTag("GroupC"))
        {
            if (doorManager.buttonC == false)
            {
                rend.material.SetColor("_Color", Color.red);
                rend.material.SetColor("_EmissionColor", Color.red);
            }

            else
            {
                rend.material.SetColor("_Color", Color.green);
                rend.material.SetColor("_EmissionColor", Color.green);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.CompareTag("GroupA") && doorManager.buttonA == false)
        {
            rend.material.SetColor("_Color", Color.red);
            rend.material.SetColor("_EmissionColor", Color.red);
        }

        if (this.CompareTag("GroupB") && doorManager.buttonB == false)
        {
            rend.material.SetColor("_Color", Color.red);
            rend.material.SetColor("_EmissionColor", Color.red);
        }

        if (this.CompareTag("GroupC") && doorManager.buttonC == false)
        {
            rend.material.SetColor("_Color", Color.red);
            rend.material.SetColor("_EmissionColor", Color.red);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (this.CompareTag("GroupA"))
            {
                if (doorManager.buttonA == true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    doorManager.buttonA = false;
                }
                else if (doorManager.buttonA == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    doorManager.buttonA = true;
                    doorManager.buttonB = false;
                    doorManager.buttonC = false;
                }
            }

            if (this.CompareTag("GroupB"))
            {
                if (doorManager.buttonB == true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    doorManager.buttonB = false;

                }
                else if (doorManager.buttonB == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    doorManager.buttonB = true;
                    doorManager.buttonA = false;
                    doorManager.buttonC = false;
                }
            }

            if (this.CompareTag("GroupC"))
            {
                if (doorManager.buttonC == true)
                {
                    rend.material.SetColor("_Color", Color.red);
                    rend.material.SetColor("_EmissionColor", Color.red);
                    doorManager.buttonC = false;

                }
                else if (doorManager.buttonC == false)
                {
                    rend.material.SetColor("_Color", Color.green);
                    rend.material.SetColor("_EmissionColor", Color.green);
                    doorManager.buttonC = true;
                    doorManager.buttonA = false;
                    doorManager.buttonB = false;
                }
            }
        }


        for (int i = 0; i < doorManager.animator.Count; i++)
        {

            if (doorManager.animator[i].CompareTag("GroupA"))
            {
                doorManager.animator[i].SetBool("Button", doorManager.buttonA);
            }

            if (doorManager.animator[i].CompareTag("GroupB"))
            {
                doorManager.animator[i].SetBool("Button", doorManager.buttonB);
            }

            if (doorManager.animator[i].CompareTag("GroupC"))
            {
                doorManager.animator[i].SetBool("Button", doorManager.buttonC);
            }

        }
    }
}
