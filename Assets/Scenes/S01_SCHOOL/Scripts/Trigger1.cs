using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Trigger1 : MonoBehaviour
{
	AudioSource audio;
	bool hasEntered = false;
	
	void Start()
	{
		audio = GetComponent<AudioSource>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!hasEntered)
		{
			audio.Play(0);
			hasEntered = true;	
		}
	}
}
