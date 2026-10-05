using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThread : MonoBehaviour
{
    static readonly Queue<Action> Queue = new Queue<Action>();
    static readonly List<Action> Drain = new List<Action>(16);

    public static void Run(Action action)
    {
        if (action == null) return;
        lock (Queue) Queue.Enqueue(action);
    }

    void Update()
    {
        Drain.Clear();
        lock (Queue)
        {
            while (Queue.Count > 0) Drain.Add(Queue.Dequeue());
        }

        for (int i = 0; i < Drain.Count; i++)
        {
            try { Drain[i]?.Invoke(); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}
