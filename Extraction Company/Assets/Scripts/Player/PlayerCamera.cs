using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //Float = 0.0f -> Number with comas
    public float sensitivityX = 0.0f; //The mouse sensitivity in X
    public float sensitivityY = 0.0f; //The mouse sensitivity in Y

    public Transform playerOrientation; //The player position, rotation and scale

    float mouseX; //Mouse X Input registrer
    float mouseY; //Mouse Y Input registrer

    float xRotation; //The rotation that we will pass to the  player
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //The mouse is locked in the center of the screen
        Cursor.visible = false; //The mouse is invisible
    }

    // Update is called once per frame
    void Update()
    {
        //Get mouse X & Y inputs
        mouseX = Input.GetAxisRaw("Mouse X") * sensitivityX * Time.deltaTime;
        mouseY = Input.GetAxisRaw("Mouse Y") * sensitivityY * Time.deltaTime;

        //Weird but the Unity way that registres the inputs works like that \'-'/
        xRotation -= mouseY;
        yRotation += mouseX;

        //Here we block the max verticality look to 90f and -90f
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Now apply the roation X and Y to the camera
        this.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        //And the rotation to the player, the player only rotates in X, in case you want to make a head, the Y movement maybe interests you
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
