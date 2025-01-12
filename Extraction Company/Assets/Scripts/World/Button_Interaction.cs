using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Interaction : MonoBehaviour
{
    Door_Manager doorManager;
    Renderer rend;

    [SerializeField]
    public Serialization serialization;
    PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        doorManager = GameObject.Find("DoorManager").GetComponent<Door_Manager>();
        rend = GetComponent<Renderer>();

        if (serialization == null)
        {
            serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        }

        if (playerManager == null)
        {
            GameObject tmp = GameObject.Find("PlayerSpawner");

            if (tmp != null)
            {
                playerManager = tmp.GetComponent<PlayerManager>();
            }
        }

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

        if (this.CompareTag("GroupA") && doorManager.buttonA == true)
        {
            rend.material.SetColor("_Color", Color.green);
            rend.material.SetColor("_EmissionColor", Color.green);
        }

        if (this.CompareTag("GroupB") && doorManager.buttonB == true)
        {
            rend.material.SetColor("_Color", Color.green);
            rend.material.SetColor("_EmissionColor", Color.green);
        }

        if (this.CompareTag("GroupC") && doorManager.buttonC == true)
        {
            rend.material.SetColor("_Color", Color.green);
            rend.material.SetColor("_EmissionColor", Color.green);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            string door = "Close";

            if (other.GetComponent<PlayerMovement>().isActiveAndEnabled)
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
                        door = "A";
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
                        door = "B";
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
                        door = "C";
                    }
                }

                serialization.SendDoors(door, playerManager.player.ID);
                doorManager.ExecuteDoors();
            }
        }
    }
}