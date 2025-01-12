using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Manager : MonoBehaviour
{
    public List<GameObject> Doors;

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

        if (door == "B")
        {
            buttonB = !buttonB;
            buttonA = false;
            buttonC = false;
        }

        if (door == "C")
        {
            buttonC = !buttonC;
            buttonA = false;
            buttonB = false;
        }

        ExecuteDoors();
    }

    public void ExecuteDoors()
    {
        for (int i = 0; i < Doors.Count; i++)
        {
            if (Doors[i].CompareTag("GroupA"))
            {
                Doors[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonA);
            }

            if (Doors[i].CompareTag("GroupB"))
            {
                Doors[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonB);
            }

            if (Doors[i].CompareTag("GroupC"))
            {
                Doors[i].transform.GetChild(0).GetComponent<Animator>().SetBool("Button", buttonC);
            }
        }
    }
}
