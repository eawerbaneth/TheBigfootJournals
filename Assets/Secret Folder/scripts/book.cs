using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class book : MonoBehaviour {
	
	public string title = "default";
	bool activated = false;
	string [] content;
	int page_index = 0;
	public Texture2D book_background;
	public GUISkin bookskin;
	public AudioClip sound;
	
	float delay = 1.0f;
	
	// Use this for initialization
	void Start () {
		ReadFile();
	}
	
	// Update is called once per frame
	void Update () {
		if(activated)
			delay -= Time.deltaTime;
		
		if(delay < 0 && activated && Input.GetKey(KeyCode.E)){
			ToggleActive();
		}
	
	}
	
	public void ToggleActive(){
		activated = !activated;	
		delay = 1.0f;
		
		GameObject player = GameObject.Find("FPSPlayer");
		//player.GetComponent<vp_FPSPlayer>().LockCursor = !player.GetComponent<vp_FPSPlayer>().LockCursor;
		player.GetComponent<vp_FPSController>().enabled = !player.GetComponent<vp_FPSController>().enabled;
		player.GetComponentInChildren<vp_FPSCamera>().enabled = !player.GetComponentInChildren<vp_FPSCamera>().enabled;
		player.GetComponent<playerscript>().enabled = !player.GetComponent<playerscript>().enabled;
		
		if(!activated){
			Screen.showCursor = false;
			player.GetComponent<vp_FPSPlayer>().LockCursor = true;
		}
		else
			player.GetComponent<vp_FPSPlayer>().LockCursor = false;
			
		GetComponent<AudioSource>().PlayOneShot(sound);
		//if we are being deactivated, check if we are a quest journal, if we are, call activate off that
		if(!activated && GetComponent<QuestJournal>()){
			GetComponent<QuestJournal>().Activate();
				
		}
	}
	
	void OnGUI(){
		if(activated){
			//if(Event.current
			//if(Event.current.type == EventType.keyDown && (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.E)){
				
			//	ToggleActive();
				
			//}
			//else{
				GUI.DrawTexture(new Rect(Screen.width/2 - 256, Screen.height/2-256, 512, 512), book_background);
				//GUI.Label(new Rect(Screen.width - 200, 50, 75, 25), (page_index + 1) + "/" + pages.Count);
				
				GUI.Label(new Rect(Screen.width/2 - 125, Screen.height/2 - 150, 75, 50), (page_index + 1) + "/" + content.GetLength(0), bookskin.label);
				GUI.Label(new Rect(Screen.width/2 - 125, Screen.height/2 - 100, 300, 300), content[page_index], bookskin.label);
				
				
				if(page_index < content.GetLength(0) -1 && GUI.Button(new Rect(Screen.width/2 + 100, Screen.height/2 - 150, 45, 45), "+"))
					page_index++;
				if(page_index != 0 && GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - 150, 45, 45), "-"))
					page_index--;
					
			//}
			
			
		}
		
		
	}
	
	void ReadFile(){
		//create writer and open the file
		TextReader tr = new StreamReader("Assets\\Secret Folder\\books\\" + title + ".txt");
		
		//Debug.Log("reading book file " + title);
		
		string all = tr.ReadToEnd();
		
		//Debug.Log(all);
		content = all.Split('~');
		
		tr.Close();
		
	}
	
	
}
