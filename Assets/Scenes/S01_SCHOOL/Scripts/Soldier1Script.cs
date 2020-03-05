using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier1Script : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    float timer;

    private bool enterSchool;

    public bool HasSaidBurnThePlace { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        timer = 0.0f;

        enterSchool = false;
        HasSaidBurnThePlace = false;
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
        audios[1].Play();
    }

    public void SayBurnThePlace()
    {
        audios[0].Play();
        HasSaidBurnThePlace = true;
    }
}
