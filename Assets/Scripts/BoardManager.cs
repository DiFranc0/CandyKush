using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Classe principal do tabuleiro de jogo
public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [Header("Configurações do Tabuleiro")]
    public int width = 7;
    public int height = 7;
    public float cellSize = 1f;
    public float swapSpeed = 0.3f;
    public float fallSpeed = 0.5f;
    public int initialPoolSize = 20;

    [Header("Prefabs dos Doces")]
    public GameObject[] weedPrefabs;
    
    [Header("Audio")]
    public AudioManager audioManager;

    [Header("UI")]
    public UIManager uiManager;
    public TMP_Text scoreText;
    public TMP_Text movesText;
    public GameObject gameOverPanel;
    [Header("Player Score/Moves")]
    public int score = 0;
    public int remainingMoves = 10;
    
    public ObjectPool objectPool;

    private GameObject[,] board;
    private Weed selectedWeed;
    private int initialScore;
    private int initialMoves;
    private bool isSwapping = false;
    private bool isProcessingMatches = false;
    private bool isInitialized = false;

    


    private void Awake()
    {
        initialMoves = remainingMoves;
        initialScore = score;
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        board = new GameObject[width, height];
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        
        if (objectPool == null)
        {
            objectPool = FindObjectOfType<ObjectPool>();
            if (objectPool == null)
            {
                Debug.LogError("ObjectPool não encontrado na cena. Criando um novo.");
                GameObject poolObj = new GameObject("ObjectPool");
                objectPool = poolObj.AddComponent<ObjectPool>();
            }
        }
        gameOverPanel.SetActive(false);

        ObjectPool.Instance.InitializePool(weedPrefabs, initialPoolSize);

        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        // Garantir que o pool esteja inicializado antes de prosseguir
        yield return new WaitUntil(() => objectPool.IsInitialized());

        InitializeBoard();
        UpdateUI();
        isInitialized = true;
    }

    private void InitializeBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null)
                {
                    if (board[x, y].activeSelf)
                    {
                        Weed weed = board[x, y].GetComponent<Weed>();
                        objectPool.ReturnToPool(board[x, y], weed.cannabisType);
                    }
                    board[x, y] = null;
                }
            }
        }

        // Criar novas plantas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateWeed(x, y, true);
            }
        }

        // Remover combinações iniciais
        do
        {
            CheckMatches();
        } while (RemoveMatches());
    }

    private void CreateWeed(int x, int y, bool initialSetup)
    {
        int randomIndex;

        if (initialSetup)
        {
            // Evitar criar combinações durante a configuração inicial
            List<int> availableTypes = new List<int>();
            for (int i = 0; i < weedPrefabs.Length; i++)
            {
                availableTypes.Add(i);
            }

            // Remover tipos que criariam combinações horizontais
            if (x >= 2)
            {
                int type1 = board[x - 1, y].GetComponent<Weed>().cannabisType;
                int type2 = board[x - 2, y].GetComponent<Weed>().cannabisType;
                if (type1 == type2 && availableTypes.Contains(type1))
                {
                    availableTypes.Remove(type1);
                }
            }

            // Remover tipos que criariam combinações verticais
            if (y >= 2)
            {
                int type1 = board[x, y - 1].GetComponent<Weed>().cannabisType;
                int type2 = board[x, y - 2].GetComponent<Weed>().cannabisType;
                if (type1 == type2 && availableTypes.Contains(type1))
                {
                    availableTypes.Remove(type1);
                }
            }

            randomIndex = availableTypes.Count > 0 ?
                availableTypes[Random.Range(0, availableTypes.Count)] :
                Random.Range(0, weedPrefabs.Length);
        }
        else
        {
            randomIndex = Random.Range(0, weedPrefabs.Length);
        }

        // Obter uma planta do pool 
        GameObject newWeed = ObjectPool.Instance.GetFromPool(randomIndex);
        newWeed.transform.SetParent(transform);
        newWeed.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);

        Weed weedComponent = newWeed.GetComponent<Weed>();
        weedComponent.Initialize(x, y, randomIndex, this);
        weedComponent.isMatched = false;
        weedComponent.Deselect(); // Garantir que a planta não esteja visualmente selecionada
        
        Debug.Log($"Criada planta do tipo {randomIndex} na posição {x},{y}");

        board[x, y] = newWeed;
    }

    public void SelectWeed(Weed weed)
    {
        if (isSwapping || isProcessingMatches || remainingMoves <= 0)
            return;

        if (selectedWeed == null)
        {
            selectedWeed = weed;
            selectedWeed.Select();
        }
        else
        {
            if (IsAdjacent(selectedWeed, weed))
            {
                StartCoroutine(SwapWeedies(selectedWeed, weed));
            }
            else
            {
                selectedWeed.Deselect();
                selectedWeed = weed;
                selectedWeed.Select();
            }
        }
    }

    private bool IsAdjacent(Weed weed1, Weed weed2)
    {
        return (Mathf.Abs(weed1.x - weed2.x) == 1 && weed1.y == weed2.y) ||
               (Mathf.Abs(weed1.y - weed2.y) == 1 && weed1.x == weed2.x);
    }

    private IEnumerator SwapWeedies(Weed weed1, Weed weed2)
    {
        isSwapping = true;
        remainingMoves--;
        UpdateUI();

        // Trocar posições no tabuleiro
        board[weed1.x, weed1.y] = weed2.gameObject;
        board[weed2.x, weed2.y] = weed1.gameObject;

        // Trocar coordenadas
        int tempX = weed1.x;
        int tempY = weed1.y;
        weed1.x = weed2.x;
        weed1.y = weed2.y;
        weed2.x = tempX;
        weed2.y = tempY;

        // Animar o movimento
        Vector3 pos1 = weed1.transform.localPosition;
        Vector3 pos2 = weed2.transform.localPosition;

        float elapsedTime = 0f;
        while (elapsedTime < swapSpeed)
        {
            weed1.transform.localPosition = Vector3.Lerp(pos1, new Vector3(weed1.x * cellSize, weed1.y * cellSize, 0), elapsedTime / swapSpeed);
            weed2.transform.localPosition = Vector3.Lerp(pos2, new Vector3(weed2.x * cellSize, weed2.y * cellSize, 0), elapsedTime / swapSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        weed1.transform.localPosition = new Vector3(weed1.x * cellSize, weed1.y * cellSize, 0);
        weed2.transform.localPosition = new Vector3(weed2.x * cellSize, weed2.y * cellSize, 0);

        // Verificar se a troca criou alguma combinação
        CheckMatches();
        
        if (!RemoveMatches())
        {
            // Se não criou nenhuma combinação, desfazer a troca
            yield return new WaitForSeconds(0.2f);

            // Trocar posições no tabuleiro novamente
            board[weed1.x, weed1.y] = weed2.gameObject;
            board[weed2.x, weed2.y] = weed1.gameObject;

            // Trocar coordenadas novamente
            tempX = weed1.x;
            tempY = weed1.y;
            weed1.x = weed2.x;
            weed1.y = weed2.y;
            weed2.x = tempX;
            weed2.y = tempY;

            // Animar o movimento de volta
            pos1 = weed1.transform.localPosition;
            pos2 = weed2.transform.localPosition;

            elapsedTime = 0f;
            while (elapsedTime < swapSpeed)
            {
                weed1.transform.localPosition = Vector3.Lerp(pos1, new Vector3(weed1.x * cellSize, weed1.y * cellSize, 0), elapsedTime / swapSpeed);
                weed2.transform.localPosition = Vector3.Lerp(pos2, new Vector3(weed2.x * cellSize, weed2.y * cellSize, 0), elapsedTime / swapSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            weed1.transform.localPosition = new Vector3(weed1.x * cellSize, weed1.y * cellSize, 0);
            weed2.transform.localPosition = new Vector3(weed2.x * cellSize, weed2.y * cellSize, 0);

            remainingMoves++; // Devolver o movimento
            UpdateUI();
        }
        else
        {

            // Verificar fim de jogo
            if (remainingMoves <= 0)
            {
                yield return new WaitForSeconds(1f);
                gameOverPanel.SetActive(true);
            }
        }

        // Desselecionar a planta atual
        if (selectedWeed != null)
        {
            selectedWeed.Deselect();
            selectedWeed = null;
        }

        isSwapping = false;
    }

    private void CheckMatches()
    {
        // Verificar combinações horizontais
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                if (board[x, y] != null && board[x + 1, y] != null && board[x + 2, y] != null)
                {
                    Weed weed1 = board[x, y].GetComponent<Weed>();
                    Weed weed2 = board[x + 1, y].GetComponent<Weed>();
                    Weed weed3 = board[x + 2, y].GetComponent<Weed>();

                    if (weed1.cannabisType == weed2.cannabisType && weed2.cannabisType == weed3.cannabisType)
                    {
                        weed1.isMatched = true;
                        weed2.isMatched = true;
                        weed3.isMatched = true;
                    }
                }
            }
        }

        // Verificar combinações verticais
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                if (board[x, y] != null && board[x, y + 1] != null && board[x, y + 2] != null)
                {
                    Weed weed1 = board[x, y].GetComponent<Weed>();
                    Weed weed2 = board[x, y + 1].GetComponent<Weed>();
                    Weed weed3 = board[x, y + 2].GetComponent<Weed>();

                    if (weed1.cannabisType == weed2.cannabisType && weed2.cannabisType == weed3.cannabisType)
                    {
                        weed1.isMatched = true;
                        weed2.isMatched = true;
                        weed3.isMatched = true;
                    }
                }
            }
        }
    }

    private bool RemoveMatches()
    {
        isProcessingMatches = true;
        bool hasMatches = false;
        List<GameObject> matchedWeeds = new List<GameObject>();
        List<GameObject> particleEffects = new List<GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null)
                {
                    Weed weed = board[x, y].GetComponent<Weed>();
                    if (weed.isMatched)
                    {
                        hasMatches = true;
                        AddScore(weed);

                        GameObject weedObject = board[x, y];
                        GameObject particleEffect = weedObject.GetComponent<Weed>().smokeEffect;
                        
                        
                        weedObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
                        
                        particleEffects.Add(particleEffect);
                        matchedWeeds.Add(weedObject);
                        
                        board[x, y] = null;
                    }
                }
            }
        }

        if (hasMatches)
        {
            StartCoroutine(WaitForSmokeWeedAndCleanup(particleEffects, matchedWeeds));
            UpdateUI();
        }

        isProcessingMatches = false;
        return hasMatches;
    }
    
    public void AddScore(Weed weed)
    {
        GameObject pointsEffect = weed.pointsEffect;
        pointsEffect.SetActive(true);
        
        switch(weed.cannabisType)
        {
            case 0:
                Debug.Log("Adicionado 10 points");
                score += 10;
                uiManager.AddProgress(1);
                break;
            case 1:
                Debug.Log("Adicionado 20 points");
                score += 20;
                uiManager.AddProgress(2);
                break;
            case 2:
                Debug.Log("Adicionado 30 points");
                score += 30;
                uiManager.AddProgress(3);
                break;
            case 3:
                Debug.Log("Adicionado 40 points");
                score += 40;
                uiManager.AddProgress(5);
                break;
            case 4:
                Debug.Log("Adicionado 50 points");
                score += 50;
                uiManager.AddProgress(8);
                break;
            case 5:
                Debug.Log("Adicionado 60 points");
                score += 60;
                uiManager.AddProgress(10);
                break;  
                
                
        }
    }
    
    IEnumerator WaitForSmokeWeedAndCleanup(List<GameObject> particleEffects, List<GameObject> matchedWeeds)
    {
        foreach (GameObject effectObj in particleEffects)
        {
            ParticleSystem particleSystem = effectObj.GetComponent<ParticleSystem>();
            
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
        }

        // Aguardar a duração das partículas
        if (particleEffects.Count > 0)
        {
            ParticleSystem firstParticle = particleEffects[0].GetComponent<ParticleSystem>();
            if (firstParticle != null)
            {
                yield return new WaitForSeconds(firstParticle.main.duration);
            }
        }

        // Parar partículas e retornar objetos ao pool
        foreach (GameObject effectObj in particleEffects)
        {
            ParticleSystem particleSystem = effectObj.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }
        }

        // Retornar objetos ao pool após os efeitos estarem concluídos
        foreach (var weedObject in matchedWeeds)
        {
            //weedObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            Debug.Log($"Retornando planta do tipo{weedObject.GetComponent<Weed>().cannabisType} para o pool");
            ObjectPool.Instance.ReturnToPool(weedObject, weedObject.GetComponent<Weed>().cannabisType);
            weedObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
            weedObject.GetComponent<Weed>().pointsEffect.SetActive(false);
        }
        
        // Preencher espaços vazios apenas após todos os efeitos estarem concluídos
        FillEmptySpaces();
    }

    private void FillEmptySpaces()
    {
        // Mover plantas para baixo
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                if (board[x, y] == null)
                {
                    // Procurar a próxima planta acima para mover para baixo
                    for (int yAbove = y + 1; yAbove < height; yAbove++)
                    {
                        if (board[x, yAbove] != null)
                        {
                            // Mover esta planta para baixo
                            Weed weed = board[x, yAbove].GetComponent<Weed>();
                            board[x, y] = board[x, yAbove];
                            board[x, yAbove] = null;

                            // Atualizar coordenadas da planta
                            weed.y = y;

                            // Animar movimento
                            StartCoroutine(MoveWeed(weed, new Vector3(x * cellSize, y * cellSize, 0)));

                            break;
                        }
                    }
                }
            }
        }

        // Criar novas plantas no topo
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    CreateWeed(x, y, false);
                    Weed weed = board[x, y].GetComponent<Weed>();

                    // Definir posição inicial acima do tabuleiro
                    weed.transform.localPosition = new Vector3(x * cellSize, height * cellSize, 0);

                    // Animar queda
                    StartCoroutine(MoveWeed(weed, new Vector3(x * cellSize, y * cellSize, 0)));
                }
            }
        }
    }

    private IEnumerator MoveWeed(Weed weed, Vector3 targetPosition)
    {
        Vector3 startPosition = weed.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < fallSpeed)
        {
            weed.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / fallSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        weed.transform.localPosition = targetPosition;
        
        CheckMatches();
        RemoveMatches();
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        movesText.text = "Moves: " + remainingMoves;
    }

    public void RestartGame()
    {
        // Limpar o tabuleiro e retornar todas as plantas ao pool
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null)
                {
                    Weed weed = board[x, y].GetComponent<Weed>();
                    ObjectPool.Instance.ReturnToPool(board[x, y], weed.cannabisType);
                    board[x, y] = null;
                }
            }
        }

        score = initialScore;
        remainingMoves = initialMoves;
        selectedWeed = null;
        gameOverPanel.SetActive(false);
        uiManager.progressBar.ResetProgress();
        audioManager.ResetAudio();

        InitializeBoard();
        UpdateUI();
    }
}





