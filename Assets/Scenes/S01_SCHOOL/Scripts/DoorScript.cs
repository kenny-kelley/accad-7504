using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
	private bool close;
	
    // Start is called before the first frame update
    void Start()
    {
        close = false;
    }

    // Update is called once per frame
    void Update()
    {
		float rotateSpeed = 0.0f;
        if (close)
			rotateSpeed = 6.0f;
		transform.Rotate(0, rotateSpeed, 0, Space.Self);
    }
	
	public void Close()
	{
		close = true;
	}
}
