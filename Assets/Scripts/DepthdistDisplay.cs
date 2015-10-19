using UnityEngine;
using System.Collections;

public class DepthdistDisplay : MonoBehaviour {

	public  float updateInterval = 0.5F;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval

	public PDepth pdepth;
	
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
	
	void Update()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0 )
		{
			int depth = pdepth.particleDepthDist;
			string depthdist = System.String.Format("{0:N}mm", depth);
			GetComponent<UnityEngine.UI.Text>().text = depthdist;
			GetComponent<UnityEngine.UI.Text>().material.color = Color.yellow;
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}
}
