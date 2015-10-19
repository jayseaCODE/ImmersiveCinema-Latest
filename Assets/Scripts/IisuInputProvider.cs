using UnityEngine;
using System.Collections;
using Iisu;
using IisuUnity;
using System;
using System.Collections.Generic;

/// <summary>
/// Takes care of the communication between iisu and the Unity application by providing
/// the necessary data from iisu
/// </summary>
public class IisuInputProvider : MonoBehaviour
{

	//the IisuUnityBehaviour object handles the iisu device, including its update thread, and disposing.
	private IisuUnityBehaviour _iisuUnity;

	private IDataHandle<Iisu.Data.IImageData> _depthImage;
	private IDataHandle<Iisu.Data.IImageData> _colorImage; //Me:
	private IDataHandle<Iisu.Data.IImageData> _UVImage; //Me:
	private IParameterHandle<uint> _wid; //Me:
	private IParameterHandle<uint> _hei; //Me:
	private IParameterHandle<uint> _registrationMode; //Me:
//	private IParameterHandle<float> _minDepth; //Me:
//	private IParameterHandle<float> _maxDepth; //Me:
//	private IParameterHandle<bool> _confidenceEnabled; //Me:
//	private IParameterHandle<uint> _confidenceThres; //Me:
	//private IDataHandle<Iisu.Data.IImageData> _labelImage;
	//private IDataHandle<int> _hand1ID;
	//private IDataHandle<int> _hand2ID;
	
	private delegate void OnPoseDelegate(string gestureName, int handId1, int handId2, uint gestureId);
	
	private List<uint> _poseIDsDetected;
	
	void Awake ()
	{
		//this has to be done first. Inside the IisuUnityBehaviour object, iisu is initialized, and the update thread for the current device (camera, movie) is started
		_iisuUnity = GetComponent<IisuUnityBehaviour> ();
		_iisuUnity.Initialize ();
		
		//register iisu data needed to display the depthimage
		_depthImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.DEPTH.Image"); //Me: This is the depth map image
		_colorImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.COLOR.Image"); //Me: This is the color map image
		_UVImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.COLOR.REGISTRATION.UV.Image"); //Me: This is the registration UV mapping image
		_wid = _iisuUnity.Device.RegisterParameterHandle<uint> ("SOURCE.CAMERA.COLOR.Width"); //Me: Curious
		_hei = _iisuUnity.Device.RegisterParameterHandle<uint> ("SOURCE.CAMERA.COLOR.Height"); //Me: curious
		_registrationMode = _iisuUnity.Device.RegisterParameterHandle<uint> ("SOURCE.CAMERA.COLOR.REGISTRATION.Mode"); //Me: curious
		Debug.Log (_hei); Debug.Log (_wid);
		Debug.Log (_registrationMode); //value of this should be 2, for the UV map

//		_minDepth = _iisuUnity.Device.RegisterParameterHandle<float> ("SOURCE.FILTER.MinDepth"); //Me: curious
//		_maxDepth = _iisuUnity.Device.RegisterParameterHandle<float> ("SOURCE.FILTER.MaxDepth"); //Me: curious
//		_confidenceEnabled = _iisuUnity.Device.RegisterParameterHandle<bool> ("SOURCE.FILTER.CONFIDENCE.Enabled"); //Me: curious
//		_minDepth.Value = 0.5f; _maxDepth.Value = 7.0f;
//		_confidenceEnabled.Value = true;
//		_confidenceThres = _iisuUnity.Device.RegisterParameterHandle<uint> ("SOURCE.FILTER.CONFIDENCE.MinThreshold"); //Me: curious
//		_confidenceThres.Value = 10;

		//_iisuUnity.Device.CommandManager.SendCommand("SYSTEM.PARAMETERS.Load","SOURCE.FILTER.MaxDepth", "Load.xml"); //Load does not seem to work at all
		//_iisuUnity.Device.CommandManager.SendCommand("SYSTEM.PARAMETERS.Reset","");
		//_iisuUnity.Device.CommandManager.SendCommand("SYSTEM.PARAMETERS.Save","SOURCE.FILTER", "AllSaved2.xml");

//		_hand1ID = _iisuUnity.Device.RegisterDataHandle<int> ("CI.HAND1.Label");
//		_hand2ID = _iisuUnity.Device.RegisterDataHandle<int> ("CI.HAND2.Label");
//		_labelImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("CI.SceneLabelImage");
//		
//		_iisuUnity.Device.EventManager.RegisterEventListener("CI.HandPosingGesture", new OnPoseDelegate(OnPoseEvent));
//		
//		_poseIDsDetected = new List<uint>();
	}
	
//	public List<uint> DetectedPoses
//	{
//		get
//		{
//			List<uint> poses = new List<uint>(_poseIDsDetected);
//			_poseIDsDetected.Clear();
//			return poses;	
//		}
//	}
//	
//	private void OnPoseEvent(string gestureName, int handId1, int handId2, uint gestureId)
//	{
//		_poseIDsDetected.Add(gestureId);
//	}
	
//	public IDevice Device 
//	{
//		get 
//		{ 
//			return _iisuUnity.Device; 
//		}
//	}

	public Iisu.Data.IImageData DepthMap 
	{
		get 
		{ 
			return _depthImage.Value;
		}
	}

	public Iisu.Data.IImageData ColorMap //Me:
	{
		get 
		{ 
			return _colorImage.Value;
		}
	}

	public Iisu.Data.IImageData UVMap //Me:
	{
		get 
		{ 
			return _UVImage.Value;
		}
	}

	/// <summary>
	/// The IDs of the label image indicate which pixels of the depthmap belong to a certain object, in this case the hand.
	/// </summary>
//	public int Hand1Label 
//	{
//		get 
//		{ 
//			return _hand1ID.Value; 
//		}
//	}

	/// <summary>
	/// The IDs of the label image indicate which pixels of the depthmap belong to a certain object, in this case the hand.
	/// </summary>
//	public int Hand2Label 
//	{
//		get 
//		{ 
//			return _hand2ID.Value; 
//		}
//	}

	/// <summary>
	/// Provides the label image that contains the IDs for each depth pixel to define pixels that belong to the same object in the scene.
	/// </summary>
//	public Iisu.Data.IImageData LabelImage 
//	{
//		get 
//		{ 
//			return _labelImage.Value; 
//		}
//	}
	
}
