using UnityEngine;
using System.Collections;

public class npcscript : MonoBehaviour {
	public GameObject [] destinations;
	int cur_dest = 0;
	public bool test = false;
	
	
	// Use this for initialization
	void Start () {
		//generate our nodes
		
	
	}
	
	// Update is called once per frame
	void Update () {
		if(test){
			GoToDestination();
			test = false;			
		}
	
	}
	
	void GoToDestination(){
		GetComponent<NavMeshAgent>().destination = destinations[cur_dest].transform.position;
		cur_dest++;
		if(cur_dest == destinations.GetLength(0))
			cur_dest = 0;
		
		
	}
}
