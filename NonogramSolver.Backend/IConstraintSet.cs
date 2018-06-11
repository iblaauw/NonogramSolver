using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    public interface IConstraintSet : IReadOnlyList<Constraint>
    {
    }

    internal class ConstraintSet : IConstraintSet
    {
        private readonly List<Constraint> constraints;

        public ConstraintSet(IEnumerable<Constraint> constraints)
        {
            this.constraints = constraints.ToList();
        }

        public Constraint this[int index] => constraints[index];

        public int Count => constraints.Count;

        public IEnumerator<Constraint> GetEnumerator() => constraints.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => constraints.GetEnumerator();
    };
}
