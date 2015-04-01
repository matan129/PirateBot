// Decompiled with JetBrains decompiler
// Type: Britbot.ExpIterator
// Assembly: b1iry5ht, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E84BA16-E0F0-4D91-BADE-8A0B9BF81F38
// Assembly location: C:\Users\Matan\AppData\Local\Temp\b1iry5ht.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  internal class ExpIterator
  {
    public int[] Dimensions { get; private set; }

    public int[] Values { get; set; }

    public ExpIterator(int[] dims)
    {
      this.Dimensions = dims;
      if (Enumerable.Any<int>((IEnumerable<int>) this.Dimensions, (Func<int, bool>) (dim => dim <= 0)))
        throw new InvalidIteratorDimension("Dimensions must be strictly positive");
      this.Values = new int[dims.Length];
      for (int index = 0; index < this.Values.Length; ++index)
        this.Values[index] = 0;
    }

    public bool IsZero()
    {
      foreach (int num in this.Values)
      {
        if (num != 0)
          return false;
      }
      return true;
    }

    public bool NextIteration()
    {
      for (int index = 0; index < this.Values.Length; ++index)
      {
        if (this.Values[index] < this.Dimensions[index] - 1)
        {
          ++this.Values[index];
          break;
        }
        this.Values[index] = 0;
      }
      return !this.IsZero();
    }

    public override string ToString()
    {
      return string.Concat(new object[4]
      {
        (object) "Dimensions: ",
        (object) this.Dimensions,
        (object) "\nMultiIndex: ",
        (object) this.Values
      });
    }
  }
}
