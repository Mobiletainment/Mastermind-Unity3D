using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//we want to present a custom editor component for all GameObjects with a MastermindPlayable attached
[CustomEditor(typeof(MastermindPlayable))]
public class MastermindEditorScript : Editor
{
	static List<GameObject> gameObjects = new List<GameObject>(); //these are the balls that are displayed in the editor
	static List<Texture2D> colors = new List<Texture2D>(); //these represent the colors dynamically created from the materials in resources, used for SelectionGrid in Inspector
	static List<Material> materials = new List<Material>(); //for assigning the different materials (colors) to the balls
	
	static List<int> selectedColorIndexes = new List<int>();
	static List<int> correctColorIndexes = new List<int>(); //this represents the correct colors which the player is going to guess
	static List<GameObject> matchObjects = new List<GameObject>();
	static int round = 0;
	private int width = 20; //the width of a color selection item in the SelectionGrid
	private int height = 15; //the height of a color selection item in the SelectionGrid
	
	public void OnEnable()
	{

	}
	
	public override void OnInspectorGUI()
	{
		GUILayout.Label("Wanna play a session Mastermind?");
		

		
		if (gameObjects.Count == 0) //only setup objects if they're not already set up!
		{
//			GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
//			foreach( GameObject go in objects )
//			{
//				if (go.tag == "Ball")
//				{
//    				DestroyImmediate(go);
//				}
//			}
			
			for (int i = 0; i < 6; i++)
			{
				Material material =  Resources.Load("Color" + (i+1).ToString(), typeof(Material)) as Material;
				materials.Add(material);
				
				Color materialColor = material.color;
				
				Texture2D color = new Texture2D(width, height); //create a colored texture to use it as a color preview in the SelectionGrid
				
				for (int k = 0; k < width; k++)
				{
					for (int j = 0; j < height; j++)
					{
						color.SetPixel(j, k, materialColor);
					}
				}
				color.Apply();
				colors.Add(color);
				//Debug.Log(material);
				//colors[i].width = ;
				//colors[i].height = 5;
				//colors[i].color = Color.red;
				//colors[i].SetPixels(new Color[1] { Color.red });
				//colors[i].SetPixel(0,0, Color.red);
				//colors[i].Apply();
				//colors[i] = Resources.Load("Color1", typeof(Material)) as Material;
				//Debug.Log(colors[i]);
				
			}
			
			//create game objects
			RestoreGameAndCreateInitialGameObjects();
		}
		
		//draw the Inspector Layout
		
		//draw the selection grid items
		for (int i = 0; i < 4 ; i++)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Color Ball {0}:", i+1));
				selectedColorIndexes[i] = GUILayout.SelectionGrid(selectedColorIndexes[i], colors.ToArray(), 6);
				gameObjects[round*4+i].renderer.material = materials[selectedColorIndexes[i]]; //assign the game object the chosen material
			GUILayout.EndHorizontal();
		}
		
		//draw the buttons
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Reset"))
		{
			Debug.Log("Button reset pressed!");
			RestoreGameAndCreateInitialGameObjects();
		}
				
