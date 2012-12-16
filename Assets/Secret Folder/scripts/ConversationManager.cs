using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour {
	
	string npc;
	public WorldManager world_manager;
	public GUISkin conversation_style;
	public GUISkin response_style;
	
	string game_name = "";
	
	class node{
		
		
		public string description;
		public string conv;
		public List <int> next;
		public bool active;
		public List <string[]> assoc_events = new List<string[]>();
		
		public node(){}
		public node(string desc, string c, char a, List <int> n){
			description = desc;
			conv = c;
			next = n;
			if(a == 'y')
				active = true;
			else
				active = false;
		}
		
		public void AddEvent(string[] args){
			assoc_events.Add(args);
		}
		
		public void FireEvent(WorldManager wm){
			foreach(string[] ev in assoc_events)
				wm.AddEvent(ev);
			assoc_events.Clear();
			
			//Debug.Log("firing event");
		}
		
		
	};
	
	List <node> nodes = new List<node>();
	int current = 0;
	bool active = false;
	public Texture2D background;
	
	public void DeactivateNodes(int begin, int end){
		Debug.Log("Deactivating nodes " + begin + " - " + end);
		
		for(int i = begin; i <= end && i < nodes.Count;i++)
			nodes[i].active = false;
		
	}
	
	public void ActivateNodes(int begin, int end){
		Debug.Log("Deactivating nodes " + begin + " - " + end);
		
		for(int i = begin; i <= end && i < nodes.Count; i++)
			nodes[i].active = true;
	}
	
	
	void ReadFile(bool new_game){
		//create writer and open the file
		TextReader tr;
		if(new_game)
			tr = new StreamReader("Assets\\Secret Folder\\Conversations\\fresh_conversations\\" + name + ".txt");
		else
			tr = new StreamReader("Assets\\Secret Folder\\Conversations\\"+ game_name + name + ".txt");
		
		string line = tr.ReadLine();
		//read a line of text to the file
		
		while(line != null){
			//Debug.Log(line);
			if(line[0]=='#'){//this line is a comment, skip it
				line = tr.ReadLine();
				continue;
			}
			
			/*#Template: 
			#[num]#description#contents#active?[y or n]#[num available from here]
			#-[num]#type+args
			#Args Format:
			#	0+Character+Destination -> character goto destination
			#	1+Character+conv_start_index-conv_end_index -> open conversation indices
			#	2+Character+conv_start_index-conv_end_index -> closes conversation indices
			#	3+item -> item becomes available to pick up*/
			
			//if a line starts with a -, it corresponds to an event that happens when you talk to this person
			
			string[] parts = line.Split('#');
			if(int.Parse(parts[0]) > 0){
				string[] nums = parts[4].Split(' ');
				List <int> n = new List<int>();
				for(int i = 0; i < nums.Length; i++)
					n.Add(int.Parse(nums[i])-1);
				nodes.Add(new node(parts[1], parts[2], char.Parse(parts[3]), n));
			}
			else{
				int index = (-(int.Parse(parts[0]))-1);
				nodes[index].AddEvent(parts[1].Split('+'));
			}
						
			line = tr.ReadLine();
			
			
		}
		
		tr.Close();
		
	}
	
	public void AutoSave(){
		
		TextWriter tw = new StreamWriter("Assets\\Secret Folder\\Conversations\\" + game_name + name + ".txt", false);
		
		for(int i = 0; i < nodes.Count; i++){
			string line = (i + 1) + "#" + nodes[i].description + "#" + nodes[i].conv + "#";
			if(nodes[i].active)
				line += "y#";
			else line += "n#";
			for(int k = 0; k < nodes[i].next.Count; k++){
				line += (nodes[i].next[k] + 1);
				if(k < nodes[i].next.Count - 1)
					line += " ";
			}			
			
			tw.WriteLine(line);
			for(int k = 0; k < nodes[i].assoc_events.Count; k++){
				string event_line = "-" + (i + 1) + "#";
				for(int j = 0; j < nodes[i].assoc_events[k].Length; j++){
					event_line += nodes[i].assoc_events[k][j];
					if(j < nodes[i].assoc_events[k].Length - 1)
						event_line += "+";
				}
				tw.WriteLine(event_line);
			}
		}
		
		tw.Close();
	}
	
	
	
	void OnGUI(){		
		if(active){
			//make the box
			GUI.DrawTexture(new Rect(0, Screen.height/3, Screen.width, 2*Screen.height/3), background);
			//GUI.Box(new Rect(0, 0, Screen.width, 200), background);
			//write the line
			if(name == "Saito")
				GUI.Label(new Rect(50, Screen.height/3 + 50, Screen.width - 100, 50), "Salt", conversation_style.label);
			else
				GUI.Label(new Rect(50, Screen.height/3 + 50, Screen.width - 100, 50), name, conversation_style.label);
			GUI.Label(new Rect(50, Screen.height/3 + 100, Screen.width-100, 200), nodes[current].conv, conversation_style.label);
			//fire event if we haven't already
			if(nodes[current].assoc_events.Count > 0)
				nodes[current].FireEvent(world_manager);
			
			//label options for progression if there are any
			List<int> next = nodes[current].next;
			int k = 0;
			for(int i = 0; i < nodes[current].next.Count; i++){	
				//exiting conversation
				if(next[i] == -1){
					GUI.Label(new Rect(50, Screen.height/3 + 250 + 50*k, Screen.width - 100, 50), k+1 + " Exit", response_style.label);
					k++;
				}
				//list another
				else if(nodes[next[i]].active){
					GUI.Label(new Rect(50, Screen.height/3 + 250 + 50*k, Screen.width - 100, 50), k+1 + " " + nodes[nodes[current].next[i]].description, response_style.label);
					k++;
				}
			}
			
			
			//apparently this stuff has to happen in OnGUI
			//check to see if we can advance the conversation
			k = 0;
			for(int i = 0; i < nodes[current].next.Count; i++){
				if(next[i]==-1 && Event.current.isKey && Input.anyKeyDown && Event.current.keyCode.ToString() =="Alpha"+(k+1)){
					ToggleActive();
					return;
				}
				else if(next[i]!=-1 && nodes[next[i]].active && Event.current.isKey && Input.anyKeyDown && Event.current.keyCode.ToString() =="Alpha"+(k+1)){
					current = nodes[current].next[i];
					return;	
				}
					
				if(next[i]==-1 || nodes[next[i]].active)
					k++;
			}
			
		}		
	}
	
	public void ToggleActive(){
		active = !active;
		
		GameObject player = GameObject.Find("FPSPlayer");
		player.GetComponent<vp_FPSController>().enabled = !player.GetComponent<vp_FPSController>().enabled;
		
		
		if(!active){
			current = 0;
			AutoSave();
		}
	}
	
	
	// Use this for initialization
	void Start () {
		npc = name;
		world_manager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
		
	}
	
	void LoadGame(){
		game_name = world_manager.game_name;
		ReadFile(false);
		
	}
	
	void NewGame(){
		
		game_name = world_manager.game_name;
		ReadFile(true);
		AutoSave();
		
	}
	
	
	// Update is called once per frame
	void Update () {
		//if keys are up
		if(active && Input.GetKeyDown(KeyCode.Escape))
			ToggleActive();

	}
}