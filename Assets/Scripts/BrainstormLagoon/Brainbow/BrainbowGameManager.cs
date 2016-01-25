﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BrainbowGameManager : AbstractGameManager {

	private static BrainbowGameManager instance;
	private int score;
	private BrainbowFood activeFood;
	private bool gameStarted;
	private int difficultyLevel;
	private Dictionary<int, int> scoreGoals;
	private bool animIsPlaying = false;
	private bool runningTutorial = false;
	private Text nowYouTryText;
	private BrainbowFood banana;
	private Transform bananaOrigin;

	public Canvas instructionPopup;
	public Canvas gameoverCanvas;
	public Canvas stickerPopupCanvas;
	public List<GameObject> foods;
	public List<GameObject> inGameFoods = new List<GameObject>();
	public Transform[] spawnPoints;
	public Transform spawnParent;
	public int foodScale;
	public Text scoreText;
	public Text timerText;
	public float timeLimit;
	public Timer timer;
	public AudioClip backgroundMusic;
	public AudioClip correctSound;
	public AudioClip incorrectSound;
	public GameObject endGameAnimation;
	public AudioClip munchSound;
	public GameObject subtitlePanel;
	public AudioClip[] correctMatchClips;
	public AudioClip[] wrongMatchClips;
	public Transform tutorialOrigin;
	
	void Awake() {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);
		}
		difficultyLevel = GameManager.GetInstance().GetLevel("Brainbow");

		scoreGoals = new Dictionary<int, int>()
		{
			{1, 8},
			{2, 12},
			{3, 20}
		};
	}

	public static BrainbowGameManager GetInstance() {
		return instance;
	}

	void Start(){
		score = 0;

		if(timer != null) {
			timer = Instantiate(timer);
			timer.SetTimeLimit(this.timeLimit);
		}
		UpdateScoreText();
		SoundManager.GetInstance().ChangeBackgroundMusic(backgroundMusic);
	
		//create enums for each part of the island that represents the games to avoid using numbers to access the arrays
		//in GameManager. Ex: brainstormLagoonTutorial[BrainstormLagoon.BRAINBOW]
		if(GameManager.GetInstance().brainstormLagoonTutorial[0]) {
			StartCoroutine(RunTutorial());
		}
		else {
			StartGame ();
		}

	}

	void Update() {

		if(runningTutorial && score == 1) {
			StartCoroutine(TutorialTearDown());
		}

		if(gameStarted) {
			if(score == 20 || timer.TimeRemaining() <= 0.0f) {
				// Animation.
				if(!animIsPlaying) {
					EndGameTearDown();
				}
			}
		}
	}

	void FixedUpdate() {
		if(gameStarted) {
			timerText.text = "Time: " + timer.TimeRemaining();
		}
	}

	IEnumerator RunTutorial() {
		runningTutorial = true;
		instructionPopup.gameObject.SetActive(true);
		yield return new WaitForSeconds(5.0f);

		subtitlePanel.GetComponent<SubtitlePanel>().Display("Now You Try!", correctSound);

		BrainbowFood[] foods = instructionPopup.GetComponentsInChildren<BrainbowFood>();
		foreach(BrainbowFood f in foods) {
			if(f.name == "Banana") {
				f.GetComponent<PolygonCollider2D>().enabled = true;
				banana = f;
				bananaOrigin = tutorialOrigin;
				banana.SetOrigin(bananaOrigin);
			}
		}
	}

	IEnumerator TutorialTearDown ()
	{
		score = 0;
		runningTutorial = false;
		subtitlePanel.GetComponent<SubtitlePanel>().Display("Great Job! Let's play Brainbow!", correctSound);
		yield return new WaitForSeconds(2.0f);
		instructionPopup.gameObject.SetActive(false);
		StartGame ();
	}

	public bool IsRunningTutorial() {
		return runningTutorial;
	}

	public void StartGame() {
		scoreText.gameObject.SetActive(true);
		timerText.gameObject.SetActive(true);

		StartCoroutine (DisplayGo());

	}

	public IEnumerator DisplayGo () {
		StartCoroutine(gameObject.GetComponent<Countdown>().RunCountdown());
		yield return new WaitForSeconds (4.0f);
		PostCountdownSetup ();
	}

	void PostCountdownSetup ()
	{
		gameStarted = true;
		for (int i = 0; i < 4; ++i) {
			SpawnFood (spawnPoints [i]);
		}
		timer.StartTimer ();
	}

	public void Replace(GameObject toReplace) {
		++score;
		if(!runningTutorial) {
			scoreText.text = "Score: " + score;
			if(toReplace.GetComponent<Food>() != null && foods.Count > 0) {
				SpawnFood(toReplace.GetComponent<BrainbowFood>().GetOrigin());
			}
		}
	}

	void SpawnFood(Transform spawnPos) {
		int randomIndex = Random.Range (0, foods.Count);
		GameObject newFood = Instantiate(foods[randomIndex]);
		newFood.name = foods[randomIndex].name;
		newFood.GetComponent<BrainbowFood>().SetOrigin(spawnPos);
		newFood.GetComponent<BrainbowFood>().Spawn(spawnPos, spawnParent, foodScale);
		SetActiveFood(newFood.GetComponent<BrainbowFood>());
		inGameFoods.Add (newFood);
		foods.RemoveAt(randomIndex);
	}

	public void SetActiveFood(BrainbowFood food) {
		activeFood = food;
	}

	override public void GameOver() {
		if(score >= scoreGoals[difficultyLevel]) {
			if(difficultyLevel == 1) {
				stickerPopupCanvas.gameObject.SetActive(true);
				GameManager.GetInstance().ActivateSticker("BrainstormLagoon", "Brainbow");
				GameManager.GetInstance ().brainstormLagoonTutorial[0] = false;
			}
			GameManager.GetInstance().LevelUp("Brainbow");
		}

		if(difficultyLevel > 1) {
			DisplayGameOverCanvas ();
		}
	}

	void EndGameTearDown ()
	{
		subtitlePanel.GetComponent<SubtitlePanel>().Hide ();
		gameStarted = false;
		timer.StopTimer();

		if (activeFood != null) {
			activeFood.StopMoving ();
		}

		timerText.gameObject.SetActive (false);

		StartCoroutine(RunEndGameAnimation());
	}

	IEnumerator RunEndGameAnimation() {
		animIsPlaying = true;

		foreach (GameObject food in inGameFoods) {
			food.GetComponent<Collider2D>().enabled = true;
		}

		GameObject animation = (GameObject)Instantiate(endGameAnimation);
		animation.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(GameManager.GetInstance().getMonster());
		yield return new WaitForSeconds (endGameAnimation.gameObject.GetComponent<Animator> ().runtimeAnimatorController.animationClips [0].length);
		GameOver ();
	}

	public void DisplayGameOverCanvas () {
		gameoverCanvas.gameObject.SetActive (true);
		Text gameoverScore = gameoverCanvas.GetComponentInChildren<Text> ();
		gameoverScore.text = "Good job! You fed your monster: " + score + " healthy foods!";
	}

	void UpdateScoreText() {
		scoreText.text = "Score: " + score;
	}
}