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

	static System.Random rnd = new System.Random();

	public KMSelectable[] btns;
	public GameObject[] pins;
	public GameObject cube;
	public Material[] colors;
	public Material lit;
	public Material until;

	int[][] strtPosPool = { 
							new int[] {0, 1, 2, 3, 5, 6, 7, 8},
							new int[] {11, 12, 14, 15, 16, 17},
							new int[] {18, 19, 20, 21, 23, 24, 25, 26},
							new int[] {27, 28, 29, 30, 32, 33, 34, 35},
							new int[] {36, 37, 38, 39, 41, 42, 43, 44},
							new int[] {45, 46, 47, 48, 50, 51, 52, 53},
						};

	int node;
	int xRot, yRot, zRot;

	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	Dictionary<Vector3, KeyValuePair<int, int>> rotationMap = new Dictionary<Vector3, KeyValuePair<int, int>>();
	Dictionary<int, MapNode> maze = new Dictionary<int, MapNode>();

	void Awake()
	{
		moduleId = moduleIdCounter++;
		
		// btns[0].OnInteract += delegate () { HandleUp(); return false; };
		// btns[1].OnInteract += delegate () { HandleRight(); return false; };
		// btns[2].OnInteract += delegate () { HandleDown(); return false; };
		// btns[3].OnInteract += delegate () { HandleLeft(); return false; };
		// btns[4].OnInteract += delegate () { HandleSubmit(); return false; };

	}

	void Start () 
	{
		Debug.LogFormat("[Maze^3 #{0}] Node mapping for logging purposes:\n                       __________\n                       | 09 10 11 |\n                       | 12 13 14 |\n                       | 15 16 17 |\n        _________ ________ ________\n       | 18 19 20 | 00 01 02 | 27 28 29 |\n       | 21 22 23 | 03 04 05 | 30 31 32 |\n       | 24 25 26 | 06 07 08 | 33 34 35 |\n                        ________\n                       | 36 37 38 |\n                       | 39 40 41 |\n                       | 42 43 44 |\n                       __________\n                       | 45 46 47 |\n                       | 48 49 50 |\n                       | 51 52 53 |\n                       __________", moduleId);		

		PrepRotationMap();
		PrepMaze();
		ResizeLights();

		RandomizeStartingPos();
		SetColorLights();
	}
	
	void Update () 
	{
		
	}

	void PrepRotationMap()
	{
		rotationMap.Add(new Vector3(0, 0, 0), new KeyValuePair<int, int>(0, 0));
		rotationMap.Add(new Vector3(0, 0, 90), new KeyValuePair<int, int>(3, 0));
		rotationMap.Add(new Vector3(0, 0, 180), new KeyValuePair<int, int>(5, 180));
		rotationMap.Add(new Vector3(0, 0, 270), new KeyValuePair<int, int>(2, 0));
		rotationMap.Add(new Vector3(0, 90, 0), new KeyValuePair<int, int>(0, 90));
		rotationMap.Add(new Vector3(0, 90, 90), new KeyValuePair<int, int>(3, 90));
		rotationMap.Add(new Vector3(0, 90, 180), new KeyValuePair<int, int>(5, 270));
		rotationMap.Add(new Vector3(0, 90, 270), new KeyValuePair<int, int>(2, 90));
		rotationMap.Add(new Vector3(0, 180, 0), new KeyValuePair<int, int>(0, 180));
		rotationMap.Add(new Vector3(0, 180, 90), new KeyValuePair<int, int>(3, 180));
		rotationMap.Add(new Vector3(0, 180, 180), new KeyValuePair<int, int>(5, 0));
		rotationMap.Add(new Vector3(0, 180, 270), new KeyValuePair<int, int>(2, 180));
		rotationMap.Add(new Vector3(0, 270, 0), new KeyValuePair<int, int>(0, 270));
		rotationMap.Add(new Vector3(0, 270, 90), new KeyValuePair<int, int>(3, 270));
		rotationMap.Add(new Vector3(0, 270, 180), new KeyValuePair<int, int>(5, 90));
		rotationMap.Add(new Vector3(0, 270, 270), new KeyValuePair<int, int>(2, 270));

		rotationMap.Add(new Vector3(90, 0, 0), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new Vector3(90, 0, 90), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new Vector3(90, 0, 180), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new Vector3(90, 0, 270), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new Vector3(90, 90, 0), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new Vector3(90, 90, 90), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new Vector3(90, 90, 180), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new Vector3(90, 90, 270), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new Vector3(90, 180, 0), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new Vector3(90, 180, 90), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new Vector3(90, 180, 180), new KeyValuePair<int, int>(4, 0));
		rotationMap.Add(new Vector3(90, 180, 270), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new Vector3(90, 270, 0), new KeyValuePair<int, int>(4, 270));
		rotationMap.Add(new Vector3(90, 270, 90), new KeyValuePair<int, int>(4, 180));
		rotationMap.Add(new Vector3(90, 270, 180), new KeyValuePair<int, int>(4, 90));
		rotationMap.Add(new Vector3(90, 270, 270), new KeyValuePair<int, int>(4, 0));

		rotationMap.Add(new Vector3(180, 0, 0), new KeyValuePair<int, int>(5, 0));
		rotationMap.Add(new Vector3(180, 0, 90), new KeyValuePair<int, int>(2, 180));
		rotationMap.Add(new Vector3(180, 0, 180), new KeyValuePair<int, int>(0, 180));
		rotationMap.Add(new Vector3(180, 0, 270), new KeyValuePair<int, int>(3, 180));
		rotationMap.Add(new Vector3(180, 90, 0), new KeyValuePair<int, int>(5, 90));
		rotationMap.Add(new Vector3(180, 90, 90), new KeyValuePair<int, int>(2, 270));
		rotationMap.Add(new Vector3(180, 90, 180), new KeyValuePair<int, int>(0, 270));
		rotationMap.Add(new Vector3(180, 90, 270), new KeyValuePair<int, int>(3, 270));
		rotationMap.Add(new Vector3(180, 180, 0), new KeyValuePair<int, int>(5, 180));
		rotationMap.Add(new Vector3(180, 180, 90), new KeyValuePair<int, int>(2, 0));
		rotationMap.Add(new Vector3(180, 180, 180), new KeyValuePair<int, int>(0, 0));
		rotationMap.Add(new Vector3(180, 180, 270), new KeyValuePair<int, int>(3, 0));
		rotationMap.Add(new Vector3(180, 270, 0), new KeyValuePair<int, int>(5, 270));
		rotationMap.Add(new Vector3(180, 270, 90), new KeyValuePair<int, int>(2, 90));
		rotationMap.Add(new Vector3(180, 270, 180), new KeyValuePair<int, int>(0, 90));
		rotationMap.Add(new Vector3(180, 270, 270), new KeyValuePair<int, int>(3, 90));

		rotationMap.Add(new Vector3(270, 0, 0), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new Vector3(270, 0, 90), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new Vector3(270, 0, 180), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new Vector3(270, 0, 270), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new Vector3(270, 90, 0), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new Vector3(270, 90, 90), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new Vector3(270, 90, 180), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new Vector3(270, 90, 270), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new Vector3(270, 180, 0), new KeyValuePair<int, int>(1, 180));
		rotationMap.Add(new Vector3(270, 180, 90), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new Vector3(270, 180, 180), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new Vector3(270, 180, 270), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new Vector3(270, 270, 0), new KeyValuePair<int, int>(1, 270));
		rotationMap.Add(new Vector3(270, 270, 90), new KeyValuePair<int, int>(1, 0));
		rotationMap.Add(new Vector3(270, 270, 180), new KeyValuePair<int, int>(1, 90));
		rotationMap.Add(new Vector3(270, 270, 270), new KeyValuePair<int, int>(1, 180));
	} 

	void PrepMaze()
	{
		maze.Add(0, new MapNode(0, new int[] {-1, -1, 20, 1}, new bool[]{true, false, true, false}));
		maze.Add(1, new MapNode(1, new int[] {16, 4, 0, -1}, new bool[]{true, false, false, false}));
		maze.Add(2, new MapNode(2, new int[] {17, -1, -1, 27}, new bool[]{true, false, false, true}));
		maze.Add(3, new MapNode(3, new int[] {-1, -1, 23, 4}, new bool[]{false, false, true, false}));
		maze.Add(4, new MapNode(4, new int[] {1, 7, 3, -1}, new bool[]{false, false, false, false}));
		maze.Add(5, new MapNode(5, new int[] {-1, 8, -1, 30}, new bool[]{false, false, false, true}));
		maze.Add(6, new MapNode(6, new int[] {-1, 36, 26, -1}, new bool[]{false, true, true, false}));
		maze.Add(7, new MapNode(7, new int[] {4, 37, -1, 8}, new bool[]{false, true, false, false}));
		maze.Add(8, new MapNode(8, new int[] {5, 38, 7, 33}, new bool[]{false, true, false, true}));

		maze.Add(9, new MapNode(9, new int[] {-1, -1, -1, 10}, new bool[]{true, false, true, false}));
		maze.Add(10, new MapNode(10, new int[] {-1, -1, 9, -1}, new bool[]{true, false, false, false}));
		maze.Add(11, new MapNode(11, new int[] {53, 14, -1, 29}, new bool[]{true, false, false, true}));
		maze.Add(12, new MapNode(12, new int[] {-1, -1, -1, 13}, new bool[]{false, false, true, false}));
		maze.Add(13, new MapNode(13, new int[] {-1, -1, 12, 14}, new bool[]{false, false, false, false}));
		maze.Add(14, new MapNode(14, new int[] {11, -1, 13, -1}, new bool[]{false, false, false, true}));
		maze.Add(15, new MapNode(15, new int[] {20, -1, -1, -1}, new bool[]{false, true, true, false}));
		maze.Add(16, new MapNode(16, new int[] {-1, 1, -1, 17}, new bool[]{false, true, false, false}));
		maze.Add(17, new MapNode(17, new int[] {-1, 16, 2, -1}, new bool[]{false, true, false, true}));
	
		maze.Add(18, new MapNode(18, new int[] {-1, 21, -1, 19}, new bool[]{true, false, true, false}));
		maze.Add(19, new MapNode(19, new int[] {-1, -1, 18, 20}, new bool[]{true, false, false, false}));
		maze.Add(20, new MapNode(20, new int[] {15, -1, 19, 0}, new bool[]{true, false, false, true}));
		maze.Add(21, new MapNode(21, new int[] {18, 24, 48, 23}, new bool[]{false, false, true, false}));
		maze.Add(22, new MapNode(22, new int[] {-1, -1, 21, -1}, new bool[]{false, false, false, false}));
		maze.Add(23, new MapNode(23, new int[] {-1, 26, -1, 3}, new bool[]{false, false, false, true}));
		maze.Add(24, new MapNode(24, new int[] {21, -1, 45, -1}, new bool[]{false, true, true, false}));
		maze.Add(25, new MapNode(25, new int[] {-1, 39, -1, 26}, new bool[]{false, true, false, false}));
		maze.Add(26, new MapNode(26, new int[] {23, -1, 25, 6}, new bool[]{false, true, false, true}));
	
		maze.Add(27, new MapNode(27, new int[] {-1, -1, 2, 28}, new bool[]{true, false, true, false}));
		maze.Add(28, new MapNode(28, new int[] {-1, -1, 27, 29}, new bool[]{true, false, false, false}));
		maze.Add(29, new MapNode(29, new int[] {11, 32, 28, 53}, new bool[]{true, false, false, true}));
		maze.Add(30, new MapNode(30, new int[] {-1, -1, 5, -1}, new bool[]{false, false, true, false}));
		maze.Add(31, new MapNode(31, new int[] {-1, -1, -1, 32}, new bool[]{false, false, false, false}));
		maze.Add(32, new MapNode(32, new int[] {29, -1, 31, 50}, new bool[]{false, false, false, true}));
		maze.Add(33, new MapNode(33, new int[] {-1, 38, 8, -1}, new bool[]{false, true, true, false}));
		maze.Add(34, new MapNode(34, new int[] {-1, 41, -1, -1}, new bool[]{false, true, false, false}));
		maze.Add(35, new MapNode(35, new int[] {-1, 44, 34, 47}, new bool[]{false, true, false, true}));
	
		maze.Add(36, new MapNode(36, new int[] {6, -1, -1, -1}, new bool[]{true, false, true, false}));
		maze.Add(37, new MapNode(37, new int[] {7, 40, -1, -1}, new bool[]{true, false, false, false}));
		maze.Add(38, new MapNode(38, new int[] {8, -1, -1, 33}, new bool[]{true, false, false, true}));
		maze.Add(39, new MapNode(39, new int[] {-1, -1, 25, -1}, new bool[]{false, false, true, false}));
		maze.Add(40, new MapNode(40, new int[] {37, -1, -1, 41}, new bool[]{false, false, false, false}));
		maze.Add(41, new MapNode(41, new int[] {-1, -1, 40, 34}, new bool[]{false, false, false, true}));
		maze.Add(42, new MapNode(42, new int[] {-1, 45, -1, 43}, new bool[]{false, true, true, false}));
		maze.Add(43, new MapNode(43, new int[] {-1, 46, 42, 44}, new bool[]{false, true, false, false}));
		maze.Add(44, new MapNode(44, new int[] {-1, 47, 43, -1}, new bool[]{false, true, false, true}));
	
		maze.Add(45, new MapNode(45, new int[] {42, -1, 24, -1}, new bool[]{true, false, true, false}));
		maze.Add(46, new MapNode(46, new int[] {43, -1, -1, 47}, new bool[]{true, false, false, false}));
		maze.Add(47, new MapNode(47, new int[] {44, -1, 46, 35}, new bool[]{true, false, false, true}));
		maze.Add(48, new MapNode(48, new int[] {-1, 51, 21, -1}, new bool[]{false, false, true, false}));
		maze.Add(49, new MapNode(49, new int[] {-1, 52, -1, -1}, new bool[]{false, false, false, false}));
		maze.Add(50, new MapNode(50, new int[] {-1, -1, 32, -1}, new bool[]{false, false, false, true}));
		maze.Add(51, new MapNode(51, new int[] {48, -1, -1, -1}, new bool[]{false, true, true, false}));
		maze.Add(52, new MapNode(52, new int[] {49, -1, -1, 53}, new bool[]{false, true, false, false}));
		maze.Add(53, new MapNode(53, new int[] {-1, 11, 52, 29}, new bool[]{false, true, false, true}));
	}

	void ResizeLights()
	{
		for(int i = 0; i < pins.Length; i++)
		{
			float scalar = transform.lossyScale.x;
    		pins[i].transform.GetChild(0).GetComponentsInChildren<Light>()[0].range *= scalar;
		}
	}

	void RandomizeStartingPos()
	{
		xRot = rnd.Next() % 4 * 90;
		yRot = rnd.Next() % 4 * 90;	
		zRot = rnd.Next() % 4 * 90;

		cube.transform.eulerAngles = new Vector3(xRot, yRot, zRot); 

		KeyValuePair<int, int> p;
		rotationMap.TryGetValue(new Vector3(xRot, yRot, zRot), out p);

		Debug.LogFormat("[Maze^3 #{0}] Starting face: {1} (rotated {2} degrees)", moduleId, GetColor(p.Key), p.Value);

		int[] pool = strtPosPool[p.Key];
		node = pool[rnd.Next(0, pool.Length)];

		pins[node].GetComponentInChildren<Renderer>().material = lit;
		pins[node].transform.GetChild(0).gameObject.SetActive(true);

		Debug.LogFormat("[Maze^3 #{0}] Starting node: {1}", moduleId, node);
	}

	String GetColor(int i)
	{
		switch(i)
		{
			case 0:
				return "Red";
			case 1:
				return "Blue";
			case 2:
				return "Yellow";
			case 3:
				return "Green";
			case 4:
				return "Purple";
			case 5:
				return "Orange";
		}

		return "";
	}


	void SetColorLights()
	{
		KeyValuePair<int, int> p;
		rotationMap.TryGetValue(new Vector3(xRot, yRot, zRot), out p);

		int pos = p.Key * 9 + 4;

		pins[pos].GetComponentInChildren<Renderer>().material = colors[p.Key];
		pins[pos].transform.GetChild(0).gameObject.SetActive(true);

		int adjColor = GetRndAdjacentColor(p.Key);
		pos = adjColor * 9 + 4;

		pins[pos].GetComponentInChildren<Renderer>().material = colors[adjColor];
		pins[pos].transform.GetChild(0).gameObject.SetActive(true);
	}

	int GetRndAdjacentColor(int color)
	{
		int pos = rnd.Next() % 4;

		switch(color)
		{
			case 0: return new int[]{1, 2, 3, 4}[pos];
			case 1: return new int[]{0, 2, 3, 5}[pos];
			case 2: return new int[]{0, 1, 4, 5}[pos];
			case 3: return new int[]{0, 1, 4, 5}[pos];
			case 4: return new int[]{0, 2, 3, 5}[pos];
			case 5: return new int[]{1, 2, 3, 4}[pos];
		}

		return -1;
	}
}
