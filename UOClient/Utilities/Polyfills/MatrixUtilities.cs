using Microsoft.Xna.Framework;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UOClient.Utilities.Polyfills
{
    internal static class MatrixUtilities
    {
        public static Matrix Multiply(in Matrix matrix1, in Matrix matrix2)
        {
            ref readonly Matrix4x4 vMatrix1 = ref UnsafeUtility.As<Matrix, Matrix4x4>(in matrix1);
            ref readonly Matrix4x4 vMatrix2 = ref UnsafeUtility.As<Matrix, Matrix4x4>(in matrix2);

            Matrix4x4 vResult = Matrix4x4.Multiply(vMatrix1, vMatrix2);
            return Unsafe.As<Matrix4x4, Matrix>(ref vResult);
        }

        public static void Multiply(in Matrix matrix1, in Matrix matrix2, out Matrix result)
        {
            ref readonly Matrix4x4 vMatrix1 = ref UnsafeUtility.As<Matrix, Matrix4x4>(in matrix1);
            ref readonly Matrix4x4 vMatrix2 = ref UnsafeUtility.As<Matrix, Matrix4x4>(in matrix2);

            Matrix4x4 vResult = Matrix4x4.Multiply(vMatrix1, vMatrix2);
            result = Unsafe.As<Matrix4x4, Matrix>(ref vResult);
        }

        public static void Invert(in Matrix matrix, out Matrix result)
        {
            ref readonly Matrix4x4 vMatrix = ref UnsafeUtility.As<Matrix, Matrix4x4>(in matrix);

            Matrix4x4.Invert(vMatrix, out Matrix4x4 vResult);
            result = Unsafe.As<Matrix4x4, Matrix>(ref vResult);
        }
    }
}
