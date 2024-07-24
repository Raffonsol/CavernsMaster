using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Util
{
    // Start is called before the first frame update
    public static float[] GetPositionPerType(RoomType type, int index)
    {
        float[] value = new float[]{0,0};
        switch (type) {
            case RoomType.DefaultRoom: {
                if (index == 0){value[0]=0f;value[1]=0f;}
                break;
            }
        }
        return value;
    }

}
public static class MyExtensions
{
    private static readonly System.Random rng = new System.Random();
    
    //Fisher - Yates shuffle
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    // https://stackoverflow.com/questions/3132126/how-do-i-select-a-random-value-from-an-enumeration
    public static Enum GetRandomEnumValue(this Type t)
    {
        return Enum.GetValues(t)          // get values from Type provided
            .OfType<Enum>()               // casts to Enum
            .OrderBy(e => Guid.NewGuid()) // mess with order of results
            .FirstOrDefault();            // take first item in result
    }
}