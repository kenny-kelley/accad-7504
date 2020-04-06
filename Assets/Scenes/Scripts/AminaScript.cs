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
    private bool faceSoldiers;
    private bool slideBack;

    public bool HasTakenAttendance { get; private set; }
    public bool HasMmphedAbdoul { get; private set; }
    public bool HasMmphedAbdoulAndIssouf { get; private set; }
    public bool HasOrderedIssoufToGetChalk { get; private set; }
    public bool HasToldChildrenToHide { get; private set; }
    public bool HasToldChildrenTheSoldiersAreBack { get; private set; }

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
        faceSoldiers = false;
        slideBack = false;

        HasTakenAttendance = false;
        HasMmphedAbdoul = false;
        HasMmphedAbdoulAndIssouf = false;
        HasOrderedIssoufToGetChalk = false;
        HasToldChildrenToHide = false;
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
            if (transform.eulerAngles.y < 60.0f)
            {
                rotateSpeed = 1f;     //originally, 3.0f
                animator.Play("Turning");
            }
            else if (transform.eulerAngles.y < 180.0f)
            {
                if (pauseInCloset)
                {
                    speed = 0;
                    timePassedInCloset += Time.deltaTime;
                    if (timePassedInCloset > 4.0f)
                        pauseInCloset = false;
                }
                else
                {
                    rotateSpeed = 0.75f;     //originally, 3.0f
                    //animator.Play("Turning");     //doing this will cause a jump in the middle of the first call for Turning; just let the first play out
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
            else
            {
                turnBackTowardsClass = false;
                rotateSpeed = 0;
            }
        }

        /*
         * Face soldiers and slide back
         */
        if (faceSoldiers)
        {
            float angle = transform.localEulerAngles.y;
            angle = (angle > 180) ? angle - 360 : angle;

            if (angle > -90.0f)
            {
                rotateSpeed = -3.0f;
            }
            else
            {
                faceSoldiers = false;
                animator.Play("Walk Backward");
                slideBack = true;
            }
        }
        if (slideBack)
        {
            forward = new Vector3(1, 0, 0);
            speed = 1.0f;
        }

        //Turning(rotateSpeed, speed);

        // Apply translations/rotations

        transform.Rotate(0, rotateSpeed, 0, Space.Self);
        controller.SimpleMove(forward * speed);
    }
	
    //function for playing turn animations based on whether turning left or right, while idle or moving
    public void Turning(float rotateSpeed, float speed)
    {
        if(rotateSpeed < 0)   //turning left
        {
            if(speed > 0)   //while walking
            {

            }
            else//while idle
            {

            }
        }
        else if(rotateSpeed > 0)    //turning right
        {
            if(speed > 0)   //while walking
            {
                animator.Play("Turning");
            }
            else//while idle
            {
                animator.Play("Turning");
            }
        }
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
	}

    public void AskIssoufWhereAreYou()
    {
        audios[1].Play();
    }

    public void TakeAttendance()
    {
        audios[2].Play();
        HasTakenAttendance = true;
    }

    public void MmphAbdoul()
    {
        //audios[2].Play(); --- Removed from script
        animator.Play("Angry Gesture");
        HasMmphedAbdoul = true;
    }

    public void MmphAbdoulAndIssouf()
    {
        audios[5].Play();
        animator.Play("Angry Gesture");
        HasMmphedAbdoulAndIssouf = true;
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

    public void OrderIssoufToGetChalk()
    {
        audios[3].Play();
        animator.Play("Angry Gesture");
        HasOrderedIssoufToGetChalk = true;
    }

    public void TellChildrenToHide()
    {
        audios[6].Play();
        HasToldChildrenToHide = true;
    }

    public void TellChildrenTheSoldiersAreBack()
    {
        audios[4].Play();
        HasToldChildrenTheSoldiersAreBack = true;
    }

    public void FaceSoldiersAndSlideBack()
    {
        faceSoldiers = true;
    }

    public void StopSlideBack()
    {
        slideBack = false;
        animator.Play("Terrified");
    }
}
