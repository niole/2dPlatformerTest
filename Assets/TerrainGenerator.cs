using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    // CURRENT TILE WIDTH IN SCREEN IS >= 22 < 24
    static private int softUpperBound = 20;

    public GameObject locationCollider;

    public int maxHeight = 4;

    public Tilemap tileMap;

    public TileBase horizontalTileStart; // 1

    public TileBase horizontalTileMid; // 2

    public TileBase horizontalTileEnd; // 3

    public TileBase slantedDownTileStart; // 4

    public TileBase slantedDownTileEnd; // 5

    public TileBase slantedUpTileStart; // 6

    public TileBase slantedUpTileEnd; // 7

    public TileBase dirt; // 8

    public Rigidbody2D coin;

    Dictionary<int, TileBase> tileBaseTable = new Dictionary<int, TileBase>();

    static float fetchError = 0.25f;

    static int xAbsoluteStart = -2;

    static int yAbsoluteStart = -3;

    float playerXLocation = xAbsoluteStart;

    int xStart = xAbsoluteStart;

    int yStart = yAbsoluteStart;

    float shouldFetchRightOffset = (1f - fetchError) * softUpperBound;

    float shouldFetchLeftOffset = fetchError * softUpperBound;

    bool updatingState = false;

    // [ [ [yval, tile type, coin does/nt exist 0 or 1, is rendered?], [], [] ], ...]
    List<List<List<int>>> state;

    // key is the absolute x-y position of the tile on top of which this is rendered
    Dictionary<string, Rigidbody2D> renderedCoins = new Dictionary<string, Rigidbody2D>();

    // 0 flat
    // 1 up one
    // 2 up two
    void Start()
    {
        updatingState = true;

        tileBaseTable.Add(1, horizontalTileStart);
        tileBaseTable.Add(2, horizontalTileMid);
        tileBaseTable.Add(3, horizontalTileEnd);
        tileBaseTable.Add(4, slantedDownTileStart);
        tileBaseTable.Add(5, slantedDownTileEnd);
        tileBaseTable.Add(6, slantedUpTileStart);
        tileBaseTable.Add(7, slantedUpTileEnd);
        tileBaseTable.Add(8, dirt);

        tileMap = GetComponent<Tilemap>();

        BoundsInt bounds = tileMap.cellBounds;
        PlayerMoveEvents.current.onPlayerMoveTriggerEnter += OnPlayerMove;
        CoinEvents.current.onCoinDestroyedTriggerEnter += OnCoinDestroyed;
        TriggerDetector.current.onTrigger += OnTriggerThing;
        state = BuildState(0, softUpperBound);
        RenderState();
        updatingState = false;
    }

    bool ShouldGetRight(float playerX)
    {
        float playerOffset = playerX - xStart;
        // always allow right fetching when possible
        // TODO create absolute upper bound eventually
        return playerOffset >= shouldFetchRightOffset;
    }

    bool ShouldGetLeft(float playerX)
    {
        float playerOffset = playerX - xStart;

        // only fetch left if not at start
        return playerOffset <= shouldFetchLeftOffset && playerX > xAbsoluteStart + shouldFetchLeftOffset;
    }

    void DestroyGameInRange(int start, int end)
    {
        for (int x = start; x < end; x++)
        {
            List<List<int>> xSliceData = state[x];

            for ( int i = 0; i < xSliceData.Count(); i++)
            {
                List<int> ySliceData = xSliceData[i];
                int yPos = ySliceData[0];
                bool hasCoin = ySliceData[2] == 1;
                if (hasCoin)
                {
                    // destroy coin
                    string coinKey = GetCoinKey(x, yPos);
                    Rigidbody2D currentlyRenderedCoin = renderedCoins[coinKey];
                    if (currentlyRenderedCoin != null)
                    {
                        Destroy(currentlyRenderedCoin.gameObject);
                        renderedCoins.Remove(coinKey);
                    } else
                    {
                        Debug.Log($"May want to check coin at {coinKey}. Key still exists, coin does not");
                    }
                }

                Vector3Int tilePos = new Vector3Int(xIndexToWorldX(x), yStart + yPos, 0);
                tileMap.SetTile(tilePos, null);
            }
        }
    }

    void OnTriggerThing(bool isLeftTrigger, int x, int y)
    {
        Debug.Log($"TRIGGER THING {isLeftTrigger}, {x}, {y}");
    }

    void MaybeUpdateActiveSection(float playerX)
    {
        if (!updatingState)
        {
            bool getLeft = ShouldGetLeft(playerX) && !ShouldGetLeft(playerXLocation);
            bool getRight = ShouldGetRight(playerX) && !ShouldGetRight(playerXLocation);
            if (getLeft || getRight)
            {
                updatingState = true;
                if (getLeft)
                {
                    // fetch left, garbage collect right, render terrain, update startX
                    int destroyStartIndex = getStateXIndex(xStart) + Convert.ToInt32(shouldFetchRightOffset);
                    int destroyEndIndex = getStateXIndex(xStart)  + softUpperBound;
                    DestroyGameInRange(destroyStartIndex, destroyEndIndex);
                    xStart = Mathf.Max(xStart - softUpperBound, xAbsoluteStart);
                    RenderState();
                } else if (getRight)
                {
                    int destroyStartIndex = getStateXIndex(xStart);
                    int destroyEndIndex = destroyStartIndex + Convert.ToInt32(shouldFetchLeftOffset);
                    DestroyGameInRange(destroyStartIndex, destroyEndIndex);

                    List<List<int>> lastX = state[state.Count() - 1];
                    List<int> lastYSlice = lastX[lastX.Count() - 1];
                    int lastYOffset = lastYSlice[0];
                    xStart += Convert.ToInt32(shouldFetchLeftOffset); // TODO global state is evil
                    List<List<List<int>>> nextState = BuildState(lastYOffset, Convert.ToInt32(shouldFetchLeftOffset));
                    state.AddRange(nextState);

                    RenderState();
                }
                updatingState = false;
            }
        }
    }

    // [ | | | ]
    List<List<List<int>>> BuildState(int initialLevel, int length)
    {
        List<int> levelChoices = new List<int>{ -1, 0, 1 };

        int level = initialLevel;

        List<List<List<int>>> nextState = new List<List<List<int>>>(softUpperBound);

        for (int i = 0; i < length; i++)
        {
            var choices = levelChoices.Where(l => level + l >= 0 && level + l <= maxHeight).ToList();
            int nextChoiceIndex = Convert.ToInt32(UnityEngine.Random.Range(0f, Convert.ToSingle(choices.Count() - 1)));

            int choice = choices[nextChoiceIndex];

            level += choice;

            if (choice == -1)
            {
                nextState.Add(new List<List<int>> {
                    new List<int> {level + 1, 4, 0},
                    new List<int> {level, 5, 0}
                });
            } else if (choice == 0)
            {
                bool shouldRenderCoin = UnityEngine.Random.Range(0f, 100f) <= 20f;
                if (shouldRenderCoin)
                {
                    nextState.Add(new List<List<int>> {
                        new List<int> {level, 2, 1}
                    });
                } else
                {
                    nextState.Add(new List<List<int>> {
                        new List<int> {level, 2, 0}
                    });
                }

            } else if (choice == 1)
            {
                nextState.Add(new List<List<int>> {
                    new List<int> {level - 1, 6, 0},
                    new List<int> {level, 7, 0}
                });
            }
        }

        return nextState;
    }

    /**
     * This renders from offsets, not according to world coordinates
     */
    void RenderState()
    {
        int xRenderStart = Mathf.Max(getStateXIndex(xStart), xAbsoluteStart);
        for (int xOffset = xRenderStart; xOffset < xRenderStart + softUpperBound; xOffset++)
        {
            List<List<int>> renderableXSection = state[xOffset];
            for (int j = 0; j < renderableXSection.Count(); j++)
            {
                List<int> yDetails = renderableXSection[j];
                int yOffset = yDetails[0];
                int tileType = yDetails[1];
                bool hasCoin = yDetails[2] == 1;

                Vector3Int tilePos = new Vector3Int(xIndexToWorldX(xOffset), yStart + yOffset, 0);
                if (!tileMap.HasTile(tilePos))
                {
                    TileBase tile = tileBaseTable[tileType];
                    if (tile != null)
                    {
                        tileMap.SetTile(tilePos, tile);
                        bool isRightDetector = xOffset == shouldFetchRightOffset;
                        bool isLeftDetector = xOffset == shouldFetchLeftOffset;
                        if (isLeftDetector || isRightDetector)
                        {
                            Vector3Int colliderPos = new Vector3Int(tilePos.x, tilePos.y + 1, tilePos.z);
                            GameObject newLC = Instantiate(
                                locationCollider,
                                colliderPos,
                                Quaternion.identity
                            );
                            TriggerDetector td = newLC.GetComponent<TriggerDetector>();
                            td.x = tilePos.x;
                            td.y = tilePos.y;
                            td.isLeftTrigger = isLeftDetector;
                            td.isRightTrigger = isRightDetector;
                            td.onTrigger += OnTriggerThing;
                        }
                    }

                    if (hasCoin)
                    {
                        Rigidbody2D newCoin = Instantiate(
                            coin,
                            new Vector3(xIndexToWorldX(xOffset), yOffset + yStart + 3, 0),
                            Quaternion.identity
                        );
                        string coinKey = GetCoinKey(xOffset, yOffset);
                        renderedCoins.Add(coinKey, newCoin);
                    }
                }
            }
        }

    }

    string GetCoinKey(int relativeTileX, int relativeTileY)
    {
        string coinKey = $"{relativeTileX + xStart}-{relativeTileY + yStart}";
        return coinKey;
    }

    int getStateXIndex(float xLocation)
    {
        return Convert.ToInt32(xLocation) - xAbsoluteStart;
    }

    int xIndexToWorldX(int x)
    {
        return x + xAbsoluteStart;
    }

    private void OnPlayerMove(float xLocation)
    {
        MaybeUpdateActiveSection(xLocation);
        playerXLocation = xLocation;
    }

    private void OnCoinDestroyed(float xLocation)
    {
        int x = getStateXIndex(xLocation);
        // get the coins
        for (int i = 0; i < state[x].Count(); i++)
        {
            bool hasCoin = state[x][i][2] == 1;
            if (hasCoin)
            {
                int y = state[x][i][0];
                string coinKey = GetCoinKey(x, y);
                if (renderedCoins.ContainsKey(coinKey))
                {
                    renderedCoins.Remove(coinKey);
                }
                state[x][i][2] = 0;
            }
        }
    }
}