		if (GUILayout.Button("Evaluate"))
		{
			Debug.Log("Button2 pressed");
			NextRound();
			
		}
		GUILayout.EndHorizontal();
		
		
	}
	
	protected void NextRound()
	{
		List<int> blackMatchList = new List<int>();
		List<int> whiteMatchList = new List<int>();
		
		int[] correctColorCounts = {0, 0, 0, 0, 0, 0};
		int[] chosenColorCounts = {0, 0, 0, 0, 0, 0};
		
		int[] blackColorCounts = {0, 0, 0, 0, 0, 0};
		int[] whiteColorCounts = {0, 0, 0, 0, 0, 0};
		
		for (int i = 0; i < 4; i++) //determine black matches and create new row
		{
			int selectedColor = selectedColorIndexes[i];
			
			if (correctColorIndexes[i] == selectedColor)
			{
				blackColorCounts[selectedColor]++;
				blackMatchList.Add(i);
			}
		}
		
		if (blackMatchList.Count == 4) //player has won!
		{
			GameObject winMessage = Instantiate(Resources.Load("WinMessage")) as GameObject; //Instantiate Prefab
			gameObjects.Add(winMessage); //add for cleanup
			return;
		}
		
		
		//determine white matches and add new game objects
		for (int i = 0; i < 4; i++)
		{
			chosenColorCounts[selectedColorIndexes[i]]++;		
			correctColorCounts[correctColorIndexes[i]]++;
			
			GameObject nextGameObject = Instantiate(gameObjects[round*4+i]) as GameObject;
			nextGameObject.isStatic = true;
			nextGameObject.name = "Ball" + (gameObjects.Count+1).ToString();
			nextGameObject.transform.position += new Vector3(2.0f, 0.0f, 0.0f);
			gameObjects.Add(nextGameObject);
		}
		
		for (int i =0; i < 6; i++)
		{
			whiteColorCounts[i] = correctColorCounts[i] - blackColorCounts[i];
		}
		
		for (int i = 0; i < 6; i++)
		{
			if (whiteColorCounts[i] > 0) //player has a white match, determine how many
			{
				if (chosenColorCounts[i] >=whiteColorCounts[i]+blackColorCounts[i])
					whiteMatchList.Add (whiteColorCounts[i]); 
			}
		}
		
		float matchPositionOffset = 2.0f;
		foreach(int match in blackMatchList)
		{
			GameObject goColorAndPositionMatch = Instantiate(Resources.Load("ColorAndPositionMatchBall")) as GameObject; //Instantiate Prefab
			goColorAndPositionMatch.isStatic = true;
			goColorAndPositionMatch.transform.position = gameObjects[round*4].transform.position + new Vector3(0.0f, 0.0f, matchPositionOffset);
			matchPositionOffset += 0.75f;
			Debug.Log("Match");
			matchObjects.Add(goColorAndPositionMatch);
		}
		
		foreach(int match in whiteMatchList)
		{
			GameObject goColorAndPositionMatch = Instantiate(Resources.Load("ColorMatchBall")) as GameObject; //Instantiate Prefab
			goColorAndPositionMatch.isStatic = true;
			goColorAndPositionMatch.transform.position = gameObjects[round*4].transform.position + new Vector3(0.0f, 0.0f, matchPositionOffset);
			matchPositionOffset += 0.75f;
			Debug.Log("Match");
			matchObjects.Add(goColorAndPositionMatch);
		}
		
		++round;
	}

	protected void RestoreGameAndCreateInitialGameObjects()
	{
		foreach(GameObject go in gameObjects)
		{	
			DestroyImmediate(go);
		}
		
		foreach(GameObject go in matchObjects)
		{
			DestroyImmediate(go);
		}
			
		gameObjects.Clear();
		matchObjects.Clear();
		selectedColorIndexes.Clear();
		correctColorIndexes.Clear();
		
		round = 0;
		
		for (int i = 0; i < 4; i++)
		{
			AddGameObject(i);
			selectedColorIndexes.Add(i);
			correctColorIndexes.Add(Random.Range(0, 6));
			gameObjects[i].renderer.material = materials[selectedColorIndexes[i]];
		}
	}
	
	protected void AddGameObject(int i)
	{
		GameObject gameObject = Instantiate(Resources.Load("Ball")) as GameObject; //Instantiate Prefab
		gameObject.isStatic = true; //really important, otherwise they're always recreated!
		gameObject.name = "Ball" + (gameObjects.Count+1).ToString();
		gameObject.transform.position = new Vector3(0.0f, 0.0f, -i*2.0f); //order from left to right in z-axis
		gameObject.renderer.material =  Resources.Load("Color" + (i+1).ToString(), typeof(Material)) as Material; //assign a material from the resources
		gameObjects.Add(gameObject);
	}
	
}
