using UnityEngine;
using System.Collections;
using System.IO;

public class CharacterScript : MonoBehaviour {
	
	public Renderer target;
	
	float yAngle, yStep;
	
	int maxFiles;
	string path, fileType , filename;
	int fileCount = 0;
	
	int stageCount = 6;
	int loopCount = 0;
	bool setFlag = true;
	
	public void Initialise(float y, int f, string l)
	{
		yStep = y;
		maxFiles = f;
		path = l;
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		stageCount = 0;
		loopCount = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (stageCount < 6)
		{
			// Decide whether this loop is colour or mask
			if (loopCount % 2 == 0)
			{
				target.renderer.material.color = Color.white; // colour
				fileType = "image";
				
				transform.Rotate(0, yStep, 0);
				yAngle += yStep;
			}
			else
			{
				target.renderer.material.color = Color.black; // mask
				fileType = "mask";
			}
			
			if (yAngle >= 360)
			{
				stageCount++;
				yAngle = 0;
				setFlag = true;
			}
			
			// add tilt
			if (stageCount == 2 && setFlag)
			{
				transform.eulerAngles = new Vector3(30, 0, 0);
				setFlag = false;
			}
			
			// swap to opposite tilt
			if (stageCount == 4 && setFlag)
			{
				transform.eulerAngles = new Vector3(-30, 0, 0);
				setFlag = false;
			}
			
			
		}
		
		// To add animations use something like this 
 
//	 	if (stageCount == 6)
//	 	{
//	 		CallNewAnimationMethod(aniCount);
//	 		aniCount++;
//			stageCount = 0;
//		}

	}
	
	void LateUpdate()
	{
		if (stageCount < 6)
		{
			filename = ScreenShotName();
			
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
		
		return string.Format(@"{0}\{1}{2}.png", path, fileType, fileCount.ToString());	
	}
	
	// Take screenshot here
	private IEnumerator TakeScreenshot()
	{
		// wait for render
		yield return new WaitForEndOfFrame();
			
		RenderTexture rt = new RenderTexture(Screen.width , Screen.height, 24);
		Texture2D shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		Camera.main.targetTexture = rt;
		Camera.main.Render();
		RenderTexture.active = rt;
		
		shot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		
		Camera.main.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);
		
		yield return 0;
		
		byte[] bytes = shot.EncodeToPNG();
		File.WriteAllBytes(filename, bytes);
		
	}
}
