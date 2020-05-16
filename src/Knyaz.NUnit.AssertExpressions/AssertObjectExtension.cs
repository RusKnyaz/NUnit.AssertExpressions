using System;
using System.Linq;
using System.Linq.Expressions;

namespace Knyaz.NUnit.AssertExpressions
{
	/// <summary> Provides 'Assert' extension method for the object. </summary>
	public static class AssertObjectExtension
	{
		/// <summary> Verifies object's properties. </summary>
		/// <param name="obj">The object to be verified</param>
		/// <param name="predicate">Success condition</param>
		public static void Assert<T>(this T obj, Expression<Func<T, bool>> predicate)
		{
			try
			{
				Assert(obj, predicate, predicate.Body);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private static void Assert<T>(T obj, Expression<Func<T, bool>> parent, Expression expression)
		{
			switch (expression)
			{
				case BinaryExpression binary:
					Assert(obj, parent, binary);
					break;
				case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
				{
					var result = Invoke(obj, parent, unary.Operand);
					global::NUnit.Framework.Assert.IsFalse((bool)result, GetMessage(unary.Operand));
					break;
				}
				default:
				{
					var result = Invoke(obj, parent, expression);
					global::NUnit.Framework.Assert.IsTrue((bool)result, GetMessage(expression));
					break;
				}
			}
		}

		private static void Assert<T>(T obj, Expression<Func<T, bool>> parent, BinaryExpression expression)
		{
			if (expression.NodeType == ExpressionType.AndAlso)
			{
				Assert(obj, parent, expression.Left);
				Assert(obj, parent, expression.Right);
				return;
			}

			var leftResult = Invoke(obj, parent, expression.Left);
			var rightResult = Invoke(obj, parent, expression.Right);

			switch (expression.NodeType)
			{
				case ExpressionType.Equal:
					global::NUnit.Framework.Assert.AreEqual(rightResult, leftResult, GetMessage(expression.Left));
					break;
				case ExpressionType.NotEqual:
					global::NUnit.Framework.Assert.AreNotEqual(rightResult, leftResult, GetMessage(expression.Left));
					break;
				case ExpressionType.GreaterThan:
					//todo: be carefull with unconditional conversion to decimal
					global::NUnit.Framework.Assert.Greater((decimal)rightResult, (decimal)leftResult, GetMessage(expression.Left));
					break;
				case ExpressionType.GreaterThanOrEqual:
					global::NUnit.Framework.Assert.GreaterOrEqual((decimal)rightResult, (decimal)leftResult, GetMessage(expression.Left));
					break;
				case ExpressionType.LessThan:
					global::NUnit.Framework.Assert.Less((decimal)rightResult, (decimal)leftResult, GetMessage(expression.Left));
					break;
				case ExpressionType.LessThanOrEqual:
					global::NUnit.Framework.Assert.LessOrEqual((decimal)rightResult, (decimal)leftResult, GetMessage(expression.Left));
					break;
				default:
					throw new InvalidOperationException("invalid assertion expression");
			}
		}

		private static object Invoke<T>(T obj, Expression<Func<T, bool>> parent, Expression expression)
		{
			if (expression.NodeType == ExpressionType.MemberAccess)
			{
				var memberAcc = expression as MemberExpression;
				var memberResult = Invoke(obj, parent, memberAcc.Expression);
				global::NUnit.Framework.Assert.IsNotNull(memberResult, memberAcc.Expression.ToString());
			}

			if (expression.NodeType == ExpressionType.Call)
			{
				var call = (MethodCallExpression)expression;
				var callObj = call.Object ?? call.Arguments[0];
				var objResult = Invoke(obj, parent, callObj);
				global::NUnit.Framework.Assert.IsNotNull(objResult, callObj.ToString());
			}

			var lambdaExpr = Expression.Lambda<Func<T, object>>(
				Expression.Convert(expression, typeof(object)), parent.Parameters.ToArray());
			var exprCompiled = lambdaExpr.Compile();
			try
			{
				return exprCompiled.Invoke(obj);
			}
			catch(Exception e)
			{
				throw new Exception("Exception occured on evaluating of expression: " + GetMessage(lambdaExpr), e);
			}
		}

		private static string GetMessage(Expression expr)
		{
			var convert = expr as UnaryExpression;
			if (convert != null && convert.NodeType == ExpressionType.Convert)
			{
				return GetMessage(convert.Operand);
			}

			return expr.ToString();
		}
	}
}