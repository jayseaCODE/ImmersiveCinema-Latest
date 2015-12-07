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
			//CLOUDSOVERSIDRA
//			if (Mathf.Abs(Time.time - 0.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 32.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 71.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 99.0f) < 0.5f) PDepth.particleDepthDist = 0;
//			else if (Mathf.Abs(Time.time - 185.0f) < 0.5f) PDepth.particleDepthDist = 1490;
//			else if (Mathf.Abs(Time.time - 228.0f) < 0.5f) PDepth.particleDepthDist = 0;
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
