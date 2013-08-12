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
	bool firstframe = true;
	
	// File / Screenshot variables
	string path, fileType = "image", filename;
	int fileCount = 0;
	
	// Point display
	bool pxFlag;
	Texture2D brush;
	int size = 4;
	int pxNo;
	
	// Extra points
	bool eFlag;
	
	// Stages/loops
	int stageCount = 4;
	int loopCount = 0;

	#endregion
	
	#region Initialise Overloads
	
	// Without Tilt
	public void Initialise(float y, bool x, string l, bool p, bool e, float a, float v, int c, int pN)
	{
		yStep = y;
		tiltFlag = x;
		path = l;
		pxFlag = p;
		eFlag = e;
		aStep = a;
		disThreshold = v;
		fileCount = c;
		pxNo = pN;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// With Tilt
	public void Initialise(float y, bool x, float xS, float mT, string l, bool p, bool e, float a, float v, int c, int pN)
	{
		xStep = xS;
		maxT = mT;

		Initialise(y, x, l, p, e, a, v, c, pN);
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
		
		// Locate the animations
		animations = Resources.LoadAll("Animations", typeof(AnimationClip));
		
		SetAnimator();
	}
	
	void OnGUI()
	{
		float bigX = 0;
		float bigY = 0;
		float smallX = 500;
		float smallY = 500;
		// Add points to screen
		if (pxFlag)
		{
			foreach (Vector3 v in currentSkeleton.GetParts())
			{
				GUI.DrawTexture(new Rect(v.x, (Screen.height - v.y) - 1, 2, 2), brush);	
				if (v.x > bigX)
				{
					bigX = v.x;
				}
				else if (v.x < smallX)
				{
					smallX = v.x;
				}
				if (v.y > bigY)
				{
					bigY = v.y;
				}
				else if (v.y < smallY)
				{
					smallY = v.y;
				}
			}
			GUI.DrawTexture(new Rect((smallX)-15, (Screen.height - bigY)-50,(bigX-smallX)+30,2), brush);
			GUI.DrawTexture(new Rect((smallX)-15, (Screen.height - smallY)+30,(bigX-smallX)+30,2), brush);
			GUI.DrawTexture(new Rect((smallX)-15, (Screen.height - bigY)-50,2,(bigY-smallY)+80), brush);
			GUI.DrawTexture(new Rect((bigX)+15, (Screen.height - bigY)-50,2,(bigY-smallY)+80), brush);
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
			}
			
			// Collect the point locations
    		currentSkeleton.SetParts(charAnimator, eFlag, pxNo);
			
			if (stageCount == 2)
			{
				if (pauseFlag)
				{
					if (CompareParts(currentSkeleton))
					{
						AnimateChar();
					}
					else
					{
						pauseFlag = false;
						stageCount = 0;
					}
				}
				else
				{
					AnimateChar();
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
		// skip first screenshot of every pose
		if (!pauseFlag && stageCount < 3)
		{
			if (skeletonList.Count == 0)
				skeletonList.Add(new SkeletonItem(currentSkeleton.GetParts()));
			
			filename = ScreenShotName();
			StartCoroutine(TakeScreenshot());
			
			// full 'loop'
			loopCount++;
		}
		
		if (firstframe)
		{
			pauseFlag = false;
			firstframe = false;
		}
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
		
		if (currTime < 1.0f)
		{
			currTime += aStep;
			
			// Advances animation to next step
			charAnimator.ForceStateNormalizedTime(currTime);
		}
		else
		{
			if (animCount < animations.Length)
			{
				// Switch animation
				SetAnimator();
			}
			else
			{
				// If last animation has passed jump to char change
				stageCount = 3;
			}
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
		
		// Set time
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
		
		// Adds .txt File
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
		
		if (isSame == false)
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
	List<Vector3> coords = new List<Vector3>();
	int eNumber;
	
	public void SetParts(Animator CharAnim, bool eFlag, int eNo)
	{
		coords.Clear();
		fileoutput = "";
		eNumber = eNo;
		
		foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
		{
			Transform temp = (Transform)CharAnim.GetBoneTransform(bone);
			if ((temp == null) || (bone == HumanBodyBones.Jaw) || (bone == HumanBodyBones.LastBone) || (bone == HumanBodyBones.LeftEye) || (bone == HumanBodyBones.LeftToes) || (bone == HumanBodyBones.RightEye) || (bone == HumanBodyBones.RightToes)) 
			{
				continue;
			}
			else
			{
				Vector3 objectPos =  Camera.main.WorldToScreenPoint(temp.position);
				fileoutput += string.Format("Name: {0} X: {1} Y: {2}\r\n", temp.name, objectPos.x, objectPos.y);
				coords.Add(objectPos);
			}
		}
		
		if (eFlag)
			extrapolatePoints(CharAnim);
	}
	
	public List<Vector3> GetParts()
	{
		return coords;
	}
	
	public string GetPartsWithName()
	{
		return fileoutput;
	}
	
	void extrapolatePoints(Animator Char)
	{
		
		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position,Char.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position, "LeftThigh");


		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position,Char.GetBoneTransform(HumanBodyBones.LeftFoot).position, "LeftShin");
		

		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.RightUpperLeg).position,Char.GetBoneTransform(HumanBodyBones.RightLowerLeg).position,"RightThigh");
		

		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.RightLowerLeg).position,Char.GetBoneTransform(HumanBodyBones.RightFoot).position, "RightShin");
		
		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.LeftShoulder).position,Char.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, "LeftCollar");
		

		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.LeftUpperArm).position,Char.GetBoneTransform(HumanBodyBones.LeftLowerArm).position,"LeftArmTop");
		

		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.LeftLowerArm).position,Char.GetBoneTransform(HumanBodyBones.LeftHand).position,"LeftArmBottom");
		
		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.RightShoulder).position,Char.GetBoneTransform(HumanBodyBones.RightUpperArm).position, "RightCollar");
		
	
		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.RightUpperArm).position,Char.GetBoneTransform(HumanBodyBones.RightLowerArm).position, "RightArmTop");
		
		calcExtrapolation(Char.GetBoneTransform(HumanBodyBones.RightLowerArm).position,Char.GetBoneTransform(HumanBodyBones.RightHand).position, "RightArmBottom");
		
	}
	
	void calcExtrapolation(Vector3 a, Vector3 b, string Part)
	{
		
		Vector3 c = b;
		for (int ii = 1; ii < eNumber; ii++)
		{
			Vector3 plus =(a-b) / (float)eNumber;
			c = c+plus;
			Vector3 temp = Camera.main.WorldToScreenPoint(c);
			fileoutput += string.Format("Name: {0}({3}) X: {1}  Y: {2}\r\n",  Part,  temp.x, temp.y,ii);
			coords.Add(temp);
		}
		
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