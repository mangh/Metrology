/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Man.Metrology.XsltGeneratorApp
{
    internal class Program
    {
        private static string? targetNamespace;
        private static string? templateFolder;
        private static string? targetFolder;

        private static int Main(string[] args)
        {
            Console.WriteLine("Units of Measurement for C# applications. Copyright (©) MAN.");
            Console.WriteLine("This program is provided to you under the terms of the license");
            Console.WriteLine("as published at https://github.com/mangh/Metrology.");
            Console.WriteLine();
            Console.WriteLine($"Unit and Scale Generator, Version: {typeof(Program).Assembly.GetName().Version}");
            Console.WriteLine();

            if (CheckArguments(args))
            {
                Generator gtor = new(targetNamespace!, templateFolder!, targetFolder!);

                if (gtor.LoadDefinitions("definitions.txt") &&
                    gtor.MakeUnits("unit.xslt") &&
                    gtor.MakeScales("scale.xslt") &&
                    gtor.MakeCatalog("catalog.xslt") &&
                    gtor.MakeAliases("aliases.xslt", global: true) &&
                    gtor.MakeReport("report.xslt"))
                {
                    Console.WriteLine("Succeeded:");
                    Console.WriteLine($"  {gtor.Units.Count} unit(s), {gtor.UnitFamilyCount} family(ies),");
                    Console.WriteLine($"  {gtor.Scales.Count} scale(s), {gtor.ScaleFamilyCount} family(ies),");
                    Console.WriteLine($"  in namespace \"{gtor.TargetNamespace}\".");
                    return 0;
                }
            }
            Console.WriteLine();
            Console.WriteLine("Generation failed: errors have occurred.");
            return 1;
        }

        private static bool CheckArguments(string[] args)
        {
            if (args.Length > 0)
            {
                targetNamespace = args[0];
                if (args.Length > 1)
                {
                    templateFolder = args[1];
                    targetFolder = (args.Length > 2) ? args[2] : templateFolder;
                }
                else
                {
                    targetFolder = templateFolder = Directory.GetCurrentDirectory();
                }
                return true;
            }

            Console.WriteLine("Missing or invalid command line arguments");
            Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} targetNamespace [templateFolder [targetFolder]]");
            return false;
        }
    }
}
