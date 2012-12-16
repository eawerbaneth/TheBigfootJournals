using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour {
	public GameObject [] autosave_objects;
	public GameObject [] load_objects;
	public string game_name = "";
	
	public GameObject player;
	
	
	public class ScheduledEvent{
		string day;
		DateTime ToD;
		string[] args;
		bool active;
		
		
		public ScheduledEvent(){}	
		public ScheduledEvent(string d, DateTime dt, string[] a){
			ToD = dt;
			day = d;
			args = a;
			active = true;
		}
		
		IEnumerator Deactivate(){
			active = false;
			yield return new WaitForSeconds(5);
			active = true;
			
		}
		
		void ExecuteEvent(){
			int type = int.Parse(args[0]);
				
			//GOTO: args format: Character, Destination 
			if(type == 0){
				//find character
				GameObject c = GameObject.Find(args[1]);
				if(c == null){
					Debug.Log("Character " + args[1] + " not found...");
					return;
				}
				//find destination node
				GameObject d = GameObject.Find("AI_dest-"+args[2]);
				if(d == null){
					Debug.Log("Destination " + args[2] + " not found...");
					return;
				}
				
				//send message for character c to go to destination d
				c.GetComponent<NavMeshAgent>().destination = d.transform.position;
				
			}
			//OPEN TOPICS: args format: Character, conv_start-conv_end
			else if(type == 1){
				//find character
				GameObject c = GameObject.Find(args[1]);
				if(c == null){
					Debug.Log("Character " + args[1] + " not found...");
					return;
				}
				Debug.Log("WorldManager:a " + args[2]);
				string[] n = args[2].Split('-');
				c.GetComponent<ConversationManager>().ActivateNodes(int.Parse(n[0]) - 1, int.Parse(n[1]) - 1);
				c.GetComponent<ConversationManager>().AutoSave();
			}
			//CLOSE_TOPICS: args format: Character, conv_start-conv_end
			else if(type == 2){
				GameObject c = GameObject.Find(args[1]);
				if(c == null){
					Debug.Log("Character " + args[1] + " not found...");
					return;
				}
				string[] n = args[2].Split('-');
				Debug.Log("WorldManager: d " + args[2]);
				c.GetComponent<ConversationManager>().DeactivateNodes(int.Parse(n[0]) -1, int.Parse(n[1]) - 1);
				c.GetComponent<ConversationManager>().AutoSave();
			}
			//MAKE ITEM AVAILABLE FOR PICKUP
			// [1] item name
			else if(type == 3) {
				GameObject i = GameObject.Find(args[1]);
				if (!i) {
					Debug.Log ("ItemPickup " + args[1] + " not found...");
					return;
				}
				i.GetComponent<ItemPickup>().AllowPickup();
				PlayerPrefs.SetInt(i.GetComponent<ItemPickup>().itemSaveName, 2);
			}
			//GAME OVER - TODO - insert a better
			else if(type == 4){
				if(args[1] == "kill_bigfoot")
					Application.LoadLevel("BigfootKiller");
				if(args[1] == "kill_mansfield")
					Application.LoadLevel("HunterKiller");
				if(args[1] == "picture")
					Application.LoadLevel("picture_ending");
				if(args[1] == "dead")
					Application.LoadLevel("dead_ending");
				
			}
			//ADD MAP MARKER: args format: 5+description+x y
			else if(type == 5){
				GameObject player = GameObject.Find("FPSPlayer");
				string[] coords = args[2].Split(' ');
				player.GetComponent<map>().AddNote(float.Parse(coords[0]), float.Parse(coords[1]), args[1]);
			}
			//BOOL - MISCELLANOUS
			else if(type == 6){
				GameObject player = GameObject.Find("FPSPlayer");
				if(args[1] == "Yang")
					player.GetComponentInChildren<ItemCamera>().mayor_ready = true;
				if(args[1] == "mansfieldquest"){
					QuestJournal journal = GameObject.Find("mansfieldquest").GetComponent<QuestJournal>();
					journal.quest_primed = true;
					journal.quest_character = args[2];
					journal.quest_index = int.Parse(args[3]);
					PlayerPrefs.SetInt(journal.game_name + journal.name, 1);
				}
				if(args[1] == "georgejournal"){
					QuestJournal journal = GameObject.Find("georgejournal").GetComponent<QuestJournal>();
					journal.quest_primed = true;
					journal.quest_character = args[2];
					journal.quest_index = int.Parse(args[3]);
					PlayerPrefs.SetInt(journal.game_name + journal.name, 1);
				}
				if(args[1] == "mansfieldquestdone")
					PlayerPrefs.SetInt(GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name + "mansfieldquest", 3);	
				if(args[1] == "georgejournaldone")
					PlayerPrefs.SetInt(GameObject.Find("WorldManager").GetComponent<WorldManager>().game_name + "georgejournal", 3);	
				
			}
			
			
			Deactivate();
			
		}
		
		//remove from events if returned false
		public bool Update(){
			if(!active)
				return true;
			
			//execute the event
			//if(DateTime.Now.DayOfWeek == day && DateTime.Now.TimeOfDay == ToD.TimeOfDay){
			if(day == "Once"){
				ExecuteEvent();				
				return false;
			}
			else if(day == "RepeatSecond" && DateTime.Now.Second == ToD.Second)
				ExecuteEvent();
			else if(day == "RepeatMinute" && DateTime.Now.Second == 0 && DateTime.Now.Minute == ToD.Minute)
				ExecuteEvent();
			else if(day == "RepeatHour" && DateTime.Now.TimeOfDay == ToD.TimeOfDay)
				ExecuteEvent();
			else if(DateTime.Now.TimeOfDay == ToD.TimeOfDay  && GetDay(DateTime.Now.DayOfWeek))
				ExecuteEvent();
			
			return true;
			
		}
		
		bool GetDay(DayOfWeek d){
			if(day == "Sunday" && d == DayOfWeek.Sunday)
				return true;
			else if(day=="Monday" && d == DayOfWeek.Monday)
				return true;
			else if(day=="Tuesday" && d == DayOfWeek.Tuesday)
				return true;
			else if(day=="Wednesday" && d == DayOfWeek.Wednesday)
				return true;
			else if(day=="Thursday" && d == DayOfWeek.Thursday)
				return true;
			else if(day=="Friday" && d == DayOfWeek.Friday)
				return true;
			else if(day=="Saturday" && d == DayOfWeek.Saturday)
				return true;
			return false;
			
		}
	};
	
	List <ScheduledEvent> events = new List<ScheduledEvent>();
	
	//autosave every minute
	void AutoSave(){
		if(DateTime.Now.Second%29 == 0){
			Debug.Log("Autosave");
			Persistent[] objs = FindSceneObjectsOfType(typeof(Persistent)) as Persistent[];
			foreach (Persistent o in objs) {
				o.OnSave();
			}
			
			foreach(GameObject obj in autosave_objects)
				obj.SendMessage("AutoSave");
		}
	}
	
	public void BeginGame(bool new_game, string g_name){
		game_name = g_name;
		player.SetActiveRecursively(true);
		//player.active = true;
		
		
		foreach(GameObject obj in load_objects){
			if(new_game)
				obj.SendMessage("NewGame");
			else {
				obj.SendMessage("LoadGame");
				
				Persistent[] objs = FindSceneObjectsOfType(typeof(Persistent)) as Persistent[];
				foreach (Persistent o in objs) {
					o.OnLoad();
				}
			}
				
		}
		
		
	}
	
	void ReadFile(){
		//create writer and open the file
		TextReader tr = new StreamReader("Assets\\Secret Folder\\events.txt");
		
		string line = tr.ReadLine();;
		//read a line of text to the file
		
		while(line != null){
			if(line[0]=='#'){//this line is a comment, skip it
				line = tr.ReadLine();
				continue;
			}
			
			//split the string into its distinct parts
			string[] parts = line.Split('#');
			
			
			string[] time = parts[1].Split(':');		
			DateTime tod = new DateTime(1990, 1, 1, int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
			
			events.Add(new ScheduledEvent(parts[0], tod, parts[2].Split('+')));
			//DateTime ndt = new DateTime(
			                           
			
			line = tr.ReadLine();
			
		}
		
		tr.Close();
		
	}
	
	//format: mm/dd/yyyy h:mm:ss XM
	DateTime dt;
	
	// Use this for initialization
	void Start () {
		
		dt = DateTime.Now;
		ReadFile();
	
	}
	
	
	
	// Update is called once per frame
	void Update () {
		dt = DateTime.Now;
		for(int i = 0; i < events.Count; i++){
			if(!events[i].Update()){
				events.RemoveAt(i);
				i--;
			}
		}
		
		//AutoSave();
		
	
	}
	
	//add an event that will be executed one time IMMEDIATELYYYYYYYY
	public void AddEvent(string [] args){
		/*
		 * events.Add(new ScheduledEvent(dow, tod, parts[2].Split('+')));
		 * */
		events.Add(new ScheduledEvent("Once", DateTime.Now, args));		
	}
}
