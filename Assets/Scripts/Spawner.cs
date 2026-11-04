using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject[] powerUpPrefabs;     // e.g. a Speed prefab and a Shield prefab
    [SerializeField] Transform player;
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 5;
    [SerializeField] int coinsPerPowerUp = 3;
    [SerializeField] float powerUpLifetime = 10f;     // seconds before an uncollected power-up vanishes (0 = never)
    [SerializeField] float powerUpHeight = 0f;        // height of power-ups above the spawner
    [SerializeField] float powerUpMargin = 1f;        // keeps power-ups this far from the room edges

    // Half-width (X) and half-depth (Z) of the playable room, measured from this object's position
    [SerializeField] Vector2 area = new Vector2(7.5f, 9.5f);
    [SerializeField] float spawnHeight = 0.75f;

    int coinsSpawned;

    void Start() => InvokeRepeating(nameof(SpawnCoin), 1f, interval);

    Vector3 RandomPointInRoom(float margin = 0f)
    {
        float halfX = Mathf.Max(0f, area.x - margin);
        float halfZ = Mathf.Max(0f, area.y - margin);
        float x = Random.Range(-halfX, halfX);
        float z = Random.Range(-halfZ, halfZ);
        return transform.position + new Vector3(x, spawnHeight, z);
    }

    public void SpawnCoin()
    {
        if (coinPrefab == null || transform.childCount >= maxCoins) return;

        Vector3 pos = RandomPointInRoom();
        if (player != null && Vector3.Distance(pos, player.position) < 2f) return;

        Instantiate(coinPrefab, pos, Quaternion.identity, transform);

        coinsSpawned++;
        if (coinsPerPowerUp > 0 && coinsSpawned % coinsPerPowerUp == 0) SpawnPowerUp();
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        // Retry until the spot isn't on top of the player
        Vector3 pos = RandomPointInRoom(powerUpMargin);
        for (int tries = 0; tries < 20; tries++)
        {
            pos = RandomPointInRoom(powerUpMargin);
            if (player == null || Vector3.Distance(pos, player.position) >= 2f) break;
        }
        pos.y = transform.position.y + powerUpHeight;

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        // No parent: power-ups must not use up one of the coin slots
        GameObject powerUp = Instantiate(prefab, pos, Quaternion.identity);
        if (powerUpLifetime > 0f) Destroy(powerUp, powerUpLifetime);
    }

    // Shows the spawn area in the Scene view when the Spawner is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(area.x * 2f, 0.1f, area.y * 2f));
        Gizmos.color = Color.cyan;
        Vector3 inner = new Vector3(Mathf.Max(0f, area.x - powerUpMargin) * 2f, 0.1f, Mathf.Max(0f, area.y - powerUpMargin) * 2f);
        Gizmos.DrawWireCube(transform.position, inner);
    }
}