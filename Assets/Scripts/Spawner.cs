using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;
    [SerializeField] Transform player;
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 5;

    [SerializeField] Vector2 area = new Vector2(7.5f, 9.5f);
    [SerializeField] float spawnHeight = 0.75f;

    void Start() => InvokeRepeating(nameof(SpawnCoin), 1f, interval);

    Vector3 RandomPointInRoom()
    {
        float x = Random.Range(-area.x, area.x);
        float z = Random.Range(-area.y, area.y);
        return transform.position + new Vector3(x, spawnHeight, z);
    }

    public void SpawnCoin()
    {
        if (coinPrefab == null || transform.childCount >= maxCoins) return;

        Vector3 pos = RandomPointInRoom();
        if (player != null && Vector3.Distance(pos, player.position) < 2f) return;

        Instantiate(coinPrefab, pos, Quaternion.identity, transform);
    }
}