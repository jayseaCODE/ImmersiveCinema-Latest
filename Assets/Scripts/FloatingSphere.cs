using UnityEngine;
using System.Collections;

public class FloatingSphere : MonoBehaviour {

	public float speed=20f;
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(Vector3.zero, Vector3.up, speed * Time.deltaTime);
	}
}
