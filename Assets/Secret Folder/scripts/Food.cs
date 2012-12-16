using UnityEngine;
using System.Collections;

public class Food : MonoBehaviour {
	public float foodValue;
	
	public float respawnRate;
	
	protected float respawnTimer = 0.0f;
	
	void Start () {
	
	}
	
	void Update () {
		respawnTimer = Mathf.Max(0.0f, respawnTimer - Time.deltaTime);
	}
	
	public bool Spawned() {
		return respawnTimer == 0.0f;
	}
	
	public float Eat() {
		if (Spawned()) {
			Bait playerbait = GameObject.Find("FPSPlayer").GetComponentInChildren<Bait>();
			playerbait.fed = true;
			GameObject.Find("WorldManager").GetComponent<WorldManager>().AddEvent("1+Saito+19-19".Split('+'));
			Debug.Log("FED BIGFOOT");
			
			respawnTimer = respawnRate;
			return foodValue;
		} else {
			Debug.LogWarning("Trying to eat non-spawned food at " + transform.position);
			return 0.0f;
		}
	}
		
}
