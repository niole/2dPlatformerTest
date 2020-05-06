using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    public int softUpperBound = 60; // can go over this bound in on of the iterations, but then that's enough

    public int maxHeight = 4;

    public Tilemap tileMap;

    public TileBase horizontalTileStart;

    public TileBase horizontalTileMid;

    public TileBase horizontalTileEnd;

    public TileBase slantedDownTileStart;

    public TileBase slantedDownTileEnd;

    public TileBase slantedUpTileStart;

    public TileBase slantedUpTileEnd;

    public TileBase dirt;

    public Rigidbody2D coin;

    int xStart = -2;

    int yStart = -3;

    // 0 flat
    // 1 up one
    // 2 up two
    void Start()
    {
        tileMap = GetComponent<Tilemap>();
        int level = 0;
        int yOffset = 0;
        int xOffset = 0;

        while (xOffset < softUpperBound)
        {
            List<int> levelChoices = new List<int>{ -1, 0, 1 };
            var choices = levelChoices.Where(l => level + l >= 0 && level + l <= maxHeight).ToList();
            int nextChoiceIndex = Convert.ToInt32(UnityEngine.Random.Range(0f, Convert.ToSingle(choices.Count() - 1)));
            bool shouldRenderCoin = false;

            int choice = choices[nextChoiceIndex];

            level += choice;
            xOffset += 1;

            if (choice < 0)
            {
                // going down
                Vector3Int firstTilePos = new Vector3Int(xStart + xOffset, yStart + yOffset, 0);
                tileMap.SetTile(firstTilePos, slantedDownTileStart);
                Vector3Int secondTilePos = new Vector3Int(xStart + xOffset, yStart + yOffset - 1, 0);
                tileMap.SetTile(secondTilePos, slantedDownTileEnd);

                yOffset -= 1;
            } else if (choice == 0)
            {
                shouldRenderCoin = UnityEngine.Random.Range(0f, 100f) <= 20f;

                // staying level, y doesn't change
                Vector3Int tilePos = new Vector3Int(xStart + xOffset, yStart + yOffset, 0);
                tileMap.SetTile(tilePos, horizontalTileMid);
            } else if (choice > 0)
            {
                // going up, y goes up
                Vector3Int firstTilePos = new Vector3Int(xStart + xOffset, yStart + yOffset, 0);
                tileMap.SetTile(firstTilePos, slantedUpTileStart);
                Vector3Int secondTilePos = new Vector3Int(xStart + xOffset, yStart + yOffset + 1, 0);
                tileMap.SetTile(secondTilePos, slantedUpTileEnd);

                yOffset += 1;
            }

            if (shouldRenderCoin)
            {
                Instantiate(coin, new Vector3(xOffset + xStart, yOffset + yStart + 3, 0), Quaternion.identity);
            }
        }
    }
}
