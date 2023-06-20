using Microsoft.CodeAnalysis.Text;
using System;

namespace RuntimeUnits
{
    internal class TranslationContext : Demo.UnitsOfMeasurement.TranslationContext
    {
        #region Properties

        ///////////////////////////////////////////////////////////////////////
        //
        //      Translation Settings
        //

        public ConsoleColor ReportColor { get; }
        public ConsoleColor ErrorColor { get; }

        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TranslationContext"/> constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Mangh.Metrology.Language.Context"/> for the C# language is not available.</exception>
        /// <exception cref="InvalidOperationException">The namespace of compile-time units is not available.</exception>
        /// <exception cref="NotSupportedException"><see cref="System.Reflection.Assembly.Location"/> property is not supported.</exception>
        public TranslationContext()
            : base(
                  targetNamespace: null,    // => typeof(Catalog).Namespace,
                  workDirectory: null,      // => Path.GetDirectoryName(typeof(Catalog).Assembly.Location),
                  templateDirectory: null,  // => workDirectory/Templates
                  dumpDirectory: null       // => templateDirectory
                )
        {
            /*
             * It is recommended to save the translated late definitions in a DLL library (DumpOption.Assembly);
             * the next time you run it, RuntimeLoader will load the definitions from the previously created DLL
             * library, which is much faster than recompiling them.
             *
             * You can also dump the model (DumpOption.Model) and source (DumpOption.SourceCode) files;
             * normally you don't need them, but they can help when debugging.
             *
             */
            DumpOptions = Mangh.Metrology.DumpOption.Assembly
                        | Mangh.Metrology.DumpOption.Model
                        | Mangh.Metrology.DumpOption.SourceCode
                ;

            // Colors:
            ReportColor = ConsoleColor.Green;
            ErrorColor = ConsoleColor.Red;
        }
        #endregion

        #region Methods

        ///////////////////////////////////////////////////////////////////////
        //
        //      Error logging
        //

        public override void Report(string path, string message, Exception? ex)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine($"{path}: {message}");
            if (ex is not null)
            {
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
            Console.ResetColor();
        }

        public override void Report(string path, TextSpan extent, LinePositionSpan span, string message, Exception? ex)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine($"{path}{LinePositionString(span)}: {message}");
            if (ex is not null)
            {
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
            Console.ResetColor();

            static string LinePositionString(LinePositionSpan lp) =>
                (!lp.Start.Equals(lp.End)) ? $", {lp}" :
                (!lp.Start.Equals(LinePosition.Zero)) ? $", ({lp.Start})" :
                // LinePosition.Zero actually means that
                // there is no information about the error position:
                string.Empty;
        }
        #endregion
    }
}
