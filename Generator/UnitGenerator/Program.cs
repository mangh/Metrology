/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using System.Linq;

using static System.Console;

namespace Mangh.Metrology.UnitGenerator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            WriteLine("Units of Measurement for C# and C++ applications.");
            WriteLine("Copyright (©) MAN. This program is provided to you under the terms");
            WriteLine("of the license as published at https://github.com/mangh/Metrology.");
            WriteLine();
            WriteLine($"Unit and Scale Generator, Version: {typeof(Program).Assembly.GetName().Version}");
            WriteLine();

            TranslationContext? tc = GetTranslationContext(args);
            if (tc is not null)
            {
                Generator gtor = new(tc);
                if (gtor.Execute())
                {
                    WriteLine($"DONE (for {tc.Language.Id} language):");
                    WriteLine($"  * {gtor.Units.Count} unit(s) in {gtor.FamilyCount(gtor.Units)} family(ies),");
                    WriteLine($"  * {gtor.Scales.Count} scale(s) in {gtor.FamilyCount(gtor.Scales)} family(ies),");
                    WriteLine($"  * in \"{tc.TargetNamespace}\" namespace.");
                    return 0;
                }
            }
            else
            {
                WriteLine("Missing or invalid command line arguments");
                WriteLine();
                WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} targetLanguage targetNamespace templateFolder targetFolder [--with-models]");
                WriteLine();
                WriteLine($"  targetLanguage  : {string.Join(" | ", Definitions.Contexts.Select(c => c.Id))}.");
                WriteLine();
                WriteLine("  targetNamespace : namespace string, e.g.:");
                WriteLine("                    \"Demo.Metrology\" for CS language,");
                WriteLine("                    \"Demo::Metrology\" for CPP language,");
                WriteLine();
                WriteLine("  templateFolder  : path to the folder containing definitions and templates,");
                WriteLine();
                WriteLine("  targetFolder    : path to the target folder for generated units and scales,");
                WriteLine();
                WriteLine("  --with-models   : request to save intermediate models in files (in");
                WriteLine("                    the targetFolder)");
            }

            WriteLine();
            WriteLine("FAILED: errors have occurred.");
            WriteLine();
            return 1;
        }

        private static TranslationContext? GetTranslationContext(string[] args)
        {
            if (args.Length > 0)
            {
                string targetLang = args[0].ToUpper();

                Language.Context? targetContext = 
                    Definitions.Contexts.FirstOrDefault(c => c.Id.ToString() == targetLang);

                if ((targetContext is not null) && (args.Length > 3))
                {
                    TranslationContext tc = new(
                        targetContext,
                        targetNamespace: args[1],
                        templateDirectory: args[2],
                        targetDirectory: args[3]
                    );

                    if (args.Length <= 4)
                    {
                        return tc;
                    }
                    else if (args[4].Equals("--with-models", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        tc.DumpOptions = DumpOption.Model;
                        return tc;
                    }
                }
            }
            return null;
        }
    }
}