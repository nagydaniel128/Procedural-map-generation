using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the data about the blocks and placable object types in a biom.
/// </summary>
public class Biom : MonoBehaviour
{
    public Block.BlockInfo[] blocks;

    public List<PlacableObject.PlacableObjectInfo> placableObjects = new List<PlacableObject.PlacableObjectInfo>();
}

