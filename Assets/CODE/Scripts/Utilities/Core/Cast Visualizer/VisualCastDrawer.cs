using System.Collections.Generic;
using UnityEngine;

internal abstract class GizmoDrawHandler
{
    //------PROPERTIES
    public float CurrentAlpha => _fadeTime == 0 ? 1 : 1 / (_fadeTime / (_fadeTime - (_fadeTime - TimeLeft)));

    public float TimeLeft => _bornTime + _fadeTime - Time.realtimeSinceStartup;

    //------FIELDS
    private readonly float _bornTime;
    private readonly float _fadeTime;

    //------CONSTRUCTORS
    public GizmoDrawHandler(float fadeTime)
    {
        _fadeTime = fadeTime;
        _bornTime = Time.realtimeSinceStartup;
    }

    //------METHODS
    public abstract void Draw();
}

internal sealed class RaycastDrawHandler : GizmoDrawHandler
{
    private readonly ICastInfo _info;

    public RaycastDrawHandler(ICastInfo info, float fadeTime) : base(fadeTime)
    {
        _info = info;
    }

    public override void Draw()
    {
        if (_info.CastResult)
        {
            //Is using cast all? -> Draw additional gizmo
            if (_info.CastAll)
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r, VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b, CurrentAlpha);

                //Line from origin to first hit
                Vector3 lineOriginPoint = _info.OriginPoint, lineEndPoint = _info.CastResult.FirstHit.point;
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);

                //Draw points for each hit
                for (int i = 0; i < _info.CastResult.Hits.Length; i++)
                {
                    if (i != 0)
                        Gizmos.DrawLine(_info.OriginPoint + _info.Direction * _info.CastResult.Hits[i - 1].distance,
                            _info.OriginPoint + _info.Direction * _info.MaxDistance);

                    Gizmos.DrawSphere(_info.CastResult.Hits[i].point, VisualCastDrawer.Instance.HitIndicatorScale);
                }

                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                    VisualCastDrawer.Instance.NoHitColor.g,
                    VisualCastDrawer.Instance.NoHitColor.b,
                    CurrentAlpha);

                //Line from last hit to max distance point
                lineOriginPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance;
                lineEndPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance +
                               _info.Direction * (_info.MaxDistance - _info.CastResult.LastHit.distance);
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);
            }
            else
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
                    VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b,
                    CurrentAlpha);

                //Line from origin to last hit
                Gizmos.DrawLine(_info.OriginPoint,
                    _info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance);
                Gizmos.DrawSphere(_info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance,
                    VisualCastDrawer.Instance.HitIndicatorScale);
            }
        }
        else
        {
            //Set no hitcolor
            Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                VisualCastDrawer.Instance.NoHitColor.g,
                VisualCastDrawer.Instance.NoHitColor.b,
                CurrentAlpha);


            //Line from origin point to end point
            Gizmos.DrawLine(_info.OriginPoint, _info.OriginPoint + _info.Direction * _info.MaxDistance);
        }
    }
}

internal sealed class BoxCastDrawHandler : GizmoDrawHandler
{
    private readonly ICastInfo _info;

    public BoxCastDrawHandler(ICastInfo info, float fadeTime) : base(fadeTime)
    {
        _info = info;
    }

    public override void Draw()
    {
        if (_info.CastResult)
        {
            //Is using cast all? -> Draw additional gizmo 
            if (_info.CastAll)
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
                    VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b,
                    CurrentAlpha);

                //Line from origin to first hit
                Vector3 lineOriginPoint = _info.OriginPoint,
                    lineEndPoint = _info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance;
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);


