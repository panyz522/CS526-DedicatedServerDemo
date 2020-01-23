using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Serializer<T>
{
    private List<PropertyInfo> intProperties = new List<PropertyInfo>();
    private List<PropertyInfo> floatProperties = new List<PropertyInfo>();
    private List<PropertyInfo> vector2Properties = new List<PropertyInfo>();
    private List<PropertyInfo> vector3Properties = new List<PropertyInfo>();
    private List<PropertyInfo> intArrayProperties = new List<PropertyInfo>();
    private List<PropertyInfo> floatArrayProperties = new List<PropertyInfo>();
    private List<PropertyInfo> vector3ArrayProperties = new List<PropertyInfo>();
    private List<PropertyInfo> vector2ArrayProperties = new List<PropertyInfo>();

    public Serializer()
    {
        var propInfos = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach (var prop in propInfos.OrderBy((m) => m.Name))
        {
            if (prop.PropertyType == typeof(int))
            {
                intProperties.Add(prop);
            }
            else if (prop.PropertyType == typeof(float))
            {
                floatProperties.Add(prop);
            }
            else if (prop.PropertyType == typeof(Vector2))
            {
                vector2Properties.Add(prop);
            }
            else if (prop.PropertyType == typeof(Vector3))
            {
                vector3Properties.Add(prop);
            }
            else if (prop.PropertyType == typeof(int[]))
            {
                intArrayProperties.Add(prop);
            }
            else if (prop.PropertyType == typeof(float[]))
            {
                floatArrayProperties.Add(prop);
            }
            else if (prop.PropertyType == typeof(Vector2[]))
            {
                vector2ArrayProperties.Add(prop);
            }
            else if (prop.PropertyType == typeof(Vector3[]))
            {
                vector3ArrayProperties.Add(prop);
            }
            else
            {
                Debug.LogError($"Wrong Type Found! {prop.Name} with type {prop.PropertyType.Name}");
                Debug.Assert(false);
            }
        }
    }

    public int SerializeTo(T syncObj, byte[] output)
    {
        int pos = 0;
        foreach (var prop in intProperties)
        {
            pos = Serialize((prop.GetValue(syncObj) as int?).Value, output, pos);
        }
        foreach (var prop in floatProperties)
        {
            pos = Serialize((prop.GetValue(syncObj) as float?).Value, output, pos);
        }
        foreach (var prop in vector2Properties)
        {
            pos = Serialize((prop.GetValue(syncObj) as Vector2?).Value, output, pos);
        }
        foreach (var prop in vector3Properties)
        {
            pos = Serialize((prop.GetValue(syncObj) as Vector3?).Value, output, pos);
        }
        foreach (var prop in intArrayProperties)
        {
            pos = Serialize(prop.GetValue(syncObj) as int[], output, pos);
        }
        foreach (var prop in floatArrayProperties)
        {
            pos = Serialize(prop.GetValue(syncObj) as float[], output, pos);
        }
        foreach (var prop in vector2ArrayProperties)
        {
            pos = Serialize(prop.GetValue(syncObj) as Vector2[], output, pos);
        }
        foreach (var prop in vector3ArrayProperties)
        {
            pos = Serialize(prop.GetValue(syncObj) as Vector3[], output, pos);
        }
        return pos;
    }

    public int DeserializeFrom(byte[] input, T syncObj)
    {
        int pos = 0;
        foreach (var prop in intProperties)
        {
            pos = Deserialize(input, pos, out int value);
            prop.SetValue(syncObj, value);
        }
        foreach (var prop in floatProperties)
        {
            pos = Deserialize(input, pos, out float value);
            prop.SetValue(syncObj, value);
        }
        foreach (var prop in vector2Properties)
        {
            pos = Deserialize(input, pos, out Vector2 value);
            prop.SetValue(syncObj, value);
        }
        foreach (var prop in vector3Properties)
        {
            pos = Deserialize(input, pos, out Vector3 value);
            prop.SetValue(syncObj, value);
        }
        foreach (var prop in intArrayProperties)
        {
            int[] array = prop.GetValue(syncObj) as int[];
            pos = Deserialize(input, pos, array);
        }
        foreach (var prop in floatArrayProperties)
        {
            float[] array = prop.GetValue(syncObj) as float[];
            pos = Deserialize(input, pos, array);
        }
        foreach (var prop in vector2ArrayProperties)
        {
            Vector2[] array = prop.GetValue(syncObj) as Vector2[];
            pos = Deserialize(input, pos, array);
        }
        foreach (var prop in vector3ArrayProperties)
        {
            Vector3[] array = prop.GetValue(syncObj) as Vector3[];
            pos = Deserialize(input, pos, array);
        }
        return pos;
    }


    private int Serialize(float input, byte[] array, int pos)
    {
        unsafe
        {
            fixed (byte* pArray = array)
                *((float*)(pArray + pos)) = input;
        }
        return pos + sizeof(float);
    }

    private int Deserialize(byte[] array, int pos, out float output)
    {
        unsafe
        {
            fixed (byte* pArray = array)
                output = *((float*)(pArray + pos));
        }
        return pos + sizeof(float);
    }


    unsafe private int Serialize(Vector2 input, byte[] array, int pos)
    {
        fixed (byte* pArray = array)
            *((Vector2*)(pArray + pos)) = input;
        return pos + sizeof(Vector2);
    }

    unsafe private int Deserialize(byte[] array, int pos, out Vector2 output)
    {
        fixed (byte* pArray = array)
            output = *((Vector2*)(pArray + pos));
        return pos + sizeof(Vector2);
    }


    unsafe private int Serialize(Vector3 input, byte[] array, int pos)
    {
        fixed (byte* pArray = array)
            *((Vector3*)(pArray + pos)) = input;
        return pos + sizeof(Vector3);
    }

    unsafe private int Deserialize(byte[] array, int pos, out Vector3 output)
    {
        fixed (byte* pArray = array)
            output = *((Vector3*)(pArray + pos));
        return pos + sizeof(Vector3);
    }


    private int Serialize(int input, byte[] array, int pos)
    {
        unsafe
        {
            fixed (byte* pArray = array)
                *((int*)(pArray + pos)) = input;
        }
        return pos + sizeof(int);
    }

    private int Deserialize(byte[] array, int pos, out int output)
    {
        unsafe
        {
            fixed (byte* pArray = array)
                output = *((int*)(pArray + pos));
        }
        return pos + sizeof(float);
    }


    private int Serialize(float[] input, byte[] array, int pos)
    {
        int n = sizeof(float) * input.Length;
        Buffer.BlockCopy(input, 0, array, pos, n);
        return pos + n;
    }

    private int Deserialize(byte[] array, int pos, float[] output)
    {
        int n = sizeof(float) * output.Length;
        Buffer.BlockCopy(array, pos, output, 0, n);
        return pos + n;
    }


    private int Serialize(int[] input, byte[] array, int pos)
    {
        int n = sizeof(int) * input.Length;
        Buffer.BlockCopy(input, 0, array, pos, n);
        return pos + n;
    }

    private int Deserialize(byte[] array, int pos, int[] output)
    {
        int n = sizeof(int) * output.Length;
        Buffer.BlockCopy(array, pos, output, 0, n);
        return pos + n;
    }


    unsafe private int Serialize(Vector2[] input, byte[] array, int pos)
    {
        int n = sizeof(Vector2) * input.Length;
        fixed (Vector2 *pIn = input)
        fixed (byte *pArr = array)
        {
            Buffer.MemoryCopy(pIn, pArr + pos, n, n);
        }
        return pos + n;
    }

    unsafe private int Deserialize(byte[] array, int pos, Vector2[] output)
    {
        int n = sizeof(Vector2) * output.Length;
        fixed (byte *pArr = array)
        fixed (Vector2 *pOut = output)
        {
            Buffer.MemoryCopy(pArr + pos, pOut, n, n);
        }
        return pos + n;
    }


    unsafe private int Serialize(Vector3[] input, byte[] array, int pos)
    {
        int n = sizeof(Vector3) * input.Length;
        fixed (Vector3* pIn = input)
        fixed (byte* pArr = array)
        {
            Buffer.MemoryCopy(pIn, pArr + pos, n, n);
        }
        return pos + n;
    }

    unsafe private int Deserialize(byte[] array, int pos, Vector3[] output)
    {
        int n = sizeof(Vector3) * output.Length;
        fixed (byte* pArr = array)
        fixed (Vector3* pOut = output)
        {
            Buffer.MemoryCopy(pArr + pos, pOut, n, n);
        }
        return pos + n;
    }
}
