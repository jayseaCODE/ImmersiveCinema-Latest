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
	public GameObject ArrowDown;
	public SpriteRenderer OpenHand;
	public GameObject ThumbsUp;
	public GameObject ThumbsDown;
	public Text GesturesFeedbackText;
	public Text DepthDistText;
	private int DepthDistIncrements;
	private bool HUD_bool = false;
	private bool openhand_bool = false;
	
	void Awake()
	{
		_poses = new List<uint>();
		_moves = new List<uint>();
		OpenHand.color = Color.clear;
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
		format = "Detected poses (id):" +'\n';
		/* I think the problem with putting the pose fadeouts here is with storing the pose values in _poses
		 * It will not work as we just need one occurence of either "Open hand" or "thumbs up" or "thumbs down"
		 * and set the fading to happen.
		 */
//		for(int i = _poses.Count - 1; i >= 0; --i) 
//		{
//			poseID = _poses[i].ToString();
//			if (poseID == "0") {
//				format += "Open Hand\n";
//			}
//			else if (poseID == "6")	{
//				format += "Thumbs Up\n";
//			}
//			else if (poseID == "8") {
//				format += "Thumbs Down\n";
//			}
//		}
		if (openhand_bool) OpenHand.color = new Color(0,0,0,1);
		else OpenHand.color = Color.Lerp (OpenHand.color, Color.clear, Time.deltaTime);
		openhand_bool = false;
		//format += "Detected moves (id):" +'\n';
		for(int i = moves.Count - 1; i >= 0; --i)
		{
			moveID = moves[i].ToString(); //format += moveID + "\n";
			if (moveID != null) HUD_bool = !HUD_bool;
		}
		// Displaying the HUD
		if (!HUD_bool) {
			HUD.SetActive(true);
		}
		else {
			HUD.SetActive(false);
		}
		// Based on pose gesture, change the depth distance increment value, then apply it
		if (poses.Count > 0) {
			if (poses[poses.Count - 1] == 0) {
				format += "Open Hand\n";
				openhand_bool = true;
				DepthDistIncrements = 0;
			}
			else if (poses[poses.Count - 1] == 6) {
				format += "Thumbs Up\n"; 
				DepthDistIncrements = 5;
			}
			else if (poses[poses.Count - 1] == 8) {
				format += "Thumbs Down\n";
				DepthDistIncrements = -5;
			}
		}
		GesturesFeedbackText.text = format;
		PDepth.particleDepthDist = PDepth.particleDepthDist + DepthDistIncrements;
		// Checking for up/down button input to adjust depth distance
		if (PDepth.particleDepthDist > 0 && PDepth.particleDepthDist < 1550) {
			PDepth.particleDepthDist = PDepth.particleDepthDist + (int)(Input.GetAxisRaw("Vertical")*10);
		}
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
