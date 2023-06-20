/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mangh.Metrology
{
    /// <summary>
    /// <see cref="Unit"/> and <see cref="Scale"/> type definitions.
    /// </summary>
    public class Definitions : TextFile
    {
        #region Statics
        /// <summary>
        /// Target language contexts available.
        /// </summary>
        public static IEnumerable<Language.Context> Contexts => new Language.Context[]
        {
            new Language.ContextCS(),
            new Language.ContextCPP()
        };
        #endregion

        #region Fields
        /// <summary>
        /// Family Id (for the next family found).
        /// </summary>
        private int _family = 0;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="Unit"/> collection.
        /// </summary>
        public List<Unit> Units { get; }

        /// <summary>
        /// <see cref="Scale"/> collection.
        /// </summary>
        public List<Scale> Scales { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Definitions"/> constructor.
        /// </summary>
        /// <param name="path">Path to the definitions file.</param>
        /// <param name="context">Translation context.</param>
        public Definitions(string path, TranslationContext context)
            : base(path, context, useAsync: false)
        {
            Units = new List<Unit>();
            Scales = new List<Scale>();
        }
        #endregion

        #region Family method(s)
        /// <summary>
        /// Assign a family to a <see cref="Measure"/> object (sets the measure's family ID).
        /// </summary>
        /// <param name="m">The measure to which we want assign a family.</param>
        public void AssignFamilyTo(Measure m)
            => m.SetFamily(m.Prime is null ? _family++ : m.Prime.Family);

        /// <summary>
        /// Restore the measure's family membership (i.e. restore the former value of the family ID).
        /// </summary>
        /// <param name="m">The measure that rejoins the family.</param>
        /// <param name="former">Former family ID (e.g. the one set at build time, being restored at run time).</param>
        public void RestoreFamilyFor(Measure m, int former)
        {
            m.SetFamily(former);
            if (former >= _family) _family = former + 1;
        }

        /// <summary>
        /// Returns the number of families of the selected type <typeparamref name="M"/>.
        /// </summary>
        /// <typeparam name="M"><see cref="Unit"/>, <see cref="Scale"/> or <see cref="Measure"/>.</typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OverflowException"></exception>
        public int FamilyCount<M>(IEnumerable<M> collection) where M : Measure
            => collection.Select(m => m.Family).Distinct().Count();
        #endregion

        #region Load method(s)
        /// <summary>
        /// Loads <see cref="Definitions"/> from a text <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Definition text stream.</param>
        /// <returns><see langword="true"/> on successful load, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// NOTE:<br/>
        /// The <paramref name="stream"/> remains open when the method returns.<br/>
        /// Disposal (as well as creation) of the stream is the responsibility of the caller.
        /// </remarks>
        public override bool Load(Stream stream)
        {
            try
            {
                // It is the caller who decides when the stream is to be closed (leaveOpen: true):
                using StreamReader reader = new(stream, _encoding, detectEncodingFromByteOrderMarks: true, _bufferSize, leaveOpen: true);
                Parser parser = new(reader, this);
                return parser.Parse(Context.CancellationToken);
            }
            catch (ArgumentException ex) { ReportError(ex); }
            catch (InvalidOperationException ex) { ReportError(ex); }
            catch (IOException ex) { ReportError(ex); }
            catch (NotSupportedException ex) { ReportError(ex); }

            return false;

            void ReportError(Exception ex) =>
                Context.Report(Path, "definitions could not be read.", ex);
        }
        #endregion
    }
}
