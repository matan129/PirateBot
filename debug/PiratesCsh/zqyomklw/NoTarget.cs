// Decompiled with JetBrains decompiler
// Type: Britbot.NoTarget
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;

namespace Britbot
{
  internal class NoTarget : ITarget
  {
    public Score GetScore(Group origin)
    {
      return new Score((ITarget) this, TargetType.NoTarget, 0.0, 0.0, 1.0);
    }

    public Location GetLocation()
    {
      return new Location(0, 0);
    }

    public Direction GetDirection(Group origin)
    {
      return (Direction) 45;
    }

    public bool Equals(ITarget operandB)
    {
      return operandB is NoTarget;
    }

    public TargetType GetTargetType()
    {
      return TargetType.NoTarget;
    }

    public string GetDescription()
    {
      return "NoTarget. Nothing interesting here";
    }
  }
}
