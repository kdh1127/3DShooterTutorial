using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

	public Wave[] waves;
	public Enemy enemy;

	LivingEntity playerEntity;
	Transform playerT;

	Wave currentWave;
	int currentWaveNumber;

	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;

	MapGenerator map;

	float timeBetweenCampingCheckTime = 2;
	float campThresholdDistance = 1.5f;
	float nextCampCheckTime;
	Vector3 campPositionOld;
	bool isCamping;

	bool isDisabled;

	void Start()
	{
		playerEntity = FindObjectOfType<Player>();
		playerT = playerEntity.transform;

		nextCampCheckTime = timeBetweenCampingCheckTime + Time.time;
		campPositionOld = playerT.position;
		playerEntity.OnDeath += OnPlayerDeath;

		map = FindObjectOfType<MapGenerator>();
		NextWave();
	}

	void Update()
	{
		if (!isDisabled)
		{
			if (Time.time > nextCampCheckTime)
			{
				nextCampCheckTime = Time.time + timeBetweenCampingCheckTime;

				isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
				campPositionOld = playerT.position;
			}
			if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
			{
				enemiesRemainingToSpawn--;
				nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

				StartCoroutine(SpawnEnemy());
			}
		}
	}

	IEnumerator SpawnEnemy()
    {
		float spawnDelay = 1;
		float tileFlashspeed = 4; 

		Transform spawnTile = map.GetRandomOpenTile();
		if (isCamping)
        {
			spawnTile = map.GetTileFromPosition(playerT.position);
        }
		Material tileMat = spawnTile.GetComponent<Renderer>().material;
		Color initialColour = tileMat.color;
		Color flashColour = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay)
        {
			tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashspeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
        }


		Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;
	}

	void OnPlayerDeath()
    {
		isDisabled = true;
    }

	void OnEnemyDeath()
	{
		enemiesRemainingAlive--;

		if (enemiesRemainingAlive == 0)
		{
			NextWave();
		}
	}

	void NextWave()
	{
		currentWaveNumber++;
		print("Wave: " + currentWaveNumber);
		if (currentWaveNumber - 1 < waves.Length)
		{
			currentWave = waves[currentWaveNumber - 1];

			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;
		}
	}

	[System.Serializable]
	public class Wave
	{
		public int enemyCount;
		public float timeBetweenSpawns;
	}

}
