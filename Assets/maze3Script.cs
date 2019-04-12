using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

/*
Color mapping

RED - 0
BLUE - 1
YELLOW - 2
GREEN - 3
PURPLE - 4
ORANGE - 5

 */

public class maze3Script : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	Dictionary<vector3, KeyValuePair<int, int>> rotationMap = new Dictionaty<vector3, KeyValuePair<int, int>();


	void Awake()
	{
		moduleId = moduleIdCounter++;
		PrepRotationMap();
	}

	void Start () 
	{
		
	}
	
	void Update () 
	{
		
	}

	void PrepRotationMap()
	{
		rotationMap.Add(new vector3(0, 0, 0), new KeyValuePair<int, int>(0, 0));
		rotationMap.Add(new vector3(0, 0, 90), new KeyValuePair<int, int>(3, 0));
		rotationMap.Add(new vector3(0, 0, 180), new KeyValuePair<int, int>(5, 180));
		rotationMap.Add(new vector3(0, 0, 270), new KeyValuePair<int, int>(2, 0));
		rotationMap.Add(new vector3(0, 90, 0), new KeyValuePair<int, int>(0, 90));
		rotationMap.Add(new vector3(0, 90, 90), new KeyValuePair<int, int>(3, 90));
		rotationMap.Add(new vector3(0, 90, 180), new KeyValuePair<int, int>(5, 270));
		rotationMap.Add(new vector3(0, 90, 270), new KeyValuePair<int, int>(2, 90));
		rotationMap.Add(new vector3(0, 180, 0), new KeyValuePair<int, int>(0, 180));
		rotationMap.Add(new vector3(0, 180, 90), new KeyValuePair<int, int>(3, 180));
		rotationMap.Add(new vector3(0, 180, 180), new KeyValuePair<int, int>(5, 0));
		rotationMap.Add(new vector3(0, 180, 270), new KeyValuePair<int, int>(2, 180));
		rotationMap.Add(new vector3(0, 270, 0), new KeyValuePair<int, int>(0, 270));
		rotationMap.Add(new vector3(0, 270, 90), new KeyValuePair<int, int>(3, 270));
		rotationMap.Add(new vector3(0, 270, 180), new KeyValuePair<int, int>(5, 90));
		rotationMap.Add(new vector3(0, 270, 270), new KeyValuePair<int, int>(2, 270));

		rotationMap.Add(new vector3(90, 0, 0), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new vector3(90, 0, 90), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new vector3(90, 0, 180), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new vector3(90, 0, 270), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new vector3(90, 90, 0), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new vector3(90, 90, 90), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new vector3(90, 90, 180), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new vector3(90, 90, 270), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new vector3(90, 180, 0), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new vector3(90, 180, 90), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new vector3(90, 180, 180), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new vector3(90, 180, 270), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new vector3(90, 270, 0), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new vector3(90, 270, 90), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new vector3(90, 270, 180), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new vector3(90, 270, 270), new KeyValuePair<int, int>(4, 0));

		rotationMap.Add(new vector3(180, 0, 0), new KeyValuePair<int, int>(5, 0));
		rotationMap.Add(new vector3(180, 0, 90), new KeyValuePair<int, int>(2, 180));
		rotationMap.Add(new vector3(180, 0, 180), new KeyValuePair<int, int>(0, 180));
		rotationMap.Add(new vector3(180, 0, 270), new KeyValuePair<int, int>(3, 180));
		rotationMap.Add(new vector3(180, 90, 0), new KeyValuePair<int, int>(5, 90));
		rotationMap.Add(new vector3(180, 90, 90), new KeyValuePair<int, int>(2, 270));
		rotationMap.Add(new vector3(180, 90, 180), new KeyValuePair<int, int>(0, 270));
		rotationMap.Add(new vector3(180, 90, 270), new KeyValuePair<int, int>(3, 270));
		rotationMap.Add(new vector3(180, 180, 0), new KeyValuePair<int, int>(5, 180));
		rotationMap.Add(new vector3(180, 180, 90), new KeyValuePair<int, int>(2, 0));
		rotationMap.Add(new vector3(180, 180, 180), new KeyValuePair<int, int>(0, 0));
		rotationMap.Add(new vector3(180, 180, 270), new KeyValuePair<int, int>(3, 0));
		rotationMap.Add(new vector3(180, 270, 0), new KeyValuePair<int, int>(5, 270));
		rotationMap.Add(new vector3(180, 270, 90), new KeyValuePair<int, int>(2, 90));
		rotationMap.Add(new vector3(180, 270, 180), new KeyValuePair<int, int>(0, 90));
		rotationMap.Add(new vector3(180, 270, 270), new KeyValuePair<int, int>(3, 90));

		rotationMap.Add(new vector3(270, 0, 0), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new vector3(270, 0, 90), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new vector3(270, 0, 180), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new vector3(270, 0, 270), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new vector3(270, 90, 0), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new vector3(270, 90, 90), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new vector3(270, 90, 180), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new vector3(270, 90, 270), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new vector3(270, 180, 0), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new vector3(270, 180, 90), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new vector3(270, 180, 180), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new vector3(270, 180, 270), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new vector3(270, 270, 0), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new vector3(270, 270, 90), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new vector3(270, 270, 180), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new vector3(270, 270, 270), new KeyValuePair<int, int>(1, 180));
	} 
}
