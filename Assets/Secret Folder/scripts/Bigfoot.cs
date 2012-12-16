using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Bigfoot : Persistent {
	
	public string DEBUGINFO = "";
	
	public AudioClip roar;
	
	public playerscript player;
	
	private DateTime lastUpdateTime = DateTime.Now;
	
	#region locations
	
	public Transform path0;
	public Transform path1;
	public Transform path2;
	public Transform path3;
	public Transform path4;
	public Transform path5;
	public Transform path6;
	public Transform path7;
	
	public Transform home;
	
	
	
	#endregion
	
	#region states
	
	private enum Mood {
		NONE,
		ANGRY,
		PANICKED,
		CURIOUS,
		ALARMED,
		GROGGY,
	}
	
	private enum Goal {
		NONE,
		INSPECT,
		SLEEP,
		EAT,
		ESCAPE,
	}
	
	Mood mood = Mood.NONE;
	
	Goal goal = Goal.NONE;
	
	System.Collections.Generic.SortedList<DateTime, Transform> schedule = new System.Collections.Generic.SortedList<DateTime, Transform>();
	
	private const float walkSpeed = 1.5f;
	private const float runSpeed = 5.0f;
	
	// Assume that when following a path, speed will only be this much of optimal travel
	private const float pathEfficiency = 0.9f;
	
	#endregion
	
	#region mood
	
	//private const 
	
	#endregion
	
	#region interaction
	private readonly TimeSpan unknownAttentionSpan = TimeSpan.FromSeconds(5.0f);
	private readonly TimeSpan threatAttentionSpan = TimeSpan.FromSeconds(30.0f);
	private readonly TimeSpan curiousityAttentionSpan = TimeSpan.FromSeconds(10.0f);
	
	private Vector3 unknownLastPosition = Vector3.zero;
	private Vector3 threatLastPosition = Vector3.zero;
	private Vector3 curiosityLastPosition = Vector3.zero;
	
	private DateTime unknownTime = new DateTime(0);
	private DateTime threatTime = new DateTime(0);
	private DateTime curiosityTime = new DateTime(0);
	
	#endregion
	
	#region stats
	private const float foodMax = 100.0f;
	private const float foodRate = -0.01f;
	private const float foodSleepRate = -0.005f;
	
	private const float eatDistance = 2.0f;
	
	private readonly TimeSpan sleepAvgTime = TimeSpan.FromHours(8.0f);
	
	private const float energyMax = 100.0f;
	// Fix this stuff
	private const float energyRate = -100.0f / (16.0f * 3600.0f);
	private const float energySleepRate = 100.0f / (8.0f * 3600.0f);
	
	private const float fearMax = 100.0f;
	private const float angerMax = 100.0f;
	
	private float food = foodMax;
	private float energy = energyMax;
	private float fear = 0.0f;
	private float anger = 0.0f;
	
	private bool sleeping = false;
	
	private TimeSpan groggySpan = TimeSpan.FromSeconds(120.0f);
	private DateTime wakeTime = new DateTime(0);
	
	#endregion
	
	#region vision
	private float fieldOfViewHorz = 90.0f;
	private float fieldOfViewVert = 45.0f;
	
	private float viewDistMin = 30.0f;
	private float viewDistMax = 50.0f;
	#endregion
	
	#region smell
	private float smellDist = 5.0f;
	#endregion
	
	#region hearing
	// A sound of moderate volume can be heard this far away
	private float hearingDistance = 100.0f;
	// When hearing a sound from an unseen source, choose a random position up to this far away
	private float hearingPrecision = 10.0f;
	//private float hearingVolume = 
	private readonly HashSet<string> unknownSounds = new HashSet<string>{"PlayerFootstep"};
	private readonly HashSet<string> threatSounds = new HashSet<string>{"Gunshot"};
	private readonly string callSound = "CallBlasterSound";
	#endregion
	
	
	#region head
	public Transform head;
	
	private float headHorzTurnMax = 160.0f;
	private float headVertTurnMax = 50.0f;
	
	private float headTurnThreshold = 1.0f;
	
	private float headRotationSpeed = 70.0f;
	private float bodyRotationSpeed = 40.0f;
	
	#endregion
	
	private float euler360ToEuler180(float euler360) {
		return euler360 >= 180.0f ? 360.0f - euler360 : euler360;
	}
	
	private float euler180ToEuler360(float euler180) {
		return euler180 < 0.0f ? 360.0f + euler180 : euler180;
	}
	
	private float getViewDist(Vector3  eulerDirection) {
		// Determines how far away Bigfoot can see in a certain direction based on his facing
		// Objects in the center of vision are easier to see than objects in peripheral vision
		Vector3 eulerDifference = eulerDirection - head.rotation.eulerAngles;
		eulerDifference.Set(eulerDifference.x % 360.0f, eulerDifference.y % 360.0f, eulerDifference.z % 360.0f);
		
		if (Mathf.Abs(eulerDifference.x) > fieldOfViewHorz / 2.0f) {
			return 0.0f;
		}
		if (Mathf.Abs(eulerDifference.y) > fieldOfViewVert / 2.0f) {
			return 0.0f;
		}
		float dist = Mathf.Sqrt(eulerDifference.x * eulerDifference.x + eulerDifference.y * eulerDifference.y) / 2.0f;
		float maxDist = Mathf.Sqrt(fieldOfViewHorz * fieldOfViewHorz + fieldOfViewVert * fieldOfViewVert) / 2.0f;
		return viewDistMin + ((maxDist - dist) / maxDist) * (viewDistMax - viewDistMin);
	}
	
	private bool IsOccluded(GameObject obj) {
		Vector3[] samplePoints = {
			obj.collider.bounds.center,
			obj.collider.bounds.center + new Vector3(0.0f, obj.collider.bounds.extents.y, 0.0f),
			obj.collider.bounds.center + new Vector3(0.0f, -obj.collider.bounds.extents.y, 0.0f),
		};
		
		RaycastHit hit;
		foreach (Vector3 v in samplePoints) {
			if (Physics.Raycast(head.transform.position, v - head.transform.position, out hit)) {
				if (hit.collider.gameObject == obj) {
					return false;
				}
			}
		}
		return true;
	}
	
	private bool CanSee(GameObject obj) {
		float sqrDist = (obj.transform.position - transform.position).sqrMagnitude;
		if (sqrDist < viewDistMax * viewDistMax) {
			float viewDist = getViewDist(Quaternion.LookRotation(obj.transform.position - transform.position).eulerAngles);
			if (sqrDist < viewDist * viewDist) {
				if (!IsOccluded(obj)) {
					return true;
				}
			}
		}
		return false;
	}
	
	private bool CanSmell(GameObject go) {
		return Vector3.Distance(player.transform.position, transform.position) < smellDist;
	}
	
	private bool IsSafe() {
		return DateTime.Now - threatTime > threatAttentionSpan;
	}
	
	public bool IsSleeping() {
		return sleeping;
	}
	
	private bool CanSleep() {
		return IsSafe();
	}
	
	private LinkedList<Food> VisibleFood() {
		Food[] allfoods = FindObjectsOfType(typeof(Food)) as Food[];
		LinkedList<Food> foods = new LinkedList<Food>();
		foreach (Food f in allfoods) {
			if (f.Spawned() && CanSee(f.gameObject)) {
				foods.AddLast(f);
			}
		}
		return foods;
	}
	
	public void HearSound(GameObject source, AudioClip ac, float soundIntensity) {
		if (Vector3.Distance(transform.position, source.transform.position) > soundIntensity * hearingDistance) {
			// Sound is too quiet/far away
			return;
		}
		
		Vector3 pos = source.transform.position;
		if (!CanSee(source)) {
			// Generate evenly distributed random point on sphere
			
			float u;
			float v;
			
			do {
				u = UnityEngine.Random.value;
				v = UnityEngine.Random.value;
			} while(u * u + v * v >= 1.0f);
			
			pos += hearingPrecision * new Vector3(
				2.0f * u * Mathf.Sqrt(1 - u * u - v * v),
				2.0f * v * Mathf.Sqrt(1 - u * u - v * v),
				1.0f - 2.0f * (u * u + v * v));
		}
		LookAt(pos);
		if (threatSounds.Contains(ac.name)) {
			Startle();
			threatLastPosition = pos;
			threatTime = DateTime.Now;
		} else if (unknownSounds.Contains(ac.name)) {
			unknownLastPosition = pos;
			unknownTime = DateTime.Now;
		} else if (ac.name == callSound) {
			// call back
		}
	}
	
	private bool LookToward(Vector3 pos) {
		Vector3 bodyTarget = pos - head.position;
		bodyTarget.y = head.position.y;
		
		// Rotate body slightly
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(bodyTarget), bodyRotationSpeed * Time.deltaTime);
		head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.LookRotation(pos - head.position), headRotationSpeed * Time.deltaTime);
		// Clamp head rotation to reasonable angles to prevent Exorcist-like situations
		Vector3 localEuler = head.localEulerAngles;
		head.localEulerAngles = new Vector3(
			euler180ToEuler360(Mathf.Clamp(euler360ToEuler180(localEuler.x), -headVertTurnMax/2.0f, headVertTurnMax/2.0f)),
			euler180ToEuler360(Mathf.Clamp(euler360ToEuler180(localEuler.y), -headHorzTurnMax/2.0f, headHorzTurnMax/2.0f)),
			localEuler.z
			);
		return Quaternion.Angle(head.rotation, Quaternion.LookRotation(pos - head.position)) < headTurnThreshold;
	}
	
	private void UpdateInteractions() {
		
	}
	
	private void UpdateMood() {
		if (mood == Mood.GROGGY && DateTime.Now - wakeTime > groggySpan) {
			mood = Mood.NONE;
		}
	}
	
	private void UpdateStats(TimeSpan delta) {
		if (sleeping) {
			energy = Mathf.Clamp(energy + (float)delta.TotalSeconds * energySleepRate, 0.0f, energyMax);
			food = Mathf.Clamp(food + (float)delta.TotalSeconds * foodSleepRate, 0.0f, foodMax);
		} else {
			energy = Mathf.Clamp(energy + (float)delta.TotalSeconds * energyRate, 0.0f, energyMax);
			food = Mathf.Clamp(food + (float)delta.TotalSeconds * foodRate, 0.0f, foodMax);
		}
	}
	
	private void Sleep() {
		if (!IsSleeping()) {
			if (CanSleep()) {
				sleeping = true;
				
			}
		}
	}
	
	private void Wake() {
		if (IsSleeping()) {
			sleeping = false;
			wakeTime = DateTime.Now;
			mood = Mood.GROGGY;
		}
	}
	
	private void Startle() {
		Wake();
		mood = Mood.ALARMED;
	}
	
	private void Move() {
		if (IsSleeping()) {
			return;
		}
		
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		
		if (DateTime.Now - threatTime < threatAttentionSpan) {
			DEBUGINFO = "THREATENED";
			agent.speed = runSpeed;
			if (mood == Mood.ANGRY) {
				// attack threat
				agent.destination = threatLastPosition;
			} else {
				// escape threat
				agent.destination = transform.position - (threatLastPosition * 20.0f);
			}
		} else {
			agent.speed = walkSpeed;
			if (food < 10.0f) {
				// search for food (critical)
				LinkedList<Food> visibleFood = VisibleFood();
				if (visibleFood.Count == 0) {
					ForceWander();
				} else {
					Food closest = visibleFood.First.Value;
					foreach (Food f in visibleFood) {
						if (Vector3.Distance(f.transform.position, transform.position) < Vector3.Distance(closest.transform.position, transform.position)) {
							closest = f;
						}
					}
					agent.destination = closest.transform.position;
					if (Vector3.Distance(closest.transform.position, transform.position) < eatDistance) {
						food += closest.Eat();
					}
				}
			} else if (DateTime.Now - curiosityTime < curiousityAttentionSpan) {
				// investigate curiousity
				agent.destination = curiosityLastPosition;
			} else if (energy < 5.0f) {
				// search for safe place to sleep
				Sleep();
			} else if (energy < 10.0f) {
				// go back to home cave (?)
			} else if (FollowSchedule()) {
					// does scheduled destination
			} else if (food < 40.0f) {
				// search for food (non-critical)
				LinkedList<Food> visibleFood = VisibleFood();
				if (visibleFood.Count == 0) {
					ForceWander();
				} else {
					Food closest = visibleFood.First.Value;
					foreach (Food f in visibleFood) {
						if (Vector3.Distance(f.transform.position, transform.position) < Vector3.Distance(closest.transform.position, transform.position)) {
							closest = f;
						}
					}
					agent.destination = closest.transform.position;
					if (Vector3.Distance(closest.transform.position, transform.position) < eatDistance) {
						food += closest.Eat();
					}
				}
			} else {
				Wander();
			}
		}
	}
	
	bool FollowSchedule() {
		DEBUGINFO = "";
		foreach (KeyValuePair<DateTime, Transform> dtt in schedule) {
			if (ScheduledDestination(dtt.Key, dtt.Value.position)) {
				DEBUGINFO = dtt.Value.name;
				return true;
			}
		}
		return false;
	}
	
	private bool ScheduledDestination(DateTime time, Vector3 position) {
		if (!IsSafe() || DateTime.Now.DayOfWeek != time.DayOfWeek) {
			return false;
		}
		
		// Will not work perfectly for events shortly after midnight
		TimeSpan now = DateTime.Now.TimeOfDay;
		TimeSpan then = time.TimeOfDay;
		
		if (now > then) {
			return false;
		}
		
		NavMeshPath path = new NavMeshPath();
		if (NavMesh.CalculatePath(transform.position, position, GetComponent<NavMeshAgent>().walkableMask, path)) {
			float dist = 0.0f;
			
			Vector3 prev = transform.position;
			foreach (Vector3 next in path.corners) {
				dist += Vector3.Distance(next, prev);
				prev = next;
			}
			
			if (dist / GetComponent<NavMeshAgent>().speed > (then - now).TotalSeconds * pathEfficiency) {
				GetComponent<NavMeshAgent>().destination = position;
				return true;
			}
		}
		return false;
	}
	
	private void UpdateRealTimeEvents() {
		DateTime now = DateTime.Now;
		TimeSpan delta = now - lastUpdateTime;
		lastUpdateTime = now;
		
		float sliceTime = 5.0f;
		
		TimeSpan slice;
		do {
			slice = TimeSpan.FromSeconds(Mathf.Min((float)delta.TotalSeconds, sliceTime));
			delta -= slice;
			UpdateStats(slice);
		} while	(slice.TotalSeconds >= 0.001f);
	}
	
	private void DoDebug() {
		Debug.DrawRay(head.transform.position, viewDistMax * head.transform.forward, Color.magenta);
		if ((CanSmell(player.gameObject) || CanSee(player.gameObject)) && (!player.equipped || !(player.equipped is Bait))) {
			//LookAt(player.transform.position);
			//GetComponent<NavMeshAgent>().destination = player.transform.position;
			threatLastPosition = player.transform.position;
			threatTime = DateTime.Now;
		}
			
	}
	
	private void ForceWander() {
		if (Vector3.Distance(transform.position, GetComponent<NavMeshAgent>().destination) < 1.0f) {
			DoWander();
		}
	}
	
	private void DoWander() {
		float wanderRadius = 1000.0f;
		
		Vector2 offset = UnityEngine.Random.insideUnitCircle * wanderRadius;
		//Debug.Log (offset);
		Vector3 target = transform.position + new Vector3(offset.x, 0.0f, offset.y);
		LookAt(target);
		GetComponent<NavMeshAgent>().destination = target;
	}
	
	private void Wander() {
		// HARDCODE
		float avgTime = 20.0f;
		if (UnityEngine.Random.value < Time.deltaTime / avgTime) {
			//Debug.Log("Wandering");
			DoWander();
		} else {
			ForceWander();
		}
	}
	
	private void LookAhead() {
		LookAt(head.transform.position + head.transform.forward);	
	}
	
	private void LookAt(Vector3 pos) {
		StopCoroutine("LookAtCoroutine");
		StartCoroutine("LookAtCoroutine", pos);
	}
	
	private IEnumerator LookAtCoroutine(Vector3 pos) {
		while (!LookToward(pos)) {
			yield return null;
		}
	}
	
	void ScheduleDestination(DateTime dt, Transform t) {
		//Debug.Log(dt);
		schedule.Add(dt, t);
	}
	
	void Start() {
		//LookAt(player.transform.position);
		GetComponent<NavMeshAgent>().Stop();
		
		for (int dow = 1; dow <= 7; ++dow) {
			for (int hr = 11; hr <= 20; ++hr) {
				int interval = 5;
				int min = 10;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path0); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path1); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path2); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path3); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path4); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path5); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path6); min += interval;
				ScheduleDestination(new DateTime(1, 4, dow, hr, min, 0), path7); min += interval;
			}
			ScheduleDestination(new DateTime(1, 4, dow, 22, 0, 0), path0);
		}
	}
	
	void Update() {
		
		
		UpdateRealTimeEvents();
		UpdateInteractions();
		DoDebug();
		Move();
		//GameObject obj = GameObject.Find("Cube");
		//Debug.Log((new DateTime(1, 4, 1, 17, 12, 0)).DayOfWeek);
		
		
	}
	
	public override void OnSave() {
		SaveTransform("Bigfoot");
	}
	
	public override void OnLoad() {
		LoadTransform("Bigfoot");
	}
	
	//callblast - makes bigfoot roar
	public IEnumerator Roar(){
		yield return new WaitForSeconds(3.5f);
		GetComponent<AudioSource>().PlayOneShot(roar);
		
		
	}
}
