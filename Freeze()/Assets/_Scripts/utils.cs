using System;
using System.Collections.Generic;
using UnityEngine;

public static class utils {
    public static int ManhattanDistance(Vector3Int position, Vector3Int ghostPos) {
        return Math.Abs(position.x - ghostPos.x) + Math.Abs(position.y - ghostPos.y) + Math.Abs(position.z - ghostPos.z);
    }

    public static float ManhattanDistance(Vector3 position, Vector3 ghostPos) {
        return Math.Abs(position.x - ghostPos.x) + Math.Abs(position.y - ghostPos.y) + Math.Abs(position.z - ghostPos.z);
    }

    public static float MazeDistance(Vector3Int point1, Vector3Int point2, Level level)
    {
        return 0;
    }

}

public class PriorityQueue<T>
{
    private List<Tuple<int, long, T>> heap;
    private long count;

    public PriorityQueue()
    {
        heap = new List<Tuple<int, long, T>>();
        count = 0;
    }

    public void Enqueue(T item, int priority)
    {
        var entry = new Tuple<int, long, T>(priority, count, item);
        heap.Add(entry);
        count++;
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (Compare(heap[i], heap[parent]) < 0)
            {
                Swap(i, parent);
                i = parent;
            }
            else
            {
                break;
            }
        }
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("The priority queue is empty.");
        }

        var entry = heap[0];
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        int i = 0;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;
            if (left < heap.Count && Compare(heap[left], heap[smallest]) < 0)
            {
                smallest = left;
            }
            if (right < heap.Count && Compare(heap[right], heap[smallest]) < 0)
            {
                smallest = right;
            }
            if (smallest == i)
            {
                break;
            }
            Swap(i, smallest);
            i = smallest;
        }
        return entry.Item3;
    }

    public bool IsEmpty()
    {
        return heap.Count == 0;
    }

    public void Update(T item, int priority)
    {
        for (int i = 0; i < heap.Count; i++)
        {
            if (Equals(heap[i].Item3, item))
            {
                if (heap[i].Item1 <= priority)
                {
                    break;
                }
                heap.RemoveAt(i);
                Enqueue(item, priority);
                break;
            }
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }

    private int Compare(Tuple<int, long, T> a, Tuple<int, long, T> b)
    {
        if (a.Item1 < b.Item1)
        {
            return -1;
        }
        if (a.Item1 > b.Item1)
        {
            return 1;
        }
        if (a.Item2 < b.Item2)
        {
            return -1;
        }
        if (a.Item2 > b.Item2)
        {
            return 1;
        }
        return 0;
    }
}
