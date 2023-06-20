/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

namespace %NAMESPACE%
{
    using Mangh.Metrology;
    using System;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Translation context for the <see cref="RuntimeLoader"/>.
    /// </summary>
    public abstract class TranslationContext : Mangh.Metrology.XML.TranslationContext
    {
        #region Properties

        ///////////////////////////////////////////////////////////////////////
        //
        //      Translation Settings
        //

        /// <summary>
        /// Target namespace for late units.
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// This MUST be the same namespace in which the compile-time units are defined; otherwise late units will not see them.
        /// </remarks>
        public override string TargetNamespace { get; }

        /// <summary>
        /// Cancellation notification.
        /// </summary>
        public override CancellationToken CancellationToken => CancellationToken.None;

        ///////////////////////////////////////////////////////////////////////
        //
        //      Directory paths
        //

        /// <summary>
        /// Path to the <see cref="RuntimeLoader"/> working (root) directory,<br/>
        /// from which the late definitions text file is to be read and the associated DLL library to be written<br/>
        /// (by default, this is taken from the <see cref="System.Reflection.Assembly.Location"/> property of type <see cref="Catalog"/>).
        /// </summary>
        public virtual string WorkDirectory { get; }

        /// <summary>
        /// Name of the templates subdirectory (in the <see cref="WorkDirectory"/>).
        /// </summary>
        public const string TEMPLATES = "Templates";

        /// <summary>
        /// Template directory path<br/>
        /// (defaults to the <see cref="WorkDirectory"/>/<see cref="TEMPLATES"/> subdirectory path).
        /// </summary>
        public virtual string TemplateDirectory { get; }

        /// <summary>
        /// Path to the directory for the (optional) dump of model &amp; source code files<br/>
        /// (defaults to the <see cref="TemplateDirectory"/> directory path).
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// The output assembly file (DLL) is not saved here.
        /// It is saved next to the late definitions file (in the same directory from which the definitions were loaded).
        /// </remarks>
        public virtual string DumpDirectory { get; }

        ///////////////////////////////////////////////////////////////////////
        //
        //      File paths
        //

        /// <summary>
        /// Scale template file path.
        /// </summary>
        public virtual string ScaleTemplateFilePath { get; }

        /// <summary>
        /// Unit template file path.
        /// </summary>
        public virtual string UnitTemplateFilePath { get; }

        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TranslationContext"/> constructor.
        /// </summary>
        /// <param name="targetNamespace">Target namespace for late units; use <see langword="null"/> to request a default value.</param>
        /// <param name="workDirectory">Path to the working (root) directory; use <see langword="null"/> to request a default value.</param>
        /// <param name="templateDirectory">Path to the template directory; use <see langword="null"/> to request a default value.</param>
        /// <param name="dumpDirectory">Path to the dump directory; use <see langword="null"/> to request a default value.</param>
        /// <remarks>
        /// NOTE:<br/>
        /// The constructor may throw exceptions when it cannot determine the <see cref="TargetNamespace"/><br/>
        /// and/or <see cref="WorkDirectory"/> properties (by default, these are taken from <see cref="Catalog"/> type properties,<br/>
        /// which may not be available on some platforms). To overcome this problem, i.e. to prevent searching for<br/>unavailable properties, pass them directly as (non-null) values in the constructor arguments.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="Mangh.Metrology.Language.Context"/> for the C# language is not available.</exception>
        /// <exception cref="InvalidOperationException">The namespace of compile-time units is not available.</exception>
        /// <exception cref="NotSupportedException"><see cref="System.Reflection.Assembly.Location"/> property is not supported.</exception>
        public TranslationContext(string? targetNamespace, string? workDirectory, string? templateDirectory, string? dumpDirectory)
            : base(Definitions.Contexts.First(c => c.Id == Mangh.Metrology.Language.ID.CS))
        {
            Type prototype = typeof(Catalog);

            TargetNamespace = 
                targetNamespace ?? 
                prototype.Namespace ??
                throw new InvalidOperationException("The namespace of compile-time units is not available.");

            WorkDirectory = workDirectory ??
                FilePath.GetDirectoryName(prototype.Assembly.Location);

            TemplateDirectory = templateDirectory ??
                FilePath.Combine(WorkDirectory, TEMPLATES);

            DumpDirectory = dumpDirectory ??
                TemplateDirectory;

            /*
             * It is recommended to save the translated late definitions in a DLL library (DumpOption.Assembly).
             * The next time you run it, RuntimeLoader will load the definitions from the previously created
             * DLL library, which is much faster than recompiling them.
             *
             * You can also dump the model (DumpOption.Model) and source (DumpOption.SourceCode) files;
             * these can help with debugging but are otherwise not needed.
             *
             */
            DumpOptions = DumpOption.Assembly
                        //| Mangh.Metrology.DumpOption.Model
                        //| Mangh.Metrology.DumpOption.SourceCode
                ;

            ScaleTemplateFilePath =
                FilePath.Combine(TemplateDirectory, ScaleTemplateFileName);

            UnitTemplateFilePath =
                FilePath.Combine(TemplateDirectory, UnitTemplateFileName);
        }
        #endregion
    }
}
