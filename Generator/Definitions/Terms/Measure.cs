/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace Mangh.Metrology
{
    /// <summary>
    /// Base type for <see cref="Unit"/> and <see cref="Scale"/> types.
    /// </summary>
    public abstract class Measure : Term
    {
        #region Properties
        /// <summary>
        /// The numeric type underlying the <see cref="Measure"/> arithmetic.
        /// </summary>
        public Term NumericType { get; }

        /// <summary>
        /// A reference to the (next) relative.
        /// </summary>
        /// <remarks>
        /// NOTE:
        /// Subsequent items linked by the property make a <i>family</i>, i.e. a list <i>F</i> of relatives such that:<br/>
        /// - the list is <i>cyclic</i>: <c>∀r∈F: r.Relative[.Relative[.Relative[...]]] == r</c>,<br/>
        /// - the list contains <i>this</i>: <c>∃r∈F: r == this</c>,<br/>
        /// - items in the list are <i>unique</i>: <c>∀i,j∈I, i ≠ j: r[i] ≠ r[j]</c>,<br/>
        /// - there is exactly one <i>prime</i>: <c>∃!p∈F: p.Prime == null</c>,<br/>
        /// - all other items <i>reference</i> that <i>prime</i>: <c>∀r∈F, r ≠ p: r.Prime == p</c>.
        /// </remarks>
        public Measure Relative { get; private set; }

        /// <summary>
        /// Reference to the primary measure (<see langword="null"/> if it is primary itself).
        /// </summary>
        public Measure? Prime { get; private set; }

        /// <summary>
        /// Family Id.
        /// </summary>
        public int Family { get; private set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Measure"/> constructor.
        /// </summary>
        /// <param name="name">Name of the <see cref="Measure"/>.</param>
        /// <param name="numericType">Numeric type that implements the <see cref="Measure"/> structure.</param>
        /// <remarks>
        /// A newly created <see cref="Measure"/> is a one-element family the other measures can join in.<br/>
        /// It can also join (along with its relatives) in the family of other measures.
        /// </remarks>
        protected Measure(string name, Term numericType) :
            base(sourceKeyword: name, targetKeyword: name)
        {
            NumericType = numericType;
            Prime = null;       // No prime entity yet
            Relative = this;    // No relatives yet (except itself)
            Family = -1;        // No valid family id yet (to be set after the entity build is completed).
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add another <see cref="Measure"/> (along with its relatives) to the family.
        /// </summary>
        /// <param name="other">Another <see cref="Measure"/> that is to join in the family.</param>
        public void AddRelative(Measure other)
        {
            // Do nothing if the other is already among relatives
            if (!Relatives().Any(r => ReferenceEquals(other, r)) && !ReferenceEquals(other, this))
            {
                Measure? thisRelative = Relative;
                Relative = other;
                other.Relative = thisRelative;
                other.Prime = Prime ?? this;
            }
        }

        /// <summary>
        /// Returns enumerator of all relatives (except itself).
        /// </summary>
        public IEnumerable<Measure> Relatives()
        {
            Measure next = Relative;
            while (!ReferenceEquals(next, this))
            {
                yield return next;
                next = next.Relative;
            }
        }

        /// <summary>
        /// Set family id (i.e. family membership).
        /// </summary>
        /// <param name="family"></param>
        public void SetFamily(int family)
            => Family = family;

        #endregion
    }
}
