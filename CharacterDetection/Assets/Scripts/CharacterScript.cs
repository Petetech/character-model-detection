using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CharacterScript : MonoBehaviour {
	
	#region Fields
	
	// Components
	public Renderer target;
	public Animator charAnimator;
	public MenuScript script;
	public Transform[] allChildren;
	
	// Skeleton objects
	List<SkeletonItem> skeletonList = new List<SkeletonItem>();
	Skeleton currentSkeleton = new Skeleton();
	float disThreshold;
	
	// Rotate
	float yAngle, yStep;
	
	// Tilt
	bool tiltFlag;
	float xAngle, xStep, maxT;
	
	// Animations
	Object[] animations;
	float aStep, currTime;
	int animCount = 0;
	bool pauseFlag = true;
	
	// File / Screenshot variables
	string path, fileType = "image", filename;
	int fileCount = 0;
	
	// Point display
	bool pxFlag;
	Texture2D brush;
	int size = 4;
	
	// Stages/loops
	int stageCount = 4;
	int loopCount = 0;

	#endregion
	
	#region Initialise Overloads
	
	// Without Tilt
	public void Initialise(float y, bool x, string l, bool p, float a, float v, int c)
	{
		yStep = y;
		tiltFlag = x;
		path = l;
		pxFlag = p;
		aStep = a;
		disThreshold = v;
		fileCount = c;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// With Tilt
	public void Initialise(float y, bool x, float xS, float mT, string l, bool p, float a, float v, int c)
	{
		xStep = xS;
		maxT = mT;

		Initialise(y, x, l, p, a, v, c);
	}
	
	#endregion
	
	#region MonoBehaviour Methods
	
	void Awake()
	{
		// Create the point brush
		brush = new Texture2D(size, size);
		CreateTex(Color.green);
	}
	
	// create the blocks
	void CreateTex(Color c)
	{
	    for (int x = 0; x < size; x++)
	        for (int y = 0; y < size; y++)
	         	brush.SetPixel(x, y, c);
		
	    brush.Apply();
	}
	
	void Start()
	{
		// Link the MenuScript
		script = Camera.main.GetComponent<MenuScript>();
		
		// Grab the renderer from the object this script is attached to
		target = gameObject.GetComponentInChildren<Renderer>();
		
		// Get all the characters parts
		allChildren = gameObject.transform.GetComponentsInChildren<Transform>();
		
		// Locate the animations
		animations = Resources.LoadAll("Animations", typeof(AnimationClip));
		
		SetAnimator();
	}
	
	void OnGUI()
	{
		// Add points to screen
		if (pxFlag)
		{
			foreach (Vector3 v in currentSkeleton.GetParts())
			{
				GUI.DrawTexture(new Rect(v.x, (Screen.height - v.y) - 1, 2, 2), brush);	
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{		
		// Keep character in shot
		transform.position = new Vector3(0, 0, 0);
		
		if (stageCount < 3)
		{
			// colour or mask
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
					do
					{
						AnimateChar();
						currentSkeleton.SetParts(allChildren);
					} 
					while (CompareParts(currentSkeleton) && stageCount != 3);
				}
				
			}
			
			// Collect the point locations
            currentSkeleton.SetParts(allChildren);

			// Once all 360s are completed call for next character
			if (stageCount == 3)
			{
				script.ChangeChar(fileCount);
			}
		}
	}
	
	void LateUpdate()
	{
		// skip first screenshot of every pose
		if (!pauseFlag)
		{
			filename = ScreenShotName();
			StartCoroutine(TakeScreenshot());
			
			// full 'loop'
			loopCount++;
		}
		
		pauseFlag = false;
	}
	
	#endregion
	
	#region Extra Methods
	
	void RotateChar()
	{
		// Wait for first 2 screenshots
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
		pauseFlag = true;
		loopCount = 0;
		stageCount = 0;
		
		if (currTime < 1.0f)
		{
			currTime += aStep;
			
			// Advances animation to next step
			charAnimator.ForceStateNormalizedTime(currTime);
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
	
	// Create and assign controller and animation
	void SetAnimator()
	{
		// Add the component if it's not already there
		if (gameObject.GetComponent<Animator>() == null)
			gameObject.AddComponent<Animator>();
		
		charAnimator = (Animator)gameObject.GetComponent<Animator>();
		
		// Set speed to 0 so no real animation takes place
		charAnimator.speed = 0.0f;
		
		UnityEditorInternal.AnimatorController aController = new UnityEditorInternal.AnimatorController();
		aController.name = "animation_controller";
		
		if (aController.GetLayerCount() == 0)
			aController.AddLayer("Base");
		
		StateMachine sm = new StateMachine();
		sm.AddState("default");
		
		// Add clip
		sm.GetState(0).SetMotion(0, (AnimationClip)animations[animCount]);
		animCount++;
		
		aController.SetLayerStateMachine(0, sm);
		
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
		
		// image format and size
		Texture2D shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		//Adds .txt File
		if (fileType == "image")
		{
			TextWriter tw = new StreamWriter(string.Format(@"{0}\{1}{2}.txt", path, "points", fileCount.ToString().PadLeft(5,'0')));
			tw.Write(currentSkeleton.GetPartsWithName());
			tw.Close();
		}
		
		// take shot
		shot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		
		yield return 0;
		
		// Create file
		byte[] bytes = shot.EncodeToPNG();
		File.WriteAllBytes(filename, bytes);
		DestroyObject(shot);
	}
	#endregion
	
	#region Scoring and Points System
	
	bool CompareParts(Skeleton currentSet)
	{
		List<Vector3> compareList;
		List<Vector3> newList = currentSet.GetParts();
		
		float score;
		bool isSame = false;
		
		for (int i = 0; i < skeletonList.Count; i++)
		{
			score = 0;
			compareList = skeletonList[i].GetParts();
			
			for (int i2 = 0; i2 < newList.Count; i2++)
			{
				score += Vector3.Distance(newList[i2], compareList[i2]);
			}
			
			if (score < disThreshold)
			{
				isSame = true;
			}
		}
		
		if (isSame == false || skeletonList.Count == 0)
		{
			// Add to list
			skeletonList.Add(new SkeletonItem(newList));
			return false;
		}
		else
		{
			return true;
		}
	}
}

// Collection of point data, values will be changing all the time
public class Skeleton
{
	string fileoutput;
	List<string> pointnames;
	List<Vector3> coords = new List<Vector3>();
	
	
	public Skeleton()
	{
		pointnames = new List<string>();
		pointnames.Add("Hips");
		pointnames.Add("LeftUpLeg");
		pointnames.Add("LeftLeg");
		pointnames.Add("LeftFoot");
		pointnames.Add("RightUpLeg");
		pointnames.Add("RightLeg");
		pointnames.Add("RightFoot");
		pointnames.Add("LeftShoulder");
		pointnames.Add("LeftArm");
		pointnames.Add("LeftForeArm");
		pointnames.Add("LeftHand");
		pointnames.Add("Head");
		pointnames.Add("RightShoulder");
		pointnames.Add("RightArm");
		pointnames.Add("RightForeArm");
		pointnames.Add("RightHand");	
	}
	
	public void SetParts(Transform[] allChildren)
	{
		coords.Clear();
		fileoutput = "";
		
		foreach (Transform child in allChildren)
		{
			bool check = CheckPart(child);
			if (check == true)
			{
				Vector3 objectPos = Camera.main.WorldToScreenPoint(child.transform.position);
				fileoutput += string.Format("Name: {0} X: {1} Y: {2}\r\n", child.transform.name, objectPos.x, objectPos.y);
				
				coords.Add(objectPos);
			}
			else
			{
				continue;
			}
		}
		extrapolatePoints(allChildren);
	}
	
	public List<Vector3> GetParts()
	{
		return coords;
	}
	
	public string GetPartsWithName()
	{
		return fileoutput;
	}
	
	bool CheckPart(Transform t)
	{
		string temp = t.transform.name;

		if (pointnames.Contains(temp))
		{
			return true;
		}
			
		else
		{
			return false;
		}
		
	}
	
	void extrapolatePoints(Transform[] allChildren)
	{
		foreach (Transform childA in allChildren)
		{
			foreach (Transform childB in allChildren)
			{
				if ((childA.name == "Hips")&&(childB.name == "Head"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: Chest(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);						
				}
				else if ((childA.name == "LeftUpLeg")&&(childB.name == "LeftLeg"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: LeftThigh(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				
				else if ((childA.name == "LeftLeg")&&(childB.name == "LeftFoot"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: LeftShin(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "RightUpLeg") && (childB.name == "RightLeg"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: RightThigh(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "RightLeg") && (childB.name == "RightFoot"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: RightShin(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "LeftShoulder") && (childB.name == "LeftArm"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: LeftCollar(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "LeftArm") && (childB.name == "LeftForeArm"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: LeftArmTop(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "LeftForeArm") && (childB.name == "LeftHand"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: LeftArmBottom(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "RightShoulder") && (childB.name == "RightArm"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: RightCollar(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "RightArm") && (childB.name == "RightForeArm"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: RightArmTop(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else if ((childA.name == "RightForeArm") && (childB.name == "RightHand"))
				{
					Vector3 temp = Camera.main.WorldToScreenPoint(calcExtrapolation(childA.position,childB.position));
					fileoutput += string.Format("Name: RightArmBottom(exp) X: {0}  Y: {1}\r\n",  temp.x, temp.y);
					coords.Add(temp);
				}
				else 	
				{
					continue;
				}
			}
		}
	}
	
	Vector3 calcExtrapolation(Vector3 a, Vector3 b)
	{
		Vector3 c = (a-b) / 2f;
		c = b+c;
		
		return c;
	}
}

// List items only
public class SkeletonItem
{
	List<Vector3> itemCoords;
	
	public SkeletonItem(List<Vector3> L)
	{
		itemCoords = new List<Vector3>(L);	
	}
	
	public List<Vector3> GetParts()
	{
		return itemCoords;	
	}
}

#endregion