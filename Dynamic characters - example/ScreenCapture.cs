using UnityEngine;
using UnityEditorInternal;
using System.Collections;

public class ScreenCapture : MonoBehaviour {
	
	//capture size variables
	public int capture_width = 640; 
    public int capture_height = 480;
	public bool use_screen_capture_size = true;

	//materials
	Renderer character_renderer;
	bool original_settings = true;
	Material material_orig, material_mod;
	
	//camera view
	public int rotation_step = 8;
	int rotation_count = 0;
	
	void Start()
	{
		StartCharacter();

		if (use_screen_capture_size)
		{
			capture_width = (int)camera.pixelWidth;
			capture_height = (int)camera.pixelHeight;			
		}
	}

	enum AnimState
	{
		ANIM_START,
		ANIM_PAUSED,
		ANIM_ADV,
		ANIM_END
	};
	
	AnimState anim_state;
	GameObject current_character;
	
	Object[] character_list;
	Object[] animation_list;

	//initialise the main character
	void StartCharacter () {
		character_list = Resources.LoadAll("characters",typeof(GameObject));
		animation_list = Resources.LoadAll("animations",typeof(AnimationClip));
		ReloadCharacterAnim();
		character_renderer.material = material_orig;
		original_settings = true;
	}
	
	bool capture_screen = false;
	
	//update objects
	void Update()
	{
		//reset the character's position so it remains in the camera's FOV
		current_character.transform.position = new Vector3(0,0,0);
		
//        if (Input.GetKeyDown("k"))
		{
			if (anim_state == AnimState.ANIM_PAUSED)
				UpdateCharacter();
			else if (anim_state == AnimState.ANIM_END)
				ReloadCharacterAnim();
        }
		
		UpdateAnimation();		
	}
	
	int char_index = 0;
	int anim_index = 0;
	
	void ReloadCharacterAnim()
	{
		if (char_index < character_list.Length)
		{
			///load a new character
			if (anim_index == 0)
			{
				if (current_character != null)
					Destroy(current_character);		
				current_character = (GameObject)Instantiate(character_list[char_index]);
				SkinnedMeshRenderer mf = (SkinnedMeshRenderer)FindSceneObjectsOfType(typeof(SkinnedMeshRenderer))[0];
				current_character.transform.position = new Vector3(0,-mf.bounds.size.y/2,0);
				AdjustCamera();
				Object[] renderer_list = GameObject.FindSceneObjectsOfType(typeof(Renderer));
				character_renderer = (Renderer)renderer_list[0]; //take the first one
				material_orig = character_renderer.material;
				material_mod = new Material(material_orig);
				material_mod.color = new Color(0,0,0);
			}
			
			AssignAnimationToCharacter((AnimationClip)animation_list[anim_index], current_character);
			Debug.Log(string.Format("CI {0}, AI {1}",char_index, anim_index));
			Debug.Log(string.Format("Ch {0}, A {1}",current_character.name,animation_list[anim_index]));
			if (++anim_index >= animation_list.Length)
			{
				anim_index = 0;
				++char_index;
			}

			current_character.GetComponent<Animator>().speed = 0.0f;
			anim_state = AnimState.ANIM_START;
			dest_time = anim_step;			
		}
	}
	
	public float height_ration = 1.4f;
	
	void AdjustCamera()
	{
		SkinnedMeshRenderer mf = (SkinnedMeshRenderer)FindSceneObjectsOfType(typeof(SkinnedMeshRenderer))[0];
		float screen_size = height_ration * mf.bounds.size.y;
		float distance = screen_size/2/(Mathf.Tan(camera.fieldOfView/2 * Mathf.Deg2Rad));
		camera.transform.position = new Vector3(0,mf.bounds.size.y/2,distance);
	}
	
	public float anim_step = 0.5f;
	
	float current_time, dest_time;
	
