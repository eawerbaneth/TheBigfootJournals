using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class Persistent : MonoBehaviour {
	
	public abstract void OnSave();
	public abstract void OnLoad();
	
	protected void SaveTransform(string name) {
		WorldManager wm = GameObject.Find("WorldManager").GetComponent<WorldManager>();
		name = wm.game_name + name;
		
		PlayerPrefs.SetFloat(name + "PX", transform.position.x);
		PlayerPrefs.SetFloat(name + "PY", transform.position.y);
		PlayerPrefs.SetFloat(name + "PZ", transform.position.z);
		PlayerPrefs.SetFloat(name + "RW", transform.rotation.w);
		PlayerPrefs.SetFloat(name + "RX", transform.rotation.x);
		PlayerPrefs.SetFloat(name + "RY", transform.rotation.y);
		PlayerPrefs.SetFloat(name + "RZ", transform.rotation.z);
	}
	
	protected void LoadTransform(string name) {
		WorldManager wm = GameObject.Find("WorldManager").GetComponent<WorldManager>();
		name = wm.game_name + name;
		
		transform.position = new Vector3(
			PlayerPrefs.GetFloat(name + "PX"),
			PlayerPrefs.GetFloat(name + "PY"),
			PlayerPrefs.GetFloat(name + "PZ"));
		transform.rotation = new Quaternion(
			PlayerPrefs.GetFloat(name + "RX"),
			PlayerPrefs.GetFloat(name + "RY"),
			PlayerPrefs.GetFloat(name + "RZ"),
			PlayerPrefs.GetFloat(name + "RW"));
	}
}
