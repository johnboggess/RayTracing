using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using ObjectTK.Buffers;
using ObjectTK.Shaders;
using ObjectTK.Shaders.Sources;
using ObjectTK.Shaders.Variables;
using ObjectTK.Textures;

namespace RayTracing
{
    [VertexShaderSource("RenderShader.Vertex")]
    [FragmentShaderSource("RenderShader.Fragment")]
    class RenderProgram : ObjectTK.Shaders.Program
    {
        [VertexAttrib(3, VertexAttribPointerType.Float)]
        public VertexAttrib InPosition { get; protected set; }

        [VertexAttrib(2, VertexAttribPointerType.Float)]
        public VertexAttrib InUV { get; protected set; }

        public TextureUniform<Texture2D> Texture { get; set; }

    }
}
