using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Photo {
	
	public static int width = 640;
	public static int height = 480;
	
	public Texture2D texture {
		get;
		private set;
	}
	
	//private bool hasBigfoot;
	
	string ptag;
	
	public Photo(string t) {
		texture = new Texture2D(width, height, TextureFormat.RGB24, false);
		texture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
		//texture.Apply(false, true);
		// Can't set texture as no longer editable because it needs to be accessed to save later...
		texture.Apply(false);
		ptag = t;
	}
	
	public bool HasMayor() {
		return ptag == "mayor";
	}
	
	public bool HasBigfoot() {
		return ptag == "bigfoot";
	}
	
	public Photo(Texture2D t) {
		texture = t;
	}
}
