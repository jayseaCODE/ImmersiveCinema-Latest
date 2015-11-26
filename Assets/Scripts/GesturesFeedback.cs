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
	private bool HUD_bool = false;
	
	void Awake()
	{
		_poses = new List<uint>();
		_moves = new List<uint>();	
	}

	void Start()
	{
		timeleft = updateInterval;  
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
			if(_poses.Count == 10)
			{
				_poses.RemoveAt(0);
			}
		}
		foreach(uint move in moves)
		{
			_moves.Add(move);
			if(_moves.Count == 10)
			{
				_moves.RemoveAt(0);
			}
		}

		//Interval ended - update GUI text and display it to HUD, then start new interval
		format = "Detected poses (id):" +'\n';
		//add them to the events list, and if we have more than
		//10 events, remove the oldest.
		for(int i = _poses.Count - 1; i >= 0; --i)
		{
			poseID = _poses[i].ToString();
			//format += handID + "\n";
			if (poseID == "0") format += "Open Hand\n";
			else if (poseID == "6") format += "Thumbs Up\n";
			else if (poseID == "8") format += "Thumbs Down\n";
		}
		format += "Detected moves (id):" +'\n';
		//add them to the events list, and if we have more than
		//10 events, remove the oldest.
		for(int i = moves.Count - 1; i >= 0; --i)
		{
			Debug.Log (moves.Count);
			moveID = moves[i].ToString();
			Debug.Log (moveID);
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
