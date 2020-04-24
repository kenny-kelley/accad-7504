using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    float timer;

    private bool walkToAisle;
    private bool walkToDoor;
    private bool exitSchool;

    public bool HasReachedAisle { get; private set; }
    public bool HasReachedDoor { get; private set; }
    public bool HasLeftSchool { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        timer = 0.0f;

        walkToAisle = false;
        walkToDoor = false;
        exitSchool = false;

        HasReachedAisle = false;
        HasReachedDoor = false;
        HasLeftSchool = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        Vector3 forward = new Vector3(0, 0, 0);
        float speed = 0;
        float rotateSpeed = 0;

        if (walkToAisle || exitSchool)
        {
            if (transform.eulerAngles.y < 270.0f)
            {
                rotateSpeed = 3.0f;
            }
            else
            {
                forward = new Vector3(-1, 0, 0);
                speed = 1.0f;
            }
        }

        if (walkToDoor)
        {
            if (transform.eulerAngles.y > 180.0f)
            {
                rotateSpeed = -3.0f;
            }
            else
            {
                forward = new Vector3(0, 0, -1);
                speed = 1.0f;
            }
        }

        // Apply translations/rotations
        transform.Rotate(0, rotateSpeed, 0, Space.Self);
        controller.SimpleMove(forward * speed);
    }

    public void WalkToAisle()
    {
        animator.Play("Walking");
        walkToAisle = true;
    }

    public void WalkToDoor()
    {
        walkToAisle = false;
        walkToDoor = true;
        HasReachedAisle = true;
    }

    public void ExitSchool()
    {
        exitSchool = true;
        walkToDoor = false;
        HasReachedDoor = true;
    }
}
