using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExtendedArithmetic
{
	/// <summary>
	/// A generic Vector numeric type.
	/// </summary>
	public class Vector<T>
	{
		/// <summary>The dimension of the vector, i.e. the number of elements or its length.</summary>
		public int Dimensions { get { return (_internalArray == null || !_internalArray.Any()) ? 0 : _internalArray.Length; } }

		/// <summary>An array for accessing the elements of this vector.</summary>
		public IReadOnlyCollection<T> Elements
		{
			get { return (IReadOnlyCollection<T>)_internalArray.ToList(); }
		}
		private T[] _internalArray = null;

		/// <summary>An indexer for accessing the elements of this vector.</summary>
		public T this[int index]
		{
			get { return _internalArray[index]; }
			set { _internalArray[index] = value; }
		}

		/// <summary>Instantiates a new zero vector instance.</summary>
		public Vector() { _internalArray = new T[0]; }

		/// <summary>Instantiates a new Vector instance given an array of elements.</summary>
		[JsonConstructor]
		public Vector(T[] elements)
		{
			_internalArray = elements.ToArray(); // ToArray() to make a copy of the array
		}

		/// <summary>Instantiates a new Vector instance given an array of elements.</summary>
		public static Vector<T> Factory(params T[] elements)
		{
			return new Vector<T>(elements);
		}

		/// <summary>Clones a vector by creating a new copy of each element into a new instance.</summary>    
		public Vector<T> Clone()
		{
			return new Vector<T>(_internalArray.Select(e => GenericArithmetic<T>.Clone(e)).ToArray());
		}

		/// <summary>
		/// Returns the normalized or unit vector of a Vector.
		/// That is, it returns a vector with the same direction as the given vector, but with a length of 1.
		/// </summary>
		public static Vector<T> Normalize(Vector<T> input)
		{
			T norm = Norm(input);
			return ScalarDivide(input, norm);
		}

		/// <summary>
		/// Returns the L2-Norm of a Vector.
		/// That is, it returns the square root of the sum of every element squared.
		/// </summary>
		public static T Norm(Vector<T> input)
		{
			T dotProduct = DotProduct(input, input);
			return GenericArithmetic<T>.SquareRoot(dotProduct);
		}

		/// <summary>
		/// A measure of similarity between two non-zero vectors defined in an inner product space.
		/// Cosine similarity is the cosine of the angle between the vectors;
		/// that is, it is the dot product of the vectors divided by the product of their lengths. 
		/// It follows that the cosine similarity does not depend on the magnitudes of the vectors,
		/// but only on their angle. 
		/// The cosine similarity is given in the interval [ −1 , 1 ].
		/// Two proportional vectors have a cosine similarity of 1, 
		/// two orthogonal vectors have a similarity of 0, 
		/// and two opposite vectors have a similarity of -1
		/// </summary>
		public static T CosineSimilarity(Vector<T> left, Vector<T> right)
		{
			Vector<T> products = Multiply(left, right);
			T dividend = products.Elements.Aggregate(GenericArithmeticFactory<T>.Zero, (accumulator, current) => GenericArithmetic<T>.Add(accumulator, current));

			T leftSumOfSquares = left.Elements.Aggregate(GenericArithmeticFactory<T>.Zero, (accumulator, current) => GenericArithmetic<T>.Add(accumulator, GenericArithmetic<T>.Multiply(current, current)));

			T rightSumOfSquares = right.Elements.Aggregate(GenericArithmeticFactory<T>.Zero, (accumulator, current) => GenericArithmetic<T>.Add(accumulator, GenericArithmetic<T>.Multiply(current, current)));

			T sqrtLeft = GenericArithmetic<T>.SquareRoot(leftSumOfSquares);
			T sqrtRight = GenericArithmetic<T>.SquareRoot(rightSumOfSquares);

			T divisor = GenericArithmetic<T>.Multiply(sqrtLeft, sqrtRight);

			T quotient = GenericArithmetic<T>.Divide(dividend, divisor);
			return quotient;
		}

		/// <summary>
		/// Squares every element, then returns their sum.
		/// </summary>
		/// <param name="vector">The input vector.</param>
		/// <returns>The sum of the elements of the vector squared.</returns>
		public static T SumOfSquares(Vector<T> vector)
		{
			T result = vector.Elements.Aggregate(GenericArithmeticFactory<T>.Zero, (accumulator, current) => GenericArithmetic<T>.Add(accumulator, GenericArithmetic<T>.Multiply(current, current)));
			return result;
		}

		/// <summary>
		/// Returns the dot product of two vectors.
		/// </summary>
		/// <param name="left">The first vector.</param>
		/// <param name="right">The second vector.</param>
		/// <returns>The dot product.</returns>
		public static T DotProduct(Vector<T> left, Vector<T> right)
		{
			Vector<T> productVector = Multiply(left, right);
			return productVector.Elements.Aggregate(GenericArithmeticFactory<T>.Zero, (accumulator, current) => GenericArithmetic<T>.Add(accumulator, current));
		}

		/// <summary>
		/// Adds two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The summed vector.</returns>
		public static Vector<T> Add(Vector<T> left, Vector<T> right)
		{
			return PairwiseForEach(left, right, ExpressionType.Add);
		}

		/// <summary>
		/// Subtracts the second vector from the first.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The difference vector.</returns>
		public static Vector<T> Subtract(Vector<T> left, Vector<T> right)
		{
			return PairwiseForEach(left, right, ExpressionType.Subtract);
		}

		/// <summary>
		/// Multiplies two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The product vector.</returns>
		public static Vector<T> Multiply(Vector<T> left, Vector<T> right)
		{
			return PairwiseForEach(left, right, ExpressionType.Multiply);
		}

		/// <summary>
		/// Divides the first vector by the second.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The vector resulting from the division.</returns>
		public static Vector<T> Divide(Vector<T> left, Vector<T> right)
		{
			return PairwiseForEach(left, right, ExpressionType.Divide);
		}

		/// <summary>
		/// Takes the first vector modulo the second.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>A vector of resulting congruences.</returns>
		public static Vector<T> Mod(Vector<T> left, Vector<T> right)
		{
			return PairwiseForEach(left, right, ExpressionType.Modulo);
		}

		/// <summary>
		/// Adds a vector by a given scalar.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="scalar">The scalar value.</param>
		/// <returns>The result of summation.</returns>
		public static Vector<T> ScalarAdd(Vector<T> vector, T scalar)
		{
			return PairwiseScalarOperation(vector, scalar, ExpressionType.Add);
		}

		/// <summary>
		/// Multiplies a vector by a given scalar.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="scalar">The scalar value.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector<T> ScalarMultiply(Vector<T> vector, T scalar)
		{
			return PairwiseScalarOperation(vector, scalar, ExpressionType.Multiply);
		}

		/// <summary>
		/// Divides a vector by a given scalar.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="scalar">The scalar value.</param>
		/// <returns>The result of the division.</returns>
		public static Vector<T> ScalarDivide(Vector<T> vector, T scalar)
		{
			return PairwiseScalarOperation(vector, scalar, ExpressionType.Divide);
		}

		/// <summary>
		/// Takes each element of a vector modulo a given scalar.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="scalar">The scalar value.</param>
		/// <returns>The resulting congruences, as a vector.</returns>
		public static Vector<T> ScalarMod(Vector<T> vector, T scalar)
		{
			return PairwiseScalarOperation(vector, scalar, ExpressionType.Modulo);
		}

		/// <summary>private method that containst the common functionality amongst all the scalar arithmetic operations.</summary>
		private static Vector<T> PairwiseScalarOperation(Vector<T> vector, T scalar, ExpressionType operation)
		{
			Vector<T> scalarVector = new Vector<T>(Enumerable.Repeat(scalar, vector.Dimensions).ToArray());
			return PairwiseForEach(vector, scalarVector, operation);
		}

		/// <summary>
		/// Returns a vector whose elements are the square root of each of the source vector's elements.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <returns>The square root vector.</returns>
		public static Vector<T> SquareRoot(Vector<T> vector)
		{
			return ForEach(vector, GenericArithmetic<T>.SquareRoot);
		}

		/// <summary>
		/// Returns the reflection of a vector off a surface that has the specified normal.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">The normal of the surface being reflected off.</param>
		/// <returns>The reflected vector.</returns>
		public static Vector<T> Reflect(Vector<T> vector, Vector<T> normal)
		{
			T dot = DotProduct(vector, normal);
			Vector<T> result = ScalarMultiply(normal, dot);
			return ScalarMultiply(result, GenericArithmetic<T>.Two);
		}

		/// <summary>
		/// Performs a linear interpolation between two vectors based on the given weighting.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <param name="amount">A value between 0 and 1 that indicates the weight of the second vector.</param>
		/// <returns>The distance.</returns>
		public static Vector<T> Lerp(Vector<T> first, Vector<T> second, T amount)
		{
			Vector<T> left = ScalarMultiply(first, GenericArithmetic<T>.Subtract(GenericArithmetic<T>.One, amount));
			Vector<T> right = ScalarMultiply(second, amount);
			return Add(left, right);
		}

		/// <summary>
		/// Performs a the inverse of lerp;
		/// Gives you the weighting based on the value between two vectors.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <param name="value">A value between the first vector and the second vector.</param>
		/// <returns>The weight.</returns>
		public static T InverseLerp(Vector<T> first, Vector<T> second, Vector<T> value)
		{
			Vector<T> AB = Subtract(second, first);
			Vector<T> AV = Subtract(value, first);
			return GenericArithmetic<T>.Divide(Vector<T>.DotProduct(AV, AB), Vector<T>.DotProduct(AB, AB));
		}

		/// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
		/// <param name="value">A vector.</param>
		/// <returns>The absolute value vector.</returns>
		public static Vector<T> Abs(Vector<T> value)
		{
			return ForEach(value, GenericArithmeticFactory<T>.CreateAbsFunction());
		}

		/// <summary>Restricts a vector between a minimum and a maximum value.</summary>
		/// <param name="value1">The vector to restrict.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		/// <returns>The restricted vector.</returns>
		public static Vector<T> Clamp(Vector<T> value1, Vector<T> min, Vector<T> max)
		{
			if (min.Dimensions != max.Dimensions || value1.Dimensions != max.Dimensions)
			{
				throw new Exception("All vector dimensions must be the same.");
			}

			Func<T, T, bool> greaterThanComparison = GenericArithmeticFactory<T>.GetComparisonExpression(ExpressionType.GreaterThan);
			Func<T, T, bool> lessThanComparison = GenericArithmeticFactory<T>.GetComparisonExpression(ExpressionType.LessThan);

			int index = 0;
			Func<T, T> clamp = new Func<T, T>(
				(t) =>
				{
					T result = t;

					if (greaterThanComparison(t, max[index]))
					{
						result = max[index];
					}
					else if (lessThanComparison(t, min[index]))
					{
						result = min[index];
					}

					index++;
					return result;
				}
			);

			return ForEach(value1, clamp);
		}

		/// <summary>
		/// Returns the distance of the vector.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <returns>The distance.</returns>
		public static T Distance(Vector<T> first, Vector<T> second)
		{
			T distanceSquared = DistanceSquared(first, second);
			return GenericArithmetic<T>.SquareRoot(distanceSquared);
		}

		/// <summary>
		/// Returns the distance of the vector squared.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <returns>The distance squared.</returns>
		public static T DistanceSquared(Vector<T> first, Vector<T> second)
		{
			Vector<T> difference = Subtract(first, second);
			return DotProduct(difference, difference);
		}

		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <returns>The length.</returns>
		public static T Length(Vector<T> first, Vector<T> second)
		{
			T lengthSquared = LengthSquared(first, second);
			return GenericArithmetic<T>.SquareRoot(lengthSquared);
		}

		/// <summary>
		/// Returns the length of the vector squared.
		/// </summary>
		/// <param name="first">The first vector.</param>
		/// <param name="second">The second vector.</param>
		/// <returns>The length squared.</returns>
		public static T LengthSquared(Vector<T> first, Vector<T> second)
		{
			return DotProduct(first, second);
		}

		/// <summary>
		/// Projects the vector "a" orthogonally onto vector "b".
		/// </summary>
		/// <param name="a">The vector to project.</param>
		/// <param name="b">The vector on which will be projected.</param>
		/// <returns>The projection.</returns>
		public static Vector<T> Project(Vector<T> a, Vector<T> b)
		{
			T quotient = GenericArithmetic<T>.Divide(DotProduct(a, b), DotProduct(a, a));
			return Vector<T>.ScalarMultiply(a, quotient);
		}

		/// <summary>
		/// Calculates the magnitude of the given vector 
		/// as the square root of the dot product of the vector with itself.
		/// </summary>
		/// <param name="a">The vector to compute the magnitude of.</param>
		/// <returns>The magnitude.</returns>
		public static T Magnitude(Vector<T> a)
		{
			T dot = DotProduct(a, a);
			return GenericArithmetic<T>.SquareRoot(dot);
		}

		/*
		/// <summary>
		/// Turns a Vector into a polynomial, using the vector values in order as the coefficients.
		/// Starts with the highest exponent of one less than the number of elements and works
		/// down to zero, in order.
		/// </summary>
		/// <returns>The ExtendedArithmetic.Polynomial class.</returns>
		public Polynomial ToPolynomial()
		{
			List<int> intElements =
				Elements
				.Where(i => GenericArithmetic<T>.NotEqual(i, GenericArithmeticFactory<T>.Zero))
				.ToList();

			int degree = intElements.Count - 1;
			List<Term<T>> terms = new List<Term<T>>();
			foreach (int i in intElements)
			{
				Term<T> newTerm = new Term<T>(i, degree);
				terms.Add(newTerm);
				degree--;
			}
			return new Polynomial<T>(terms.ToArray());
		}

		/// <summary>Turns a coefficient array from a ExtendedArithmetic.Polynomial instance into a Vector instance.</summary>
		public static Vector<T> FromPolynomial(Polynomial m)
		{
			T[] elements = m.Terms.Select(trm => (double)trm.CoEfficient)
									.Select(d => GenericArithmetic<T>.Convert<double>(d))
									.ToArray();
			return Vector<T>.Factory(elements);
		}
		*/

		/// <summary>
		/// Begins two vectors, takes an element from each vector 
		/// and applies a function to each bijective pair, 
		/// collecting the results in a new vector.
		/// </summary>
		private static Vector<T> PairwiseForEach(Vector<T> left, Vector<T> right, ExpressionType operation)
		{
			return PairwiseForEach(left, right, GenericArithmeticFactory<T>.GetBinaryExpression(operation));
		}

		/// <summary>
		/// Begins two vectors, takes an element from each vector
		/// and applies a function to each bijective pair,
		/// collecting the results in a new vector.
		/// </summary>
		private static Vector<T> PairwiseForEach(Vector<T> left, Vector<T> right, Func<T, T, T> operation)
		{
			if (left.Dimensions != right.Dimensions)
			{
				throw new Exception("Both vector dimensions must be the same.");
			}

			int index = 0;
			int max = left.Dimensions;
			List<T> results = new List<T>();
			while (index < max)
			{
				T result = operation.Invoke(left[index], right[index]);
				results.Add(result);
				index++;
			}
			return new Vector<T>(results.ToArray());
		}

		/// <summary>Applies a function to each vector element, collecting the results in a new vector.</summary>
		private static Vector<T> ForEach(Vector<T> input, Func<T, T> operation)
		{
			if (input.Dimensions == 0)
			{
				return input;
			}

			List<T> results = new List<T>();
			foreach (T element in input.Elements)
			{
				results.Add(operation.Invoke(element));
			}
			return new Vector<T>(results.ToArray());
		}

		/// <summary>
		/// Returns a String representing this Vector instance.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			return $"[ {string.Join(", ", this.Elements.Select(e => e.ToString()))} ]";
		}
	}
}