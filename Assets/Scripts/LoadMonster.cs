﻿using UnityEngine;
using System.Collections;

public class LoadMonster : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Sprite monsterSprite = Resources.Load<Sprite>(GameManager.GetInstance().getMonster());
		gameObject.GetComponent<SpriteRenderer>().sprite = monsterSprite;
	}

}
