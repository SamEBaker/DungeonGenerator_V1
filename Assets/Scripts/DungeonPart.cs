using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonPart : MonoBehaviour
{
    public enum DungeonPartType
    {
        Room,
        Hallway
    }
    [SerializeField]
    private LayerMask roomsLayer;
    [SerializeField]
    private DungeonPartType dungeonPartType;
    [SerializeField]
    private GameObject fillerWall;

    public List<Transform> entryPts;
    public new Collider collider;
    public bool HasAvailableEntryPoint(out Transform entryPt)
    {
        Transform resultingEntry = null;
        bool result = false;

        int totalRetries = 100;
        int retryIndex = 0;

        if (entryPts.Count == 1)
        {
            Transform entry = entryPts[0];
            if (entry.TryGetComponent<EntryPoint>(out EntryPoint res))
            {
                if (res.IsOccupied())
                {
                    result = false;
                    resultingEntry = null;
                }
                else
                {
                    result = true;
                    resultingEntry = entry;
                    res.SetOccupied();
                }
                entryPt = resultingEntry;
                return result;
            }
        }

        while (resultingEntry == null && retryIndex < totalRetries)
        {
            int randEntryIndex = UnityEngine.Random.Range(0, entryPts.Count);
            Transform entry = entryPts[randEntryIndex];

            if(entry.TryGetComponent<EntryPoint>(out EntryPoint entryPoint))
            {
                if (!entryPoint.IsOccupied())
                {
                    resultingEntry = entry;
                    result = true;
                    entryPoint.SetOccupied();
                    break;
                }
            }
            retryIndex++;
        }
        entryPt = resultingEntry;
        return result;
    }
    public void UnuseEntryPoint(Transform entrypoint)
    {
        if(entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))
        {
            entry.SetOccupied(false);
        }
    }
    public void FillEmptyDoors()
    {
        entryPts.ForEach((entry) =>
        {
            if (entry.TryGetComponent(out EntryPoint entryPoint))
            {
                if (!entryPoint.IsOccupied())
                {
                    GameObject wall = Instantiate(fillerWall);
                    wall.transform.position = entry.transform.position;
                    wall.transform.rotation = entry.transform.rotation;
                }
            }
        });
    }
}

    
