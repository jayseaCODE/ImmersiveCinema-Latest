using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using Iisu.Data;
using System.IO;
using Emgu.CV;
using UnityEngine.UI; //For Canvas, Text, GUI

/// <summary>
/// Helper class to convert iisu (color and depth) images to Unity point cloud through iisu UV-mapping
/// </summary>
public class PDepth2 : MonoBehaviour {
	
	// We'll use these to control our particle system
	private double UVmap_shift=0.0; //Shifting of UV-mapping to inline with the computed surrounding depth-color map
	public IisuInputProvider IisuInput;
	public ParticleSystem PS; //Our particle system
	public float MaxPointSize; // Size of particles
	public int Xgrid, Ygrid; // Num of particles in grid (along horizontal x-axis, along vertical y-axis)
	//	public float MaxSceneDepth, MaxWorldDepth; // Maximum Z-amount for particle positions, and Maximum distance from camera to SEARCH for depth points
	//	private int startXindex=0,endXindex=320,startYindex=0,endYIndex=240;
	//	private int startXindex=0,endXindex=290,startYindex=46,endYIndex=208;// Index of pixels that have positive UV values in the UV image map, Saves computations on grabbing pixels with negative UV values
	private int startXindex=40,endXindex=280,startYindex=35,endYindex=220; //Index of pixels (tested with the rendering of surroundings)
	public float NormRatioGridX=1.0f, NormRatioGridY=1.0f; //The normalized (range 0 to 1) distances of the normal particle system grid
	public float NormRatioBackgroundX=1.3f, NormRatioBackgroundY=1.0f; //The length ratios to render the background colors(A rough hack to match with particle system)
	public float backgroundXoffset, backgroundYoffset, gridXoffset, gridYoffset;
	public int particleDepthDist = 550; //The Depth cut off value for particles (in millimeters) - Can be the person's height
	public float displaydist = 0.0f;
	public float particleDepthWeight = 1f; //50f; // A weight placed on normalized depth values of particles
	
	private ParticleSystem.Particle[] points; //holds individual particle objects
	private int Xstep, Ystep; //Control spacing between particles
	
	private byte[] depthimageRaw, UVimageRaw, colorimageRaw, colorUndistortedimageRaw, idimageRaw;
	private IImageData depthimage, uvimagemap, colorimage, idimage;
	private int depthX, depthY, colorimageWidth, colorimageHeight, idimageWidth, idimageHeight;
	private float floatConvertor = 1f / 255f;
	public Transform OculusTransform;
	public float headTimer=0f; 
	private float fadeOutWaitingTimer = 1f; // Timer used to smoothen the fading out of real world
	private float fadeInWaitingTimer = 1f; // Timer used to smoothen the fading into real world
	public float quaternion_y=0f, Prev_quaternion_y=0f;
	public float timeToShakeHead = 1f; //should be 0.15f
	public float changesInY = 0f, signChangesInY = 0f;
	public float currentSign = 1.0f; //Start out with positive
	private bool UserHeadMovement_bool = false;
	private float UserHeadMovement_sensitivity = 0.04f;
	public GameObject GUI;
	public Text DepthDistText;
	public Text HeadShakingText;
	public Text HeadShakingSensitText;
	private bool GUI_bool = false;
	public GameObject FPSDisplay;
	private bool framerate_bool = false;
	//public float speed=0f;
	/*
	 * Note that the particle system size grid is fixed, but we can change its particle spacing which tentatively reduces
	 * the number of particles in the whole particle system. We could change the whole size of the particle system grid in
	 * how it is presented to the viewer but this will still render all the particles in the system. Basically we are only
	 * able to decrease/increase the number of particles in a fixed size particle system grid.
	 */
	double depthcx = 160.0; double depthcy = 120.0;
	double depthFocalx = 224.502; double depthFocaly = 230.494;
	double depthk1 = -0.170103; double depthk2 = 0.144064; double depthk3 = -0.0476994; double depthp1 = 0; double depthp2 = 0;
	double colorcx = 320.0; double colorcy = 240.0;
	double colorFocalx = 587.452; double colorFocaly = 600.675;
	double colork1 = 0.0225752; double colork2 = -0.162668; double colork3 = 0.186138; double colorp1 = 0; double colorp2 = 0;
	double r11= 0.999911   , r12=  0.00142974 , r13= -0.0132621;
	double r21= 0.00109085 , r22= -0.999674  ,  r23= -0.0255248;
	double r31= 0.0132942  , r32= -0.0255081 ,  r33=  0.999586;
	double t1 = 0.026;
	double t2 = -0.000507992;
	double t3 = -0.000862588;
	Matrix<double> M1 = new Matrix<double>(3,3);
	Matrix<double> M2 = new Matrix<double>(3,1);
	Matrix<double> R = new Matrix<double>(3,3);
	Matrix<double> T = new Matrix<double>(3,1);
	Matrix<double> temp = new Matrix<double>(3,3);
	Matrix<double> depthIntrinsicsMat = new Matrix<double>(3,3), depthIntrinsicsInvMat = new Matrix<double>(3,3);
	Matrix<double> colorIntrinsicsMat = new Matrix<double>(3,3);
	Matrix<double> depthDistortCoeff = new Matrix<double>(5,1);
	Matrix<double> colorDistortCoeff = new Matrix<double>(5,1);
	Matrix<double> ans = new Matrix<double>(3,240); // Used to speed up calculations for indexEquivalent()
	Matrix<double> N1 = new Matrix<double>(3,240); // Used to speed up calculations for indexEquivalent()
	Matrix<double> N2 = new Matrix<double>(3,240); // Used to speed up calculations for indexEquivalent()
	Matrix<byte> N3 = new Matrix<byte>(3,240); // Mask array to update only 2nd row of N1 with the right dy values
	Emgu.CV.Mat cvdepthUndistorted, cvcolorUndistorted;//, depthDrawableImage;
	Emgu.CV.Mat cvdepthSource, cvcolorSource, cvdepthSource2, cvcolorSource2;
	
