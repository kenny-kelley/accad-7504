using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
	private bool close;
	private float currentRotation;
	
    // Start is called before the first frame update
    void Start()
    {
        close = true;
		currentRotation = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (close && currentRotation < 90.0f)
		{
			float rotateSpeed = 5.0f;
			transform.Rotate(0, rotateSpeed, 0, Space.Self);
			currentRotation += rotateSpeed;
		}
    }
	
	public void Close()
	{
		close = true;
	}
}
