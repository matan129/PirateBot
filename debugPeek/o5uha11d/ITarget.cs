// Decompiled with JetBrains decompiler
// Type: Britbot.ITarget
// Assembly: o5uha11d, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FD1E92EE-5B89-4A73-A517-70BE2E057BAD
// Assembly location: C:\Users\Matan\AppData\Local\Temp\o5uha11d.dll

using Pirates;

namespace Britbot
{
  public interface ITarget
  {
    Score GetScore(Group origin);

    Location GetLocation();

    Direction GetDirection(Group origin);

    bool Equals(ITarget operandB);

    string ToS();
  }
}
