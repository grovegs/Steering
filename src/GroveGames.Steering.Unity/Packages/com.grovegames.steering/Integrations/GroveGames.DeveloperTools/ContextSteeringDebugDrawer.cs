using System;
using System.Collections.Generic;
using GroveGames.DependencyInjection;
using UnityEngine;
using UnityEngine.Rendering;

namespace GroveGames.Steering.Unity
{
    public sealed class ContextSteeringDebugDrawer : IPostFrameUpdatable, IDisposable
    {
        private const string Name = nameof(ContextSteeringDebugDrawer);
        private const string ShaderName = "Sprites/Default";
        private const int SortingOrder = 30000;
        private const int RingSegmentCount = 32;
        private const int InitialVertexCapacity = 2048;
        private const float FullCircle = Mathf.PI * 2f;
        private const float MapRadius = 1.5f;
        private const float InterestWidth = 0.07f;
        private const float DangerWidth = 0.16f;
        private const float ThinWidth = 0.03f;
        private const float DirectionWidth = 0.09f;
        private const float PointSize = 0.2f;

        private static readonly Color s_ringColor = new(1f, 1f, 1f, 0.35f);
        private static readonly Color s_interestColor = new(0.2f, 1f, 0.35f, 1f);
        private static readonly Color s_blockedColor = new(0.45f, 0.45f, 0.45f, 1f);
        private static readonly Color s_dangerColor = new(1f, 0.15f, 0.15f, 0.8f);
        private static readonly Color s_directionColor = new(1f, 0.9f, 0.1f, 1f);

        private readonly IContextSteering _steering;
        private readonly List<Vector3> _vertices;
        private readonly List<Color32> _colors;
        private readonly List<int> _triangles;
        private GameObject? _gameObject;
        private MeshRenderer? _renderer;
        private Mesh? _mesh;
        private Material? _material;
        private bool _isVisible;

        public ContextSteeringDebugDrawer(IContextSteering steering)
        {
            _steering = steering;
            _vertices = new List<Vector3>(InitialVertexCapacity);
            _colors = new List<Color32>(InitialVertexCapacity);
            _triangles = new List<int>(InitialVertexCapacity);
        }

        public void PostFrameUpdate(DeltaTime deltaTime)
        {
            if (!_isVisible || _mesh == null)
            {
                return;
            }

            if (_renderer == null)
            {
                Create();
            }

            var snapshots = _steering.Snapshots;
            _vertices.Clear();
            _colors.Clear();
            _triangles.Clear();

            for (var i = 0; i < snapshots.Count; i++)
            {
                if (snapshots[i].IsRecorded)
                {
                    Draw(snapshots[i]);
                    snapshots[i].Clear();
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        public void Dispose()
        {
            _steering.SetRecording(false);
            Destroy(_gameObject);
            Destroy(_mesh);
            Destroy(_material);
        }

        public void SetVisible(bool isVisible)
        {
            _isVisible = isVisible;
            _steering.SetRecording(isVisible);

            if (_renderer == null)
            {
                if (!isVisible)
                {
                    return;
                }

                Create();
            }

            _renderer!.enabled = isVisible;
        }

        private void Create()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh { name = Name, indexFormat = IndexFormat.UInt32 };
                _mesh.MarkDynamic();
            }

            _gameObject = new GameObject(Name);
            _gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = _gameObject.AddComponent<MeshRenderer>();
            _renderer.sortingOrder = SortingOrder;

            if (_material == null)
            {
                var shader = Shader.Find(ShaderName);

                if (shader != null)
                {
                    _material = new Material(shader);
                }
            }

            _renderer.sharedMaterial = _material;
        }

        private void Draw(ContextSteeringSnapshot snapshot)
        {
            var center = ToVector2(snapshot.Position);

            for (var i = 0; i < RingSegmentCount; i++)
            {
                AddLine(GetRingPoint(center, i), GetRingPoint(center, i + 1), ThinWidth, s_ringColor);
            }

            for (var i = 0; i < snapshot.SlotCount; i++)
            {
                AddLine(center, center + ToVector2(_steering.Danger.GetDirection(i)) * (snapshot.GetDanger(i) * MapRadius), DangerWidth, s_dangerColor);
            }

            for (var i = 0; i < snapshot.SlotCount; i++)
            {
                AddLine(center, center + ToVector2(_steering.Interest.GetDirection(i)) * (snapshot.GetInterest(i) * MapRadius), InterestWidth, snapshot.IsBlocked(i) ? s_blockedColor : s_interestColor);
            }

            for (var i = 0; i < snapshot.DangerPoints.Count; i++)
            {
                AddPoint(center, ToVector2(snapshot.DangerPoints[i]), s_dangerColor);
            }

            for (var i = 0; i < snapshot.InterestPoints.Count; i++)
            {
                AddPoint(center, ToVector2(snapshot.InterestPoints[i]), s_interestColor);
            }

            AddLine(center, center + ToVector2(snapshot.Direction) * MapRadius, DirectionWidth, s_directionColor);
        }

        private static Vector2 ToVector2(System.Numerics.Vector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        private static Vector2 GetRingPoint(Vector2 center, int segment)
        {
            var angle = FullCircle * segment / RingSegmentCount;
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * MapRadius;
        }

        private void AddPoint(Vector2 center, Vector2 point, Color color)
        {
            AddLine(center, point, ThinWidth, color);
            AddQuad(point + Vector2.left * PointSize, point + Vector2.up * PointSize, point + Vector2.right * PointSize, point + Vector2.down * PointSize, color);
        }

        private void AddLine(Vector2 from, Vector2 to, float width, Color color)
        {
            var along = to - from;

            if (along.sqrMagnitude <= 0f)
            {
                return;
            }

            var side = new Vector2(-along.y, along.x).normalized * (width * 0.5f);
            AddQuad(from - side, from + side, to + side, to - side, color);
        }

        private void AddQuad(Vector2 first, Vector2 second, Vector2 third, Vector2 fourth, Color color)
        {
            var start = _vertices.Count;
            _vertices.Add(first);
            _vertices.Add(second);
            _vertices.Add(third);
            _vertices.Add(fourth);

            for (var i = 0; i < 4; i++)
            {
                _colors.Add(color);
            }

            _triangles.Add(start);
            _triangles.Add(start + 1);
            _triangles.Add(start + 2);
            _triangles.Add(start);
            _triangles.Add(start + 2);
            _triangles.Add(start + 3);
        }

        private static void Destroy(UnityEngine.Object? target)
        {
            if (target != null)
            {
                UnityEngine.Object.Destroy(target);
            }
        }
    }
}