	// Populate the particle grid
	void Start () {
//		idimage = IisuInput.LabelImage;
		depthimage = IisuInput.DepthMap;
		depthX = (int)depthimage.ImageInfos.Width; //320
		depthY = (int)depthimage.ImageInfos.Height; //240
		uvimagemap = IisuInput.UVMap;
		colorimage = IisuInput.ColorMap;
		colorimageWidth = (int)colorimage.ImageInfos.Width; //1280
		colorimageHeight = (int)colorimage.ImageInfos.Height; //720
		idimageWidth = (int)idimage.ImageInfos.Width;
		idimageHeight = (int)idimage.ImageInfos.Height;
		
		points = new ParticleSystem.Particle[(endXindex-startXindex)*(endYindex-startYindex)];//new ParticleSystem.Particle[Xgrid*Ygrid];
		Xstep = depthX/Xgrid;
		Ystep = depthY/Ygrid;
		
		int pid=0;
		for(int y=0;y<(endYindex-startYindex);y+=Ystep)//for(int y=0;y<depthY;y+=Ystep)
		{
			for(int x=0;x<(endXindex-startXindex);x+=Xstep)//for(int x=0;x<depthX;x+=Xstep)
			{
				points[pid].position = new UnityEngine.Vector3(x+startXindex,y+startYindex,0);
				points[pid].color = new Color(1,1,1,0); //Color white with no alpha, that is, making the whole grid transparent at the start
				points[pid].size = MaxPointSize;
				++pid;
			}
		}
		//Fill matrices with camera parameters values for calculations later
		depthIntrinsicsMat.Data[0,0] = depthFocalx;  depthIntrinsicsMat.Data[0,1] = 0; 		        depthIntrinsicsMat.Data[0,2] = depthcx;
		depthIntrinsicsMat.Data[1,0] = 0;			 depthIntrinsicsMat.Data[1,1] = -1*depthFocaly; depthIntrinsicsMat.Data[1,2] = depthcy; //The -1 is from the CameraModelDocumentation of SoftKinetic DepthSenseSDK
		depthIntrinsicsMat.Data[2,0] = 0;			 depthIntrinsicsMat.Data[2,1] = 0; 		        depthIntrinsicsMat.Data[2,2] = 1.0;
		colorIntrinsicsMat.Data[0,0] = colorFocalx;  colorIntrinsicsMat.Data[0,1] = 0; 		 	 colorIntrinsicsMat.Data[0,2] = colorcx;
		colorIntrinsicsMat.Data[1,0] = 0;			 colorIntrinsicsMat.Data[1,1] = colorFocaly; colorIntrinsicsMat.Data[1,2] = colorcy;
		colorIntrinsicsMat.Data[2,0] = 0;			 colorIntrinsicsMat.Data[2,1] = 0; 		  	 colorIntrinsicsMat.Data[2,2] = 1.0;
		depthDistortCoeff.Data[0,0] = depthk1; depthDistortCoeff.Data[1,0] = depthk2; depthDistortCoeff.Data[2,0] = depthp1;
		depthDistortCoeff.Data[3,0] = depthp2; depthDistortCoeff.Data[4,0] = depthk3;
		colorDistortCoeff.Data[0,0] = colork1; colorDistortCoeff.Data[1,0] = colork2; colorDistortCoeff.Data[2,0] = colorp1;
		colorDistortCoeff.Data[3,0] = colorp2; colorDistortCoeff.Data[4,0] = colork3;
		R.Data[0,0] = r11; R.Data[0,1] = r12; R.Data[0,2] = r13;
		R.Data[1,0] = r21; R.Data[1,1] = r22; R.Data[1,2] = r23;
		R.Data[2,0] = r31; R.Data[2,1] = r32; R.Data[2,2] = r33;
		T.Data[0,0] = t1; T.Data[1,0] = t2; T.Data[2,0] = t3;
		Emgu.CV.CvInvoke.Invert(depthIntrinsicsMat, depthIntrinsicsInvMat, Emgu.CV.CvEnum.DecompMethod.Cholesky);
		// M1 = colorIntrinsicsMat * R * depthIntrinsicsInvMat
		Emgu.CV.CvInvoke.Gemm(colorIntrinsicsMat, R, 1, null, 1, temp, 0);
		Emgu.CV.CvInvoke.Gemm(temp, depthIntrinsicsInvMat, 1, null, 1, M1, 0);
		// M2 = colorIntrinsicsMat * T
		Emgu.CV.CvInvoke.Gemm(colorIntrinsicsMat, T, 1, null, 1, M2, 0);
		
		cvdepthSource = new Emgu.CV.Mat(240, 320, Emgu.CV.CvEnum.DepthType.Cv16U, 1, depthimage.Raw, 320*2*1); //Mat(Int32 rows, Int32 cols, DepthType Cv16S, Int32 channels, IntPtr, Int32 step)
		//		cvcolorSource = new Emgu.CV.Mat(480, 640, Emgu.CV.CvEnum.DepthType.Cv8U, 4, colorimage.Raw, 640*1*4); //Mat(Int32, Int32, DepthType Cv8S, Int32, IntPtr, Int32)
		cvcolorSource = new Emgu.CV.Mat(720, 1280, Emgu.CV.CvEnum.DepthType.Cv8U, 4, colorimage.Raw, 1280*1*4); //Mat(Int32, Int32, DepthType Cv8S, Int32, IntPtr, Int32)
		cvcolorSource2 = new Emgu.CV.Mat(480, 640, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
		cvdepthUndistorted = new Emgu.CV.Mat(240, 320, Emgu.CV.CvEnum.DepthType.Cv16U, 1);
		cvcolorUndistorted = new Emgu.CV.Mat(480, 640, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
		//		cvcolorUndistorted = new Emgu.CV.Mat(720, 1280, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
		
		//Set up Matrices used for optimisations
		N1.SetIdentity (); N2.SetIdentity (); N3.SetIdentity ();
		for (int dx=startXindex; dx<endXindex; dx+=Xstep) {
			// N1, N2, N3 are [rows, cols] [3, 240] matrices
			N1.Data[0,dx-startXindex] = dx; N2.Data[0,dx-startXindex] = M2.Data[0,0]; N3.Data[0,dx-startXindex] = 0;
			N1.Data[1,dx-startXindex] = 1;  N2.Data[1,dx-startXindex] = M2.Data[1,0]; N3.Data[1,dx-startXindex] = 255;
			N1.Data[2,dx-startXindex] = 1;  N2.Data[2,dx-startXindex] = M2.Data[2,0]; N3.Data[2,dx-startXindex] = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Initialization
		if (depthimage == null || idimage == null )
			return;
		if (depthimage.Raw == IntPtr.Zero || idimage.Raw == IntPtr.Zero)
			return;
		if (depthimageRaw == null || depthimageRaw.Length != depthimage.ImageInfos.BytesRaw)
			depthimageRaw = new byte[depthimage.ImageInfos.BytesRaw];
		colorimageRaw = new byte[colorimage.ImageInfos.BytesRaw];
		colorUndistortedimageRaw = new byte[480*640*1*4];
		if (UVimageRaw == null || UVimageRaw.Length != uvimagemap.ImageInfos.BytesRaw)
			UVimageRaw = new byte[uvimagemap.ImageInfos.BytesRaw];
		if (idimageRaw == null || idimageRaw.Length != idimage.ImageInfos.BytesRaw)
			idimageRaw = new byte[idimage.ImageInfos.BytesRaw];
		uint byte_size = (uint)depthimage.ImageInfos.BytesRaw;
		//Debug.Log("color byte size "+color_byte_size);
		uint UV_byte_size = (uint)uvimagemap.ImageInfos.BytesRaw;
		uint color_byte_size = (uint)colorimage.ImageInfos.BytesRaw;
		uint labelImageSize = (uint)idimage.ImageInfos.BytesRaw;
		// Resize color image from 1280x720 to 640x480, distort this color image and also the 320x240 depth image
		Emgu.CV.CvInvoke.Resize (cvcolorSource, cvcolorSource2, cvcolorSource2.Size, 0.5, 0.66666666666, Emgu.CV.CvEnum.Inter.Linear);
		Emgu.CV.CvInvoke.Undistort(cvdepthSource, cvdepthUndistorted, depthIntrinsicsMat, depthDistortCoeff, null);
		Emgu.CV.CvInvoke.Undistort(cvcolorSource2, cvcolorUndistorted, colorIntrinsicsMat, colorDistortCoeff, null);
		// Copy image content into managed arrays
		Marshal.Copy(cvdepthUndistorted.DataPointer, depthimageRaw, 0, (int)byte_size);
		Marshal.Copy(cvcolorUndistorted.DataPointer, colorUndistortedimageRaw, 0, (int)480*640*1*4);
		//		Marshal.Copy(depthimage.Raw, depthimageRaw, 0, (int)byte_size);
		Marshal.Copy(colorimage.Raw, colorimageRaw, 0, (int)color_byte_size);
		Marshal.Copy(uvimagemap.Raw, UVimageRaw, 0, (int)UV_byte_size);
		Marshal.Copy(idimage.Raw, idimageRaw, 0, (int)labelImageSize);
		
		//Profiler.BeginSample("Head-checks");
		if (Input.GetButtonDown("Fire1")) {
			UserHeadMovement_bool = !UserHeadMovement_bool;
		}
		if (UserHeadMovement_bool) {
			// Check for head movement
			UserHeadMovement();
		}
		UserHeadMovement_sensitivity = UserHeadMovement_sensitivity + (Input.GetAxisRaw("Horizontal")*0.001f);
		particleDepthDist = particleDepthDist + (int)(Input.GetAxisRaw("Vertical")*10);
		if (Input.GetButtonDown("Jump")) GUI_bool = !GUI_bool;
		if (GUI_bool) {
			// Display GUI
			DepthDistText.text = "Depth Dist " + particleDepthDist;
			if (UserHeadMovement_bool) HeadShakingText.text = "Head Shaking ON";
			else HeadShakingText.text = "Head Shaking OFF";
			HeadShakingSensitText.text = "Head Sensitivity " + UserHeadMovement_sensitivity.ToString("F2");
			GUI.SetActive(true);
		}
		else {
			// Hide GUI
			GUI.SetActive(false);
		}
		if (Input.GetButtonDown ("f")) framerate_bool = !framerate_bool;
		if (framerate_bool) FPSDisplay.SetActive(true);
		else FPSDisplay.SetActive(false);
		//Profiler.EndSample();
		
		int pid=0, colorIndex=0, toIndex=0;
		float u_value, v_value;
		int labelU, labelV, labelIndex;
		//Profiler.BeginSample("ForLoop");
		for(int dy=startYindex;dy<endYindex;dy+=Ystep)
		{
			// Speed up multiplication calculations for indexEquivalent()
			N1.SetValue(dy, N3); // Update only 2nd row of N1 with the right dy values
			Emgu.CV.CvInvoke.Gemm(M1, N1, 1, N2, 1, ans, 0);
			
			for(int dx=startXindex;dx<endXindex;dx+=Xstep)
			{
				int didx = dy*depthX+dx; //Index of pixel in the depth image
				
				// reconstruct ushort value from 2 bytes (low endian) - Depth values from Depth Map
				ushort value = (ushort)(depthimageRaw[didx * 2] + (depthimageRaw[didx * 2 + 1] << 8));
				if (value > 1499) { //Particles covering the rest of the surroundings (Out of the camera's detection sweet spot)
					value = 1500;
				}
				
				if(value>=particleDepthDist) //Particle depth cut-off limit, in millimeters
				{
					points[pid].color = new Color(1,0,0,0); //Particles are cut off by making it transparent with alpha value = 0
				}
				
				//Rendering of Surroundings through depth to color mapping based on RGB camera and depth camera intrinsics and extrinsics
				else if (particleDepthDist > 1499 && value == 1500) { 
					// Get the correct index of a pixel in the undistorted color image byte array by its calculated mapped coordinates
					indexEquivalent( dx, 640, out toIndex);
					//					UVEquivalent( depthX,  depthY,  dx,  dy, 640, 480, out u_value, out v_value, out toIndex);
					
					if (toIndex < 0 || toIndex > 307199) {
						points[pid].color = new Color(1,1,1,0);
					}
					else {
						byte B = (colorUndistortedimageRaw[toIndex * 4]);
						byte G = (colorUndistortedimageRaw[toIndex * 4 + 1]);
						byte R = (colorUndistortedimageRaw[toIndex * 4 + 2]);
						byte A = (colorUndistortedimageRaw[toIndex * 4 + 3]);
						points[pid].color = new Color(R*floatConvertor, G*floatConvertor, B*floatConvertor, A*floatConvertor);
						points[pid].position = new UnityEngine.Vector3(dx*NormRatioBackgroundX+backgroundXoffset,
						                                               (depthY-dy)*NormRatioBackgroundY+backgroundYoffset,
						                                               displaydist);
						//lmap(value * floatConvertor,0,MaxWorldDepth,0,MaxSceneDepth)*particleDepthWeight);
					}
				}
				
				else {
					// Get the UV coordinates from the Registration mapping image - UV map
					u_value = System.BitConverter.ToSingle(UVimageRaw, didx * 8);
					v_value = System.BitConverter.ToSingle(UVimageRaw, didx * 8 + 4);
					
					// Assign appropriate shifting value based on pixel depth value
					UVmap_shift = 0.0;
					if (value < 750) {
						if (value <= 300) { UVmap_shift = 0.04;}
						else { UVmap_shift = 0.04 / Math.Pow(2.0, (((double)value/250.0)-1.0)/0.5 ); }
					}
					
					// Get the correct index of a pixel in the color image byte array by its UV coordinates
					colorIndex = (int)((u_value+UVmap_shift) * colorimageWidth) + ((int)(v_value * colorimageHeight ) * colorimageWidth);
					UVEquivalent( depthX,  depthY,  dx,  dy, idimageWidth, idimageHeight, out labelU, out labelV, out labelIndex);
					if (colorIndex < 0 | colorIndex > 921599) { // Just a precautionary measure to capture pixels with negative UV coordinates
						//These particles have valid depth values but no color associated with it
						points[pid].color = new Color(1,1,1,0); //Particles are cut off by making it transparent with alpha value = 0
						//Total number of pixels is 921600 in the 1280x720 color image
					}
//					else if (idimageRaw[labelIndex]== IisuInput.Hand1Label) points[pid].color = new Color(0f, 1f, 0f);
//					else if (idimageRaw[labelIndex]== IisuInput.Hand2Label) points[pid].color = new Color(0f, 0f, 1f);
					else {//if ( u_value > 0.249 && u_value < 0.749) { //Take a smaller color image area to fit the resolution of Oculus Lens
						// Extract the byte values B, G, R, A from the stored color raw data
						byte B = (colorimageRaw[colorIndex * 4]);
						byte G = (colorimageRaw[colorIndex * 4 + 1]);
						byte R = (colorimageRaw[colorIndex * 4 + 2]);
						byte A = (colorimageRaw[colorIndex * 4 + 3]);
						points[pid].color = new Color(R*floatConvertor, G*floatConvertor, B*floatConvertor, A*floatConvertor);
						points[pid].position = new UnityEngine.Vector3(dx*NormRatioGridX+gridXoffset, 
						                                               (depthY-dy)*NormRatioGridY+gridYoffset,
						                                               displaydist);
						//lmap(value * floatConvertor,0,MaxWorldDepth,0,MaxSceneDepth)*particleDepthWeight);
					}
					
				}
				
				++pid;
				
			}
		}
		//Profiler.EndSample();
		PS.SetParticles(points, points.Length);
		
	}
	
	private void UserHeadMovement()
	{
		if (headTimer == 0) {
			Prev_quaternion_y = quaternion_y;
		}
		else if (headTimer >= timeToShakeHead) {
			headTimer = 0f;
			//			changesInY = 0f;
			if (signChangesInY >= 2) {
				if (particleDepthDist < 1550) particleDepthDist += (int)(1000 * Math.Exp(fadeInWaitingTimer) * Time.deltaTime);
				fadeInWaitingTimer += Time.deltaTime;
				fadeOutWaitingTimer = 1f;
			}
			else {
				if (particleDepthDist > 50) particleDepthDist -= (int)(1.5 * Math.Exp(fadeOutWaitingTimer) * Time.deltaTime);
				fadeOutWaitingTimer += Time.deltaTime;
				fadeInWaitingTimer = 1f;
			}
			signChangesInY = 0f;
		}
		else {
			quaternion_y = OculusTransform.rotation.y;
			if ( (quaternion_y - Prev_quaternion_y) > UserHeadMovement_sensitivity ) {  //0.05
				if (currentSign < 0) {
					signChangesInY += 1; // Add one to number of sign changes
					currentSign *= -1; // Switch the sign from negative to positive
				}
			}
			else { 
				if (currentSign > 0) {
					signChangesInY += 1;
					currentSign *= -1; // Switch the sign from positive to negative
				}
			}
			//			changesInY += Mathf.Abs(Prev_quaternion_y-quaternion_y);
			//			speed = changesInY/Time.deltaTime;
			Prev_quaternion_y = quaternion_y;
		}
		headTimer += Time.deltaTime;
		
	}
	
	private void indexEquivalent( int sourceDepthU, double UndistortedColorWidth, out int sourceColorIndex)
	{
		/*
//		double depthValue = Convert.ToDouble(depthValue2/1000.0);
		
		//Change the 2D pixel array coordinate system to DepthSenseSDK 2D pixel coordinate system 
//		double x = Convert.ToDouble(sourceDepthU);
//		double y = Convert.ToDouble(sourceDepthV);
//
//		vector.Data[0,0] = x;// * depthValue;
//		vector.Data[1,0] = y;// * depthValue;
//		vector.Data [2, 0] = 1;// depthValue;

		//ans = M1 * vector + M2;
//		Emgu.CV.CvInvoke.Gemm(M1, vector, 1, M2, 1, ans, 0);

//		double ans_x = Math.Round(ans.Data[0,0] / ans.Data[2,0]);
//		double ans_y = Math.Round(ans.Data[1,0] / ans.Data[2,0]); 
		 */
		double ans_x = Math.Round(ans.Data[0,sourceDepthU-startXindex] / ans.Data[2,sourceDepthU-startXindex]); 
		double ans_y = Math.Round(ans.Data[1,sourceDepthU-startXindex] / ans.Data[2,sourceDepthU-startXindex]); 
		
		sourceColorIndex = (int) (ans_x) + (int)(ans_y * UndistortedColorWidth);
	}
	
	private void UVEquivalent(int fromWidth, int fromHeight, int fromU, int fromV, int toWidth, int toHeight, out int toU, out int toV, out int toIndex)
	{
		float uNorm = (float)fromU / (float)fromWidth;
		float vNorm = (float)fromV / (float)fromHeight;
		
		toU = (int)(uNorm * toWidth);
		toV = (int)(vNorm * toHeight);
		
		toIndex = (int) (toU + toV * (float)toWidth);
	}
	
	// A remapping process that returns a value from the 0 to 1 range.
	private float lmap(float val, float min0, float max0, float min1, float max1)
	{
		return min1 + (val-min0)*(max1-min1)/(max0-min0);
	}
	
}	