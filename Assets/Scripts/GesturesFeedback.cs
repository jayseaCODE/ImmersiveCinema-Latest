using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GesturesFeedback : MonoBehaviour {

	public IisuInputProvider InputProvider;
	private List<uint> _poses, _moves, poses, moves;

	public PDepth PDepth;
	public GameObject HUD;
	public SpriteRenderer ArrowUp;
	public SpriteRenderer ArrowDown;
	public SpriteRenderer OpenHand;
	public SpriteRenderer ThumbsUp;
	public SpriteRenderer ThumbsDown;
	public Text GesturesFeedbackText;
	public Text DepthDistText;
	private int DepthDistIncrements;
	private bool HUD_bool = false;
	private float fadespeed = 1f;
	private bool arrowup_bool = false;
	private bool arrowdown_bool = false;
	private bool openhand_bool = false;
	private bool thumbsup_bool = false;
	private bool thumbsdown_bool = false;
	
	void Awake()
	{
		_poses = new List<uint>();
		_moves = new List<uint>();
		OpenHand.color = Color.clear;
		ThumbsUp.color = Color.clear;
		ThumbsDown.color = Color.clear;
	}

	void Start()
	{
		DepthDistIncrements = 0;
	}
	
	void Update () {
		string format = null , poseID = null, moveID= null;
		// Checking for posing and moving gestures
		poses = InputProvider.DetectedPoses;
		moves = InputProvider.DetectedMoves;
		foreach(uint pose in poses)
		{
			_poses.Add(pose);
			if(_poses.Count == 5)
			{
				_poses.RemoveAt(0);
			}
		}
		// Update GUI text and display it to HUD
		//Apply fading based on gesture bool variables
		if (openhand_bool) OpenHand.color = new Color(0,0,0,1);
		else OpenHand.color = Color.Lerp (OpenHand.color, Color.clear, fadespeed*Time.deltaTime); 
		openhand_bool = false;
		if (thumbsup_bool) ThumbsUp.color = new Color(0,0,0,1);
		else ThumbsUp.color = Color.Lerp (ThumbsUp.color, Color.clear, fadespeed*Time.deltaTime);
		thumbsup_bool = false;
		if (thumbsdown_bool) ThumbsDown.color = new Color(0,0,0,1);
		else ThumbsDown.color = Color.Lerp (ThumbsDown.color, Color.clear, fadespeed*Time.deltaTime);
		thumbsdown_bool = false;
		//Check for waveing movement
		for(int i = moves.Count - 1; i >= 0; --i)
		{
			moveID = moves[i].ToString(); //format += moveID + "\n";
			if (moveID != null) HUD_bool = !HUD_bool;
		}
		// Display/Undisplay the HUD
		if (!HUD_bool) {
			HUD.SetActive(true);
		}
		else {
			HUD.SetActive(false);
		}
		// Based on pose gesture, set up fading and change the depth distance increment value
		format = "Detected poses (id):" +'\n';
		if (poses.Count > 0) {
			if (poses[poses.Count - 1] == 0) {
				format += "Open Hand\n";
				openhand_bool = true;
				DepthDistIncrements = 0;
			}
			else if (poses[poses.Count - 1] == 6) {
				format += "Thumbs Up\n";
				thumbsup_bool = true;
				DepthDistIncrements = 3;
			}
			else if (poses[poses.Count - 1] == 8) {
				format += "Thumbs Down\n";
				thumbsdown_bool = true;
				DepthDistIncrements = -3;
			}
		}
		GesturesFeedbackText.text = format;
		if (PDepth.particleDepthDist > 1550 && DepthDistIncrements > 0) DepthDistIncrements = 0;
		if (PDepth.particleDepthDist < -50 && DepthDistIncrements < 0) DepthDistIncrements = 0;
		PDepth.particleDepthDist = PDepth.particleDepthDist + DepthDistIncrements;
		// Checking for up/down button input to adjust depth distance
		PDepth.particleDepthDist = PDepth.particleDepthDist + (int)(Input.GetAxisRaw("Vertical")*10);
		//Update the GUI depth distance value
		DepthDistText.text = "Depth Dist " + PDepth.particleDepthDist;
	}

//	IEnumerator FadeIn(SpriteRenderer gesture) {
//		for (float f = 0f; f <= 1; f += 0.1f) {
//			Color c = gesture.color;
//			c.a = f;
//			gesture.color = c;
//		}
//		for (float f = 1f; f >= 0; f -= 0.1f) {
//			Color c = gesture.color;
//			c.a = f;
//			gesture.color = c;
//			yield return null;
//		}
//	}
}
