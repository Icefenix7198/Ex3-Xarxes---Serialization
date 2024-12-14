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
}
