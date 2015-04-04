// Decompiled with JetBrains decompiler
// Type: Britbot.NoTarget
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Pirates;

namespace Britbot
{
  internal class NoTarget : ITarget
  {
    public Score GetScore(Group origin)
    {
      return new Score((ITarget) this, TargetType.NoTarget, 0.0, 1.0);
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
