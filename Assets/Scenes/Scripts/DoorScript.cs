using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
	private bool close;
    private bool open;
	public float currentRotation;
	
    // Start is called before the first frame update
    void Start()
    {
        close = false;
        open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((close && currentRotation < 90.0f) ||
            (open && currentRotation < 180.0f))
		{
            float rotateSpeed = 0.0f;
            rotateSpeed = 5.0f;
			currentRotation += rotateSpeed;
            transform.Rotate(0, rotateSpeed, 0, Space.Self);
        }
    }
	
	public void Close()
	{
		close = true;
	}

    public void Open()
    {
        open = true;
    }
}
