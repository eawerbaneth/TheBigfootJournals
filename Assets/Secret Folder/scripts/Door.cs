using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {
	bool open = false;
	
	public float angle = 90.0f;
	public float speed = 145.0f;
	public AudioClip sound;
	public AudioClip sound2;
	float angleThreshold = 0.5f;
	
	float restAngle;
	
	void Start () {
		restAngle = transform.parent.rotation.eulerAngles.y;
	}
	
	void Update () {
	}
	
	IEnumerator OpenCloseCoroutine(float _angle) {
		//while (_angle
		//transform.Rotate(0.0f,
		while (Mathf.Abs(transform.parent.rotation.eulerAngles.y - (restAngle + _angle)) > angleThreshold) {
			transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation, Quaternion.Euler(new Vector3(0.0f, restAngle + _angle, 0.0f)), speed * Time.deltaTime);
			yield return null;
		}
		yield break;
	}
	
	void Open() {
		open = true;
		StartCoroutine(OpenCloseCoroutine(angle));
			GetComponent<AudioSource>().PlayOneShot(sound);
	}
		
	void Close() {
		open = false;
		StartCoroutine(OpenCloseCoroutine(0.0f));
			GetComponent<AudioSource>().PlayOneShot(sound2);
	}
	
	public void Toggle() {
		StopAllCoroutines();
		if (open) {
			Close();
		} else {
			Open();
		}
	}
}
