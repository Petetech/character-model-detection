using System;
using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {
	
	public CharacterScript chr;
	bool setActive = true;

	Rect winRect = new Rect((Screen.width / 2) -150, (Screen.height / 2) - 100, 300, 200);
	
	float sliderValueY = 5f;
	string fileNoStr = "1000";
	string fileLoc = @"C:\Images";
	
	// Use this for initialization
	void Start() {
		
	}
	
	// Display the menu
	void OnGUI()
	{
		if (setActive)
		{
			GUI.Window(0, winRect, MenuWindow, "Menu");
		}
	}
	
	void MenuWindow(int ID)
	{
		GUILayout.BeginArea (new Rect(20, 30, 260, 160));
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Angle Step Y: " + sliderValueY.ToString("f0").PadLeft(2, '0'), GUILayout.Width(120));
		sliderValueY = GUILayout.HorizontalScrollbar(sliderValueY, 5, 5f, 35f);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Maximum Files", GUILayout.Width(120));
		fileNoStr = GUILayout.TextField(fileNoStr); 
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("File Location", GUILayout.Width(120));
		fileLoc = GUILayout.TextField(fileLoc);
		GUILayout.EndHorizontal();
		
		if (GUILayout.Button("Click", GUILayout.Width(80)))
		{
			setActive = false;
			chr.Initialise(sliderValueY, Convert.ToInt32(fileNoStr), fileLoc);
		}
		
		GUILayout.EndArea();
	}
			
	// Update is called once per frame
	void Update() {
	
	}
	
}
