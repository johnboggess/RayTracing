using System;
using System.Collections.Generic;
using System.Text;

using ObjectTK.Shaders;
using ObjectTK.Shaders.Sources;
using ObjectTK.Shaders.Variables;

using OpenTK.Mathematics;

namespace RayTracing
{
    [ComputeShaderSource("RayTracer.Compute")]
    public class RayTracer : ComputeProgram
    {
        public ImageUniform destTex { get; set; }
        public ImageUniform backgroundTex { get; set; }
        public Uniform<Matrix4> ToWorldSpace { get; set; }
        public Uniform<Vector3> CameraPos { get; set; }
        public Uniform<float> ViewPortWidth { get; set; }
        public Uniform<float> ViewPortHeight { get; set; }
        public Uniform<int> BackgroundWidth { get; set; }
        public Uniform<int> BackgroundHeight { get; set; }
        public Uniform<int> WindowWidth { get; set; }
        public Uniform<int> WindowHeight { get; set; }
    }
}
