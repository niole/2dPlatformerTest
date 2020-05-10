using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    static public int softUpperBound = 60; // can go over this bound in on of the iterations, but then that's enough

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

    int xStart = xAbsoluteStart;

    int yStart = -3;

    float sectionStartIndex = 0f;

    float sectionEndIndex = 0f;

    float shouldFetchRightOffset = (1f - fetchError) * softUpperBound;

    float shouldFetchLeftOffset = fetchError * softUpperBound;

    List<List<List<int>>> state;

    // TODO when user goes far enough right, we generate more and delete the oldest, vice versa
    // if user wants to go backwards, they will get the same terrain that was originally there
    // if a user has collected the coins already, they will not be there
    //
    // need to store all terrain representation, includes no/coin
    // have multidimentional array, x offset is the index of main array, subarray is  y, has tile, tile type, has coin
    // [ [ [yval, tile type, has coin], [], [] ], ...]
    // [ [
    //
    // need "active section" endpoints
    //
    // need "get more right" offset
    // need "get more left" offset
    // TODO when collide with coin need to update coin state

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

    void MaybeUpdateActiveSection(float playerX)
    {
        if (ShouldGetLeft(playerX))
        {
            // fetch left, garbage collect right
        } else if (ShouldGetRight(playerX))
        {
            // fetch right, garbage collect left
        }
    }

    // 0 flat
    // 1 up one
    // 2 up two
    void Start()
    {
        tileBaseTable.Add(1, horizontalTileStart);
        tileBaseTable.Add(2, horizontalTileMid);
        tileBaseTable.Add(3, horizontalTileEnd);
        tileBaseTable.Add(4, slantedDownTileStart);
        tileBaseTable.Add(5, slantedDownTileEnd);
        tileBaseTable.Add(6, slantedUpTileStart);
        tileBaseTable.Add(7, slantedUpTileEnd);
        tileBaseTable.Add(8, dirt);

        tileMap = GetComponent<Tilemap>();
        PlayerMoveEvents.current.onPlayerMoveTriggerEnter += OnPlayerMove;
        state = BuildState(0);
        RenderState();
    }

    // TODO assumes everything previously (in space) has been built correctly
    // there is no dependency on what was previously built
    List<List<List<int>>> BuildState(int initialLevel)
    {
        List<int> levelChoices = new List<int>{ -1, 0, 1 };

        int level = initialLevel;

        List<List<List<int>>> nextState = new List<List<List<int>>>(softUpperBound);

        for (int i = 0; i < softUpperBound; i++)
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

    void RenderState()
    {
        List<List<List<int>>> stateToRender = state;

        for (int xOffset = 0; xOffset < softUpperBound; xOffset++)
        {
            List<List<int>> renderableXSection = stateToRender[xOffset];
            for (int j = 0; j < renderableXSection.Count(); j++)
            {
                List<int> yDetails = renderableXSection[j];
                int yOffset = yDetails[0];
                int tileType = yDetails[1];
                int hasCoin = yDetails[2];

                Vector3Int tilePos = new Vector3Int(xStart + xOffset, yStart + yOffset, 0);
                TileBase tile = tileBaseTable[tileType];
                if (tile != null)
                {
                    tileMap.SetTile(tilePos, tile);
                }

                if (hasCoin == 1)
                {
                    Instantiate(coin, new Vector3(xOffset + xStart, yOffset + yStart + 3, 0), Quaternion.identity);
                }

            }
        }

    }

    private void OnPlayerMove(float xLocation)
    {
        Debug.Log($"Player moved {xLocation}");
    }

}
