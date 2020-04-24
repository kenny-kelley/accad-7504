using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger3 : MonoBehaviour
{
	bool hasEntered = false;
	public AminaScript amina;
	
	void Start()
	{
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == amina.gameObject && !hasEntered)
		{
			amina.StopWalktoCloset();
			hasEntered = true;
		}
	}
}
