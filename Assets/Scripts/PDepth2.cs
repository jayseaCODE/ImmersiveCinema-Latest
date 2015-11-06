using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using Iisu.Data;
using System.IO;
using Emgu.CV;

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
	
	private byte[] depthimageRaw, UVimageRaw, colorimageRaw, colorUndistortedimageRaw;
	private IImageData depthimage, uvimagemap, colorimage;
	private int depthX, depthY, colorimageWidth, colorimageHeight;
	private float floatConvertor = 1f / 255f;
	public Camera OculusCamera;
	public Transform OculusTransform;
	public float headTimer=0f; 
	private float fadeOutWaitingTimer = 1f; // Timer used to smoothen the fading out of real world
	private float fadeInWaitingTimer = 1f; // Timer used to smoothen the fading into real world
	public float quaternion_y=0f, Prev_quaternion_y=0f;
	public float timeToShakeHead = 1f;
	public float changesInY = 0f, signChangesInY = 0f;
	public float currentSign = 1.0f; //Start out with positive
	public float speed=0f;
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

	public Material ps_material;
	public ComputeShader ps_computeShader;
//	ComputeBuffer ps_buffer;
//	int ps_count;
//	float[] ps_points;
	//ComputeBuffer depthimageRawBuffer;
//	ComputeBuffer colorimageRawBuffer;
//	ComputeBuffer colorUndistortedimageRawBuffer;
//	ComputeBuffer UVimageRawBuffer;
	ComputeBuffer ps_xyzpositions_buffer;
	ComputeBuffer argBuffer;
	int[] args = new int[]{ 0, 1, 0, 0 };
	//Texture2D depthimageTex;
	
	// Populate the particle grid
	void Start () {
		depthimage = IisuInput.DepthMap;
		depthX = (int)depthimage.ImageInfos.Width; //320
		depthY = (int)depthimage.ImageInfos.Height; //240
		uvimagemap = IisuInput.UVMap;
		colorimage = IisuInput.ColorMap;
		colorimageWidth = (int)colorimage.ImageInfos.Width; //1280
		colorimageHeight = (int)colorimage.ImageInfos.Height; //720
		
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

		//Particle system set-up
//		ps_count = (int)depthimage.ImageInfos.Width*(int)depthimage.ImageInfos.Height;
//		ps_buffer = new ComputeBuffer(ps_count, sizeof(float)*3, ComputeBufferType.Default);
//		ps_points = new float[ps_count*3];

//		depthimageRawBuffer = new ComputeBuffer((int)depthimage.ImageInfos.BytesRaw/2, sizeof(UInt16), ComputeBufferType.Default);
//		ps_xyzpositions_buffer = new ComputeBuffer(320*240, sizeof(float)*3);
		ps_xyzpositions_buffer = new ComputeBuffer(320*240, sizeof(float)*3, ComputeBufferType.Append);
		argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);

		ps_computeShader.SetBuffer(0,"xyzpositions_buffer",ps_xyzpositions_buffer);
		ps_computeShader.SetFloat("height",240);//(endYindex-startYindex)-1); //Make the height 184, instead of 185
		ps_computeShader.SetFloat("width",320);//(endXindex-startXindex));
		
		ps_computeShader.Dispatch(0, 320/32, 240/32, 1);
		argBuffer.SetData(args);
		ComputeBuffer.CopyCount(ps_xyzpositions_buffer, argBuffer, 0);
	}

	void Update() {
		//must reset particles verts to 0 each frame.
		ps_xyzpositions_buffer.SetData(new float[320*240*3]);
//
//		ps_computeShader.SetBuffer(0,"xyzpositions_buffer",ps_xyzpositions_buffer);
//		ps_computeShader.SetFloat("height",120);//(endYindex-startYindex)-1); //Make the height 184, instead of 185
//		ps_computeShader.SetFloat("width",160);//(endXindex-startXindex));
//		
//		ps_computeShader.Dispatch(0, 40/8, 40/8, 1);
		ps_computeShader.SetBuffer(0,"xyzpositions_buffer",ps_xyzpositions_buffer);
		ps_computeShader.SetFloat("height",240);//(endYindex-startYindex)-1); //Make the height 184, instead of 185
		ps_computeShader.SetFloat("width",320);//(endXindex-startXindex));
		
		ps_computeShader.Dispatch(0, 640/32, 480/32, 1);
		argBuffer.SetData(args);
		ComputeBuffer.CopyCount(ps_xyzpositions_buffer, argBuffer, 0);
	}

	void OnRenderObject() {
		ps_material.SetPass(0);
		ps_material.SetBuffer("buffer", ps_xyzpositions_buffer);
		ps_material.SetMatrix("cameraToWorldMatrix", OculusCamera.cameraToWorldMatrix);
//		Graphics.DrawProcedural(MeshTopology.Points, 320*240);
		Graphics.DrawProceduralIndirect(MeshTopology.Points, argBuffer, 0);
	}

	void OnDestroy() {
		ps_xyzpositions_buffer.Release();
		argBuffer.Release();
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
			if ( (quaternion_y - Prev_quaternion_y) > 0.04 ) {  //0.05
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
	
	private void UVEquivalent(int fromWidth, int fromHeight, float fromU, int fromV, int toWidth, int toHeight, out float toU, out float toV, out int toIndex)
	{
		float uNorm = fromU / (float)fromWidth;
		float vNorm = (float)fromV / (float)fromHeight;
		
		toU = (uNorm * (float)toWidth);
		toV = (vNorm * (float)toHeight);
		
		toIndex = (int) (toU + toV * (float)toWidth);
	}
	
	
	// A remapping process that returns a value from the 0 to 1 range.
	private float lmap(float val, float min0, float max0, float min1, float max1)
	{
		return min1 + (val-min0)*(max1-min1)/(max0-min0);
	}
	
}	