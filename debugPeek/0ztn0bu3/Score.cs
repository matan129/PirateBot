// Decompiled with JetBrains decompiler
// Type: Britbot.Score
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

using System;

namespace Britbot
{
  public class Score : IComparable, IComparable<Score>
  {
    public ITarget Target;
    public TargetType Type;
    public double Eta;

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
