using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //Stores information how sensitive input should be, such as if we move the mouse x distance, the camera should move x distance at mouse speed * y sensitivty. See Documentation For Details.
    public float sensX;
    public float sensY;

    public Transform orientation;

    //Stores rotation information for our camera. See Documentation For Details.
    float xRotation;
    float yRotation;

    private void Start()
    {
        //Sets the cursor state to be hidden and locked to the center of the screen while the script is active. See Documentation For Details.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //This grabs input information from the mouse and stores it for us so it can be used to move our camera. See Documentation For Details.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        //stores our gatehred input from the mouse into variables for unity to use. See Documentation For Details.
        yRotation += mouseX;
        xRotation -= mouseY;

        //Prevents our camera from rotating 360s vertically to behind our player. See Documentation For Details.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //This rotates our camera on the X and Y access on a given point along with rotating our player model. See Documentation For Details.
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }


}
