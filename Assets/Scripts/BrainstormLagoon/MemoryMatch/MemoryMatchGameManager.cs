﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MemoryMatchGameManager : MonoBehaviour {

	private static MemoryMatchGameManager instance;
	private bool gameStart;
	private bool gameStartup;
	private int score;
	private GameObject currentFoodToMatch;
	public int difficultyLevel; //eventually make private
	private List<GameObject> activeFoods;

	public Transform foodToMatchSpawnPos;
	public Transform[] foodSpawnPos;
	public Transform[] foodParentPos;
	public float foodScale;
	public float foodToMatchScale;
	public List<GameObject> foods;
	public List<GameObject> dishes;
	public Timer timer;
	public float timeLimit;
	public Text timerText;
	public Text scoreText;
	public Canvas gameOverCanvas;

	// Use this for initialization
	void Awake () {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);
		}

		if(timer != null) {
			timer = Instantiate(timer);
			timer.SetTimeLimit(timeLimit);
		}
		activeFoods = new List<GameObject>();
	}

	// Update is called once per frame
	void Update () {
		if(gameStart){
			scoreText.text = "Score: " + score;
			timerText.text = "Time: " + timer.TimeRemaining();
			if(score >= difficultyLevel*3 || timer.TimeRemaining () < 0) {
				GameOver();
			}
		}
	}

	public static MemoryMatchGameManager GetInstance() {
		return instance;
	}

	public void StartGame() {
		gameStartup = true;
		gameStart = true;
		timer.StartTimer();

		SelectFoods();

		List<GameObject> copy = new List<GameObject>(activeFoods);

		ChooseFoodToMatch();

		for(int i = 0; i < difficultyLevel*3; ++i) {
			GameObject newFood = SpawnFood(copy, true, foodSpawnPos[i], foodParentPos[i], foodScale);
			dishes[i].GetComponent<DishBehavior>().SetFood(newFood.GetComponent<Food>());
		}
		gameStartup = false;
	}

	void SelectFoods() {
		int foodCount = 0;
		while(foodCount < difficultyLevel*3){
			int randomIndex = Random.Range(0, foods.Count);
			GameObject newFood = foods[randomIndex];
			if(!activeFoods.Contains(newFood)){
				activeFoods.Add(newFood);
				++foodCount;
			}

		}
	}

	public void ChooseFoodToMatch() {
		if(!gameStartup) {
			print("UPDATING SCORE");
			++score;
		}

		if(GameObject.Find ("ToMatchSpawnPos").transform.childCount > 0)
			Destroy(currentFoodToMatch);

		if(activeFoods.Count > 0) {
			currentFoodToMatch = SpawnFood(activeFoods, false, foodToMatchSpawnPos, foodToMatchSpawnPos, foodToMatchScale);
		}
	}

	GameObject SpawnFood(List<GameObject> foodsList, bool setAnchor, Transform spawnPos, Transform parent, float scale) {
		int randomIndex = Random.Range (0, foodsList.Count);
		GameObject newFood = Instantiate(foodsList[randomIndex]);
		newFood.GetComponent<Food>().Spawn(spawnPos, parent, scale);
		if(setAnchor) {
			newFood.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
			newFood.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

		}
		foodsList.RemoveAt(randomIndex);
		return newFood;
	}

	public Food GetFoodToMatch() {
		return currentFoodToMatch.GetComponent<Food>();
	}

	void GameOver() {
		timer.StopTimer();
		timerText.gameObject.SetActive(false);
		scoreText.gameObject.SetActive(false);
		gameOverCanvas.gameObject.SetActive(true);
		Text gameOverText = gameOverCanvas.GetComponentInChildren<Text>();
		gameOverText.text = "Great job! You matched " + score + " healthy foods!";
	}
}