                //Draw cubes for each hit
                for (int i = 0; i < _info.CastResult.Hits.Length; i++)
                {
                    if (i != 0)
                        Gizmos.DrawLine(_info.OriginPoint + _info.Direction * _info.CastResult.Hits[i - 1].distance,
                            _info.OriginPoint + _info.Direction * _info.MaxDistance);

                    DrawCube(_info.OriginPoint + _info.Direction * _info.CastResult.Hits[i].distance);
                    Gizmos.DrawSphere(_info.CastResult.Hits[i].point, VisualCastDrawer.Instance.HitIndicatorScale);
                }

                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                    VisualCastDrawer.Instance.NoHitColor.g,
                    VisualCastDrawer.Instance.NoHitColor.b,
                    CurrentAlpha);

                //Line from last hit to max distance point
                lineOriginPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance;
                lineEndPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance +
                               _info.Direction * (_info.MaxDistance - _info.CastResult.LastHit.distance);
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);

                //Draw box
                DrawCube(_info.OriginPoint + _info.Direction * _info.MaxDistance);
            }
            else
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
                    VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b,
                    CurrentAlpha);

                //Line from origin to last hit
                Gizmos.DrawLine(_info.OriginPoint,
                    _info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance);

                //Draw box
                DrawCube(_info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance);
                Gizmos.DrawSphere(_info.CastResult.LastHit.point, VisualCastDrawer.Instance.HitIndicatorScale);
            }
        }
        else
        {
            //Set no hitcolor
            Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                VisualCastDrawer.Instance.NoHitColor.g,
                VisualCastDrawer.Instance.NoHitColor.b,
                CurrentAlpha);


            //Line from origin to center of the box
            Gizmos.DrawLine(_info.OriginPoint, _info.OriginPoint + _info.Direction * _info.MaxDistance);


            //Draw cube
            DrawCube(_info.OriginPoint + _info.Direction * _info.MaxDistance);
        }

        void DrawCube(Vector3 center)
        {
            BoxcastInfo boxcastInfo = (BoxcastInfo)_info;
            Vector3 lineEnd;


            //Top front line
            var lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top back line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom front line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom back line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Front left line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Back left line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Front right line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Back right line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top left line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom left line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.left * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top right line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.up * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom right line
            lineOrigin = lineEnd = center;
            lineOrigin += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                          boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                          boxcastInfo.Rotation * Vector3.back * boxcastInfo.Size.z * 0.5f;

            lineEnd += boxcastInfo.Rotation * Vector3.down * boxcastInfo.Size.y * 0.5f +
                       boxcastInfo.Rotation * Vector3.right * boxcastInfo.Size.x * 0.5f +
                       boxcastInfo.Rotation * Vector3.forward * boxcastInfo.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);
        }
    }
}

internal sealed class SpherecastDrawHandler : GizmoDrawHandler
{
    private readonly ICastInfo _info;

    public SpherecastDrawHandler(ICastInfo info, float fadeTime) : base(fadeTime)
    {
        _info = info;
    }

    public override void Draw()
    {
        SpherecastInfo spherecastInfo = (SpherecastInfo)_info;
        if (_info.CastResult)
        {
            //Is using cast all? -> Draw additional gizmo 
            if (_info.CastAll)
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
                    VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b,
                    CurrentAlpha);

                //Line from origin to first hit
                Vector3 lineOriginPoint = _info.OriginPoint,
                    lineEndPoint = _info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance;
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);

                //Draw spheres for each hit
                for (int i = 0; i < _info.CastResult.Hits.Length; i++)
                {
                    if (i != 0)
                        Gizmos.DrawLine(_info.OriginPoint + _info.Direction * _info.CastResult.Hits[i - 1].distance,
                            _info.OriginPoint + _info.Direction * _info.MaxDistance);

                    Gizmos.DrawWireSphere(_info.OriginPoint + _info.Direction * _info.CastResult.Hits[i].distance,
                        spherecastInfo.Radius);
                    Gizmos.DrawSphere(_info.CastResult.Hits[i].point, VisualCastDrawer.Instance.HitIndicatorScale);
                }

                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                    VisualCastDrawer.Instance.NoHitColor.g,
                    VisualCastDrawer.Instance.NoHitColor.b,
                    CurrentAlpha);

                //Line from last hit to max distance point
                lineOriginPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance;
                lineEndPoint = _info.OriginPoint + _info.Direction * _info.CastResult.LastHit.distance +
                               _info.Direction * (_info.MaxDistance - _info.CastResult.LastHit.distance);
                Gizmos.DrawLine(lineOriginPoint, lineEndPoint);

                //Draw sphere
                Gizmos.DrawWireSphere(_info.OriginPoint + _info.Direction * _info.MaxDistance,
                    spherecastInfo.Radius);
            }
            else
            {
                //Set new color
                Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
                    VisualCastDrawer.Instance.HitColor.g,
                    VisualCastDrawer.Instance.HitColor.b,
                    CurrentAlpha);


                //Line from origin to center of the box
                Gizmos.DrawLine(_info.OriginPoint,
                    _info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance);


                //Draw sphere
                Gizmos.DrawWireSphere(_info.OriginPoint + _info.Direction * _info.CastResult.FirstHit.distance,
                    spherecastInfo.Radius);
                Gizmos.DrawSphere(_info.CastResult.LastHit.point, VisualCastDrawer.Instance.HitIndicatorScale);
            }
        }
        else
        {
            //Set no hitcolor
            Gizmos.color = new Color(VisualCastDrawer.Instance.NoHitColor.r,
                VisualCastDrawer.Instance.NoHitColor.g,
                VisualCastDrawer.Instance.NoHitColor.b,
                CurrentAlpha);


            //Line from origin to max distance
            Gizmos.DrawLine(spherecastInfo.OriginPoint,
                spherecastInfo.OriginPoint + spherecastInfo.Direction * spherecastInfo.MaxDistance);


            //Draw sphere
            Gizmos.DrawWireSphere(
                spherecastInfo.OriginPoint + spherecastInfo.Direction * spherecastInfo.MaxDistance,
                spherecastInfo.Radius);
        }
    }
}

