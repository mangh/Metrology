/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;

namespace Mangh.Metrology
{
    /// <summary>
    /// Unit type definition.
    /// </summary>
    public class Unit : Measure
    {
        #region Properties
        /// <summary>
        /// Expression for the <see cref="Unit"/> dimension sense.
        /// </summary>
        public DimExpr Sense { get; }

        /// <summary>
        /// Expression for the <see cref="Unit"/> conversion factor.
        /// </summary>
        public NumExpr Factor { get; }

        /// <summary>
        /// Format for quantities (i.e. unit instances).
        /// </summary>
        /// <example>
        /// <c>"{0} {1}"</c> to have <c>273.15 DegCelsius</c> formatted as <c>"273.15 deg.C"</c>.
        /// </example>
        public string Format { get; }

        /// <summary>
        /// Unit symbol(s).
        /// </summary>
        public List<string> Tags { get; }

        /// <summary>
        /// Binary operations that refer, either in the argument(s) or return value, to other units (fellow units).
        /// </summary>
        public List<BinaryOperation> FellowOperations { get; private set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Unit"/> constructor.
        /// </summary>
        /// <param name="name">Unit name.</param>
        /// <param name="type">Underlying numeric type.</param>
        /// <param name="sense">Dimensional expression.</param>
        /// <param name="factor">Factor expression.</param>
        /// <param name="format">Formatting string.</param>
        /// <param name="tags">Unit symbol(s).</param>
        public Unit(string name,
                    Term type,
                    DimExpr sense,
                    NumExpr factor,
                    string format,
                    List<string> tags)
            : base(name, type)
        {
            Sense = sense;
            Factor = factor;
            Format = format;
            Tags = tags;

            FellowOperations = new List<BinaryOperation>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Append fellow <see cref="BinaryOperation"/> to the list of operations the unit participates in.
        /// </summary>
        /// <param name="ret">return value type</param>
        /// <param name="op">operator symbol</param>
        /// <param name="lhs">left-hand side value type</param>
        /// <param name="rhs">right-hand side value type</param>
        public void AddFellowOperation(Term ret, string op, Term lhs, Term rhs) =>
            FellowOperations.Add(new BinaryOperation(ret, op, lhs, rhs));
        #endregion

        #region Formatting
        /// <summary>
        /// Unit type info in a text form.
        /// </summary>
        /// <returns>Unit type stringified.</returns>
        public override string ToString()
        {
            string tags = (Tags.Count > 0) ? string.Format("\"{0}\"", string.Join("\", \"", Tags)) : string.Empty;
            return $"[{Sense}] {(Prime ?? this).TargetKeyword}::{TargetKeyword} {{{tags}}} : {Factor}";
        }
        #endregion
    }
}
