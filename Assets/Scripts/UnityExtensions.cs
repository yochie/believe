using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    //https://discussions.unity.com/t/check-if-layer-is-in-layermask/16007/4
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
