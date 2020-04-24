using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier1Script : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    float timer;
    private float timePassed;

    private bool enterSchool;
    private bool threaten;

    public bool HasSaidBurnThePlace { get; private set; }

    private float speed;
    private float rotateSpeed;
    private bool isAiming;
    private bool bash;
    private bool push;

    private float rotateSum;
    private bool pause;
    //private int strikeCounter;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        timer = 0.0f;
        timePassed = 0.0f;

        enterSchool = false;
        threaten = false;
        HasSaidBurnThePlace = false;

        isAiming = true;
        bash = false;
        push = false;

        rotateSum = 0;
        pause = false;
        //strikeCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        Vector3 forward = new Vector3(0, 0, 0);
        speed = 0;
        rotateSpeed = 0;

        /*
        if (strikeCounter >= 2) //stop push animation after two times
        {
            push = false;
            threaten = false;
        }
        */

        if (enterSchool)
        {
            forward = new Vector3(1, 0, 0);
            speed = 1.0f;
        }
        else if (threaten /* && strikeCounter < 2 */)
        {
            timePassed += Time.deltaTime;
            if (timePassed < 5.0f)
            {
                //timePassed = 0;
                push = true;
                //strikeCounter++;
            }
            else //if(timePassed > 5.0f)
            {
                push = false;
                threaten = false;
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
        threaten = true;
        //animator.Play("Aiming Idle");
        audios[1].Play();
    }

    public void SayBurnThePlace()
    {
        audios[0].Play();
        HasSaidBurnThePlace = true;
    }
}
