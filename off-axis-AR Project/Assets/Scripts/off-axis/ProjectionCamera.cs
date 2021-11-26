//NOTE: Undefine this if you need to move the plane at runtime
//#define PRECALC_PLANE
using System.Collections;
using UnityEngine;

namespace Apt.Unity.Projection
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class ProjectionCamera : MonoBehaviour
    {
        //reference: https://medium.com/@michel.brisis/off-axis-projection-in-unity-1572d826541e

        [Header("Projection plane")]
        public ProjectionPlane ProjectionScreen;
        public bool clampNearPlane = true; //ProjectionPlane as NearPlane of Camera
        [Header("Helpers")]
        public bool DrawGizmos = true; 

        private Vector3 eyePos;
        private float n, f, l, r, b, t; //camera planes

        Vector3 va, vb, vc, vd; //vectors from camera to plane vertecies
        Vector3 viewDir;

        [HideInInspector]
        public Camera cam; //Made public for tracking purposes

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void OnDrawGizmos()
        {
            if (ProjectionScreen == null) return;

            //Draw lines from Camera to ProjectionPlane
            if (DrawGizmos)
            {
                var pos = transform.position;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos + va);
                Gizmos.DrawLine(pos, pos + vb);
                Gizmos.DrawLine(pos, pos + vc);
                Gizmos.DrawLine(pos, pos + vd);

                Vector3 pa = ProjectionScreen.BottomLeft;
                Vector3 vr = ProjectionScreen.DirRight;
                Vector3 vu = ProjectionScreen.DirUp;

                Gizmos.color = Color.white;
                Gizmos.DrawLine(pos, viewDir);
            }
        }


        private void LateUpdate()
        {
            if (ProjectionScreen == null) return;

            //Get ProjectionPlane
            Vector3 pa = ProjectionScreen.BottomLeft;
            Vector3 pb = ProjectionScreen.BottomRight;
            Vector3 pc = ProjectionScreen.TopLeft;
            Vector3 pd = ProjectionScreen.TopRight;

            Vector3 vr = ProjectionScreen.DirRight;
            Vector3 vu = ProjectionScreen.DirUp;
            Vector3 vn = ProjectionScreen.DirNormal;

            Matrix4x4 M = ProjectionScreen.M;

            //From eye to ProjectionPlane vertecies
            eyePos = transform.position;

            va = pa - eyePos;
            vb = pb - eyePos;
            vc = pc - eyePos;
            vd = pd - eyePos;

            viewDir = eyePos + va + vb + vc + vd;

            //distance from Camera to ProjectionPlane
            float d = -Vector3.Dot(va, vn);
            if (clampNearPlane)
            {
                cam.nearClipPlane = d;
            }
            n = cam.nearClipPlane;
            f = cam.farClipPlane;

            //Create Frustum
            float nearOverDist = n / d;
            l = Vector3.Dot(vr, va) * nearOverDist;
            r = Vector3.Dot(vr, vb) * nearOverDist;
            b = Vector3.Dot(vu, va) * nearOverDist;
            t = Vector3.Dot(vu, vc) * nearOverDist;
            Matrix4x4 P = Matrix4x4.Frustum(l, r, b, t, n, f);

            //Translation to eye position
            Matrix4x4 T = Matrix4x4.Translate(-eyePos);

            //Rotation to camera (only messes up Frustum in Edit mode)
            //Matrix4x4 R = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation) * ProjectionScreen.transform.rotation);
            
            //Apply ProjectionPlaneMatrix, TranslationMatrix, RotationMatrix to Camera
            cam.worldToCameraMatrix = M * T; // * R

            //Frustum becomes Cameras ProjectionMatrix
            cam.projectionMatrix = P;
        }
    }
}
