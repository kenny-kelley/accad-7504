using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AminaScript : MonoBehaviour
{
	CharacterController controller;
	CapsuleCollider collider;
    Animator animator;
    AudioSource[] audios;
    bool moveToDoor;
    bool rotateInCloset;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
		collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();
        moveToDoor = false;
        rotateInCloset = false;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 forward = new Vector3(0,0,0);
		float speed = 0;
		float rotateSpeed = 0;
		
		if (moveToDoor)
		{
			forward = new Vector3(0,0,1);
			speed = 1.0f;
		}
		if (rotateInCloset && transform.eulerAngles.y < 90.0f)
		{
			rotateSpeed = 3.0f;
		}
		
		transform.Rotate(0, rotateSpeed, 0, Space.Self);
        controller.SimpleMove(forward * speed);
    }
	
	public void MoveToDoor()
	{
		moveToDoor = true;
        animator.Play("Walking");
	}
	
	public void StopMoveToDoor()
	{
		moveToDoor = false;
		rotateInCloset = true;
        animator.Play("Idle");
        audios[0].Play();
        //animator.Play("Turning");
	}
}
