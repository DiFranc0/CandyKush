using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;
    public static ObjectPool Instance { get { return instance; } }

    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();
    private bool isInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializePool(GameObject[] prefabs, int amountPerType)
    {
        if (isInitialized)
            return;

        poolDictionary.Clear();

        for (int i = 0; i < prefabs.Length; i++)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int j = 0; j < amountPerType; j++)
            {
                GameObject obj = Instantiate(prefabs[i], transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(i, objectPool);
        }

        isInitialized = true;
    }

    public GameObject GetFromPool(int weedType)
    {
        if (!poolDictionary.ContainsKey(weedType))
        {
            Debug.LogWarning("Pool est� vazio para o tipo " + weedType + ".  Criando novo pool.");
           
            GameObject prefab = Board.Instance.weedPrefabs[weedType];
            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolDictionary.Add(weedType, objectPool);
        }

        if (poolDictionary[weedType].Count == 0)
        {
            // Criar novo objeto se o pool estiver vazio
            GameObject prefab = Board.Instance.weedPrefabs[weedType];
            GameObject newObj = Instantiate(prefab, transform);
            return newObj;
        }

        GameObject obj = poolDictionary[weedType].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj, int weedType)
    {
        if (!poolDictionary.ContainsKey(weedType))
        {
            Debug.LogWarning("Tentando retornar objeto para pool inexistente de tipo " + weedType + ". Criando novo pool.");
            poolDictionary.Add(weedType, new Queue<GameObject>());
        }

        obj.SetActive(false);
        poolDictionary[weedType].Enqueue(obj);
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }
}
