using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GesturesFeedback : MonoBehaviour {

	public IisuInputProvider InputProvider;
	private List<uint> poses, moves;

	public PDepth PDepth;
	public GameObject HUD;
	public SpriteRenderer ArrowUp;
	public SpriteRenderer ArrowDown;
	public SpriteRenderer OpenHand;
	public SpriteRenderer ThumbsUp;
	public SpriteRenderer ThumbsDown;
//	public Text GesturesFeedbackText; //For programmer text feedback
	public Text DepthDistText;
	private int DepthDistIncrements;
	private bool HUD_bool = false;
	private float fadespeed = 1.5f;
	private bool openhand_bool = false;
	private bool thumbsup_bool = false;
	private bool thumbsdown_bool = false;
	private Color OpenHand_origcolor;
	private Color ThumbsUp_origcolor;
	private Color ThumbsDown_origcolor;
	private Color ArrowUp_origcolor;
	private Color ArrowDown_origcolor;
	private int prev_particleDepthDist;
	
	void Awake()
	{
		OpenHand_origcolor = OpenHand.color;
		OpenHand.color = Color.clear;
		ThumbsUp_origcolor = ThumbsUp.color;
		ThumbsUp.color = Color.clear;
		ThumbsDown_origcolor = ThumbsDown.color;
		ThumbsDown.color = Color.clear;
		ArrowUp_origcolor = ArrowUp.color;
		ArrowUp.color = Color.clear;
		ArrowDown_origcolor = ArrowDown.color;
		ArrowDown.color = Color.clear;
	}

	void Start()
	{
		DepthDistIncrements = 0;
	}
	
	void Update () {
		prev_particleDepthDist = PDepth.particleDepthDist; //To record changes in depthdist
		string moveID = null; //, format = null;
		// Checking for posing and moving gestures
		poses = InputProvider.DetectedPoses;
		moves = InputProvider.DetectedMoves;
		// Update GUI text and display it to HUD
		//Apply fading based on gesture bool variables
		if (openhand_bool) OpenHand.color = OpenHand_origcolor;
		else OpenHand.color = Color.Lerp (OpenHand.color, Color.clear, fadespeed*Time.deltaTime); 
		openhand_bool = false;
		if (thumbsup_bool) ThumbsUp.color = ThumbsUp_origcolor;
		else ThumbsUp.color = Color.Lerp (ThumbsUp.color, Color.clear, fadespeed*Time.deltaTime);
		thumbsup_bool = false;
		if (thumbsdown_bool) ThumbsDown.color = ThumbsDown_origcolor;
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
		//format = "Detected poses (id):" +'\n';
		if (poses.Count > 0) {
			if (poses[poses.Count - 1] == 0) {
//				format += "Open Hand\n"; //For programmer text feedback
				openhand_bool = true;
				DepthDistIncrements = 0;
			}
			else if (poses[poses.Count - 1] == 6) {
//				format += "Thumbs Up\n"; //For programmer text feedback
				thumbsup_bool = true;
				DepthDistIncrements = 3;
			}
			else if (poses[poses.Count - 1] == 8) {
//				format += "Thumbs Down\n"; //For programmer text feedback
				thumbsdown_bool = true;
				DepthDistIncrements = -3;
			}
		}
		// Checking for up/down button input to adjust depth distance and spacebar STOP button - Wizard of Oz
		if (Input.GetAxisRaw("Vertical") > 0) {
			thumbsup_bool = true;
			DepthDistIncrements = 3;
		}
		else if (Input.GetAxisRaw("Vertical") < 0) {
			thumbsdown_bool = true;
			DepthDistIncrements = -3;
		}
		else if (Input.GetButtonDown("Jump")) {
			openhand_bool = true;
			DepthDistIncrements = 0;
		}
//		GesturesFeedbackText.text = format; //For programmer text feedback
		if (PDepth.particleDepthDist > 1550 && DepthDistIncrements > 0) DepthDistIncrements = 0;
		else if (PDepth.particleDepthDist < -50 && DepthDistIncrements < 0) DepthDistIncrements = 0;
		PDepth.particleDepthDist = PDepth.particleDepthDist + DepthDistIncrements;
		//Update the GUI depth distance value
		DepthDistText.text = PDepth.particleDepthDist.ToString(); //"Depth Dist " + PDepth.particleDepthDist;
		if (PDepth.particleDepthDist == prev_particleDepthDist) {
			ArrowUp.color = Color.clear;
			ArrowDown.color = Color.clear;
		}
		else if (PDepth.particleDepthDist > prev_particleDepthDist) {
			ArrowUp.color = ArrowUp_origcolor;
			ArrowDown.color = Color.clear;
		}
		else {// (PDepth.particleDepthDist < prev_particleDepthDist) {
			ArrowUp.color = Color.clear;
			ArrowDown.color = ArrowDown_origcolor;
		}
	}
}
