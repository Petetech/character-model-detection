using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour {
	
	#region Fields
	
	List<GameObject> charList = new List<GameObject>();
	public GameObject current;
	int charCount = 0;
	
	bool MenuActive = true;

	Rect MenuRect = new Rect((Screen.width / 2) -125, 0, 250, 200);
	Rect ListRect = new Rect((Screen.width / 2) - 125, 200, 250, 150);
	
	float sliderValueY = 15f;
	bool tiltOnOff = false;
	string fileNoStr = "2000";
	string fileLoc = @"C:\Images";
	
	#endregion Fields
	
	#region Methods
	
	// Use this for initialization
	void Start() {
		
		// Load up all current characters/prefabs in resources
		Object[] prefabs = Resources.LoadAll("Characters", typeof(GameObject));
		
		// Add them to a main list
		foreach (Object o in prefabs)
		{
			charList.Add((GameObject)o);	
		}
		
		// Set first character
		current = (GameObject)Instantiate(charList[0]);
		current.AddComponent("CharacterScript");
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
		GUILayout.Label("Rotation Step: " + sliderValueY.ToString("f0").PadLeft(2, '0'), GUILayout.Width(120));
		sliderValueY = GUILayout.HorizontalScrollbar(sliderValueY, 5, 1f, 35f);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Tilt On/Off", GUILayout.Width(120));
		tiltOnOff = GUILayout.Toggle(tiltOnOff, " Add Tilts");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Maximum Files", GUILayout.Width(120));
		fileNoStr = GUILayout.TextField(fileNoStr); 
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("File Location", GUILayout.Width(120));
		fileLoc = GUILayout.TextField(fileLoc);
		GUILayout.EndHorizontal();
		
		if (GUILayout.Button("Start", GUILayout.Width(80), GUILayout.Height(50)))
		{
			// Close windows
			MenuActive = false;
			
			// Call Start on filecount 0
			StartNewChar(0);
			
		}
		
		GUILayout.EndArea();
	}
	
	void ListWindow(int ID)
	{
		string info = "";
		
		foreach (GameObject G in charList)
		{
			info += G.name + "\r\n";
		}
		
		Vector2 scrollPosition = Vector2.zero;
		
		GUILayout.BeginArea (new Rect(10, 20, 230, 130));
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Height(80));
		GUILayout.TextArea(info, GUILayout.Height(80));
		GUILayout.EndScrollView();
		
		if(GUILayout.Button ("Load new", GUILayout.Width(80)))
		{
			// add function	to load more prefabs here
		}
		
		GUILayout.EndArea();
	}
	
	public void ChangeChar(int newFileCount)
	{
		if (charCount < charList.Count - 1)
		{
			charCount++;
			Destroy(current);
			current = (GameObject)Instantiate(charList[charCount]);
			current.AddComponent("CharacterScript");
			
			StartNewChar(newFileCount);
		}
		else
		{
			Destroy(current);
			MenuActive = true;
		}
	}
	
	void StartNewChar(int fC)
	{
		current.GetComponent<CharacterScript>().Initialise(sliderValueY, tiltOnOff, System.Convert.ToInt32(fileNoStr), fileLoc, fC);
	}
			
	// Update is called once per frame
	void Update() {
	
	}
	
	#endregion Methods
}
