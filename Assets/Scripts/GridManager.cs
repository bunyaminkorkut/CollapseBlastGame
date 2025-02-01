using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class GridManager : MonoBehaviour
{
    private int rowsM;
    private int colsN;

    private int colorCountK;

    private int ALimit;
    private int BLimit;
    private int CLimit;

    public TMP_Text scoreText;
    private int BlastedBlockCount = 0;

    public TMP_Text fpsText;

    public GameObject[] possibleBlocks;


    private Block[,] grid;
    public float cellSize = 1f;
    private Vector3 gridOffset;
    private bool isProcessing = false;


    void Start()
    {
        rowsM = PlayerPrefs.GetInt("M", 4);
        colsN = PlayerPrefs.GetInt("N", 4);
        colorCountK = PlayerPrefs.GetInt("K", 4);
        ALimit = PlayerPrefs.GetInt("A", 2);
        BLimit = PlayerPrefs.GetInt("B", 3);
        CLimit = PlayerPrefs.GetInt("C", 4);



        if (colorCountK < 6)
        {
            List<GameObject> blockList = new List<GameObject>(possibleBlocks);

            for (int i = blockList.Count - 1; i >= colorCountK; i--)
            {
                blockList.RemoveAt(i);
            }

            possibleBlocks = blockList.ToArray(); 
        }

        Debug.Log("Possible blocks: " + possibleBlocks.Length);

        InitializeGrid();
        CenterGrid();

        StartCoroutine(DelayedIconUpdate());
    }


    private float deltaTime = 0.0f;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if (Time.frameCount % 60 == 0) // Update every second
        {
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + fps.ToString("F0");
        }
    }


    private IEnumerator DelayedIconUpdate()
    {
        yield return new WaitForSeconds(0.01f); 
        UpdateBlockIcons(); 
    }


    void InitializeGrid()
    {
        grid = new Block[rowsM, colsN];

        for (int i = 0; i < rowsM; i++)
        {
            for (int j = 0; j < colsN; j++)
            {
                CreateBlockAt(i, j);
            }
        }
    }

    private void CreateBlockAt(int row, int col)
    {
        if (possibleBlocks.Length == 0)
        {
            Debug.LogError("PossibleBlocks listesi boş! Lütfen Inspector'dan ayarlayın.");
            return;
        }

        Vector3 spawnPosition = GetWorldPosition(row, col);

        GameObject randomBlockPrefab = possibleBlocks[Random.Range(0, possibleBlocks.Length)];
        GameObject blockObj = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity, transform);
        Block block = blockObj.GetComponent<Block>();

        if (block == null)
        {
            Debug.LogError("Block prefab'ında Block scripti eksik! Lütfen kontrol et.");
            return;
        }

        grid[row, col] = block; 
        block.row = row;
        block.col = col;
    }

    void CenterGrid()
    {
        float gridWidth = colsN * cellSize;
        float gridHeight = rowsM * cellSize;
        gridOffset = new Vector3(-gridWidth / 2 + cellSize / 2, gridHeight / 2 - cellSize / 2, -10);
        transform.position = gridOffset;
    }

    Vector3 GetWorldPosition(int row, int col)
    {
        return new Vector3(col * cellSize, -row * cellSize, (10 + row)) + gridOffset;
    }


    public Block GetBlock(int row, int col)
    {
        if (row < 0 || row >= rowsM || col < 0 || col >= colsN) return null;
        return grid[row, col];
    }

    public void BlastBlocks(int startRow, int startCol)
    {
        Debug.Log(isProcessing);
        if (isProcessing) return;
        StartCoroutine(ProcessBlastSequence(startRow, startCol));
    }

    private IEnumerator ProcessBlastSequence(int startRow, int startCol)
    {
        isProcessing = true;

        Block startBlock = GetBlock(startRow, startCol);
        if (startBlock == null)
        {
            isProcessing = false;
            yield break;
        }

        Color targetColor = startBlock.color;
        List<Block> group = FindConnectedBlocks(startRow, startCol, targetColor);

        if (group.Count >= 2)
        {
            BlastedBlockCount += group.Count;
            scoreText.text = "Score: " + BlastedBlockCount;

            foreach (Block block in group)
            {
                grid[block.row, block.col] = null;
                Destroy(block.gameObject);
            }

            yield return new WaitForSeconds(0.2f);

            DropBlocks();
            yield return StartCoroutine(DropBlocksCoroutine());


            yield return StartCoroutine(RefillGridCoroutine());


            if (!HasAvailableMoves())
            {
                ShuffleGrid();
            }
            UpdateBlockIcons();
        }

        isProcessing = false;
    }

    private void UpdateBlockIcons()
    {
        bool[,] visited = new bool[rowsM, colsN];

        for (int row = 0; row < rowsM; row++)
        {
            for (int col = 0; col < colsN; col++)
            {
                Block currentBlock = GetBlock(row, col);
                if (currentBlock == null || visited[row, col]) continue;

                List<Block> connectedBlocks = FindConnectedBlocks(row, col, currentBlock.color);

                if (connectedBlocks.Count <= ALimit)
                {
                    foreach (Block block in connectedBlocks)
                    {
                        block.SetIcon(block.DefaultIcon);
                        visited[block.row, block.col] = true;
                    }
                }
                else if (connectedBlocks.Count > ALimit && connectedBlocks.Count <= BLimit)
                {
                    foreach (Block block in connectedBlocks)
                    {
                        block.SetIcon(block.AIcon);
                        visited[block.row, block.col] = true;
                    }
                }
                else if (connectedBlocks.Count > BLimit && connectedBlocks.Count <= CLimit)
                {
                    foreach (Block block in connectedBlocks)
                    {
                        block.SetIcon(block.BIcon);
                        visited[block.row, block.col] = true;
                    }
                }
                else
                {
                    foreach (Block block in connectedBlocks)
                    {
                        block.SetIcon(block.CIcon);
                        visited[block.row, block.col] = true;
                    }
                }
            }
        }
    }


    private IEnumerator DropBlocksCoroutine()
    {
        float dropSpeed = 10f;
        bool isMoving = true;

        while (isMoving)
        {
            isMoving = false;

            for (int col = 0; col < colsN; col++)
            {
                for (int row = rowsM - 1; row >= 0; row--)
                {
                    if (grid[row, col] != null)
                    {
                        Block block = grid[row, col];
                        Vector3 targetPos = GetWorldPosition(block.row, block.col);

                        if (Vector3.Distance(block.transform.position, targetPos) > 0.01f)
                        {
                            isMoving = true;
                            block.transform.position = Vector3.MoveTowards(
                                block.transform.position,
                                targetPos,
                                dropSpeed * Time.deltaTime
                            );
                        }
                    }
                }
            }

            yield return null;
        }
    }

    private void DropBlocks()
    {
        for (int col = 0; col < colsN; col++)
        {
            int writeRow = rowsM - 1;
            for (int readRow = rowsM - 1; readRow >= 0; readRow--)
            {
                if (grid[readRow, col] != null)
                {
                    if (writeRow != readRow)
                    {
                        Block block = grid[readRow, col];
                        grid[writeRow, col] = block;
                        grid[readRow, col] = null;
                        block.row = writeRow;
                        block.col = col;
                    }
                    writeRow--;
                }
            }
        }

    }


    private IEnumerator RefillGridCoroutine()
    {
        float dropSpeed = 5f; 

        for (int col = 0; col < colsN; col++)
        {
            for (int row = rowsM - 1; row >= 0; row--)
            {
                if (grid[row, col] == null) 
                {
                    Vector3 spawnPosition = GetWorldPosition(-1, col); 
                    GameObject randomBlockPrefab = possibleBlocks[Random.Range(0, possibleBlocks.Length)];
                    GameObject blockObj = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity, transform);
                    Block block = blockObj.GetComponent<Block>();
                    grid[row, col] = block;

                    block.row = row;
                    block.col = col;

                    StartCoroutine(DropBlockToPosition(block, GetWorldPosition(row, col), dropSpeed));
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        yield return new WaitForSeconds(0.5f); 
    }

    private IEnumerator DropBlockToPosition(Block block, Vector3 targetPosition, float speed)
    {
        while (Vector3.Distance(block.transform.position, targetPosition) > 0.01f)
        {
            block.transform.position = Vector3.MoveTowards(block.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }


    private List<Block> FindConnectedBlocks(int startRow, int startCol, Color targetColor)
    {
        List<Block> connectedBlocks = new List<Block>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(new Vector2Int(startRow, startCol));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int row = current.x;
            int col = current.y;

            if (row < 0 || row >= rowsM || col < 0 || col >= colsN) continue;
            if (visited.Contains(current)) continue;

            Block block = GetBlock(row, col);
            if (block == null || block.color != targetColor) continue;

            visited.Add(current);
            connectedBlocks.Add(block);

            queue.Enqueue(new Vector2Int(row + 1, col));
            queue.Enqueue(new Vector2Int(row - 1, col));
            queue.Enqueue(new Vector2Int(row, col + 1));
            queue.Enqueue(new Vector2Int(row, col - 1));
        }

        return connectedBlocks;
    }


    private bool ColorEquals(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r) &&
               Mathf.Approximately(a.g, b.g) &&
               Mathf.Approximately(a.b, b.b);
    }

    private bool HasAvailableMoves()
    {
        for (int row = 0; row < rowsM; row++)
        {
            for (int col = 0; col < colsN; col++)
            {
                Block currentBlock = GetBlock(row, col);
                if (currentBlock == null) continue;

                Color currentColor = currentBlock.color;

                if (GetBlock(row, col + 1) != null && GetBlock(row, col + 1).color == currentColor)
                {
                    return true; 
                }
                if (GetBlock(row + 1, col) != null && GetBlock(row + 1, col).color == currentColor)
                {
                    return true;
                }
            }
        }

        return false;
    }
    private void ShuffleGrid()
    {
        // Store existing blocks
        Block[,] oldGrid = new Block[rowsM, colsN];
        for (int row = 0; row < rowsM; row++)
        {
            for (int col = 0; col < colsN; col++)
            {
                oldGrid[row, col] = grid[row, col];
            }
        }

        // Create a list of all current blocks for redistribution
        List<Block> allBlocks = new List<Block>();
        for (int row = 0; row < rowsM; row++)
        {
            for (int col = 0; col < colsN; col++)
            {
                if (oldGrid[row, col] != null)
                {
                    allBlocks.Add(oldGrid[row, col]);
                }
            }
        }

        // Ensure we have at least one guaranteed match
        if (allBlocks.Count >= 2)
        {
            // Pick a random block type for the guaranteed match
            GameObject randomBlockPrefab = possibleBlocks[Random.Range(0, possibleBlocks.Length)];
            Color matchColor = randomBlockPrefab.GetComponent<Block>().color;

            // Find blocks of the selected color
            List<Block> matchingBlocks = allBlocks.FindAll(b => ColorEquals(b.color, matchColor));

            if (matchingBlocks.Count < 2)
            {
                // If we don't have enough matching blocks
                while (matchingBlocks.Count < 2)
                {
                    randomBlockPrefab = possibleBlocks[Random.Range(0, possibleBlocks.Length)];
                    matchColor = randomBlockPrefab.GetComponent<Block>().color;
                    matchingBlocks = allBlocks.FindAll(b => ColorEquals(b.color, matchColor));
                }
            }

            // Remove 2 matching blocks from the main list
            int count = 0;
            foreach (Block block in matchingBlocks)
            {
                if (count >= 2) break; // Sadece 2 blok çıkarmak için döngüyü durdur
                allBlocks.Remove(block);
                count++;
            }

            // Shuffle remaining blocks
            for (int i = allBlocks.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Block temp = allBlocks[i];
                allBlocks[i] = allBlocks[randomIndex];
                allBlocks[randomIndex] = temp;
            }

            // Clear the grid
            for (int row = 0; row < rowsM; row++)
            {
                for (int col = 0; col < colsN; col++)
                {
                    grid[row, col] = null;
                }
            }

            // Decide whether to place the matching blocks horizontally or vertically
            bool isHorizontal = Random.Range(0, 2) == 0;

            int matchRow, matchCol;

            if (isHorizontal)
            {
                matchRow = Random.Range(0, rowsM);
                matchCol = Random.Range(0, colsN - 1);

                grid[matchRow, matchCol] = matchingBlocks[0];
                grid[matchRow, matchCol + 1] = matchingBlocks[1];

                matchingBlocks[0].row = matchRow;
                matchingBlocks[0].col = matchCol;
                matchingBlocks[1].row = matchRow;
                matchingBlocks[1].col = matchCol + 1;
            }
            else
            {
                matchRow = Random.Range(0, rowsM - 1);
                matchCol = Random.Range(0, colsN);

                grid[matchRow, matchCol] = matchingBlocks[0];
                grid[matchRow + 1, matchCol] = matchingBlocks[1];

                matchingBlocks[0].row = matchRow;
                matchingBlocks[0].col = matchCol;
                matchingBlocks[1].row = matchRow + 1;
                matchingBlocks[1].col = matchCol;
            }

            // Update positions of matching blocks
            matchingBlocks[0].transform.position = GetWorldPosition(matchingBlocks[0].row, matchingBlocks[0].col);
            matchingBlocks[1].transform.position = GetWorldPosition(matchingBlocks[1].row, matchingBlocks[1].col);


            // Fill rest of the grid
            int blockIndex = 0;
            for (int row = 0; row < rowsM; row++)
            {
                for (int col = 0; col < colsN; col++)
                {
                    if (grid[row, col] == null && blockIndex < allBlocks.Count)
                    {
                        grid[row, col] = allBlocks[blockIndex];
                        allBlocks[blockIndex].row = row;
                        allBlocks[blockIndex].col = col;
                        allBlocks[blockIndex].transform.position = GetWorldPosition(row, col);
                        blockIndex++;
                    }
                }
            }

            // Clean unused old blocks
            foreach (Block block in oldGrid)
            {
                if (block != null && !IsBlockInGrid(block))
                {
                    Destroy(block.gameObject);
                }
            }
        }
    }

    private bool IsBlockInGrid(Block block)
    {
        for (int row = 0; row < rowsM; row++)
        {
            for (int col = 0; col < colsN; col++)
            {
                if (grid[row, col] == block)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void MainPageButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}
