﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

using Rnd = UnityEngine.Random;
using System;

/*
Color mapping

RED - 0
BLUE - 1
YELLOW - 2
GREEN - 3
MAGENTA - 4
ORANGE - 5

 */

public class maze3Script : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio mAudio;
	public KMBombModule modSelf;
    public KMColorblindMode ColorblindMode;

	public KMSelectable[] btns;
	public KMSelectable submitBtn;
	public GameObject[] pins;
	public GameObject[] checkLights;
	public GameObject cube;
	public Material[] colors;
	public Material lit;
	public Material unlit;
	public GameObject[] orientators;
	public GameObject rotator;
    public TextMesh[] colorblindIndicators;

	int[][] strtPosPool = { 
							new int[] {0, 1, 2, 3, 5, 6, 7, 8},
							new int[] {9, 10, 11, 12, 14, 15, 16, 17},
							new int[] {18, 19, 20, 21, 23, 24, 25, 26},
							new int[] {27, 28, 29, 30, 32, 33, 34, 35},
							new int[] {36, 37, 38, 39, 41, 42, 43, 44},
							new int[] {45, 46, 47, 48, 50, 51, 52, 53},
						};

	int node;
    int nodestart;
	int xRot, yRot, zRot;
	int orientation;

	int[] solution;
	int currentPress = 0;

	static int moduleIdCounter = 1;
    int moduleId;
	private bool moduleSolved = false, colorblindDetected = false, rotating = false, hasStruck;

	Dictionary<Vector3, KeyValuePair<int, int>> rotationMap = new Dictionary<Vector3, KeyValuePair<int, int>>();
	Dictionary<int, MapNode> maze = new Dictionary<int, MapNode>();
	List<int> nodePath = new List<int>();

	void Awake()
	{
		moduleId = moduleIdCounter++;

		for (int x = 0; x < 4; x++)
        {
			int y = x;
			btns[x].OnInteract += delegate {
				HandleMovement(y);
				return false;
			};
        }
		/*
		btns[0].OnInteract += delegate () { HandleUp(); return false; };
		btns[1].OnInteract += delegate () { HandleRight(); return false; };
		btns[2].OnInteract += delegate () { HandleDown(); return false; };
		btns[3].OnInteract += delegate () { HandleLeft(); return false; };
		*/
		submitBtn.OnInteract += delegate () { HandleSubmit(); return false; };

		try
        {
			colorblindDetected = ColorblindMode.ColorblindModeActive;
        }
		catch
        {
			colorblindDetected = false;
        }
	}

	void Start () 
	{
		Debug.LogFormat("[Maze³ #{0}] Node mapping for logging purposes:", moduleId);

		PrepRotationMap();
		PrepMaze();
		ResizeLights();

		CalcSolution();

        foreach (var obj in colorblindIndicators)
            obj.gameObject.SetActive(colorblindDetected);
		RandomizeStartingPos();
		SetColorLights();
		nodePath.Add(node);
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
		//maze.Add(idx, new MapNode(idx, new int[] {resultUp, resultDown, resultLeft, resultRight}, new bool[]{turnOnUp, turnOnDown, turnOnLeft, turnOnRight}));
		maze.Add(0, new MapNode(0, new int[] {-1, -1, 20, 1}, new bool[]{true, false, true, false}));
		maze.Add(1, new MapNode(1, new int[] {16, 4, 0, -1}, new bool[]{true, false, false, false}));
		maze.Add(2, new MapNode(2, new int[] {17, -1, -1, 27}, new bool[]{true, false, false, true}));
		maze.Add(3, new MapNode(3, new int[] {-1, -1, 23, 4}, new bool[]{false, false, true, false}));
		maze.Add(4, new MapNode(4, new int[] {1, 7, 3, -1}, new bool[]{false, false, false, false}));
		maze.Add(5, new MapNode(5, new int[] {-1, 8, -1, 30}, new bool[]{false, false, false, true}));
		maze.Add(6, new MapNode(6, new int[] {-1, 36, 26, -1}, new bool[]{false, true, true, false}));
		maze.Add(7, new MapNode(7, new int[] {4, 37, -1, 8}, new bool[]{false, true, false, false}));
		maze.Add(8, new MapNode(8, new int[] {5, 38, 7, 33}, new bool[]{false, true, false, true}));

		maze.Add(9, new MapNode(9, new int[] {51, -1, -1, 10}, new bool[]{true, false, true, false}));
		maze.Add(10, new MapNode(10, new int[] {52, -1, 9, -1}, new bool[]{true, false, false, false}));
		maze.Add(11, new MapNode(11, new int[] {53, 14, -1, 29}, new bool[]{true, false, false, true}));
		maze.Add(12, new MapNode(12, new int[] {-1, -1, -1, 13}, new bool[]{false, false, true, false}));
		maze.Add(13, new MapNode(13, new int[] {-1, -1, 12, 14}, new bool[]{false, false, false, false}));
		maze.Add(14, new MapNode(14, new int[] {11, -1, 13, -1}, new bool[]{false, false, false, true}));
		maze.Add(15, new MapNode(15, new int[] {-1, -1, 20, -1}, new bool[]{false, true, true, false}));
		maze.Add(16, new MapNode(16, new int[] {-1, 1, -1, 17}, new bool[]{false, true, false, false}));
		maze.Add(17, new MapNode(17, new int[] {-1, 2, 16, -1}, new bool[]{false, true, false, true}));
	
		maze.Add(18, new MapNode(18, new int[] {-1, 21, -1, 19}, new bool[]{true, false, true, false}));
		maze.Add(19, new MapNode(19, new int[] {-1, -1, 18, 20}, new bool[]{true, false, false, false}));
		maze.Add(20, new MapNode(20, new int[] {15, -1, 19, 0}, new bool[]{true, false, false, true}));
		maze.Add(21, new MapNode(21, new int[] {18, 24, 48, 22}, new bool[]{false, false, true, false}));
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
		maze.Add(35, new MapNode(35, new int[] {-1, -1, -1, 47}, new bool[]{false, true, false, true}));
	
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
		maze.Add(50, new MapNode(50, new int[] {-1, -1, -1, 32}, new bool[]{false, false, false, true}));
		maze.Add(51, new MapNode(51, new int[] {48, 9, -1, -1}, new bool[]{false, true, true, false}));
		maze.Add(52, new MapNode(52, new int[] {49, 10, -1, 53}, new bool[]{false, true, false, false}));
		maze.Add(53, new MapNode(53, new int[] {-1, 11, 52, 29}, new bool[]{false, true, false, true}));
	}

	void ResizeLights()
	{
		for(int i = 0; i < pins.Length; i++)
		{
			float scalar = transform.lossyScale.x;
    		pins[i].transform.GetChild(0).GetComponentsInChildren<Light>()[0].range *= scalar;
		}

		for(int i = 0; i < checkLights.Length; i++)
		{
			float scalar = transform.lossyScale.x;
    		checkLights[i].transform.GetChild(0).GetComponentsInChildren<Light>()[0].range *= scalar;
		}
	}

    void RandomizeStartingPos()
    {
		
        xRot = Rnd.Range(0, 4) * 90;
        yRot = Rnd.Range(0, 4) * 90;
        zRot = Rnd.Range(0, 4) * 90;
		
        cube.transform.localEulerAngles = new Vector3(xRot, yRot, zRot);

        KeyValuePair<int, int> p;
        rotationMap.TryGetValue(new Vector3(xRot, yRot, zRot), out p);

        orientation = p.Value;

        Debug.LogFormat("[Maze³ #{0}] Starting face: {1} (rotated {2} degrees)", moduleId, GetColor(p.Key), p.Value);

        int[] pool = strtPosPool[p.Key];
        node = pool[Rnd.Range(0, pool.Length)];
        nodestart = node;

        pins[node].GetComponentInChildren<Renderer>().material = lit;
        pins[node].transform.GetChild(0).gameObject.SetActive(true);

        Debug.LogFormat("[Maze³ #{0}] Starting node: {1}", moduleId, node);
    }

	string GetColor(int i)
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
				return "Magenta";
			case 5:
				return "Orange";
		}

		return "Uncolored";
	}


    void SetColorLights()
    {
        foreach (var obj in colorblindIndicators)
        {
            obj.text = "";
            // Randomize the position/orientation of the colorblind indicators to avoid giving an advantage
            var orientation = Rnd.Range(0, 4);
            obj.transform.localPosition = new Vector3(new[] { 1, 1, -1, -1 }[orientation], .141f, new[] { -1, 1, 1, -1 }[orientation]);
            obj.transform.localEulerAngles = new Vector3(90, new[] { 0, 270, 180, 90 }[orientation], 0);
        }

        KeyValuePair<int, int> p;
        rotationMap.TryGetValue(new Vector3(xRot, yRot, zRot), out p);

        int pos = p.Key * 9 + 4;

        pins[pos].GetComponentInChildren<Renderer>().material = colors[p.Key];
        colorblindIndicators[p.Key].text = "RBYGMO"[p.Key].ToString();
        pins[pos].transform.GetChild(0).gameObject.SetActive(true);

        int adjColor = GetRndAdjacentColor(p.Key);
        pos = adjColor * 9 + 4;

        pins[pos].GetComponentInChildren<Renderer>().material = colors[adjColor];
        colorblindIndicators[adjColor].text = "RBYGMO"[adjColor].ToString();
        pins[pos].transform.GetChild(0).gameObject.SetActive(true);
    }

	int GetRndAdjacentColor(int color)
	{
        int pos = Rnd.Range(0, 4);

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
	bool IsSafeToWalk(int direction)
    {
		MapNode n;
		maze.TryGetValue(node, out n);

		switch (orientation)
        {
			case 0:
                {
					switch (direction)
                    {
						case MapNode.up:
                            {
								return n.path[MapNode.up] != -1;
                            }
						case MapNode.down:
							{
								return n.path[MapNode.down] != -1;
							}
						case MapNode.left:
							{
								return n.path[MapNode.left] != -1;
							}
						case MapNode.right:
							{
								return n.path[MapNode.right] != -1;
							}
					}
					break;
                }
			case 90:
				{
					switch (direction)
					{
						case MapNode.up:
							{
								return n.path[MapNode.left] != -1;
							}
						case MapNode.down:
							{
								return n.path[MapNode.right] != -1;
							}
						case MapNode.left:
							{
								return n.path[MapNode.down] != -1;
							}
						case MapNode.right:
							{
								return n.path[MapNode.up] != -1;
							}
					}
					break;
				}
			case 180:
				{
					switch (direction)
					{
						case MapNode.up:
							{
								return n.path[MapNode.down] != -1;
							}
						case MapNode.down:
							{
								return n.path[MapNode.up] != -1;
							}
						case MapNode.left:
							{
								return n.path[MapNode.right] != -1;
							}
						case MapNode.right:
							{
								return n.path[MapNode.left] != -1;
							}
					}
					break;
				}
			case 270:
				{
					switch (direction)
					{
						case MapNode.up:
							{
								return n.path[MapNode.right] != -1;
							}
						case MapNode.down:
							{
								return n.path[MapNode.left] != -1;
							}
						case MapNode.left:
							{
								return n.path[MapNode.up] != -1;
							}
						case MapNode.right:
							{
								return n.path[MapNode.down] != -1;
							}
					}
					break;
				}
		}
		return false;
    }

	void HandleMovement(int direction)
    {
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		btns[direction].AddInteractionPunch(.5f);
		if (moduleSolved || rotating)
			return;

		int[] directionsCCW = { MapNode.up, MapNode.left, MapNode.down, MapNode.right, };
		string[] strikeDirection = new string[] { "Up", "Left", "Down", "Right" };
		int idx = Array.IndexOf(directionsCCW, direction);
		if (idx == -1) return;
		if (IsSafeToWalk(direction))
        {
			ChangePos(directionsCCW[(idx + orientation / 90) % 4], directionsCCW[idx]);
			nodePath.Add(node);
		}
		else
        {
			modSelf.HandleStrike();
			Debug.LogFormat("[Maze³ #{0}] Path taken before strike: {1}", moduleId, nodePath.Join(" -> "));
			nodePath.Clear();
			nodePath.Add(node);
			Debug.LogFormat("[Maze³ #{0}] Strike! Tried to walk {3} on node {1} ({4} button, rotated {2} degrees).", moduleId, node, orientation, strikeDirection[(idx + orientation / 90) % 4].ToLower(), strikeDirection[idx]);
			hasStruck = true;
		}
	}

	void ChangePos(int dir, int arrow)
	{
		int color = node / 9;

		MapNode n;
		maze.TryGetValue(node, out n);
		if(node == color * 9 + 4)
		{
			pins[node].GetComponentInChildren<Renderer>().material = colors[color];
			colorblindIndicators[color].gameObject.SetActive(colorblindDetected);
			colorblindIndicators[color].text = "RBYGMO"[color].ToString();
			pins[node].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(color);
		}
		else
		{
			pins[node].GetComponentInChildren<Renderer>().material = unlit;
			pins[node].transform.GetChild(0).gameObject.SetActive(false);
		}

		node = n.path[dir];

		pins[node].GetComponentInChildren<Renderer>().material = lit;
		pins[node].transform.GetChild(0).gameObject.SetActive(true);
		pins[node].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(-1);

		if(n.turn[dir])
		{
			switch(arrow)
			{
				case MapNode.up:
				{
					StartCoroutine(RotateCube(-1, 0));
					break;
				}
				case MapNode.down:
				{
					StartCoroutine(RotateCube(1, 0)); 
					break;
				}
				case MapNode.left:
				{
					StartCoroutine(RotateCube(0, 1));
					break;
				}
				case MapNode.right:
				{
					StartCoroutine(RotateCube(0, -1));
					break;
				}
			}

			switch(dir)
			{
				case MapNode.up:
				{
					if(color == 2)
					{
						orientation = (orientation + 270) % 360;
					}
					else if(color == 3)
					{
						orientation = (orientation + 90) % 360;
					}
					break;
				}
				case MapNode.down:
				{
					if(color == 2)
					{
						orientation = (orientation + 90) % 360;
					}
					else if(color == 3)
					{
						orientation = (orientation + 270) % 360;
					}					
					break;
				}
				case MapNode.left:
				{
					if(color == 4)
					{
						orientation = (orientation + 270) % 360;
					}
					else if(color == 2 || color == 5)
					{
						orientation = (orientation + 180) % 360;
					}
					else if(color == 1)
					{
						orientation = (orientation + 90) % 360;
					}
					break;
				}
				case MapNode.right:
				{
					if(color == 1)
					{
						orientation = (orientation + 270) % 360;
					}
					else if(color == 3 || color == 5)
					{
						orientation = (orientation + 180) % 360;
					}
					else if(color == 4)
					{
						orientation = (orientation + 90) % 360;
					}
					break;
				}
			}

			//Debug.Log(orientation);

			color = node / 9;
			//Debug.LogFormat("[Maze³ #{0}] Now at {1} face.", moduleId, GetColor(color));
		}
		
		//Debug.LogFormat("[Maze³ #{0}] Now at node {1}.", moduleId, node);

	}

	void HandleSubmit()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		btns[1].AddInteractionPunch(.5f);
		if(moduleSolved)
			return;
		Debug.LogFormat("[Maze³ #{0}] Path taken before pressing the submit button: {1}", moduleId, nodePath.Join(" -> "));
		nodePath.Clear();
		nodePath.Add(node);
		if (node == solution[currentPress])
		{
			Debug.LogFormat("[Maze³ #{0}] Successfuly submited {1} position.", moduleId, GetColor(node / 9));

			checkLights[currentPress].GetComponentInChildren<Renderer>().material = colors[node / 9];
			checkLights[currentPress].transform.GetChild(0).gameObject.SetActive(true);
			checkLights[currentPress].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(node / 9);
		
			currentPress++;

			if(currentPress > 2)
			{
				Debug.LogFormat("[Maze³ #{0}] Module solved!", moduleId);
				modSelf.HandlePass();
				moduleSolved = true;

				StartCoroutine(ShowColoredLights());
			}
			else
				mAudio.PlaySoundAtTransform("bip", transform);
		}
		else
		{
			modSelf.HandleStrike();

			if(node == (node / 9) * 9 + 4)
				Debug.LogFormat("[Maze³ #{0}] Strike! Tried to submit on {1} position (Expected {2}).", moduleId, GetColor(node / 9), GetColor(solution[currentPress] / 9));
			else
				Debug.LogFormat("[Maze³ #{0}] Strike! Tried to submit on an uncolored position (Expected {1}).", moduleId, GetColor(solution[currentPress] / 9));
		}
	}

	Color GetColorVector(int color)
	{
		switch(color)
		{
			case 0:
				return new Color(1.0f, 0, 0, 1.0f);
			case 1:
				return new Color(0.2f, 0, 1.0f, 1.0f);
			case 2:
				return new Color(0.863f, 1.0f, 0, 1.0f);
			case 3:
				return new Color(0.255f, 0.847f, 0.106f, 1.0f);
			case 4:
				return new Color(0.8235f, 0.278f, 0.278f, 1.0f);
			case 5:
				return new Color(1.0f, 0.557f, 0, 1.0f);
		}

		return new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}

	void CalcSolution()
	{
		solution = new int[3];

		int batteries = bomb.GetBatteryCount();
		int indicators = bomb.GetIndicators().Count(); 
		int ports = bomb.GetPortCount();

		if(batteries <= 1)
		{
			solution[0] = 40;
		}
		else if (batteries >= 5)
		{
			solution[0] = 13;
		}
		else
		{
			solution[0] = 49;
		}

		if(indicators <= 1)
		{
			solution[1] = 4;
		}
		else if (indicators >= 5)
		{
			solution[1] = 31;
		}
		else
		{
			solution[1] = 22;
		}

		if(ports <= 1)
		{
			solution[2] = 13;
		}
		else if (ports >= 5)
		{
			solution[2] = 49;
		}
		else
		{
			solution[2] = 40;
		}

		Debug.LogFormat("[Maze³ #{0}] Correct color sequence: {1}, {2}, {3}.", moduleId, GetColor(solution[0]/9), GetColor(solution[1]/9), GetColor(solution[2]/9));

	}

	IEnumerator ShowColoredLights()
	{
		foreach (TextMesh colorblindText in colorblindIndicators)
        {
			colorblindText.gameObject.SetActive(false);
        }

        for (int x = 0; x < btns.Length; x++)
        {
			btns[x].gameObject.SetActive(false);
        }
		for(int j = 0; j < 2; j++)
		{
			mAudio.PlaySoundAtTransform("bip", transform);

			for(int i = 0; i < pins.Length; i++)
			{
				if(i % 2 == 0)
				{
					pins[i].GetComponentInChildren<Renderer>().material = colors[i / 9];
					pins[i].transform.GetChild(0).gameObject.SetActive(true);
					pins[i].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(i / 9);
				}
				else
				{
					pins[i].GetComponentInChildren<Renderer>().material = unlit;
					pins[i].transform.GetChild(0).gameObject.SetActive(false);
				}
			}

			yield return new WaitForSeconds(0.5f);

			for(int i = 0; i < pins.Length; i++)
			{
				if(i % 2 != 0)
				{
					pins[i].GetComponentInChildren<Renderer>().material = colors[i / 9];
					pins[i].transform.GetChild(0).gameObject.SetActive(true);
					pins[i].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(i / 9);
				}
				else
				{
					pins[i].GetComponentInChildren<Renderer>().material = unlit;
					pins[i].transform.GetChild(0).gameObject.SetActive(false);
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

		mAudio.PlaySoundAtTransform("bip", transform);

		for(int i = 0; i < pins.Length; i++)
		{
			pins[i].GetComponentInChildren<Renderer>().material = colors[i / 9];
			pins[i].transform.GetChild(0).gameObject.SetActive(true);
			pins[i].transform.GetChild(0).GetComponentInChildren<Light>().color = GetColorVector(i / 9);
		}
		Vector3 givenDirection = Rnd.onUnitSphere;

		for (float x = 1; x >= 0; x -= Time.deltaTime)
        {
			float curDelay = Time.deltaTime;
			cube.transform.localScale = new Vector3(x, x, x);
            cube.transform.Rotate(360f * givenDirection * Time.deltaTime);
			yield return new WaitForSeconds(curDelay);
        }
		cube.SetActive(false);
	}
	IEnumerator RotateCube(int xVal, int zVal)
	{
		rotating = true;

		for(int i = 0; i < 9; i++)
		{
			cube.transform.RotateAround(rotator.transform.position, rotator.transform.right, 10f * xVal);
			cube.transform.RotateAround(rotator.transform.position, rotator.transform.forward, 10f * zVal);
			yield return new WaitForSeconds(0f);
		}

		rotating = false;
	}

    //twitch plays
    #pragma warning disable 414
		private readonly string TwitchHelpMessage = "Move in a specified direction with \"!{0} udlr\" (u = up, r = right, d = down, l = left). NESW directions can be used instead, \"move\" can be used to speed up the process. Press the submit button with \"!{0} enter/submit\" Restore the module to its initial face with \"!{0} reset\" You may use \"!{0} colorblind/colourblind\" to grab the center light LEDs in case of colorblindness";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
		if (moduleSolved)
        {
			yield return string.Format("sendtochaterror Are you trying to interact with the module when it's already solved. You might want to think again. (This is an anarchy command prevention message.)");
			yield break;
		}
        if (Regex.IsMatch(command, @"^\s*colou?rblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			colorblindDetected = true;
			foreach (var obj in colorblindIndicators)
                obj.gameObject.SetActive(true);
            yield return null;
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*(enter|submit)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { submitBtn };
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            cube.transform.localEulerAngles = new Vector3(xRot, yRot, zRot);
            pins[node].GetComponentInChildren<Renderer>().material = unlit;
            pins[node].transform.GetChild(0).gameObject.SetActive(false);
            node = nodestart;

            KeyValuePair<int, int> p;
            rotationMap.TryGetValue(new Vector3(xRot, yRot, zRot), out p);

            orientation = p.Value;

            pins[node].GetComponentInChildren<Renderer>().material = lit;
            pins[node].transform.GetChild(0).gameObject.SetActive(true);
            Debug.LogFormat("[Maze³ #{0}] Reset performed! Node is now back to initial position and face!", moduleId);
			Debug.LogFormat("[Maze³ #{0}] Move directly back to node: {1}", moduleId, node);
			yield break;
        }
		bool moveFast = false;
		if (Regex.IsMatch(command, @"^\s*m(ove)?\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			command = command.Substring(command.ToLower().StartsWith("move") ? 4 : 1).Trim();
			moveFast = true;
		}
		else if (Regex.IsMatch(command, @"^\s*walk?\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			command = command.Substring(4).Trim();
			moveFast = false;
		}
		string[] parameters = command.Split();
        List<KMSelectable> buttonsToPress = new List<KMSelectable>();
		List<int> tpDirections = new List<int>();
		Dictionary<KMSelectable, char[]> intereptedDirections = new Dictionary<KMSelectable, char[]>()
		{
            { btns[0], new[] {'u', 'U', 'n', 'N'} },
            { btns[3], new[] {'r', 'R', 'e', 'E'} },
			{ btns[1], new[] {'d', 'D', 's', 'S'} },
			{ btns[2], new[] {'l', 'L', 'w', 'W'} },
		};
		Dictionary<KMSelectable, int> loggingDirections = new Dictionary<KMSelectable, int> {
			{ btns[0], 0 },
			{ btns[3], 3 },
			{ btns[1], 1 },
			{ btns[2], 2 },

		};
        foreach (string b in parameters)
        {
			foreach (char c in b)
			{
				bool successful = false;
				foreach (KeyValuePair<KMSelectable,char[]> oneDirection in intereptedDirections)
                {
					if (oneDirection.Value.Contains(c))
                    {
						successful = true;
						buttonsToPress.Add(oneDirection.Key);
						tpDirections.Add(loggingDirections[oneDirection.Key]);
						break;
                    }
                }
				if (!successful)
				{
					yield return string.Format("sendtochaterror I do not know of a direction \"{0}\"", c);
					yield break;
				}
			}
        }
        if (!buttonsToPress.Any())
        {
			yield return string.Format("sendtochaterror Your command gave no directions to move in. Abandoning command.");
			yield break;
		}
		bool canPlayMusic = buttonsToPress.Count() >= 15 && !moveFast, isPlayingMusic = false;
		hasStruck = false;
		for (int i = 0; i < buttonsToPress.Count && !hasStruck; i++)
        {
            KMSelectable km = buttonsToPress[i];
			if (!IsSafeToWalk(tpDirections[i]))
            {
				yield return string.Format("strikemessage attempting to walk {0} on press #{1} in the command that was provided!", new string[] { "up", "down", "left", "right" }[tpDirections[i]], i + 1);
            }
            yield return null;
			if (canPlayMusic && !isPlayingMusic)
            {
				isPlayingMusic = true;
				yield return "waiting music";
			}
			km.OnInteract();
            //To prevent moves before animation
            do
            {
				yield return string.Format("trycancel Your command has been canceled after {0}/{1} presses.", i, buttonsToPress.Count);
			}
			while (rotating);
            yield return new WaitForSeconds(moveFast ? 0.1f : 0.3f);
        }
		if (isPlayingMusic)
			yield return "end waiting music";
	}
}
