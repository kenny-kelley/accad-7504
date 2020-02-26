using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Trigger2 : MonoBehaviour
{
	AudioSource audio;
	bool hasEntered = false;
	public AminaScript amina;
	
	void Start()
	{
		audio = GetComponent<AudioSource>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!hasEntered)
		{
			amina.MoveToDoor();
			audio.Play(0);
			hasEntered = true;	
		}
	}
}
