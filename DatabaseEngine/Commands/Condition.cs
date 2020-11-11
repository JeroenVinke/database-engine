namespace DatabaseEngine
{
    public abstract class Condition
    {
        public virtual Condition Simplify()
        {
            if (this is AndCondition and)
            {
                Condition left = and.Left?.Simplify();
                Condition right = and.Right?.Simplify();

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
                return leaf;
            }

            return this;
        }
    }
}