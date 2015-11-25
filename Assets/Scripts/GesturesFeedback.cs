using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GesturesFeedback : MonoBehaviour {

	public IisuInputProvider InputProvider;
	public Text GesturesFeedbackText;
	private List<uint> _poses;

	public  float updateInterval = 0.5F;
	private float timeleft; // Left time for current interval
	
	void Awake()
	{
		_poses = new List<uint>();	
	}

	void Start()
	{
		if( !GetComponent<UnityEngine.UI.Text>() )
		{
			Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
			enabled = false;
			return;
		}
		timeleft = updateInterval;  
	}
	
	void Update () {
	
		string format = null , handID = null;
		timeleft -= Time.deltaTime;

		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0 )
		{
			//fetch new events registered since the last update
			List<uint> poses = InputProvider.DetectedPoses;
			foreach(uint pose in poses)
			{
				_poses.Add(pose);
				if(_poses.Count == 5)
				{
					_poses.RemoveAt(0);	
				}
			}
			format = "Detected poses (id):" +'\n';
			//add them to the events list, and if we have more than
			//10 events, remove the oldest.
			for(int i = _poses.Count - 1; i >= 0; --i)
			{
				handID = _poses[i].ToString();
//				format += handID + "\n";
				if (handID == "0") format += "Open Hand\n";
				else if (handID == "6") format += "Thumbs Up\n";
				else if (handID == "8") format += "Thumbs Down\n";
			}
			GesturesFeedbackText.text = format;
			timeleft = updateInterval;
		}
	}
}
