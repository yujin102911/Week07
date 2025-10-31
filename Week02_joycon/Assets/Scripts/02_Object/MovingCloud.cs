using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [SerializeField] GameObject[] cloudPrefab;
    [SerializeField] float minSpawnInterval = 1.0f;
    [SerializeField] float maxSpawnInterval = 5.0f;
    float spawnInterval = 3f;

    [SerializeField] float spawnYMin = 2f;
    [SerializeField] float spawnYMax = 5f;

    private List<GameObject> activeClouds = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnCloudRoutine());
    }

    void Update()
    {
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            if (activeClouds[i] == null) activeClouds.RemoveAt(i);
        }
    }

    IEnumerator SpawnCloudRoutine()
    {
        while (true)
        {
            float spawnY = transform.position.y + Random.Range(spawnYMin, spawnYMax);
            Vector3 spawnPosition = new Vector3(transform.position.x, spawnY, 0);
            int randomIndex = Random.Range(0, cloudPrefab.Length);
            GameObject newCloud = Instantiate(cloudPrefab[randomIndex], spawnPosition, Quaternion.identity);
            activeClouds.Add(newCloud);
            spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}