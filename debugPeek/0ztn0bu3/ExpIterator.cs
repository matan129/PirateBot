// Decompiled with JetBrains decompiler
// Type: Britbot.ExpIterator
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

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
        (object) "dimensions: ",
        (object) this.Dimensions,
        (object) "\nMultiIndex: ",
        (object) this.Values
      });
    }
  }
}
