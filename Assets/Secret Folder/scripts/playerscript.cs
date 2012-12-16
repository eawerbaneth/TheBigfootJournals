using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class playerscript : Persistent {
	List<PlayerItem> items = new List<PlayerItem>();
	int itemsPos = -1;
	public GUISkin menuskin;
	
	bool paused = false;
	
	string game_name = "";
	
	bool in_conversation = false;

	// Use this for initialization
	void Start () {
		//AddItem(GetComponent<ItemCamera>() as PlayerItem);
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
	}
	
	void OnGUI(){
			
		if(paused){
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 25, 100, 50), "Quit", menuskin.button))
				Application.Quit();
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 -75, 100, 50), "Resume", menuskin.button)){

				paused = false;			
				Screen.showCursor = false;
				GetComponent<vp_FPSPlayer>().LockCursor = true;
			}
		}
		
		
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			//GetComponent<vp_FPSPlayer>().LockCursor = !GetComponent<vp_FPSPlayer>().LockCursor;
			paused = !paused;
			if(paused){
				GetComponent<vp_FPSPlayer>().LockCursor = false;
				Screen.showCursor = true;	
			}
			else{
				//GetComponent<vp_FPSPlayer>().LockCursor = true;
				Screen.showCursor = false;
				GetComponent<vp_FPSPlayer>().LockCursor = true;
			}
		}
		
		if(Input.GetKeyDown(KeyCode.E)){
			//see if we're close enough to talk to anyone
			Collider[] colliders = Physics.OverlapSphere(transform.position, 5);
			float distance = 1000;
			int index = -1;
			
			//get closest npc OR book
			for(int i = 0; i < colliders.GetLength(0); i++){
				if(colliders[i].gameObject.GetComponent<ConversationManager>() || colliders[i].gameObject.GetComponent<book>()){
					if(Vector3.Distance(colliders[i].transform.position, transform.position) < distance){
						distance = Vector3.Distance(colliders[i].transform.position, transform.position);
						index = i;
					}
					
				}
					
			}
			//start talking to them
			if(index != -1){
				if(colliders[index].GetComponent<ConversationManager>())
					colliders[index].GetComponent<ConversationManager>().ToggleActive();
				else colliders[index].GetComponent<book>().ToggleActive();
			}
			
		}
		
	/*	if (Input.GetKeyDown(KeyCode.F5)) {
			Debug.Log("Saving game state");
			Persistent[] objs = FindSceneObjectsOfType(typeof(Persistent)) as Persistent[];
			foreach (Persistent obj in objs) {
				obj.OnSave();
			}
		}
		if (Input.GetKeyDown(KeyCode.F9)) {
			Debug.Log("Loading game state");
			Persistent[] objs = FindSceneObjectsOfType(typeof(Persistent)) as Persistent[];
			foreach (Persistent obj in objs) {
				obj.OnLoad();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.F11)) {
			Object[] os = FindObjectsOfType(typeof(ItemPickup));
			foreach (Object o in os) {
				(o as ItemPickup).enabled = true;
			}
		}
		
		if (Input.GetKeyDown(KeyCode.F12)) {
			PlayerPrefs.DeleteAll();
		} */
		
		if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(KeyCode.LeftBracket)) {
			UnequipItem();
			PrevItem();
			EquipItem();
		}
		
		if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(KeyCode.RightBracket)) {
			UnequipItem();
			NextItem();
			EquipItem();
		}
		
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			if (itemsPos != -1) {
				items[itemsPos].Activate();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Mouse1)) {
			if (itemsPos != -1) {
				items[itemsPos].AltActivate();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Z)) {
			RaycastHit hit;
			float grabDistance = 2.5f;
			Debug.DrawRay(Camera.mainCamera.transform.position, Camera.mainCamera.transform.forward, Color.red, 5.0f);
			if (Physics.Raycast(Camera.mainCamera.transform.position, Camera.mainCamera.transform.forward, out hit, grabDistance)) {
				ItemPickup ip = hit.transform.GetComponent<ItemPickup>();
				Door d = hit.transform.GetComponent<Door>();
				
				Transform t = hit.transform;
				if (!d) {
					while (t.parent) {
						t = t.parent;
						if (t.GetComponent<Door>()) {
							d = t.GetComponent<Door>();
							break;
						}
					}
				}
				
				if (ip && ip.enabled) {
					ip.Activate(this);
				}
				if (d) {
					d.Toggle();
				}
			}
		}
	}
	
	public PlayerItem equipped {
		get {
			if (itemsPos != -1) {
				return items[itemsPos];
			}
			return null;
		}
	}
	
	public override void OnSave() {
		SaveTransform("Player");
	}
	
	public override void OnLoad() {
		//LoadTransform("Player");
		
		/*items.Clear();
		foreach (PlayerItem pi in GetComponents<PlayerItem>()) {
			Debug.Log(game_name + "PlayerHas" + pi.GetType().Name + PlayerPrefs.GetInt(game_name + "PlayerHas" + pi.GetType().Name));
			if (PlayerPrefs.GetInt(game_name + "PlayerHas" + pi.GetType().Name) == 1) {
				AddItem(pi);
			}
		}*/
	}
	
	private void UnequipItem() {
		if (itemsPos != -1) {
			items[itemsPos].Unequip();
		}
	}
	
	private void EquipItem() {
		if (itemsPos != -1) {
			items[itemsPos].Equip();
		}
	}
	
	private void NextItem() {
		if (itemsPos != -1) {
			items[itemsPos].equipped = false;
		}
		
		if (++itemsPos >= items.Count) {
			GetComponent<vp_FPSPlayer>().SetWeapon(1);
			itemsPos = -1;
		} else {
			items[itemsPos].equipped = true;
		}
		
		Debug.Log("Equipping " + (itemsPos != -1 ? items[itemsPos].name : "nothing"));
	}
	
	private void PrevItem() {
		if (itemsPos == -1) {
			itemsPos = items.Count;
		} else {
			items[itemsPos].equipped = false;
		}
		
		if (--itemsPos != -1) {
			items[itemsPos].equipped = true;
		}
		else
			GetComponent<vp_FPSPlayer>().SetWeapon(1);
		
		Debug.Log("Equipping " + (itemsPos != -1 ? items[itemsPos].name : "nothing"));
	}
	
	public void AddItem(PlayerItem pi) {
		items.Add(pi);
		pi.enabled = true;
		//PlayerPrefs.SetInt(game_name+"PlayerHas" + pi.GetType().Name, 1);
	}
	
	void LoadGame(){
		//OnLoad();
		
	}
	
	void OnTriggerEnter(Collider other){
		if(other.GetComponent<soundsoff>())
			other.GetComponent<soundsoff>().TurnOff();
		
	}
	
	void OnTriggerExit(Collider other){
		if(other.GetComponent<soundsoff>())
			other.GetComponent<soundsoff>().TurnOn();
		
	}
}
