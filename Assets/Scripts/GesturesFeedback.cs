using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GesturesFeedback : MonoBehaviour {

	public IisuInputProvider InputProvider;
	private List<uint> _poses, _moves, poses, moves;

	public  float updateInterval = 0.5F;
	private float timeleft; // Left time for current interval

	public PDepth PDepth;
	public GameObject HUD;
	public Text GesturesFeedbackText;
	public Text DepthDistText;
	private int DepthDistIncrements;
	private bool HUD_bool = false;
	
	void Awake()
	{
		_poses = new List<uint>();
		_moves = new List<uint>();
	}

	void Start()
	{
		timeleft = updateInterval;
		DepthDistIncrements = 0;
	}
	
	void Update () {
		string format = null , poseID = null, moveID= null;
		timeleft -= Time.deltaTime;
		//Checking for up/down button input to adjust depth distance
		PDepth.particleDepthDist = PDepth.particleDepthDist + (int)(Input.GetAxisRaw("Vertical")*10);
		//Update the depth distance values
		DepthDistText.text = "Depth Dist " + PDepth.particleDepthDist;
		//Checking for posing gestures to adjust depth distance
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
//		foreach(uint move in moves)
//		{
//			_moves.Add(move);
//			if(_moves.Count == 10)
//			{
//				_moves.RemoveAt(0);
//			}
//		}

		// Update GUI text and display it to HUD
		format = "Detected poses (id):" +'\n';
		for(int i = _poses.Count - 1; i >= 0; --i)
		{
			poseID = _poses[i].ToString();
			if (poseID == "0") format += "Open Hand\n";
			else if (poseID == "6")	format += "Thumbs Up\n";
			else if (poseID == "8") format += "Thumbs Down\n";
		}
		//Based on pose gesture, change the depth distance increment value, then apply it
		if (poses.Count > 0) {
			if (poses[poses.Count - 1] == 0) DepthDistIncrements = 0;
			else if (poses[poses.Count - 1] == 6) DepthDistIncrements = 1;
			else if (poses[poses.Count - 1] == 8) DepthDistIncrements = -1;
		}
		PDepth.particleDepthDist = PDepth.particleDepthDist + DepthDistIncrements;
		format += "Detected moves (id):" +'\n';
		for(int i = moves.Count - 1; i >= 0; --i)
		{
			moveID = moves[i].ToString();
			//format += moveID + "\n";
			if (moveID != null) HUD_bool = !HUD_bool; format+= "there's movement!!\n";
		}
		GesturesFeedbackText.text = format;
		timeleft = updateInterval;
		//Displaying the HUD
		if (!HUD_bool) {
			HUD.SetActive(true);
		}
		else {
			HUD.SetActive(false);
		}
	}
}
