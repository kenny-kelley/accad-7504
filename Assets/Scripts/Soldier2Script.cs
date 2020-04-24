using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier2Script : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    float timer;
    private float timePassed;

    private bool enterSchool;
    private bool faceStudents;
    private bool walkTowardCloset;
    private bool faceIssouf;
    private bool transition1;
    private bool transition2;
    private bool threateningStudents;

    private bool isAiming;
    private bool bash;
    private bool push;

    private int strikeCounter;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        timer = 0.0f;
        timePassed = 0.0f;

        enterSchool = false;
        faceStudents = false;
        walkTowardCloset = false;
        faceIssouf = false;
        transition1 = true;
        transition2 = true;
        threateningStudents = false;

        isAiming = true;
        bash = false;
        push = false;

        strikeCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        Vector3 forward = new Vector3(0, 0, 0);
        float speed = 0;
        float rotateSpeed = 0;

        if (enterSchool)
        {
            forward = new Vector3(1, 0, 0);
            speed = 1.0f;
        }



        if (faceStudents)
        {
            if (transform.eulerAngles.y > 4.0f)
            {
                rotateSpeed = -3.0f;
            }
            else
            {
                faceStudents = false;
                walkTowardCloset = true;
                //animator.Play("Walking");

                isAiming = false;
            }
        }

        if (walkTowardCloset)
        {
            forward = new Vector3(0, 0, 1);
            speed = 1.0f;
        }

        if (faceIssouf)
        {
            //isAiming = true;

            if (transition2)
            {
                timePassed += Time.deltaTime;
                if (timePassed < 10000000000000000000000000000000.0f)
                {
                    timePassed = 0;
                    transition2 = false;
                }
            }
            else if (transform.eulerAngles.y < 120.0f)
            {
                rotateSpeed = 3.0f;
            }
            else
            {
                faceIssouf = false;
                threateningStudents = true;
            }
        }

/*
        if (strikeCounter >= 2) //stop bash animation after two times
        {
            bash = false;
            threateningStudents = false;
        }
        */

        if (threateningStudents)
        {
            timePassed += Time.deltaTime;
            if (timePassed < 10.0f)
            {
                //timePassed = 0;
                bash = true;
                //strikeCounter++;
            }
            else //if(timePassed > 10.0f)
            {
                bash = false;
                threateningStudents = false;

            }
        }

        UpdateAnimator(speed, rotateSpeed, isAiming, bash, push);

        // Apply translations/rotations
        transform.Rotate(0, rotateSpeed, 0, Space.Self);

        controller.SimpleMove(forward * speed);
    }

    public void UpdateAnimator(float speed, float rotateSpeed, bool isAiming, bool bash, bool push)
    {
        animator.SetFloat("Speed", speed);
        animator.SetFloat("Rotate Speed", rotateSpeed);
        animator.SetBool("Is Aiming", isAiming);
        animator.SetBool("Bash", bash);
        animator.SetBool("Push", push);
    }

    public void EnterSchool()
    {
        enterSchool = true;
    }

    public void StopEnterSchool()
    {
        enterSchool = false;
        //animator.Play("Aiming Idle");
    }

    public void TurnAndWalkTowardCloset()
    {
        faceStudents = true;
    }

    public void StopTurnAndWalkTowardCloset()
    {
        walkTowardCloset = false;
        //animator.Play("Aiming Idle");
    }

    public void TellIssoufToMoveIt()
    {
        audios[0].Play();
        faceIssouf = true;
    }
}
