using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WorldSaveGameManager : MonoBehaviour
{
    public static WorldSaveGameManager Instance {get; private set;}

    [SerializeField] private int worldSceneIndex = 1;
    private void Awake()
    {
        // Может находится только один экземпляр этого скрипта, если есть другой, уничтожаем
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadNewGame()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(worldSceneIndex);

        yield return null;
    }
}
