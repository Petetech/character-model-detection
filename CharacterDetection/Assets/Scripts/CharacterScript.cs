using UnityEngine;
using System.Collections;
using System.IO;

public class CharacterScript : MonoBehaviour {
	
	#region Fields
	
	public Renderer target;
	public MenuScript script;
	
	// Rotate
	float yAngle, yStep;
	
	// Tilt
	bool tiltFlag;
	float xAngle, xStep, maxT;
	
	// File / Screenshot variables
	string path, fileType = "image", filename;
	int fileCount = 0;
	
	// Stages/loops
	int stageCount = 4;
	int loopCount = 0;

	#endregion // Fields
	
	#region Methods
	
	void Start()
	{
		// Link the MenuScript
		script = Camera.main.GetComponent<MenuScript>();
		
		// Grab the renderer from the object this script is attached to
		target = gameObject.GetComponentInChildren<Renderer>();
	}
	
	// Without Tilt
	public void Initialise(float y, bool x, string l, int c)
	{
		yStep = y;
		tiltFlag = x;
		path = l;
		fileCount = c;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// With Tilt
	public void Initialise(float y, bool x, float xS, float mT, string l, int c)
	{
		xStep = xS;
		maxT = mT;

		Initialise(y, x, l, c);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (stageCount < 2)
		{
			// Decide whether this loop is colour or mask
			if (loopCount % 2 == 0)
			{
				target.material.color = Color.white; // colour
				fileType = "image";
				
				// Rotate on even
				RotateChar();
			}
			else
			{
				target.renderer.material.color = Color.black; // mask
				fileType = "mask";
			}
			
			// After one full turn
			if (yAngle >= 360)
			{
				yAngle = 0;
			
				// Turn tilt on/off via menu
				if (tiltFlag)
				{
					TiltChar();
				}
				else
				{
					// If there are to be no tilts, skip straight to new char/animations
					stageCount = 2;
				}
				
				// INSERT ANIMATION CHANGE STAGE HERE AS STAGE 2
				
			}
			
			// Once all 360s are completed call for next character
			if (stageCount == 2)
			{
				script.ChangeChar(fileCount);
			}
			
		}
		
		// To add animations use something like this 
 
//	 	if (stageCount == X)
//	 	{
//	 		CallNewAnimationMethod(aniCount);
//	 		aniCount++;
//			stageCount = 0;
//		}

	}
	
	void RotateChar()
	{
		// Wait for first 2 loops
		if (loopCount > 1)
		{
			this.transform.Rotate(0, yStep, 0);
			yAngle += yStep;
		}
	}
	
	void TiltChar()
	{
		// Check if max has been hit
		if (xAngle >= maxT || xAngle <= -maxT)
		{
			xAngle = 0;
			this.transform.eulerAngles = new Vector3(0, 0, 0);
			stageCount++;
		}
		
		// add tilt
		if (stageCount == 0)
		{
			this.transform.Rotate(xStep, 0, 0);
			xAngle += xStep;
		}
		
		// swap to opposite tilt
		if (stageCount == 1)
		{
			this.transform.Rotate(-xStep, 0, 0);
			xAngle -= xStep;
		}	
	}
	
	
	void LateUpdate()
	{
		// prevent constant screenshots
		if (stageCount < 2)
		{
			filename = ScreenShotName();
			
			// full 'loop'
			loopCount++;
			
			StartCoroutine(TakeScreenshot());
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
		
		
		//Adds .txt File
		
		TextWriter tw = new StreamWriter(filename+".txt");
		Vector3 objectPos = Camera.main.WorldToScreenPoint(this.transform.GetChild(1).position);
		tw.WriteLine("{2}x is: {0}, y is : {1}", objectPos.x, objectPos.y,this.transform.GetChild(1).name);
		tw.Close();
		
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
