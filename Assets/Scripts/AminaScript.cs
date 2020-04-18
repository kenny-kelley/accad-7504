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

    private float rotateSum;

    private bool isAngry;
    private bool isTerrified;

    private bool transition;
    private bool rotationPause1;
    private bool rotationPause2;
    private bool secondOrder;

    private bool midturnCalc;
    private float midturn;
    private int i;

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

        rotateSum = 0;

        isAngry = false;
        isTerrified = false;

        transition = false;
        rotationPause1 = true;
        rotationPause2 = true;
        secondOrder = false;

        midturnCalc = false;
        midturn = 0;
        i = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        Vector3 forward = new Vector3(0, 0, 0);
        float speed = 0;
        float rotateSpeed = 0;

        /*
         * Amina walks to the closet
         */
        if (walktoCloset)
        {
            forward = new Vector3(0, 0, 1);
            speed = 1.0f;
        }

        /*
         * Amina rotates and faces Issouf, tells him it's time to get started,
         * then turns around and begins approaching front of classroom
         */
        if (rotateInCloset)
        {
            if (transform.eulerAngles.y + rotateSum < 60.0f)
            {
                rotateSpeed = 3.0f; //1.0f; //originally, 3.0f;
                rotateSum += rotateSpeed;
                //animator.Play("Turning");
            }
            else if (transform.eulerAngles.y + rotateSum < 180.0f && rotationPause1) //add pause to let turn animation complete
            {
                rotateSpeed = 3.0f; //1.0f;
                timePassedInCloset += Time.deltaTime;
                if (timePassedInCloset > 0.7f)
                {
                    rotationPause1 = false;
                    timePassedInCloset = 0;
                    rotateSpeed = 0;
                }
            }
            else if (transform.eulerAngles.y + rotateSum < 180.0f && !rotationPause1)
            {
                if (pauseInCloset)
                {
                    speed = 0;
                    timePassedInCloset += Time.deltaTime;
                    if (timePassedInCloset > 4.0f)
                    {
                        pauseInCloset = false;
                        timePassedInCloset = 0;
                    }
                }
                else
                {
                    rotateSpeed = 2.0f; //1.0f; //originally, 3.0f;
                    rotateSum += rotateSpeed;
                    //animator.Play("Turning");     //doing this will cause a jump in the middle of the first call for Turning; just let the first play out
                }
            }
            else if (transform.eulerAngles.y + rotateSum >= 180.0f && rotationPause2) //add pause to let turn animation complete
            {
                rotateSpeed = 3.0f; //1.0f;
                timePassedInCloset += Time.deltaTime;

                if (timePassedInCloset > 0.5f)
                {
                    rotationPause2 = false;
                    timePassedInCloset = 0;
                    rotateSpeed = 0;
                }
            }
            else
            {
                rotateInCloset = false;
                walkToFront = true;
                //animator.Play("Walking");

                transition = true;
            }
        }

        /*
         * Amina walks to the front of the classroom
         */
        if (walkToFront)
        {
            if (transition)
            {
                transition = false;
            }
            else
            {
                forward = new Vector3(0, 0, -1);
                speed = 1.0f;
            }
        }

        /*
         * Turn towards center and walk towards it
         */
        if (!midturnCalc)
        {
            midturn = (transform.eulerAngles.y - 150.0f) / 0.5f;
            midturnCalc = true;
        }

        if (walkToFrontCenter)
        {
            speed = 0.9f;

            if (transform.eulerAngles.y + rotateSum > 150.0f)
            {
                rotateSpeed = -0.5f;    //originally, -3.0f;
                rotateSum += rotateSpeed;

                i++;
                //forward = new Vector3(forward.x + 0.5f/midturn/(1 + i/10), 0, -1);
                forward = new Vector3(forward.x + Mathf.Cos(rotateSpeed / 180 * Mathf.PI), 0, -1);
                forward = forward.normalized;
                forward = new Vector3(forward.x, 0, -1);
            }
            else
            {
                forward = new Vector3(0.5f, 0, -1);
                speed = 1.0f;

                //rotateSpeed = -0.5f;
            }
        }

        /*
         * Turn back to face the class and stand in idle
         */
        if (turnBackTowardsClass)
        {
            if (transform.eulerAngles.y + rotateSum > 0.0f && transform.eulerAngles.y + rotateSum < 270.0f)
            {
                rotateSpeed = -1.5f;    //originally, -3.0f;
                rotateSum += rotateSpeed;
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
                rotateSum += rotateSpeed;
            }
            else
            {
                faceSoldiers = false;
                //animator.Play("Walk Backward");
                slideBack = true;
            }
        }
        if (slideBack)
        {
            forward = new Vector3(-1, 0, 0);    //originally, (1, 0, 0)
            speed = -0.5f;  //originally, 1.0f;
        }

        //Update animator
        UpdateAnimator(speed, rotateSpeed, isAngry, isTerrified);

        // Apply translations/rotations
        transform.Rotate(0, rotateSum, 0, Space.Self);
        rotateSum = 0;

        controller.SimpleMove(forward * speed);
    }

    public void UpdateAnimator(float speed, float rotateSpeed, bool isAngry, bool isTerrified)
    {
        animator.SetFloat("Speed", speed);
        animator.SetFloat("Rotate Speed", rotateSpeed);
        animator.SetBool("Is Angry", isAngry);
        animator.SetBool("Is Terrified", isTerrified);
    }

    public void WalktoCloset()
    {
        walktoCloset = true;
        //animator.Play("Walking");
    }

    public void StopWalktoCloset()
    {
        walktoCloset = false;
        rotateInCloset = true;
        //animator.Play("Idle");
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
        //animator.Play("Idle");
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

    public void NoChalk()
    {
        if (!secondOrder)
        {
            audios[7].Play();
            secondOrder = true;
        }
        else if (!audios[7].isPlaying)
        {
            audios[8].Play();
        }
        //REplace this
    }

    public void StopSlideBack()
    {
        slideBack = false;
        //animator.Play("Terrified");
        isTerrified = true;
    }
}
