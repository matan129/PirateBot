// Decompiled with JetBrains decompiler
// Type: Britbot.PriorityQueue.IPriorityQueue`1
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using System.Collections;
using System.Collections.Generic;

namespace Britbot.PriorityQueue
{
  public interface IPriorityQueue<T> : IEnumerable<T>, IEnumerable where T : PriorityQueueNode
  {
    T First { get; }

    int Count { get; }

    int MaxSize { get; }

    void Remove(T node);

    void UpdatePriority(T node, double priority);

    void Enqueue(T node, double priority);

    T Dequeue();

    void Clear();

    bool Contains(T node);
  }
}
