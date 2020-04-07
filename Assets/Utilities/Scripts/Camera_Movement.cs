using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Movement : MonoBehaviour
{
    public Transform playerBody;
    float mouse_sensitivity;
    float x_rotation;

    // Start is called before the first frame update
    void Start()
    {
        mouse_sensitivity = 100f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouse_x = Input.GetAxis("Mouse X") * mouse_sensitivity * Time.deltaTime;
        float mouse_y = Input.GetAxis("Mouse Y") * mouse_sensitivity * Time.deltaTime;

        x_rotation -= mouse_y;
        x_rotation = Mathf.Clamp(x_rotation, -90f, 75f); //prevent player from looking up or down past 90*

        transform.localRotation = Quaternion.Euler(x_rotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouse_x);

    }
}
