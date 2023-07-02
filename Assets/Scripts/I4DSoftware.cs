using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ThreeDDisplayで呼び出して mesh の情報を出力するインターフェース。
public interface I4DSoftware
{
    void Run(ref Vector3[] vertices, ref int[] triangles, ref Color[] colors, double[][] input);
}
