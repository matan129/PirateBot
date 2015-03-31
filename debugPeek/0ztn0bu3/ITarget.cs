// Decompiled with JetBrains decompiler
// Type: Britbot.ITarget
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

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
