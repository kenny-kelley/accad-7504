using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier2Script : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    float timer;

    private bool enterSchool;
    private bool faceStudents;
    private bool walkTowardCloset;
    private bool faceIssouf;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        timer = 0.0f;

        enterSchool = false;
        faceStudents = false;
        walkTowardCloset = false;
        faceIssouf = false;
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
            }
        }

        if (walkTowardCloset)
        {
            forward = new Vector3(0, 0, 1);
            speed = 1.0f;
        }

        if (faceIssouf)
        {
            if (transform.eulerAngles.y > 25.0f)
            {
                rotateSpeed = 3.0f;
            }
            else
            {
                faceIssouf = false;
            }
        }

        // Apply translations/rotations
        transform.Rotate(0, rotateSpeed, 0, Space.Self);
        controller.SimpleMove(forward * speed);
    }

    public void EnterSchool()
    {
        enterSchool = true;
    }

    public void StopEnterSchool()
    {
        enterSchool = false;
        animator.Play("Aiming Idle");
    }

    public void TurnAndWalkTowardCloset()
    {
        faceStudents = true;
        animator.Play("Walking");
    }

    public void StopTurnAndWalkTowardCloset()
    {
        walkTowardCloset = false;
        animator.Play("Walking");
    }

    public void TellIssoufToMoveIt()
    {
        audios[0].Play();
        animator.Play("Aiming Idle");
        faceIssouf = true;
    }
}
