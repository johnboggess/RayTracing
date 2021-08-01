using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Mathematics;

namespace RayTracing
{
    public class Transform
    {
        public Quaternion Rotation = new Quaternion(0,0,0,1);
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Vector3 Right { get { return Rotation * Vector3.UnitX; } }
        public Vector3 Up { get { return Rotation * Vector3.UnitY; } }
        public Vector3 Forward { get { return Rotation * Vector3.UnitZ; } }

        public Vector3 XAxis { get { return Right;  } }
        public Vector3 YAxis { get { return Up; } }
        public Vector3 ZAxis { get { return Forward; } }

        public Matrix4 GetMatrix()
        {
            Vector3 r = Right;
            Vector3 u = Up;
            Vector3 f = Forward;
            return new Matrix4(new Vector4(r.X, u.X, f.X, Position.X), new Vector4(r.Y, u.Y, f.Y, Position.Y), new Vector4(r.Z, u.Z, f.Z, Position.Z), new Vector4(0, 0, 0, 1));
        }
    }
}
