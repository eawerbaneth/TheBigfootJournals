using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour {
	
	public PlayerItem itemObject;
	public string itemSaveName;
	
	string game_name;
	
	void NewGame() {
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		
		itemSaveName = game_name + "PlayerHas" + itemObject.GetType().Name;
		if (PlayerPrefs.GetInt(itemSaveName) == 1) {
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			Activate(p.GetComponent<playerscript>());
		} else {
			enabled = false;
			if(PlayerPrefs.GetInt(itemSaveName) == 2 || name == "PickupBinoculars")
				AllowPickup();
		}
		
	}
	
	void LoadGame(){
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		
		itemSaveName = game_name + "PlayerHas" + itemObject.GetType().Name;
		if (PlayerPrefs.GetInt(itemSaveName) == 1) {
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			Activate(p.GetComponent<playerscript>());
		} else {
			enabled = false;
			if (name == "PickupBinoculars") {
				AllowPickup();
			}
		}
		
	}
	
	void Start () {
		/*itemSaveName = "PlayerHas" + itemObject.GetType().Name;
		if (PlayerPrefs.GetInt(itemSaveName) == 1) {
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			Activate(p.GetComponent<playerscript>());
		} else {
			enabled = false;
			if (name == "PickupBinoculars") {
				AllowPickup();
			}
		}*/
	}
	
	public void AllowPickup() {
		if (renderer) {
			renderer.material.SetColor("_OutlineColor", Color.magenta);
		} else {
			for (int i = 0; i < transform.GetChildCount(); ++i) {
				if (transform.GetChild(i).renderer) {
					transform.GetChild(i).renderer.material.SetColor("_OutlineColor", Color.magenta);
				}
			}
		}
		enabled = true;
	}
	
	public void Activate(playerscript p) {
		Debug.Log(itemSaveName);
		PlayerPrefs.SetInt(itemSaveName, 1);
		//itemObject.active = true;
		p.AddItem(itemObject);
		Destroy(gameObject);
	}
	
	void Update () {
	
	}
}
