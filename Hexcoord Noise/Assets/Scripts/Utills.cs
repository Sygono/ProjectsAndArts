using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Utills
{

    // Array methods.
    public static T[] Slice<T>(this T[] source, int startIndex = 0, int length = 0)
    {
        if (length == 0&&startIndex != 0) {
            length = startIndex;
            startIndex = 0;
        }
        int sourceLen = source.Length;

        int a1 = (startIndex+sourceLen)%sourceLen;
        int a2 = (startIndex+length+sourceLen)%sourceLen;

        bool b = length>0==a1<=a2;

        int array1start = (b)?0 : Mathf.Min(a1, a2);
        int array1end = (b)?Mathf.Min(a1,a2) : Mathf.Max(a1, a2);
        int array2start = (b)?Mathf.Max(a1,a2) : sourceLen;
        int array2end = sourceLen;
        
        int sliceLen = sourceLen-Mathf.Abs(length);
        T[] slice = new T[sliceLen];
        int sliceIndex = 0;
        for (int i=array1start; i<array1end; i++) {
            slice[sliceIndex] = source[i];
            sliceIndex++;
        }
        for (int j=array2start; j<array2end; j++) {
            slice[sliceIndex] = source[j];
            sliceIndex++;
        }
        return slice;
    }

    public static int[] Add(this int[] array1, int[] array2, Func<int, int> lambda = null)
    {
        if (array1.Length != array2.Length) {
            Debug.Log("Length not match!");
            return null;
        }
        if (lambda == null) {
            lambda = (x)=>x;
        }
        int[] arr = (int[])array1.Clone();
        for (int i=0; i<arr.Length; i++) {
            arr[i] += lambda(array2[i]);
        }
        return arr;
    }

    public static T[] Concat<T>(this T[] array1, T[] array2)
    {
        int len1 = array1.Length;
        int len2 = array2.Length;
        T[] mergedArray = new T[len1 + len2];
        Array.Copy(array1, mergedArray, len1);
        Array.Copy(array2, 0, mergedArray, len1, len2);
        Array.Resize(ref mergedArray, len1 + len2);
        return mergedArray;
    }

    public static T[] Clone<T>(this T[] source) {
        T[] arr = new T[source.Length];
        Array.Copy(source, arr, source.Length);
        return arr;
    }

    public static T[] PaceSelect<T>(this T[] source, int amount, int pace, int index = 0) {
        List<T> lst = new List<T>();
        while(index<source.Length) {
            int count = 0;
            while(count<amount) {
                lst.Add(source[index]);
                count++;
                index++;
            }
            index+=pace-amount+1;
        }
        T[] arr = lst.ToArray();
        return arr;
    }

    public static int CoordToIndex(int[] coord, int[] limits) {
        if (limits.Length == 0) {
            return -1;
        }
        if (limits.Length == 1) {
            return coord[0];
        }
        return coord[0] + limits[0] * CoordToIndex(coord.Slice(1), limits.Slice(1));
    }

    public static int[] IndexToCoord(int index, int[] limits) {
        if (limits.Length == 0) {
            return null;
        }
        if (limits.Length == 1) {
            return new int[]{index};
        }
        int mul = ArrayMul(limits.Slice(-1));
        return IndexToCoord(index%mul,limits.Slice(-1)).Concat(new int[]{index/mul});
    }


    public static int[] OffestCoord(int[] coord, int index, int offset) {
        int[] arr = (int[])coord.Clone();
        arr[index] += offset;
        return arr;
    }

    public static int ArrayMul(int[] arr, int iteration = -1) {
        if(arr.Length == 0) {
            return 0;
        }
        if (iteration == -1) {
            iteration = arr.Length;
        }
        int n = 1;
        for (int i=0; i<iteration; i++) {
            n*=arr[i];
        }
        return n;
    }

    public static int[] ValidateCoord(int[] coord, int[] limits) {
        int[] arr = (int[])coord.Clone();
        for(int i=0; i<limits.Length; i++) {
            arr[i] = (arr[i]+limits[i])%limits[i];
        }
        return arr;
    }

    public static T CoordToValue<T> (T[] arr, int[] coord, int[] limits) {
        return arr[CoordToIndex(ValidateCoord(coord, limits), limits)];
    }

    public static void PrintArray<T>(T[] arr)
    {
        if (arr == null) {
            return;
        }
        string output = "{";

        for (int i = 0; i < arr.Length; i++)
        {
            output += arr[i].ToString();
            if (i < arr.Length - 1)
            {
                output += ", ";
            }
        }
        output += "}";
        Debug.Log(output);
    }

    public static string ToString(this byte[] byteArray) {
        char[] charArray = new char[byteArray.Length];
        for (int i = 0; i < byteArray.Length; i++)
        {
            charArray[i] = (char)byteArray[i];
        }
        string str = new string(charArray);
        return str;
    }

    public static int RandomByWeights(float[] weights) {
        float randomValue = UnityEngine.Random.Range(0, weights.Sum());
        float sum = 0;
        int index = 0;
        foreach (float f in weights) {
            sum += f;
            if (randomValue <= sum)
            {
                break;
            }
            index++;
        }
        return index;
    }

    public static List<int> GetNeighbors(int index, int[] limits) {
        int dim = limits.Length;
        List<int> neighbors = new List<int>();
        int[] coord = IndexToCoord(index, limits);
        for (int dir=0; dir<dim; dir++) {
            for (int offset=-1; offset<=1; offset+=2) {
                int[] neighborCoord = (int[])coord.Clone();
                neighborCoord[dir] += offset;
                if (neighborCoord[dir]>=0 && neighborCoord[dir]<limits[dir]) {
                    neighbors.Add(CoordToIndex(neighborCoord, limits));
                }
            }
        }
        return neighbors;
    }

    public static T NextOf<T>(this T[] array, T currentElement)
    {
        int currentIndex = Array.IndexOf(array, currentElement);
        int nextIndex = (currentIndex + 1) % array.Length;
        return array[nextIndex];
    }

    public static T PrevOf<T>(this T[] array, T currentElement)
    {
        int currentIndex = Array.IndexOf(array, currentElement);
        int nextIndex = (currentIndex - 1 + array.Length) % array.Length;
        return array[nextIndex];
    }





}
