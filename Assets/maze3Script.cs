using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class maze3Script : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
	}

	void Start () 
	{
		
	}
	
	void Update () 
	{
		
	}
}
