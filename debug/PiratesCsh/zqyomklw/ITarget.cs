// Decompiled with JetBrains decompiler
// Type: Britbot.ITarget
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;

namespace Britbot
{
  public interface ITarget
  {
    Score GetScore(Group origin);

    Location GetLocation();

    Direction GetDirection(Group origin);

    bool Equals(ITarget operandB);

    TargetType GetTargetType();

    string GetDescription();
  }
}
