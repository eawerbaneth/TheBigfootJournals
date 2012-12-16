using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class nodescript : MonoBehaviour {
	public bool active = true;
	public Vector2 loc;
	public List<GameObject> adjacent;
	
	
	// Use this for initialization
	void Start () {
		//make sure we're not overlapping with anything
		Collider[] nearby = Physics.OverlapSphere(transform.position, 0.5f);
		if(nearby.GetLength(0) > 0){
			active = false;
			GetComponent<MeshRenderer>().material.color = Color.red;	
		}
	}
	
	public float CalculateDistance(Vector3 goal_pos){
		return Vector3.Distance(transform.position, goal_pos);		
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
}
