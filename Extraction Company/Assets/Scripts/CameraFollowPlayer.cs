using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform whereCamPosition;

    // Update is called once per frame
    void Update()
    {
        //This will take the cameraPosition objects position, and set it to the camera in order to follow it 
        this.transform.position = whereCamPosition.position;
    }
}
