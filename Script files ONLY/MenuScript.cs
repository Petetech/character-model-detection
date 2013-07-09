using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour {
	
	#region Fields
	
	List<GameObject> charList = new List<GameObject>();
	string cListStr;
	
	public GameObject current;
	int charCount = 0;
	
	bool MenuActive = true;

	Rect MenuRect = new Rect(0, 0, 250, 200);
	Rect ListRect = new Rect(0, Screen.height - 150, 250, 150);
	
	float rotateStep = 15f;
	bool tiltOnOff = false;
	float tiltStep = 5f;
	float maxTilt = 10;
	string fileLoc = @"C:\Images";
	
	#endregion Fields
	
	#region Methods
	
	// Use this for initialization
	void Start() {
		// Call Load
		LoadFromResources();
	}
	
	void LoadFromResources()
	{
		charList.Clear();
		cListStr = "";
		
		// Load up all current characters/prefabs in resources
		Object[] prefabs = Resources.LoadAll("Characters", typeof(GameObject));
		
		// Add them to a main list
		foreach (Object o in prefabs)
		{
			charList.Add((GameObject)o);
			cListStr += o.name + "\r\n";
		}
	}
	
	// Display the menus
	void OnGUI()
	{
		if (MenuActive)
		{
			GUI.Window(0, MenuRect, MenuWindow, "Menu");
			GUI.Window(1, ListRect, ListWindow, "Current Characters");
		}
	}
	
	void MenuWindow(int ID)
	{
		GUILayout.BeginArea (new Rect(20, 30, 210, 160));
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Rotation Step: " + rotateStep.ToString("f0").PadLeft(2, '0'), GUILayout.Width(120));
		rotateStep = GUILayout.HorizontalSlider(rotateStep, 1f, 35f);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Tilt On/Off", GUILayout.Width(120));
		tiltOnOff = GUILayout.Toggle(tiltOnOff, " Add Tilts");
		GUILayout.EndHorizontal();
		
		if (tiltOnOff)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Tilt Step: " + tiltStep.ToString("f0").PadLeft(2, '0'), GUILayout.Width(120));
			tiltStep = GUILayout.HorizontalSlider(tiltStep, 1f, 10f);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max Tilt: " + maxTilt.ToString("f0").PadLeft(2, '0'), GUILayout.Width(120));
			maxTilt = GUILayout.HorizontalSlider(maxTilt, tiltStep, 30f); 
			GUILayout.EndHorizontal();
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Save Location", GUILayout.Width(120));
		fileLoc = GUILayout.TextField(fileLoc);
		GUILayout.EndHorizontal();
		
		if (GUILayout.Button("Start", GUILayout.Width(80), GUILayout.Height(25)))
		{
			// Close windows
			MenuActive = false;
			
			// Set first character
			ChangeChar();
			
			// Call Start on filecount 0
			StartChar(0);
			
		}
		
		GUILayout.EndArea();
	}
	
	void ListWindow(int ID)
	{
		Vector2 scrollPosition = Vector2.zero;
		
		GUILayout.BeginArea (new Rect(10, 20, 230, 130));
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Height(80));
		GUILayout.TextArea(cListStr, GUILayout.Height(80));
		GUILayout.EndScrollView();
		
		if(GUILayout.Button ("Refresh", GUILayout.Width(80)))
		{
			LoadFromResources();
		}
		
		GUILayout.EndArea();
	}
	
	// Overloaded to prevent repeat code and for the first character
	public void ChangeChar()
	{
		Destroy(current);
		current = (GameObject)Instantiate(charList[charCount]);
		current.AddComponent("CharacterScript");
		
		charCount++;
	}
	
	// For new chars
	public void ChangeChar(int newFileCount)
	{
		if (charCount < charList.Count)
		{
			ChangeChar();
	
			StartChar(newFileCount);
		}
		else
		{
			charCount = 0;
			MenuActive = true;
		}
		
	}
	
	public void StartChar(int fC)
	{
		CharacterScript newScript = current.GetComponent<CharacterScript>();
		
		if (tiltOnOff)
		{
			newScript.Initialise(rotateStep, tiltOnOff, tiltStep, maxTilt, fileLoc, fC);
		}
		else
		{
			newScript.Initialise(rotateStep, tiltOnOff, fileLoc, fC);
		}
	}
			
	// Update is called once per frame
	void Update() {
	
	}
	
	#endregion Methods
}
