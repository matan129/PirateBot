// Decompiled with JetBrains decompiler
// Type: Britbot.ITarget
// Assembly: b1iry5ht, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E84BA16-E0F0-4D91-BADE-8A0B9BF81F38
// Assembly location: C:\Users\Matan\AppData\Local\Temp\b1iry5ht.dll

using Pirates;

namespace Britbot
{
  public interface ITarget
  {
    Score GetScore(Group origin);

    Location GetLocation();

    Direction GetDirection(Group origin);

    bool Equals(ITarget operandB);

    string GetDescription();
  }
}
