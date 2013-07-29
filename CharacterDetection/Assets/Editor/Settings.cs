using UnityEngine;
using UnityEditor;

public class Settings : EditorWindow 
{
	string tempw = PlayerSettings.defaultScreenWidth.ToString(),
		   temph = PlayerSettings.defaultScreenHeight.ToString();
	
	Transform cam;
	Transform lig;
	
	Vector2 scrollPosition;
	
	bool foldBool = true;
		
	[MenuItem ("Character Detection/Set up program" , false, 0)]
	public static void Init()
	{
		Setup();
		
		EditorWindow.GetWindow<Settings>("Settings", typeof(SceneView));	
	}
	
	public static void Setup()
	{
		PlayerSettings.defaultScreenHeight = 640;
		PlayerSettings.defaultScreenWidth = 405;
		
		GameObject source = GameObject.Find("Light Source");
		// Automatically add a lightsource
		if (source == null)
		{
			source = new GameObject("Light Source");
			source.AddComponent(typeof(Light));
			
			// As default
			source.light.type = LightType.Directional;
			source.light.intensity = 0.5f;
		}
		
		// Set the default positions
		source.transform.position = new Vector3(0f, 3f, 0f);
		Camera.main.transform.position = new Vector3(0f, 1f, -2.5f);
		
		// Add the first script
		if (Camera.main.GetComponent<MenuScript>() == null)
			Camera.main.gameObject.AddComponent<MenuScript>();
		
		if (Camera.main.backgroundColor != Color.white)
			Camera.main.backgroundColor = Color.white;
		
		Layout.LoadLayoutHack();
	}
	
	void OnGUI()
	{
		cam = Camera.main.transform;
		lig = GameObject.Find("Light Source").GetComponent<Transform>();
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		{
			GUILayout.Label("Screenshot Settings", EditorStyles.boldLabel);
			GUILayout.Space(5f);
			
			tempw = EditorGUILayout.TextField("Screenshot Width" , tempw);
			
			temph = EditorGUILayout.TextField("Screenshot Height" , temph);
			
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if (GUILayout.Button("Apply Settings", GUILayout.Width(120)))
				{
					PlayerSettings.defaultScreenWidth = System.Convert.ToInt16(tempw);
					PlayerSettings.defaultScreenHeight = System.Convert.ToInt16(temph);
					
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5f);
			
			GUILayout.Label("Common Settings", EditorStyles.boldLabel);
			
			// Foldout
			foldBool = EditorGUILayout.Foldout(foldBool, " Camera and Lighting");
			if (foldBool)
			{
				cam.position = EditorGUILayout.Vector3Field("Camera Position", cam.position);
				
				cam.eulerAngles = EditorGUILayout.Vector3Field("Camera Rotation", cam.eulerAngles);
				
				GUILayout.Space(10f);
				//Light type
				lig.position = EditorGUILayout.Vector3Field("Light Position", lig.position);

				lig.eulerAngles = EditorGUILayout.Vector3Field("Light Rotation", lig.eulerAngles);
			
			}
			//End Foldout
			
			GUILayout.Space(10f);
		}
		EditorGUILayout.EndScrollView();
		Repaint();
	}
}

public static class Layout
{
	[MenuItem("Character Detection/Save Layout", false, 50)]
    static void SaveLayoutHack() {
        // Saving the current layout to an asset
        LayoutUtility.SaveLayoutToAsset("Assets/Editor/Layout/Main Layout.wlt");
    }
 
    [MenuItem("Character Detection/Load Layout", false, 51)]
    public static void LoadLayoutHack() {
        // Loading layout from an asset
        LayoutUtility.LoadLayoutFromAsset("Assets/Editor/Layout/Main Layout.wlt");
    }
}