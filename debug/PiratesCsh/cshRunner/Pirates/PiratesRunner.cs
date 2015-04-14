// Decompiled with JetBrains decompiler
// Type: Pirates.PiratesRunner
// Assembly: PiratesCsh, Version=1.0.5581.42591, Culture=neutral, PublicKeyToken=null
// MVID: F9F1F072-EFD6-461C-A5E1-7E4E5CE853F7
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pirates
{
  internal class PiratesRunner
  {
    private IPirateBot Bot { get; set; }

    private PiratesRunner(string path)
    {
      this.Bot = this.InitializeBotFromFile(path);
    }

    public static void Main(string[] args)
    {
      if (Enumerable.Count<string>((IEnumerable<string>) args) == 0)
      {
        Console.Error.WriteLine("USAGE: This exe should get a 'bot' path as a single parameter");
      }
      else
      {
        string path = args[0];
        if (!File.Exists(path) & !Directory.Exists(path))
        {
          Console.Error.WriteLine(string.Format("File [{0}] not found. Are you sure the filename is correct?", (object) path));
        }
        else
        {
          try
          {
            new PiratesRunner(path).Run();
          }
          catch (CompilationErrorException ex)
          {
            Console.Error.WriteLine(string.Format("Failure while running bot from [{0}]: Please fix compilation errors", (object) path));
          }
          catch (NoClassInFileException ex)
          {
            Console.Error.WriteLine(string.Format("Failure while running bot from [{0}]: No class found in file. Did you just copy the DoTurn without the Bot class?", (object) path));
          }
          catch (InterfaceNotFoundException ex)
          {
            Console.Error.WriteLine(string.Format("Failure while running bot from [{0}]: No IPirateBot found in file. Are you sure you inherited from IPirateBot interface?", (object) path));
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(string.Format("Failure while running bot from [{0}]: {1}", (object) path, (object) ex.Message));
            Console.Error.WriteLine(ex.StackTrace);
          }
        }
      }
    }

    private IPirateBot InitializeBotFromFile(string path)
    {
      using (CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider())
      {
        Assembly assembly = typeof (IPirateBot).Assembly;
        CompilerParameters options = new CompilerParameters();
        options.GenerateInMemory = false;
        options.ReferencedAssemblies.Add("System.dll");
        options.ReferencedAssemblies.Add("System.Core.dll");
        options.ReferencedAssemblies.Add(assembly.Location);
        options.IncludeDebugInformation = true;
        CompilerResults compilerResults;
        if (Directory.Exists(path))
        {
          string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
          compilerResults = csharpCodeProvider.CompileAssemblyFromFile(options, files);
        }
        else
          compilerResults = csharpCodeProvider.CompileAssemblyFromFile(options, path);
        if (compilerResults.Errors.Count > 0 && compilerResults.Errors.HasErrors)
        {
          Console.Error.WriteLine("Compilation Errors:");
          bool flag = false;
          foreach (CompilerError compilerError in (CollectionBase) compilerResults.Errors)
          {
            if (compilerError.ErrorNumber.Equals("CS1518"))
              flag = true;
            Console.Error.WriteLine(compilerError.ToString());
          }
          if (flag)
            throw new NoClassInFileException();
          throw new CompilationErrorException();
        }
        Type type1 = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>) compilerResults.CompiledAssembly.GetTypes(), (Func<Type, bool>) (type => type.GetInterface("IPirateBot") != (Type) null)));
        if (Enumerable.Count<Type>(Enumerable.Where<Type>((IEnumerable<Type>) compilerResults.CompiledAssembly.GetTypes(), (Func<Type, bool>) (type => type.GetInterface("IPirateBot") != (Type) null))) > 1)
          Console.Error.WriteLine(string.Format("WARNING: Found multiple bots. Loading {0}", (object) type1.FullName));
        if (type1 != (Type) null)
          return (IPirateBot) compilerResults.CompiledAssembly.CreateInstance(type1.FullName);
        throw new InterfaceNotFoundException();
      }
    }

    private void Run()
    {
      PirateGame pirateGame = (PirateGame) null;
      string str1 = "";
      while (true)
      {
        try
        {
          string str2 = Console.ReadLine().TrimEnd('\r', '\n');
          if (str2.ToLower() == "ready")
          {
            pirateGame = new PirateGame(str1);
            pirateGame.FinishTurn();
            str1 = "";
          }
          else if (str2.ToLower() == "go")
          {
            pirateGame.Update(str1);
            this.Bot.DoTurn((IPirateGame) pirateGame);
            pirateGame.CancelCollisions();
            pirateGame.FinishTurn();
            str1 = "";
          }
          else
            str1 = str1 + str2 + "\n";
        }
        catch (IOException ex)
        {
          break;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex.Message.ToString());
          Console.Error.WriteLine(ex.StackTrace);
          Console.Error.Flush();
        }
      }
    }
  }
}
