// Decompiled with JetBrains decompiler
// Type: Pirates.PiratesRunner
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CSharp;

namespace Pirates
{
    internal class PiratesRunner
    {
        private PiratesRunner(string path)
        {
            Bot = InitializeBotFromFile(path);
        }

        private IPirateBot Bot { get; set; }

        public static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.Error.WriteLine("USAGE: This exe should get a 'bot' path as a single parameter");
            }
            else
            {
                var path = args[0];
                if (!File.Exists(path) & !Directory.Exists(path))
                {
                    Console.Error.WriteLine("File [{0}] not found. Are you sure the filename is correct?", path);
                }
                else
                {
                    try
                    {
                        new PiratesRunner(path).Run();
                    }
                    catch (CompilationErrorException ex)
                    {
                        Console.Error.WriteLine("Failure while running bot from [{0}]: Please fix compilation errors",
                            path);
                    }
                    catch (NoClassInFileException ex)
                    {
                        Console.Error.WriteLine(
                            "Failure while running bot from [{0}]: No class found in file. Did you just copy the DoTurn without the Bot class?",
                            path);
                    }
                    catch (InterfaceNotFoundException ex)
                    {
                        Console.Error.WriteLine(
                            "Failure while running bot from [{0}]: No IPirateBot found in file. Are you sure you inherited from IPirateBot interface?",
                            path);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Failure while running bot from [{0}]: {1}", path, ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        private IPirateBot InitializeBotFromFile(string path)
        {
            using (var csharpCodeProvider = new CSharpCodeProvider())
            {
                var assembly = typeof (IPirateBot).Assembly;
                var options = new CompilerParameters();
                options.GenerateInMemory = false;
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Core.dll");
                options.ReferencedAssemblies.Add(assembly.Location);
                options.IncludeDebugInformation = true;
                CompilerResults compilerResults;
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
                    compilerResults = csharpCodeProvider.CompileAssemblyFromFile(options, files);
                }
                else
                    compilerResults = csharpCodeProvider.CompileAssemblyFromFile(options, path);
                if (compilerResults.Errors.Count > 0 && compilerResults.Errors.HasErrors)
                {
                    Console.Error.WriteLine("Compilation Errors:");
                    var flag = false;
                    foreach (CompilerError compilerError in compilerResults.Errors)
                    {
                        if (compilerError.ErrorNumber.Equals("CS1518"))
                            flag = true;
                        Console.Error.WriteLine(compilerError.ToString());
                    }
                    if (flag)
                        throw new NoClassInFileException();
                    throw new CompilationErrorException();
                }
                var type1 =
                    compilerResults.CompiledAssembly.GetTypes()
                        .FirstOrDefault(type => type.GetInterface("IPirateBot") != (Type) null);
                if (
                    compilerResults.CompiledAssembly.GetTypes()
                        .Count(type => type.GetInterface("IPirateBot") != (Type) null) > 1)
                    Console.Error.WriteLine("WARNING: Found multiple bots. Loading {0}", type1.FullName);
                if (type1 != null)
                    return (IPirateBot) compilerResults.CompiledAssembly.CreateInstance(type1.FullName);
                throw new InterfaceNotFoundException();
            }
        }

        private void Run()
        {
            PirateGame pirateGame = null;
            var str1 = "";
            while (true)
            {
                try
                {
                    var str2 = Console.ReadLine().TrimEnd('\r', '\n');
                    if (str2.ToLower() == "ready")
                    {
                        pirateGame = new PirateGame(str1);
                        pirateGame.FinishTurn();
                        str1 = "";
                    }
                    else if (str2.ToLower() == "go")
                    {
                        pirateGame.Update(str1);
                        Bot.DoTurn(pirateGame);
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
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                    Console.Error.Flush();
                }
            }
        }
    }
}