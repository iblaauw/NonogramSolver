using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramSolver.Backend
{
    public struct Constraint
    {
        public Constraint(uint color, uint number)
        {
            this.color = color;
            this.number = number;
        }

        public uint color;
        public uint number;

        public static bool operator ==(Constraint constr0, Constraint constr1)
        {
            return (constr0.color == constr1.color) && (constr0.number == constr1.number);
        }

        public static bool operator !=(Constraint constr0, Constraint constr1)
        {
            return (constr0.color != constr1.color) || (constr0.number != constr1.number);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Constraint))
            {
                Constraint constr = (Constraint)obj;
                return this == constr;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
