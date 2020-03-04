using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AbdoulScript : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TellStory()
    {
        audios[1].Play();
        animator.Play("Telling A Secret");
    }

    public void Idle()
    {
        animator.Play("Idle");
    }

    public void FinishStory()
    {
        audios[2].Play();
        animator.Play("Telling A Secret");
    }

    public void Apologize()
    {
        audios[0].Play();
        animator.Play("Talking");
    }
}
