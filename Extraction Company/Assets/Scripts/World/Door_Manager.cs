using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Manager : MonoBehaviour
{
    public List<Animator> animator;

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
        for (int i = 0; i < animator.Count; i++)
        {
            if (animator[i].CompareTag("GroupA"))
            {
                animator[i].SetBool("Button", buttonA);
            }

            if (animator[i].CompareTag("GroupB"))
            {
                animator[i].SetBool("Button", buttonB);
            }

            if (animator[i].CompareTag("GroupC"))
            {
                animator[i].SetBool("Button", buttonC);
            }
        }
    }
}
