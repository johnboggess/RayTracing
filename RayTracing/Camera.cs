using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Mathematics;

namespace RayTracing
{
    public class Camera
    {
        float _aspectRatio = 16f/9f;
        float _viewPortHeight = 2;
        float _viewPortWidth { get { return _aspectRatio * _viewPortHeight; } }
        float _focalLength = 1f;


        public Transform Transform = new Transform();
        public Matrix4 PerspectiveMatrix;
        public float AspectRatio { get { return _aspectRatio; } }
        public float ViewPortHeight { get { return _viewPortHeight; } }
        public float ViewPortWidth { get { return _viewPortWidth; } }
        public float FocalLength { get { return _focalLength; } }

        public Camera(float viewPortHeight, float aspectRatio, float focalLength)
        {
            _viewPortHeight = viewPortHeight;
            _aspectRatio = aspectRatio;
            _focalLength = focalLength;
        }
    }
}
