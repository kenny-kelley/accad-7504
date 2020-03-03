using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AminaScript : MonoBehaviour
{
    private float timer;
    private float timePassedInCloset;

    private CharacterController controller;
    private new CapsuleCollider collider;
    private Animator animator;
    private AudioSource[] audios;

    private bool walktoCloset;
    private bool rotateInCloset;
    private bool pauseInCloset;
    private bool walkToFront;
    private bool walkToFrontCenter;
    private bool turnBackTowardsClass;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        timePassedInCloset = 0.0f;

        controller = GetComponent<CharacterController>();
		collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        walktoCloset = false;
        rotateInCloset = false;
        pauseInCloset = true;
        walkToFront = false;
        walkToFrontCenter = false;
        turnBackTowardsClass = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

		Vector3 forward = new Vector3(0,0,0);
		float speed = 0;
		float rotateSpeed = 0;
		
        /*
         * Amina walks to the closet
         */ 
		if (walktoCloset)
		{
			forward = new Vector3(0,0,1);
			speed = 1.0f;
		}

        /*
         * Amina rotates and faces Issouf, tells him it's time to get started,
         * then turns around and begins approaching front of classroom
         */
        if (rotateInCloset)
		{
            if (transform.eulerAngles.y < 90.0f)
            {
                rotateSpeed = 3.0f;
            }
            else if (transform.eulerAngles.y < 180.0f)
            {
                if (pauseInCloset)
                {
                    timePassedInCloset += Time.deltaTime;
                    if (timePassedInCloset > 4.0f)
                        pauseInCloset = false;
                }
                else
                {
                    rotateSpeed = 3.0f;
                }
            }
            else
            {
                rotateInCloset = false;
                walkToFront = true;
                animator.Play("Walking");
            }
		}

        /*
         * Amina walks to the front of the classroom
         */
        if (walkToFront)
        {
            forward = new Vector3(0, 0, -1);
            speed = 1.0f;
        }

        /*
         * Turn towards center and walk towards it
         */
        if (walkToFrontCenter)
        {
            if (transform.eulerAngles.y > 150.0f)
            {
                rotateSpeed = -3.0f;
            }
            else
            {
                forward = new Vector3(0.5f, 0, -1);
                speed = 1.0f;
            }
        }

        /*
         * Turn back to face the class and stand in idle
         */
        if (turnBackTowardsClass)
        {
            if (transform.eulerAngles.y > 0.0f)
            {
                rotateSpeed = -3.0f;
            }
        }

        // Apply translations/rotations
        transform.Rotate(0, rotateSpeed, 0, Space.Self);
        controller.SimpleMove(forward * speed);
    }
	
	public void WalktoCloset()
	{
		walktoCloset = true;
        animator.Play("Walking");
	}
	
	public void StopWalktoCloset()
	{
		walktoCloset = false;
		rotateInCloset = true;
        animator.Play("Idle");
        audios[0].Play();
        //animator.Play("Turning");
	}

    public void WalkToFrontCenter()
    {
        walkToFrontCenter = true;
        walkToFront = false;
    }

    public void StopWalkToFrontCenter()
    {
        walkToFrontCenter = false;
        turnBackTowardsClass = true;
        animator.Play("Idle");
    }
}
