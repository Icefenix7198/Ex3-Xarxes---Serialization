using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Manager : MonoBehaviour
{
    public List<GameObject> DoorsA;
    public List<GameObject> DoorsB;
    public List<GameObject> DoorsC;

    public bool buttonA;
    public bool buttonB;
    public bool buttonC;

    // Start is called before the first frame update
    void Start()
    {
        buttonA = false;
        buttonB = false;
        buttonC = false;
    }

    public void DoorOpen(string door)
    {
        if (door == "A")
        {
            buttonA = !buttonA;
            buttonB = false;
            buttonC = false;
        }
        else if (door == "B")
        {
            buttonB = !buttonB;
            buttonA = false;
            buttonC = false;
        }
        else if (door == "C")
        {
            buttonC = !buttonC;
            buttonA = false;
            buttonB = false;
        }
        else 
        {
            buttonA = false;
            buttonB = false;
            buttonC = false;
        }

        ExecuteDoors();
    }

    public void ExecuteDoors()
    {
        for (int i = 0; i < DoorsA.Count; i++)
        {
            if (DoorsA[i].transform.GetChild(0).GetComponent<Animator>() != null)
            {
                DoorsA[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonA);
            }
        }

        for (int i = 0; i < DoorsB.Count; i++)
        {
            if (DoorsB[i].transform.GetChild(0).GetComponent<Animator>() != null)
            {
                DoorsB[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonB);
            }
        }

        for (int i = 0; i < DoorsC.Count; i++)
        {
            if (DoorsC[i].transform.GetChild(0).GetComponent<Animator>() != null)
            {
                DoorsC[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonC);
            }
        }
    }
}
