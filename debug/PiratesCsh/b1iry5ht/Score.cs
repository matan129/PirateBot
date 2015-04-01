// Decompiled with JetBrains decompiler
// Type: Britbot.Score
// Assembly: b1iry5ht, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E84BA16-E0F0-4D91-BADE-8A0B9BF81F38
// Assembly location: C:\Users\Matan\AppData\Local\Temp\b1iry5ht.dll

using System;

namespace Britbot
{
  public class Score : IComparable, IComparable<Score>
  {
    public double Eta;
    public ITarget Target;
    public TargetType Type;

    public double Value { get; private set; }

    public Score(ITarget target, TargetType type, double value, double eta)
    {
      this.Target = target;
      this.Type = type;
      this.Value = value;
      this.Eta = eta;
    }

    public int CompareTo(object obj)
    {
      Score other = obj as Score;
      if (other != null)
        return this.CompareTo(other);
      throw new ArgumentException("Object must a a Score in order to compare it with another score object");
    }

    public int CompareTo(Score other)
    {
      return (this.Value * 10.0 - this.Eta).CompareTo(other.Value * 10.0 - other.Eta);
    }
  }
}