	void AnimationStep()
	{
		if (anim_state != AnimState.ANIM_END)
		{
			anim_state = AnimState.ANIM_ADV;
			current_character.GetComponent<Animator>().speed = 1.0f;
		}		
	}
	
	void AnimationReset()
	{
		dest_time = anim_step;
	}
	
	void UpdateAnimation()
	{
		switch (anim_state)
		{
		case AnimState.ANIM_START:
			anim_state = AnimState.ANIM_PAUSED;
			break;
		case AnimState.ANIM_PAUSED:
			break;
		case AnimState.ANIM_ADV:
			if (current_character.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
				anim_state = AnimState.ANIM_END;
			else
			{
				current_time = current_character.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime*current_character.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
				if (current_time >= dest_time)
				{				
					current_character.GetComponent<Animator>().speed = 0.0f;
					anim_state = AnimState.ANIM_PAUSED;
					dest_time = current_time + anim_step;
				}				
			}
			break;
		case AnimState.ANIM_END:
			break;
		}
	}
	
	static int capture_id = 0;
	
	//update call after all scene objects were updated
	void LateUpdate() 
	{
		
        if (capture_screen)
		{
			string file_name;
			
			AnimationInfo[] info = current_character.GetComponent<Animator>().GetCurrentAnimationClipState(0);
			
			if (original_settings)
				file_name = string.Format("{0},{1},{2},{3}.png",capture_id,current_character.name,info[0].clip.name,"orig");
			else
				file_name = string.Format("{0},{1},{2},{3}.png",capture_id++,current_character.name,info[0].clip.name,"mask");
			capture_id++;
			
			file_name = file_name.Replace("(Clone)","");
			file_name = file_name.Replace("(UnityEngine.AnimationClip)","");
			StartCoroutine(Capture(file_name));
			capture_screen = false;
        }		
    }
	
	void UpdateCharacter() 
	{
		RotateCharacter();
		capture_screen = true;
		return;
		
		if (original_settings)
		{
			character_renderer.material = material_mod;
			original_settings = false;
		}
		else
		{
			character_renderer.material = material_orig;
			RotateCharacter();
			original_settings = true;
		}
		capture_screen = true;
	}

	/// <summary>
	/// Rotates the character by one step.
	/// </summary>
	void RotateCharacter()
	{
		if (rotation_count == 0)
			current_character.transform.rotation = new Quaternion(0,0,0,1);			

		rotation_count = (rotation_count + 1) % rotation_step;			
		current_character.transform.Rotate(Vector3.up, 360/rotation_step);
		if (rotation_count == 0)
			AnimationStep();			
	}
	
	/// <summary>
	/// Capture screen to the specified filename.
	/// </summary>
	IEnumerator Capture(string filename)
	{
		yield return new WaitForEndOfFrame();
		
        RenderTexture rt = new RenderTexture(capture_width, capture_height, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(capture_width, capture_height, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, capture_width, capture_height), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);			
		byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
	}
	
	/// <summary>
	/// Assigns the animation to the character.
	/// </summary>
	static void AssignAnimationToCharacter(AnimationClip clip, GameObject character)
	{
		//create a new controller
		UnityEditorInternal.AnimatorController my_controller = new UnityEditorInternal.AnimatorController();
		my_controller.name = "generic_controller";
		
		//check if the animator component is already attached to the character
		if (character.GetComponent<Animator>() == null)
			character.AddComponent<Animator>();

		//create the state machine with the animation clip
		StateMachine sm = new StateMachine();
		sm.AddState("default_state");
		sm.GetState(0).SetMotion(0, clip);

		//check if the controller already has a based layer
		if (my_controller.GetLayerCount() == 0)
			my_controller.AddLayer("Base Layer");
		//set the state machine
		my_controller.SetLayerStateMachine(0, sm);
		
		//assign the controller
		Animator animator = (Animator)character.GetComponent<Animator>();
		UnityEditorInternal.AnimatorController.SetAnimatorController(animator,my_controller);		
	}
	
}
