using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonogramSolver.Backend;

namespace NonogramSolver.Tests
{
    interface IConstraintHelper
    {
        IConstraintSet ToSet();
    }

    class ConstraintHelper : IConstraintHelper
    {
        public readonly Constraint constraint;
        public ConstraintHelper(uint color)
        {
            constraint = new Constraint(color, 1);
        }

        public IConstraintSet ToSet()
        {
            return BoardFactory.CreateConstraintSet(new[] { constraint });
        }

        private ConstraintHelper(uint color, uint number)
        {
            constraint = new Constraint(color, number);
        }

        public static ConstraintHelper operator*(uint val, ConstraintHelper helper)
        {
            return new ConstraintHelper(helper.constraint.color, helper.constraint.number * val);
        }

        public static ConstraintHelper operator*(ConstraintHelper helper, uint val)
        {
            return val * helper;
        }

        public static ConstraintHelperList operator+(ConstraintHelper h1, ConstraintHelper h2)
        {
            return new ConstraintHelperList(new[] { h1.constraint, h2.constraint });
        }

    }

    class ConstraintHelperList : IConstraintHelper
    {
        private readonly IReadOnlyList<Constraint> constraints;
        public ConstraintHelperList(IReadOnlyList<Constraint> constraints)
        {
            this.constraints = constraints;
        }

        public IConstraintSet ToSet()
        {
            return BoardFactory.CreateConstraintSet(constraints);
        }

        public static ConstraintHelperList operator+(ConstraintHelperList list, ConstraintHelper helper)
        {
            var clone = list.constraints.ToList();
            clone.Add(helper.constraint);
            return new ConstraintHelperList(clone);
        }

        public static ConstraintHelperList operator+(ConstraintHelper helper, ConstraintHelperList list)
        {
            return (list + helper);
        }
        
        public static ConstraintHelperList operator+(ConstraintHelperList l1, ConstraintHelperList l2)
        {
            var clone = l1.constraints.ToList();
            clone.AddRange(l2.constraints);
            return new ConstraintHelperList(clone);
        }
    }
}
