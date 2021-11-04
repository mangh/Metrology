/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;

namespace Mangh.Metrology
{
    /// <summary>
    /// Unit type info.
    /// </summary>
    public class UnitType : MeasureType
    {
        #region Properties
        /// <summary>
        /// Expression for the unit&apos; dimension (sense).
        /// </summary>
        public DimensionalExpression Sense { get; }

        /// <summary>
        /// Expression for the unit&apos; factor.
        /// </summary>
        public NumeralExpression Factor { get; }

        /// <summary>
        /// Expression for a One (1) in the underlying numeral type.
        /// </summary>
        public NumeralExpression One { get; }

        /// <summary>
        /// Expression for a Zero (0) in the underlying numeral type.
        /// </summary>
        public NumeralExpression Zero { get; }

        /// <summary>
        /// Format for quantities (i.e. unit&apos; instances).
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
        /// Binary operations, returning values of another (outer) unit type.
        /// </summary>
        public List<BinaryOperation> OuterOperations { get; private set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Unit type constructor.
        /// </summary>
        /// <param name="name">unit tyoe name</param>
        /// <param name="sense">expression for unit&apos; dimension</param>
        /// <param name="factor">expression for unit&apos; factor</param>
        /// <param name="one">expression for a One (1) in the underlying numeral type</param>
        /// <param name="zero">expression for a Zero (0) in the underlying numeral type</param>
        /// <param name="format">unit&apos; format</param>
        /// <param name="tags">unit&apos; symbol(s)</param>
        public UnitType(string name,
                        DimensionalExpression sense,
                        NumeralExpression factor,
                        NumeralExpression one,
                        NumeralExpression zero,
                        string format,
                        List<string> tags) :
            base(name)
        {
            Sense = sense;
            Factor = factor;
            One = one;
            Zero = zero;
            Format = format;
            Tags = tags;

            OuterOperations = new List<BinaryOperation>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Append <see cref="BinaryOperation"/> to the list of operations (binary relationships) the unit participates in.
        /// </summary>
        /// <param name="ret">return value type</param>
        /// <param name="op">operator symbol</param>
        /// <param name="lhs">left-hand side value type</param>
        /// <param name="rhs">right-hand side value type</param>
        public void AddOuterOperation(AbstractType ret, string op, AbstractType lhs, AbstractType rhs) =>
            OuterOperations.Add(new BinaryOperation(ret, op, lhs, rhs));
        #endregion

        #region Formatting
        /// <summary>
        /// Unit type info in a text form.
        /// </summary>
        /// <returns>UnitType stringified.</returns>
        public override string ToString()
        {
            string tags = (Tags.Count > 0) ? string.Format("\"{0}\"", string.Join("\", \"", Tags)) : string.Empty;
            return $"[{Sense}] {(Prime ?? this).Typename}::{Typename} {{{tags}}} : {Factor}";
        }
        #endregion
    }
}
