using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2015 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

[AddComponentMenu("AVPro Windows Media/Material Apply")]
public class AVProWindowsMediaMaterialApply : MonoBehaviour 
{
	public Material _material;
	public AVProWindowsMediaMovie _movie;
	public string _textureName;
	public Texture2D _defaultTexture;
	public PDepth PDepth;
	public GesturesFeedback GestureFeedback;
	private static Texture2D _blackTexture;
	
	private static void CreateTexture()
	{
		_blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
		_blackTexture.name = "AVProWindowsMedia-BlackTexture";
		_blackTexture.filterMode = FilterMode.Point;
		_blackTexture.wrapMode = TextureWrapMode.Clamp;
		_blackTexture.SetPixel(0, 0, Color.black);
		_blackTexture.Apply(false, true);
	}
	
	void OnDestroy()
	{
		_defaultTexture = null;
		
		if (_blackTexture != null)
		{
			Texture2D.Destroy(_blackTexture);
			_blackTexture = null;
		}
	}
	
	void Start()
	{
		if (_blackTexture == null)
			CreateTexture();
		
		if (_defaultTexture == null)
		{
			_defaultTexture = _blackTexture;
		}
		
		Update();
	}
	
	void Update()
	{
		if (_movie != null)
		{
			if (_movie.OutputTexture != null)
				ApplyMapping(_movie.OutputTexture);
			else
				ApplyMapping(_defaultTexture);
			//Self edit
			//1941BATTLE1080
//			if (Mathf.Abs(Time.time - 0.0f) < 0.1f) PDepth.particleDepthDist = 1550;
//			else if (Mathf.Abs(Time.time - 2.0f) < 0.1f) GestureFeedback.DepthDistIncrements = -40;
//			else if (Mathf.Abs(Time.time - 4.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 0;
//			else if (Mathf.Abs(Time.time - 16.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 37.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 43.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 47.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 54.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 76.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 84.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 90.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 99.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 106.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 112.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 118.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 137.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 143.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 165.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 6;
			//VRDIVER2K
//			if (Mathf.Abs(Time.time - 0.0f) < 0.1f) PDepth.particleDepthDist = 1550;
//			else if (Mathf.Abs(Time.time - 2.0f) < 0.1f) GestureFeedback.DepthDistIncrements = -2;
//			else if (Mathf.Abs(Time.time - 20.8f) < 0.1f) GestureFeedback.DepthDistIncrements = 0;
//			else if (Mathf.Abs(Time.time - 90.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 109.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 123.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 139.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 164.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 171.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 195.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 6;
			//HUNGERGAMES1080-CUT
//			if (Mathf.Abs(Time.time - 0.0f) < 0.1f) PDepth.particleDepthDist = 1550;
//			else if (Mathf.Abs(Time.time - 2.0f) < 0.1f) GestureFeedback.DepthDistIncrements = -15;
//			else if (Mathf.Abs(Time.time - 7.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 0;
//			else if (Mathf.Abs(Time.time - 18.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 49.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 63.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 84.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 99.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 138.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 188.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 206.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 241.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 264.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 297.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 4;
			//SHINEORBEMADVR1080-CUT
//			if (Mathf.Abs(Time.time - 0.0f) < 0.1f) PDepth.particleDepthDist = 1550;
//			else if (Mathf.Abs(Time.time - 2.0f) < 0.1f) GestureFeedback.DepthDistIncrements = -9;
//			else if (Mathf.Abs(Time.time - 11.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 0;
//			else if (Mathf.Abs(Time.time - 42.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 67.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 73.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 83.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 93.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 104.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 123.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 132.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 159.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 179.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 180.0f) < 0.1f) PDepth.particleDepthDist = 1550;
			//Training - 360MIKU
//			if (Mathf.Abs(Time.time - 0.0f) < 0.1f) PDepth.particleDepthDist = 1550;
//			else if (Mathf.Abs(Time.time - 2.0f) < 0.1f) GestureFeedback.DepthDistIncrements = -15;
//			else if (Mathf.Abs(Time.time - 6.0f) < 0.1f) GestureFeedback.DepthDistIncrements = 0;
//			else if (Mathf.Abs(Time.time - 31.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 38.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 95.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 126.0f) < 0.5f) PDepth.particleDepthDist = 750;
//			else if (Mathf.Abs(Time.time - 155.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 181.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 210.0f) < 0.5f) PDepth.particleDepthDist = 0;
		}
	}
	
	private void ApplyMapping(Texture texture)
	{
		if (_material != null)
		{
			if (string.IsNullOrEmpty(_textureName))
				_material.mainTexture = texture;
			else
				_material.SetTexture(_textureName, texture);
		}
	}
	
	public void OnDisable()
	{
		ApplyMapping(null);
	}
}
