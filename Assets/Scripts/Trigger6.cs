using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger6 : MonoBehaviour
{
    float timePassed;

    bool hasEntered;
    
    public AminaScript amina;
    public AbdoulScript abdoul;
    public Soldier1Script soldier1;
    public Soldier2Script soldier2;
	public DoorScript closetDoor;
    public DoorScript mainDoor;
    
    private AudioSource[] audios;
    public Trigger5 trigger5;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        audios = GetComponents<AudioSource>();
    }

    void Update()
    {
        if (hasEntered)
        {
            timePassed += Time.deltaTime;
        }

        if (timePassed > 10.5f && !amina.HasToldChildrenToHide)
        {
            audios[1].Stop();
            audios[3].Play();
			amina.TellChildrenToHide();
        }
		else if (timePassed > 14.0f && !abdoul.HasAskedWhatsGoingOn)
		{
			abdoul.AskWhatsGoingOn();
		}
		else if (timePassed > 15.5f && !amina.HasToldChildrenTheSoldiersAreBack)
		{
			amina.TellChildrenTheSoldiersAreBack();
			closetDoor.Close();
            mainDoor.Open();
            amina.FaceSoldiersAndSlideBack();
            audios[2].Play();
            soldier1.EnterSchool();
            soldier2.EnterSchool();
		}
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasEntered && trigger5.HasEntered)
        {
            audios[0].Play();
            hasEntered = true;
            trigger5.SendMessage("gotChalk");
        }
    }
}
