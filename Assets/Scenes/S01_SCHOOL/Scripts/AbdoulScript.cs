using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AbdoulScript : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource[] audios;

    public bool HasToldStory { get; private set; }
    public bool HasFinishedStory { get; private set; }
    public bool HasApologized { get; private set; }
	public bool HasAskedWhatsGoingOn { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();

        HasToldStory = false;
        HasFinishedStory = false;
        HasApologized = false;
		HasAskedWhatsGoingOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TellStory()
    {
        audios[2].Play();
        animator.Play("Telling A Secret");
        HasToldStory = true;
    }

    public void Idle()
    {
        animator.Play("Idle");
    }

    public void FinishStory()
    {
        audios[3].Play();
        animator.Play("Telling A Secret");
        HasFinishedStory = true;
    }

    public void Apologize()
    {
        audios[0].Play();
        animator.Play("Talking");
        HasApologized = true;
    }

    public void AskWhatsGoingOn()
    {
        audios[1].Play();
		HasAskedWhatsGoingOn = true;
    }
}
