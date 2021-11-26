﻿using UnityEngine;

namespace Apt.Unity.Projection
{
    [ExecuteInEditMode]
    public class ProjectionPlane : MonoBehaviour
    {
        //reference: https://medium.com/@michel.brisis/off-axis-projection-in-unity-1572d826541e

        [Header("Size")]
        public Vector2 Size = new Vector2(8, 4.5f);
        public Vector2 AspectRatio = new Vector2(16, 9);
        public bool LockAspectRatio = true;
        [Header("Visualization")]
        public bool DrawGizmo = true;

        //Bottom-left, Bottom-right top-left, top-right corners of plane respectively
        public Vector3 BottomLeft { get; private set; }
        public Vector3 BottomRight { get; private set; }
        public Vector3 TopLeft { get; private set; }
        public Vector3 TopRight { get; private set; }

        //Vector right, up, normal from center of plane
        public Vector3 DirRight { get; private set; }
        public Vector3 DirUp { get; private set; }
        public Vector3 DirNormal { get; private set; }

        private Vector2 previousSize = new Vector2(8, 4.5f);
        private Vector2 previousAspectRatio = new Vector2(16, 9);

        private GameObject alignmentCube;
        private Transform backTrans;
        private Transform leftTrans;
        private Transform rightTrans;
        private Transform topTrans;
        private Transform bottomTrans;

        Matrix4x4 m;
        public Matrix4x4 M { get => m; }

        private void OnDrawGizmos()
        {
            if (DrawGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(BottomLeft, BottomRight);
                Gizmos.DrawLine(BottomLeft, TopLeft);
                Gizmos.DrawLine(TopRight, BottomRight);
                Gizmos.DrawLine(TopLeft, TopRight);

                //Draw direction towards Camera
                Gizmos.color = Color.cyan;
                var planeCenter = BottomLeft + ((TopRight - BottomLeft) * 0.5f);
                Gizmos.DrawLine(planeCenter, planeCenter + DirNormal);
            }
        }

        void Update()
        {
            
            //Redo aspect ratios
            if(LockAspectRatio)
            {
                if(AspectRatio.x != previousAspectRatio.x)
                {
                    Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                    previousAspectRatio.y = AspectRatio.y;
                }

                if(AspectRatio.y != previousAspectRatio.y)
                {
                    Size.x = Size.y / AspectRatio.y * AspectRatio.x;
                }

                if(Size.x != previousSize.x)
                {
                    Size.y = Size.x / AspectRatio.x * AspectRatio.y;
                    previousSize.y = Size.y;
                }

                if(Size.y != previousSize.y)
                {
                    Size.x = Size.y / AspectRatio.y * AspectRatio.x;
                }
            }

            //Make sure we don't crash Unity
            Size.x = Mathf.Max(1, Size.x);
            Size.y = Mathf.Max(1, Size.y);
            AspectRatio.x = Mathf.Max(1, AspectRatio.x);
            AspectRatio.y = Mathf.Max(1, AspectRatio.y);

            //Update ratio references
            previousSize = Size;
            previousAspectRatio = AspectRatio;

            //Calculate Plane verticies
            BottomLeft = transform.TransformPoint(new Vector3(-Size.x, -Size.y) * 0.5f);
            BottomRight = transform.TransformPoint(new Vector3(Size.x, -Size.y) * 0.5f);
            TopLeft = transform.TransformPoint(new Vector3(-Size.x, Size.y) * 0.5f);
            TopRight = transform.TransformPoint(new Vector3(Size.x, Size.y) * 0.5f);

            //Calculate directions
            DirRight = (BottomRight - BottomLeft).normalized;
            DirUp = (TopLeft - BottomLeft).normalized;
            DirNormal = -Vector3.Cross(DirRight, DirUp).normalized;
            
            //Combine calculations in 4x4 Matrix to further calculate in ProjectionCamera
            m = Matrix4x4.zero;

            m[0, 0] = DirRight.x;
            m[0, 1] = DirRight.y;
            m[0, 2] = DirRight.z;

            m[1, 0] = DirUp.x;
            m[1, 1] = DirUp.y;
            m[1, 2] = DirUp.z;

            m[2, 0] = DirNormal.x;
            m[2, 1] = DirNormal.y;
            m[2, 2] = DirNormal.z;

            m[3, 3] = 1.0f;
        }
    }
}
