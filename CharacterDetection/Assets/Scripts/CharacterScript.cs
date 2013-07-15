using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CharacterScript : MonoBehaviour {
	
	#region Fields
	
	public Renderer target;
	public Animator charAnimator;
	public MenuScript script;
	
	// Rotate
	float yAngle, yStep;
	
	// Tilt
	bool tiltFlag;
	float xAngle, xStep, maxT;
	
	// Animations
	Object[] animations;
	float aStep, currTime;
	int animCount = 0;
	
	// File / Screenshot variables
	string path, fileType = "image", filename;
	int fileCount = 0;
	
	// Point display
	bool pxFlag;
	List<Vector3> points = new List<Vector3>();
	Texture2D brush;
	int size = 8;
	
	// Stages/loops
	int stageCount = 4;
	int loopCount = 0;

	#endregion
	
	#region Initialise Overloads
	// Without Tilt
	public void Initialise(float y, bool x, string l, bool p, float a, int c)
	{
		yStep = y;
		tiltFlag = x;
		path = l;
		pxFlag = p;
		aStep = a;
		fileCount = c;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// With Tilt
	public void Initialise(float y, bool x, float xS, float mT, string l, bool p, float a, int c)
	{
		xStep = xS;
		maxT = mT;

		Initialise(y, x, l, p, a, c);
	}
	#endregion
	
	#region Methods
	
	void Start()
	{
		// Create the point brush
		if (pxFlag)
		{
			brush = new Texture2D(size, size);
		    CreateTex(Color.green);
		}
		
		// Link the MenuScript
		script = Camera.main.GetComponent<MenuScript>();
		
		// Grab the renderer from the object this script is attached to
		target = gameObject.GetComponentInChildren<Renderer>();
		
		// Locate the animations
		animations = Resources.LoadAll("Animations", typeof(AnimationClip));
		
		SetAnimator();
	}
	
	// create the 4 pixel wide blocks
	void CreateTex(Color c)
	{
	    for (int x = 0; x < size; x++)
	        for (int y = 0; y < size; y++)
	         	brush.SetPixel(x, y, c);
		
	    brush.Apply();
	}
	
	void OnGUI()
	{
		// Add points to screen
		foreach (Vector3 v in points)
		{
			GUI.Label(new Rect(v.x, Screen.height - v.y, size, size), brush);	
		}	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Keep character in shot
		transform.position = new Vector3(0, 0, 0);
		
		if (stageCount < 3)
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

				if (stageCount == 2)
				{
					// For new/next animation
					stageCount = 0;
					
					// Go to next animation step
					if (currTime < 1.0f)
					{
						// Step the animation
						AnimateChar();
					}
					else 
						if (animCount == animations.Length)
						{
							// If last animation has passed jump to char change
							stageCount = 3;
						}
						else
						{
							// Switch to next animation
							SetAnimator();
							
						}
	
				}
				
			}
			
			// Once all 360s are completed call for next character
			if (stageCount == 3)
			{
				script.ChangeChar(fileCount);
			}
		}
	}
	
	void LateUpdate()
	{
		// prevent constant screenshots
		if (stageCount < 3)
		{
			filename = ScreenShotName();
			
			// full 'loop'
			loopCount++;
			
			StartCoroutine(TakeScreenshot());
		}
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
	
	void AnimateChar()
	{
		currTime += aStep;
		
		// Advances animation to next step
		charAnimator.ForceStateNormalizedTime(currTime);	
	}
	
	// Create and assign controller and animation
	void SetAnimator()
	{
		// Add the component if it's not already there
		if (gameObject.GetComponent<Animator>() == null)
			gameObject.AddComponent<Animator>();
		
		// Get the Animator
		charAnimator = (Animator)gameObject.GetComponent<Animator>();
		
		// Set speed to 0 so no real animation takes place
		charAnimator.speed = 0.0f;
		
		// Create the controller
		UnityEditorInternal.AnimatorController aController = new UnityEditorInternal.AnimatorController();
		aController.name = "animation_controller";
		
		// Add a default layer
		if (aController.GetLayerCount() == 0)
			aController.AddLayer("Base");
		
		// Create state and apply
		StateMachine sm = new StateMachine();
		sm.AddState("default");
		
		// Add clip
		sm.GetState(0).SetMotion(0, (AnimationClip)animations[animCount]);
		animCount++;
		
		aController.SetLayerStateMachine(0, sm);
		
		// Set the Controller
		UnityEditorInternal.AnimatorController.SetAnimatorController(charAnimator, aController);
		
		// Get normalized time
		currTime = charAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
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
        Transform [] allChildren = this.transform.GetComponentsInChildren<Transform>();
		TextWriter tw = new StreamWriter(filename+".txt");
		
		// Clear before filling
		if (pxFlag)
			points.Clear();
		
		foreach (Transform child in allChildren)
		{
			bool check = CheckParts(child);
			if (check == true)
			{
				Vector3 objectPos = Camera.main.WorldToScreenPoint(child.transform.position);
				tw.WriteLine("Name: {0} X: {1} Y: {2}", child.transform.name, objectPos.x, objectPos.y);
				
				// Add point to screen
				points.Add(objectPos);
			}
			else
			{
				continue;
			}
		}
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
	
	bool CheckParts(Transform t)
	{
		List<string> points = new List<string>();
		points.Add("Hips");
		points.Add("LeftUpLeg");
		points.Add("LeftLeg");
		points.Add("LeftFoot");
		points.Add("RightUpLeg");
		points.Add("RightLeg");
		points.Add("RightFoot");
		points.Add("LeftShoulder");
		points.Add("LeftArm");
		points.Add("LeftForeArm");
		points.Add("LeftHand");
		points.Add("Head");
		points.Add("RightShoulder");
		points.Add("RightArm");
		points.Add("RightForeArm");
		points.Add("RightHand");
		string temp = t.transform.name;

		if (points.Contains(temp))
		{
			return true;
		}
			
		else
		{
			return false;
		}
		
	}
	
	#endregion
}
