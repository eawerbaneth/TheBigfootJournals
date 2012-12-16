                                                using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class map : MonoBehaviour {
	
	string game_name;
	
	public Texture2D map_img;
	bool map_on = false;
	//scrolling stuff
	int x_offset = 0;
	int y_offset = 0;
	bool x_movable = false;
	bool y_movable = false;
	bool moved = false;
	Texture2D map_subset;
	
	//map movement utilities
	bool scrolling = false;
	Vector3 last_pos = Vector3.zero;
	int last_screen_width;
	int last_screen_height;
	float zoom_factor = 1.0f;
	
	//click utilities
	float click_timer = 0.0f;
	bool adding = false;
	bool was_adding = false;
	int note_index = 0;
	
	string note_content = "";
	Vector3 note_pos = Vector3.zero;
	
	public GUISkin mapstyle;
	
	//map utility struct
	class MapNote{
		public Vector3 pos;
		public string note;
		
		public MapNote(){}
		public MapNote(Vector3 p, string n){
			pos = p;
			note = n;			
		}
		
	};
	List <MapNote> notes = new List<MapNote>();
	
	void AutoSave(){
		//Debug.Log("AutoSaving Map");
		WriteFile();		
	}
	
	void CalculateSubset(){
		//check bounds
		//Debug.Log("Map offset " + x_offset + ", " + y_offset);
		
		//check left boundary
		if(x_offset < 0)
			x_offset = 0;
		//check top boundary
		if(y_offset  < 0)
			y_offset = 0;

		if(Screen.width <= map_img.width || Screen.height <= map_img.height){
			x_movable = y_movable = true;
			//scroll both
			if(Screen.width*zoom_factor <= map_img.width && Screen.height*zoom_factor <= map_img.height){
				//Debug.Log("trying to create with " + Screen.width*zoom_factor + " " + Screen.height*zoom_factor);
				map_subset = new Texture2D((int)(Screen.width*zoom_factor), (int)(Screen.height*zoom_factor));
				
				//check right boundary
				if(x_offset + Screen.width*zoom_factor > map_img.width)
					x_offset = map_img.width - (int)(Screen.width*zoom_factor);
				//check bottom boundary
				if(y_offset + Screen.height*zoom_factor > map_img.height)
					y_offset = map_img.height - (int)(Screen.height*zoom_factor);
				
				map_subset.SetPixels(map_img.GetPixels(x_offset, map_img.height - y_offset - (int)(Screen.height*zoom_factor), 
					(int)(Screen.width*zoom_factor), (int)(Screen.height*zoom_factor)));
				
			}
			//scroll horizontal bounds
			else if(Screen.width*zoom_factor <= map_img.width){
				x_movable = true;
				y_movable = false;
				map_subset = new Texture2D((int)(Screen.width*zoom_factor), map_img.height);
				
				if(x_offset + Screen.width*zoom_factor > map_img.width)
					x_offset = map_img.width - (int)(Screen.width*zoom_factor);
				
				
				map_subset.SetPixels(map_img.GetPixels(x_offset, 0, (int)(Screen.width*zoom_factor), map_img.height));
				
			}
			//only scroll vertical bounds
			else{
				x_movable = false;
				y_movable = true;
				map_subset = new Texture2D(map_img.width, (int)(Screen.height*zoom_factor));
				
				//check bottom boundary
				if(y_offset + Screen.height*zoom_factor > map_img.height)
					y_offset = map_img.height - (int)(Screen.height*zoom_factor);
				map_subset.SetPixels(map_img.GetPixels(0, map_img.height - y_offset - (int)(Screen.height*zoom_factor), map_img.width, (int)(Screen.height*zoom_factor)));
				
			}
		}
		else{
			x_movable = y_movable = false;
			x_offset = 0;
			y_offset = 0;
			
			map_subset = new Texture2D(map_img.width, map_img.height);
			map_subset.SetPixels(map_img.GetPixels(0, 0, map_img.width, map_img.height));
			
		}
		
		map_subset.Apply();
		moved = false;
		
		//Debug.Log("changed subset - " + map_subset.width + " " + map_subset.height);
		
		//Debug.Log(x_offset + ", " + y_offset + " : " + Screen.height + ", " + Time.deltaTime);
	}
	
	// Use this for initialization
	void Start () {
		last_screen_height = Screen.height;
		last_screen_width = Screen.width;
		//init map subset
		CalculateSubset();
		
		//ReadFile();
		
	}
	
	void NewGame(){
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		AutoSave();
	}
	
	void LoadGame(){
		game_name = GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name;
		ReadFile();
	}
	
	
	void OnGUI(){
		if(map_on){			
			//display background
			//Graphics.DrawTexture(new Rect(left, top, map_subset.width, map_subset.height), map_subset);
			Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), map_subset);
			//Debug.Log(map_subset.width + " " + map_subset.height);
			
			//display notes - x_offset and map_subset.height - y_offset are our left and top coords, width and height dictated by map_subset
			foreach(MapNote note in notes){
				//let's say each note is going to take 100x50 to display
				if(note.pos.x >= x_offset && note.pos.y >= y_offset  && note.pos.x + 100 <= x_offset + map_subset.width && note.pos.y + 100 <= y_offset + map_subset.height){
					GUI.Label(new Rect(note.pos.x - x_offset,note.pos.y - y_offset, 200, 100), note.note, mapstyle.label);
				}
			}
			
			//adding a new note
			if(adding){
				MapNote note = notes[note_index];
				notes[note_index].note = GUI.TextField(new Rect(note.pos.x - x_offset, note.pos.y - y_offset, 200, 100), notes[note_index].note, mapstyle.textField);
				int extra_offset = 100;
				if(note.pos.y - y_offset + 150 > Screen.height)
					extra_offset = - 250;
				if(GUI.Button(new Rect(note.pos.x - x_offset, note.pos.y - y_offset + extra_offset, 100, 50), "Save")){
				//if(GUI.Button(new Rect(175, Screen.height - 100, 100, 25), "accept")){
					adding = false;
					scrolling = false;
					//WriteFile();
				}
				if(GUI.Button(new Rect(note.pos.x - x_offset + 100, note.pos.y - y_offset + extra_offset, 100, 50), "Delete")){
				//if(GUI.Button(new Rect(300, Screen.height - 100, 100, 25), "delete")){
					adding = false;
					scrolling = false;
					notes.RemoveAt(note_index);
					//WriteFile();
				}
				
			}
			
			
		}
		
	}
	
	void WriteFile(){
		TextWriter tw = new StreamWriter("Assets\\Secret Folder\\scripts\\" + game_name + "map.txt", false);
		
		foreach(MapNote note in notes)
			tw.WriteLine(note.pos.x + "," + note.pos.y + "#" + note.note);
		
		tw.Close();
	}
	
	void ReadFile(){
		//file will be encoded x,y#note
		
		//create writer and open the file
		TextReader tr = new StreamReader("Assets\\Secret Folder\\scripts\\" + game_name + "map.txt");
		
		string line = tr.ReadLine();
		//read a line of text to the file
		
		while(line != null){
			if(line[0]=='#'){//this line is a comment, skip it
				line = tr.ReadLine();
				continue;
			}
			
			string[] parts = line.Split('#');
			string[] p1 = parts[0].Split(',');
			Vector3 pos = new Vector3(float.Parse(p1[0]), float.Parse(p1[1]));
			notes.Add(new MapNote(pos, parts[1]));
			
			line = tr.ReadLine();
			
		}
		
		tr.Close();
		
	}
	
	public void AddNote(float x, float y, string desc){
		notes.Add(new MapNote(new Vector3(x, y, 0), desc));
		note_index = notes.Count - 1;
		//Debug.Log("adding note " + desc + " at " + x + ", " + y);
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Screen.width != last_screen_width || Screen.height != last_screen_height)
			CalculateSubset();
		
		if(Input.GetKeyDown(KeyCode.M)){
			map_on = !map_on;
			GetComponent<vp_FPSPlayer>().LockCursor = !GetComponent<vp_FPSPlayer>().LockCursor;
			GetComponent<vp_FPSPlayer>().enabled = !GetComponent<vp_FPSPlayer>().enabled;
			GetComponent<vp_FPSCrosshair>().enabled = !GetComponent<vp_FPSCrosshair>().enabled;
			GetComponentInChildren<vp_FPSCamera>().enabled = !GetComponentInChildren<vp_FPSCamera>().enabled;
			
			//GetComponent<MouseLook>().enabled = !GetComponent<MouseLook>().enabled;
			GetComponent<playerscript>().enabled = !GetComponent<playerscript>().enabled;
			
			if(!map_on)
				AutoSave();
		}
		
		//moving map
		if(map_on){
			bool modified = false;
			
			//zoom in/out
			float scroll = 0;//Input.GetAxis("Mouse ScrollWheel");
			if(scroll != 0){
				if((zoom_factor + scroll)*Screen.width <= map_img.width && (zoom_factor + scroll)*Screen.height <= map_img.height){
					float x_displacement = Screen.width*scroll/2;
					float y_displacement = Screen.height*scroll/2;
					
					zoom_factor += scroll;
					//adjust x- and y-offset
					x_offset+= (int)x_displacement;
					y_offset += (int)y_displacement;
					
					modified = true;
				}
				
			}
			
			//click and drag
			if(!adding && scrolling){
				click_timer += Time.deltaTime;
				if(click_timer > 0.2f){
					Vector3 disp = last_pos - Input.mousePosition;
					last_pos = Input.mousePosition;
					if(x_movable)
						x_offset += (int)disp.x;
					if(y_movable)
						y_offset -= (int)disp.y;
					modified = true;
				}
				
				
			}
			if(!adding && Input.GetMouseButtonDown(0)){
				scrolling = true;	
				last_pos = Input.mousePosition;
			}
			if(!was_adding && Input.GetMouseButtonUp(0)){
				//check to see if it was a click [add/edit note]
				if(click_timer <= 0.2f){
					//check to see if we are going to edit an existing note or add a new one
					int left = Screen.width/2 - map_subset.width/2;
					int top = Screen.height/2 - map_subset.height/2;
					
					bool found = false;
					
					for(int i = 0; i < notes.Count; i++){
						Rect r = new Rect(left + notes[i].pos.x - x_offset, top + notes[i].pos.y - y_offset, 100, 50);
						//Debug.Log(Input.mousePosition + " " + (left + notes[i].pos.x - x_offset) + ", " + (top + notes[i].pos.y - y_offset));
						
						if(r.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))){
							found = true;
							note_index = i;
						}
						
						//if(r.Contains(new Vector2(
							//left + note.pos.x - x_offset, top + note.pos.y - y_offset
						/*
						 * int left = Screen.width/2 - map_subset.width/2;
						int top = Screen.height/2 - map_subset.height/2;
			
						 * */
						
					}
					if(!found){						
						AddNote(Input.mousePosition.x*zoom_factor + x_offset, Screen.height - Input.mousePosition.y*zoom_factor + y_offset, "");
					}
					
					adding = true;
										
				}
				scrolling = false;
				click_timer = 0.0f;
				
				
				
			}
			
			if(modified)
				CalculateSubset();
			
			was_adding = adding;
		}
		
	}
}
