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
		audio = GameObject.Find("ISSOUF WHERE ARE YOU").GetComponent<AudioSource>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!hasEntered)
		{
			Debug.Log("User entered trigger");
			audio.Play(0);
			hasEntered = true;	
		}
	}
}
