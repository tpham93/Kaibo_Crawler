using SharpDX;

namespace Kaibo_Crawler
{
    class Helpers
    {
        /// <summary>
        /// Transformation of a Vector3 without projection.
        /// </summary>
        /// <param name="_v">The vector to transform</param>
        /// <param name="_m">The transformation matrix (rotation, scalation, translation)</param>
        /// <returns>The first 3 components of (_v, 1) * _m</returns>
        public static Vector3 Transform(Vector3 _v, ref Matrix _m)
        {
            Vector4 r;
            Vector3.Transform(ref _v, ref _m, out r);
            return new Vector3(r.X, r.Y, r.Z);
        }


        /// <summary>
        /// Computes the transposed inverse matrix without translation part.
        /// This matrix is usefull for normal-space transformations
        /// </summary>
        /// <param name="_m"></param>
        /// <returns></returns>
        public static Matrix CreateInverseTranspose(ref Matrix _m)
        {
            Matrix transformInvTr = _m;
            transformInvTr.TranslationVector = new Vector3(0.0f, 0.0f, 0.0f);
            transformInvTr.Invert();
            transformInvTr.Transpose();
            return transformInvTr;
        }
    }
}
