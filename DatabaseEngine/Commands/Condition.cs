namespace DatabaseEngine
{
    public abstract class Condition
    {
        public bool SatisfiesCondition(CustomTuple tuple)
        {
            if (this is AndCondition andCondition)
            {
                return andCondition.Left.SatisfiesCondition(tuple) && andCondition.Right.SatisfiesCondition(tuple);
            }
            else if (this is OrCondition orCondition)
            {
                return orCondition.Left.SatisfiesCondition(tuple) || orCondition.Right.SatisfiesCondition(tuple);
            }
            else if (this is LeafCondition leafCondition)
            {
                if (leafCondition.AlwaysTrue)
                {
                    return true;
                }

                CustomObject value = tuple.GetEntryFor(leafCondition.Column);

                switch (leafCondition.Operation)
                {
                    case Compiler.Common.RelOp.Equals:
                        return value.IsEqualTo(leafCondition.Value);
                    case Compiler.Common.RelOp.GreaterThan:
                        return value.IsGreaterThan(leafCondition.Value);
                    case Compiler.Common.RelOp.GreaterOrEqualThan:
                        return value.IsGreaterThan(leafCondition.Value) || value.IsEqualTo(leafCondition.Value);
                    case Compiler.Common.RelOp.LessThan:
                        return !value.IsGreaterThan(leafCondition.Value) && ! value.IsEqualTo(leafCondition.Value);
                    case Compiler.Common.RelOp.LessOrEqualThan:
                        return !value.IsGreaterThan(leafCondition.Value);
                }
            }

            return false;
        }

        public virtual Condition Simplify()
        {
            if (this is AndCondition and)
            {
                Condition left = and.Left?.Simplify();
                Condition right = and.Right?.Simplify();

                if (left is LeafCondition leftLeaf && leftLeaf.AlwaysTrue)
                {
                    leftLeaf = null;
                }

                if (right is LeafCondition rightLeaf && rightLeaf.AlwaysTrue)
                {
                    rightLeaf = null;
                }

                if (left == null && right == null)
                {
                    return null;
                }
                else if (right == null)
                {
                    return left;
                }
                else if (left == null)
                {
                    return right;
                }
            }
            else if (this is LeafCondition leaf)
            {
                if (leaf.AlwaysTrue)
                {
                    return null;
                }

                return leaf;
            }

            return this;
        }
    }
}