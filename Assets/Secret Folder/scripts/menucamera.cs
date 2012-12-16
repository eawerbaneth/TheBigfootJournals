using UnityEngine;
using System.Collections;

public class menucamera : MonoBehaviour {
	
	public GUISkin menuskin;
	
	string game_name = "";
	string [] saved_games;
	WorldManager wm;
	Transform original;
	Transform next;
	
	bool new_game = false;
	bool load_game = false;
	int load_index = -1;
	
	// Use this for initialization
	void Start () {
		//get all our saved games from player prefs
		string games = PlayerPrefs.GetString("SavedGames");
		saved_games = games.Split('~');
	
		wm = GameObject.Find("WorldManager").GetComponent<WorldManager>();
		
		original = GameObject.Find("Redford").transform;
		next = GameObject.Find("Yang").transform;
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.Rotate(new Vector3(0, 5*Time.smoothDeltaTime, 0));
	}
	
	void OnGUI(){
		//display the three files, then submenus for the games
		//main menu
		GUI.Label(new Rect(Screen.width/2 - 300, Screen.height/2 - 150, 600, 200), "The Bigfoot Journals", menuskin.label);
		if(load_index == -1){
			for(int i = 0; i < 3; i++){
				if(saved_games.Length -1 < i || saved_games[i] == ""){
					if(GUI.Button(new Rect(Screen.width/2 - 175 + 125*i, Screen.height/2 - 25, 100, 50), "New Game", menuskin.button)){
						load_index = i;
						new_game = true;						
					}
				}
				else{
					if(GUI.Button(new Rect(Screen.width/2 - 175 + 125*i, Screen.height/2 - 25, 100, 50), saved_games[i], menuskin.button)){
						load_index = i;
						load_game = true;			
					}
				}
			}
			if(GUI.Button(new Rect(Screen.width/2 +25, Screen.height/2 + 100, 100, 50), "Quit", menuskin.button))
				Application.Quit();
			if(GUI.Button(new Rect(Screen.width/2 - 225, Screen.height/2 + 100, 200, 50), "Clear All Data", menuskin.button)){
				saved_games = "".Split('~');
				PlayerPrefs.DeleteAll();				
			}
		}
		else if(new_game){
			game_name = GUI.TextField(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 100), game_name);
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 100, 100, 50), "GO", menuskin.button)){
				if(game_name.Contains("~")){
					Debug.Log("Invalid name - cannot contain '~'");
					return;	
				}
				bool acceptable = true;
				foreach(string game in saved_games)
					if(game_name == game)
						acceptable = false;
				if(!acceptable)
					Debug.Log("That name has already been taken");
				else
					NewGame();
				
			}
			
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 250, 100, 50), "Back", menuskin.button)){
				load_index = -1;
				new_game = false;
				load_game = false;
			}
		}
		else if(load_game){
			GUI.Label(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 100), saved_games[load_index], menuskin.label);
			if(GUI.Button(new Rect(Screen.width/2 - 125, Screen.height/2 + 100, 100, 50), "Load", menuskin.button)){
				game_name = saved_games[load_index];
				LoadGame();				
			}
			if(GUI.Button(new Rect(Screen.width/2 + 25, Screen.height/2 + 100, 100, 50), "Delete", menuskin.button)){
				string games = "";
				for(int i = 0; i < saved_games.Length; i++){
					if(i != load_index){
						games += saved_games[i];
						if((i != saved_games.Length - 1) && (load_index == saved_games.Length - 1 && i == saved_games.Length -2))
							games += "~";
					}						
				}
				PlayerPrefs.SetString("SavedGames", games);
				saved_games = games.Split('~');
				
				load_index = -1;
				new_game = false;
				load_game = false;
			}
			
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 250, 100, 50), "Back", menuskin.button)){
				load_index = -1;
				new_game = false;
				load_game = false;
			}
		}
		
		
		/*
		//main menu
		if(!new_game && !load_game){
			if(GUI.Button(new Rect(Screen.width/2 - 200, Screen.height/2 - 50, 100, 100), "New Game"))
				new_game = true;
			if(GUI.Button(new Rect(Screen.width/2 + 100, Screen.height/2 - 50, 100, 100), "Load Game"))
				load_game = true;
			if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 100, 100, 100), "Quit"))
				Application.Quit();
		}
		//new game submenu
		if(new_game){
			game_name = GUI.TextField(new Rect(Screen.width/2 - 100, Screen.height/2 - 50, 200, 50), game_name);
			if(GUI.Button(new Rect(Screen.width/2 - 125, Screen.height/2 + 25, 100, 50), "Go!")){
				if(game_name.Contains("~")){
					Debug.Log("Invalid name - cannot contain '~'");
					return;	
				}
				bool acceptable = true;
				foreach(string game in saved_games)
					if(game_name == game)
						acceptable = false;
				if(!acceptable)
					Debug.Log("That name has already been taken");
				else
					NewGame();
					
			}
			if(GUI.Button(new Rect(Screen.width/2 + 25, Screen.height/2+25, 100, 50), "Back"))
				new_game = false;			
		}
		//load game submenu
		if(load_game){
			
			if(load_index == -1){
				if(saved_games.Length == 0 || (saved_games.Length == 1 && saved_games[0] == ""))
					GUI.Label(new Rect(Screen.width/2 - 100, Screen.height/2 - 50, 200, 50), "There are no saved games.");
			
				for(int i = 0; i < saved_games.Length; i++){
					if(GUI.Button(new Rect(Screen.width/2 - (i/2)*125 + 125*i, Screen.height/2 - 50, 100, 50), saved_games[i])){
						//game_name = saved_games[i];
						//LoadGame();
						load_index = i;
					}
				
				}
				if(GUI.Button(new Rect(Screen.width/2 + 25, Screen.height/2+25, 100, 50), "Back"))
					load_game = false;
			}
			else{
				GUI.Label(new Rect(Screen.width/2 - 50, Screen.height/2 - 50, 100, 50), saved_games[load_index]);
				if(GUI.Button(new Rect(Screen.width/2 - 175, Screen.height/2 + 25, 100, 50), "Load")){
					game_name = saved_games[load_index];
					LoadGame();					
				}
				if(GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 25, 100, 50), "Delete")){
					
					string games = "";
					for(int i = 0; i < saved_games.Length; i++){
						if(i != load_index){
							games += saved_games[i];
							if((i != saved_games.Length - 1) && (load_index == saved_games.Length - 1 && i == saved_games.Length -2))
								games += "~";
						}						
					}
					PlayerPrefs.SetString("SavedGames", games);
					saved_games = games.Split('~');
					
					load_index = -1;
					
				}
				if(GUI.Button(new Rect(Screen.width/2 + 75, Screen.height/2 + 25, 100, 50), "Back"))
					load_index = -1;				
				
			}
			
			
			
		}*/
		
		
	}
	
	void LoadGame(){
		wm.BeginGame(false, game_name);
		gameObject.active = false;
		
	}
	
	void NewGame(){
		
		string new_val = PlayerPrefs.GetString("SavedGames");
		if(new_val == "")
			new_val = game_name;
		else
			new_val += ("~"+game_name);
		PlayerPrefs.SetString("SavedGames", new_val);
		wm.BeginGame(true, game_name);
		gameObject.active = false;
		
		
	}
	
	
}
