﻿// Decompiled with JetBrains decompiler
// Type: Britbot.ITarget
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

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
