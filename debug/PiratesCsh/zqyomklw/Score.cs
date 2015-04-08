// Decompiled with JetBrains decompiler
// Type: Britbot.Score
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using System;

namespace Britbot
{
  public class Score : IComparable, IComparable<Score>
  {
    public double EnemyShips;
    public double Eta;
    public ITarget Target;
    public TargetType Type;

    public double Value { get; private set; }

    public Score(ITarget target, TargetType type, double value, double EnemyShips, double eta)
    {
      this.Target = target;
      this.Type = type;
      this.Value = value;
      this.Eta = eta;
      this.EnemyShips = EnemyShips;
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

    public override string ToString()
    {
      return "Target: " + (object) this.Target.ToString() + " value: " + (string) (object) this.Value + " Enemy: " + (string) (object) this.EnemyShips + " ETA: " + (string) (object) this.Eta;
    }
  }
}