internal sealed class BoxOverlapDrawHandler : GizmoDrawHandler
{
    private readonly BoxOverlapInfo _info;

    public BoxOverlapDrawHandler(BoxOverlapInfo info, float fadeTime) : base(fadeTime)
    {
        _info = info;
    }

    public override void Draw()
    {
        //Set new color
        Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r,
            VisualCastDrawer.Instance.HitColor.g,
            VisualCastDrawer.Instance.HitColor.b,
            CurrentAlpha);

        void DrawCube(Vector3 center)
        {
            Vector3 lineEnd;


            //Top front line
            var lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top back line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom front line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom back line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Front left line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Back left line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Front right line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Back right line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top left line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom left line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.left * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Top right line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.up * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);


            //Bottom right line
            lineOrigin = lineEnd = center;
            lineOrigin += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                          _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                          _info.Rotation * Vector3.back * _info.Size.z * 0.5f;

            lineEnd += _info.Rotation * Vector3.down * _info.Size.y * 0.5f +
                       _info.Rotation * Vector3.right * _info.Size.x * 0.5f +
                       _info.Rotation * Vector3.forward * _info.Size.z * 0.5f;

            Gizmos.DrawLine(lineOrigin, lineEnd);
        }

        DrawCube(_info.Center);

        DrawHits();

        //Draw points for each hit
        void DrawHits()
        {
            foreach (var hit in _info.Hits)
            {
                if (!hit) continue;

                Gizmos.DrawSphere(hit.transform.position, VisualCastDrawer.Instance.HitIndicatorScale);
            }
        }
    }
}

internal sealed class SphereOverlapDrawHandler : GizmoDrawHandler
{
    private readonly SphereOverlapInfo _info;

    public SphereOverlapDrawHandler(SphereOverlapInfo info, float fadeTime) : base(fadeTime)
    {
        _info = info;
    }

    public override void Draw()
    {
        Gizmos.color = new Color(VisualCastDrawer.Instance.HitColor.r, VisualCastDrawer.Instance.HitColor.g, VisualCastDrawer.Instance.HitColor.b, CurrentAlpha);

        Gizmos.DrawWireSphere(_info.Center, _info.Radius);

        DrawHits();
        
        /// Draw points for each hit
        void DrawHits()
        {
            foreach (var col in _info.Hits)
            {
                if (!col) continue;

                var hitObjectTranform = col.transform;
                Gizmos.DrawSphere(hitObjectTranform.position, VisualCastDrawer.Instance.HitIndicatorScale);
            }
        }
    }
}

public class VisualCastDrawer : MonoSingleton<VisualCastDrawer>
{
    public float FadeTime
    {
        get => _fadeTime;
        set => _fadeTime = Mathf.Clamp(value, 0, float.MaxValue);
    }

    public float HitIndicatorScale
    {
        get => _hitIndicatorScale;
        set => _hitIndicatorScale = Mathf.Clamp(value, 0.01f, 0.5f);
    }

    public Color HitColor
    {
        get => _hitColor;
        set => _hitColor = value;
    }

    public Color NoHitColor
    {
        get => _noHitColor;
        set => _noHitColor = value;
    }

    //------FIELDS
    [SerializeField, Min(0f)] private float _fadeTime = 1f;

    [SerializeField] private Color _hitColor = Color.green, _noHitColor = Color.red;

    [SerializeField, Range(0.01f, 0.5f)] private float _hitIndicatorScale = 0.1f;

    private readonly List<GizmoDrawHandler> _drawHandlers = new();
    

    private void OnDrawGizmos()
    {
        List<GizmoDrawHandler> toRemove = new List<GizmoDrawHandler>();
        foreach (var item in _drawHandlers)
        {
            //Cache previous color
            Color previousColor = Gizmos.color;

            item.Draw();

            if (item.TimeLeft <= 0)
                toRemove.Add(item);

            //Reset color
            Gizmos.color = previousColor;
        }

        foreach (var item in toRemove)
        {
            _drawHandlers.Remove(item);
        }
    }

    public void DrawGizmo(ICastInfo info)
    {
        switch (info)
        {
            case RaycastInfo:
                _drawHandlers.Add(new RaycastDrawHandler(info, _fadeTime));
                break;
            case BoxcastInfo:
                _drawHandlers.Add(new BoxCastDrawHandler(info, _fadeTime));
                break;
            case SpherecastInfo:
                _drawHandlers.Add(new SpherecastDrawHandler(info, _fadeTime));
                break;
        }
    }

    internal void DrawGizmo(BoxOverlapInfo info)
    {
        _drawHandlers.Add(new BoxOverlapDrawHandler(info, _fadeTime));
    }

    internal void DrawGizmo(SphereOverlapInfo info)
    {
        _drawHandlers.Add(new SphereOverlapDrawHandler(info, _fadeTime));
    }
}