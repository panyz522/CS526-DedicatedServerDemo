using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetUtils
{

    public static int FillFloatArray(float[] floats, int begin, Vector2 vector)
    {
        floats[begin++] = vector.x;
        floats[begin++] = vector.y;
        return begin;
    }

    public static int FillFloatArray(float[] floats, int begin, Vector3 vector)
    {
        floats[begin++] = vector.x;
        floats[begin++] = vector.y;
        floats[begin++] = vector.z;
        return begin;
    }

    public static int GetVector2FromArray(float[] floats, int begin, out Vector2 vector)
    {
        vector = new Vector2(floats[begin], floats[begin + 1]);
        return begin + 2;
    }

    public static int GetVector3FromArray(float[] floats, int begin, out Vector3 vector)
    {
        vector = new Vector3(floats[begin], floats[begin + 1], floats[begin + 2]);
        return begin + 3;
    }
}
