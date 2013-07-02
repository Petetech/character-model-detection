using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void OnGUI()
	{
		if (GUI.RepeatButton(new Rect(25, 25, 75, 30), "Rotate"))
		{
			transform.Rotate(0,	60*Time.deltaTime, 0);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
