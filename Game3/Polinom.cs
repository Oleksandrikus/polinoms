using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace Game3
{
    class Polinom
    {
        private const int MIN_SQUEAR_APPROXIMATION_POWER = 3;

        /// <summary>
        /// Static method perform minimal square metod cubic approximation
        /// </summary>
        /// <param name="soursData">Dots for approximation</param>
        /// <param name="start">Start of range</param>
        /// <param name="step">Step of range</param>
        /// <param name="end">End of range</param>
        /// <returns>Data for building graphic of approximation function</returns>
        //public static KeyValuePair<Lines, SortedDictionary<double, double>> MinSquearApproximation(SortedDictionary<double, double> soursData, double start, double step, double end)
        public static  SortedDictionary<double, double> MinSquearApproximation(SortedDictionary<double, double> soursData, double start, double step, double end)
        {
            /*
             We need to approximate our dots to function f=ax^3+b^x2+cx+d. We finde our a, b, c and d from  equations system:
               a        b        c        d
             [ sum(x^6) sum(x^5) sum(x^4) sum(x^3) | sum(y*x^3) ]
             [ sum(x^5) sum(x^4) sum(x^3) sum(x^2) | sum(y*x^2) ]
             [ sum(x^4) sum(x^3) sum(x^2) sum(x)   | sum(y*x)   ]
             [ sum(x^3) sum(x^2) sum(x)   1        | sum(y)     ]
             System is solved using Gauss alhorythm
            */

            //Array contains sums of x in paticular power (from 0 to 6, according to our problem)
            double[] xPowSums = new double[MIN_SQUEAR_APPROXIMATION_POWER + 4];

            //Define sums of x in paticular power
            for (int i = 0; i < xPowSums.Length; i++)
            {
                foreach (KeyValuePair<double, double> p in soursData)
                {
                    xPowSums[i] += Math.Pow(p.Key, i);
                }
            }

            //Left sides of the equations
            double[] xySums = new double[MIN_SQUEAR_APPROXIMATION_POWER + 1];

            //Define left sides of the equations
            for (int i = 0; i < xySums.Length; i++)
            {
                foreach (KeyValuePair<double, double> p in soursData)
                {
                    xySums[i] += p.Value * Math.Pow(p.Key, 3 - i);
                }
            }

            //Build equations system

            //Define matrix (we use jagged array to operate lines independantly)
            double[][] matrix = new double[MIN_SQUEAR_APPROXIMATION_POWER + 1][];

            //Plase elements according to reqirement of minimal sqare alhorythm
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new double[matrix.Length];
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    matrix[i][j] = xPowSums[MIN_SQUEAR_APPROXIMATION_POWER + 3 - (i + j)];
                }
            }

            //Left sides of problem after Gauss transformation
            double[] solutionsLine;

            //Define right and left sides of problem after Gauss transformation
            double[][] gm = GaussTransform(matrix, xySums, out solutionsLine);

            //Finde solution of transformed equations system
            double[] coeff = BeckStep(matrix, solutionsLine);

            //return new KeyValuePair<Lines, SortedDictionary<double, double>>(Lines.MinSqear3Polinom, RangeCreator(coeff, start, step, end));
            return new SortedDictionary<double, double>(RangeCreator(coeff, start, step, end));
            //return  RangeCreator(coeff, start, step, end);
        }

        /// <summary>
        /// Method perform Gauss transformation of equations system
        /// </summary>
        /// <param name="matrix">Left side</param>
        /// <param name="originSolution">Right side</param>
        /// <param name="newSolution">Resulted left side</param>
        /// <returns>Resulted right side</returns>
        private static double[][] GaussTransform(double[][] matrix, double[] originSolution, out double[] newSolution)
        {
            //Check if data are appropriate
            if (SquareTest(matrix) && (matrix.Length == originSolution.Length))
            {
                //line counter
                int h = 0;

                //column counter
                int k = 0;

                //Temporary objects for exchanging
                double[] tempRow;
                double tempNumber;

                //While we don't come to the right bottom angle
                while ((h < matrix.Length) && (k < matrix.Length))
                {
                    //Everything less is equel to 0
                    const double ESP = 1E-9;

                    //Finde max element in k'th column
                    int i_max = h;
                    for (int i = h; i < matrix.GetLength(0); i++)
                        if (matrix[i][k] > matrix[i_max][k])
                            i_max = i;
                    //If max element is almost 0, go to the next column
                    if (Math.Abs(matrix[i_max][k]) < ESP)
                    {
                        matrix[i_max][k] = 0;
                        k++;
                    }
                    else
                    {
                        if (i_max != h)
                        {
                            //Exchange h'th row with max-element row
                            tempRow = matrix[h];
                            matrix[h] = matrix[i_max];
                            matrix[i_max] = tempRow;

                            tempNumber = originSolution[h];
                            originSolution[h] = originSolution[i_max];
                            originSolution[i_max] = tempNumber;
                        }
                        //For each rows after h's
                        for (int i = h + 1; i < matrix.Length; i++)
                        {
                            double f = matrix[i][k] / matrix[h][k];

                            //Deduct h'th row from selected row with such coeffitient that allow us to turn k'th element of selected row into 0
                            matrix[i][k] = 0;
                            for (int j = k + 1; j < matrix.GetLength(0); j++)
                                matrix[i][j] -= matrix[h][j] * f;
                            originSolution[i] -= originSolution[h] * f;
                        }
                    }
                    //go to the next row
                    h++;
                    //go to the next column
                    k++;
                }
                //Define resulted right side
                newSolution = originSolution;
                return matrix;
            }

            //If they aren't throw exeption (it can't happend in this program)
            else
            {
                if (!SquareTest(matrix))
                    throw new Exception("Number of equations isn't equel to number of unknowns!");
                else
                    throw new Exception("Number of ride sides and left sides aren't equel!");
            }
        }

        /// <summary>
        /// Getting results from Gauss-transformed equations system
        /// </summary>
        /// <param name="matrix">Left side</param>
        /// <param name="right">Right side</param>
        /// <returns>Results</returns>
        private static double[] BeckStep(double[][] matrix, double[] right)
        {
            //Check if our data are appropriate
            if (SquareTest(matrix) && (matrix.Length == right.Length))
            {
                //Define result arrey
                double[] res = new double[right.Length];

                //Define last element
                res[right.Length - 1] = right[right.Length - 1] / matrix[right.Length - 1][right.Length - 1];

                //For each else element
                for (int i = right.Length - 2; i >= 0; i--)
                {
                    //Result is its element right side...
                    res[i] = right[i];

                    //..Deduct all previous elements multiply to "their" column elements of "it's element" row...
                    for (int j = right.Length - 1; j > i; j--)
                        res[i] -= res[j] * matrix[i][j];
                    //..Devide by "it's" column element of "it's" row
                    res[i] /= matrix[i][i];
                }
                return res;
            }
            //If they aren't, throw exeption (it can't happend in this program)
            else
            {
                if (!SquareTest(matrix))
                    throw new Exception("Number of equations isn't equel to number of unknowns!");
                else
                    throw new Exception("Number of ride sides and left sides aren't equel!");
            }
        }

        /// <summary>
        /// Check if jagged array is square matrix
        /// </summary>
        /// <param name="matrix">Checked array</param>
        /// <returns>If it's square matrix</returns>
        private static bool SquareTest(double[][] matrix)
        {
            //Flag define if checked array is square matrix
            bool flag = true;

            //For each row
            for (int i = 0; i < matrix.Length - 1; i++)
                //If rows have different length or array has different number of rows and number of columns
                if ((matrix[i].Length != matrix[i + 1].Length) || (matrix.Length != matrix[i].Length))
                    //It isn't square matrix
                    flag = false;
            return flag;
        }

        /// <summary>
        /// Get range of values of polinom
        /// </summary>
        /// <param name="coeffitient">Array of coeffitients of polinom</param>
        /// <param name="start">Start of range</param>
        /// <param name="step">Step of range</param>
        /// <param name="end">End of range</param>
        /// <returns>Range</returns>
        private static SortedDictionary<double, double> RangeCreator(double[] coeffitient, double start, double step, double end)
        {
            //Define dictionary contains our range
            SortedDictionary<double, double> result = new SortedDictionary<double, double>();

            //For each x
            for (double i = start; i <= end; i += step)
            {
                double value = 0;
                //Finde polinom value
                for (int j = 0; j < coeffitient.Length; j++)
                {
                    value += coeffitient[j] * Math.Pow(i, coeffitient.Length - 1 - j);
                }
                result.Add(i, value);
            }
            return result;
        }
    }
}