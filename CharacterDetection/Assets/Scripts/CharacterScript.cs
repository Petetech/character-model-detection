using UnityEngine;
using System.Collections;
using System.IO;

public class CharacterScript : MonoBehaviour {
	
	#region Fields
	
	public Renderer target;
	
	float yAngle, yStep;
	bool tiltFlag;
	bool nextStage;
	
	// File / Screenshot variables
	int maxFiles;
	string path, fileType = "image", filename;
	int fileCount = 0;
	
	// Stages/loops
	int stageCount = 4;
	int loopCount = 0;

	#endregion // Fields
	
	#region Methods
	
	void Start()
	{
		// Grab the renderer from the object this script is attached to
		target = gameObject.GetComponentInChildren<Renderer>();
	}
	
	public void Initialise(float y, bool x, int f, string l, int c)
	{
		
		yStep = y;
		tiltFlag = x;
		maxFiles = f;
		path = l;
		fileCount = c;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (stageCount < 3)
		{
			// Decide whether this loop is colour or mask
			if (loopCount % 2 == 0)
			{
				target.material.color = Color.white; // colour
				fileType = "image";
			}
			else
			{
				target.renderer.material.color = Color.black; // mask
				fileType = "mask";
				
				// Rotate on odd
				transform.Rotate(0, yStep, 0);
				yAngle += yStep;
			}
			
			// After one full turn
			if (yAngle >= 360)
			{
				stageCount++;
				yAngle = 0;
				
				// Used to prevent the statements triggering more than once
				nextStage = true;
			}
			
			// Turn tilt on/off via menu
			// However this can be adapted to work the same way as the regular rotation
			if (tiltFlag)
			{
				// add tilt
				if (stageCount == 1 && nextStage)
				{
					transform.eulerAngles = new Vector3(30, 0, 0);
					nextStage = false;
				}
				
				// swap to opposite tilt
				if (stageCount == 2 && nextStage)
				{
					transform.eulerAngles = new Vector3(-30, 0, 0);
					nextStage = false;
				}
			}
			else if (nextStage)
			{
				// This is if there are to be no tilts, skip straight to new char
				stageCount = 3;	
			}
			
			// INSERT ANIMATION CHANGE STAGE HERE AS STAGE 3
			
			// Once all 360s are completed call for next character
			if (stageCount == 3 && nextStage)
			{
				Camera.main.GetComponent<MenuScript>().ChangeChar(fileCount);
				nextStage = false;	
			}
			
		}
		
		// To add animations use something like this 
 
//	 	if (stageCount == X && nextStage)
//	 	{
//	 		CallNewAnimationMethod(aniCount);
//	 		aniCount++;
//			stageCount = 0;
//		}

	}
	
	void LateUpdate()
	{
		// prevent constant screenshots
		if (stageCount < 3)
		{
			filename = ScreenShotName();
			
			// full 'loop'
			loopCount++;
			
			if (fileCount < maxFiles)
			{
				StartCoroutine(TakeScreenshot());
			}
		}
	}
	
	
	public string ScreenShotName()
	{
		if (loopCount % 2 == 0)
			fileCount++;
		
		return string.Format(@"{0}\{1}{2}.png", path, fileType, fileCount.ToString().PadLeft(5,'0'));	
	}
	
	// Take screenshot here
	private IEnumerator TakeScreenshot()
	{
		// wait for render
		yield return new WaitForEndOfFrame();
		
		// image format and size - POSSIBLE CHANGE to crop whitespace
		RenderTexture rt = new RenderTexture(Screen.width , Screen.height, 24);
		Texture2D shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		// Select main camera
		Camera.main.targetTexture = rt;
		Camera.main.Render();
		RenderTexture.active = rt;
		
		// take shot
		shot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		
		// reset
		Camera.main.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);
		
		yield return 0;
		
		// Create file
		byte[] bytes = shot.EncodeToPNG();
		File.WriteAllBytes(filename, bytes);
		
	}
	
	#endregion Methods
}
