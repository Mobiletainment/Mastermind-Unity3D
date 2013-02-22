using UnityEngine;
using System.Collections;

public class MastermindPlayable : MonoBehaviour {
	
	public GameObject First;
	
	// Use this for initialization
	void Start ()
	{	
		Debug.Log("Start");
	}
	
	public void DrawObjects()
	{
		Debug.Log(this.gameObject);
		
		
	}
	
	void Awake()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
