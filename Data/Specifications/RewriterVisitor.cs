using System.Linq.Expressions;

namespace RbacApi.Data.Specifications
{
    public class RewriterVisitor : ExpressionVisitor
    {
        private readonly Expression _from, _to;

        public RewriterVisitor(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _from ? _to : base.Visit(node);
        }
    }
}
