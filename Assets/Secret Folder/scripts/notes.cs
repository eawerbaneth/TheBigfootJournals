using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;


public class notes : MonoBehaviour {
	string content = "";
	bool notes_on = false;
	public GUISkin noteskin;
	string game_name = "";
	
	List <string> pages = new List<string>();
	int page_index = 0;
	
	public Texture2D notes_background;
	
	
	// Use this for initialization
	void Start () {
		/*ReadFile();
		
		if(pages.Count == 0)
			pages.Add("");
			*/
	
	}
	
	void NewGame(){
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		pages.Add("");
		AutoSave();
		
	}
	
	void LoadGame(){
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		
		ReadFile();
		
		if(pages.Count == 0)
			pages.Add("");
	}
	
	void AutoSave(){
		WriteFile();
	}
	
	void EnableAll(){
		GetComponent<playerscript>().enabled = true;
		GetComponent<vp_FPSController>().enabled = true;
		GetComponent<vp_FPSPlayer>().enabled = true;
		GetComponent<vp_FPSCrosshair>().enabled = true;
		GetComponentInChildren<vp_FPSCamera>().enabled = true;
		Screen.showCursor = true;
		GameObject.Find("FPSPlayer").GetComponent<vp_FPSPlayer>().LockCursor = true;
	}
	
	void DisableAll(){
		GetComponent<playerscript>().enabled = false;
		GetComponent<vp_FPSController>().enabled = false;
		GetComponent<vp_FPSPlayer>().enabled = false;
		GetComponent<vp_FPSCrosshair>().enabled = false;
		GetComponentInChildren<vp_FPSCamera>().enabled = false;
		GameObject.Find("FPSPlayer").GetComponent<vp_FPSPlayer>().LockCursor = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.N)){
			notes_on = !notes_on;
			if(notes_on)
				DisableAll();
			
			if(!notes_on)
				EnableAll();
		}
		/*if(Input.GetKeyDown(KeyCode.Escape)){
			notes_on = false;
			EnableAll();
		}*/
		
	}
	
	void OnGUI(){
		if(notes_on){
			//if(Event.current
			if(Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Escape){
				GUI.SetNextControlName("unfocus");
				GUI.Button(new Rect(-10000, -10000, 0, 0), GUIContent.none);
				GUI.FocusControl("unfocus");
				
				notes_on = false;
				EnableAll();
				
			}
			else{
				GUI.DrawTexture(new Rect(Screen.width/2 - 256, Screen.height/2-256, 512, 512), notes_background);
				
				//GUI.Label(new Rect(100, 100, Screen.width - 200, Screen.height - 200), notes_background);
				GUI.Label(new Rect(Screen.width - 200, 50, 75, 25), (page_index + 1) + "/" + pages.Count);
				GUI.SetNextControlName("notes");
				//GUI.Label(new Rect(Screen.width/2 - 125, Screen.height/2 - 100, 300, 300), content[page_index], bookskin.label);
				
				pages[page_index] = GUI.TextArea(new Rect(Screen.width/2 - 125, Screen.height/2 - 100, 300, 300), pages[page_index], noteskin.textField);
				GUI.FocusControl("notes");
				
				if(GUI.Button(new Rect(Screen.width/2 + 100, Screen.height/2 - 150, 45, 45), "+")){
					if(pages.Count <= page_index + 1){
						pages.Add("");	
					}
					page_index++;
				}
				if(page_index != 0 && GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 - 150, 45, 45), "-")){
					if(pages[page_index] == "")
						pages.RemoveAt(page_index);
					page_index--;
					
				}
			}
		}
	}
	
	void ReadFile(){
		//create writer and open the file
		TextReader tr = new StreamReader("Assets\\Secret Folder\\scripts\\" + game_name + "notes.txt");
		
		string all;
		
		all = tr.ReadToEnd();
		string [] split = all.Split('~');
		foreach(string _content in split)
			pages.Add(_content);
		
		tr.Close();
		
	}
	
	void WriteFile(){
		TextWriter tw = new StreamWriter("Assets\\Secret Folder\\scripts\\"+game_name+ "notes.txt");
		
		if(pages.Count > 0)
			tw.Write(pages[0]);
		for(int i = 1; i < pages.Count; i++)
			tw.Write("~" + pages[i]);
		
		tw.Close();
		
	}
}
