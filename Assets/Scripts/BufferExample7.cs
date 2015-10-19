// Buffer Example, compatible with shader7.shader
using UnityEngine;
using System.Collections;

public class BufferExample7: MonoBehaviour {
	
	public Material material;
	ComputeBuffer buffer;
	
	const int count = 1024; //number of elements in the buffer
	const float size = 5.0f;
	
	// Use this for initialization
	void Start () {
		buffer = new ComputeBuffer(count, sizeof(float)*3, ComputeBufferType.Default); //(count, stride, type)
		//stride is the size in bytes of each element in the buffer, note a float is 4 bytes
		
		float[] points = new float[count*3];
		
		Random.seed = 0;
		for(int i = 0; i < count; i++)
		{
			points[i*3+0] = Random.Range(-size,size);
			points[i*3+1] = Random.Range(-size,size) + 100;
			points[i*3+2] = 0.0f;
		}
		
		buffer.SetData(points); //pass the positions we have made to the buffer which is sent to the GPU
	}
	
	void OnRenderObject() //Drawing of the data obtained from gpu
	{
		material.SetPass(0);
		material.SetBuffer("buffer", buffer);
		Graphics.DrawProcedural(MeshTopology.Points, count, 1); //(topology type, vertex count, instance count)
		//vertex count is the numver of elements in the buffer
		//instance count is how many times to draw the same data
	}
	
	void OnDestroy()
	{
		buffer.Release();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
