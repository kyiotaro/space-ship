using System.Collections.Generic;
using UnityEngine;
using SkillTree.Core.Data;

namespace SkillTree.UI
{
    /// <summary>
    /// Culling system for skill tree UI to improve performance with large trees.
    /// Only renders nodes and connections that are visible in the viewport.
    /// </summary>
    public class SkillTreeCullingSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private float _cullingMargin = 100f; // Extra pixels around viewport
        [SerializeField] private bool _enableCulling = true;
        [SerializeField] private float _updateInterval = 0.1f; // Update culling every 0.1 seconds

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;

        private readonly List<ICullable> _cullableObjects = new List<ICullable>();
        private float _lastUpdateTime;
        private Rect _viewportRect;
        private int _visibleCount;
        private int _culledCount;

        private void Update()
        {
            if (!_enableCulling)
                return;

            // Update culling at intervals to reduce overhead
            if (Time.time - _lastUpdateTime < _updateInterval)
                return;

            _lastUpdateTime = Time.time;
            UpdateCulling();
        }

        /// <summary>
        /// Registers an object for culling.
        /// </summary>
        public void RegisterCullable(ICullable cullable)
        {
            if (!_cullableObjects.Contains(cullable))
            {
                _cullableObjects.Add(cullable);
            }
        }

        /// <summary>
        /// Unregisters an object from culling.
        /// </summary>
        public void UnregisterCullable(ICullable cullable)
        {
            _cullableObjects.Remove(cullable);
        }

        /// <summary>
        /// Clears all registered cullable objects.
        /// </summary>
        public void ClearAll()
        {
            _cullableObjects.Clear();
        }

        /// <summary>
        /// Forces an immediate culling update.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateCulling();
        }

        private void UpdateCulling()
        {
            if (_viewport == null)
            {
                Debug.LogWarning("Viewport not assigned to SkillTreeCullingSystem");
                return;
            }

            // Calculate viewport bounds with margin
            _viewportRect = GetViewportRect();

            _visibleCount = 0;
            _culledCount = 0;

            // Update visibility for each cullable object
            foreach (var cullable in _cullableObjects)
            {
                if (cullable == null)
                    continue;

                bool isVisible = IsInViewport(cullable.GetBounds());
                cullable.SetVisible(isVisible);

                if (isVisible)
                    _visibleCount++;
                else
                    _culledCount++;
            }

            if (_showDebugInfo)
            {
                Debug.Log($"Culling: {_visibleCount} visible, {_culledCount} culled");
            }
        }

        private Rect GetViewportRect()
        {
            if (_viewport == null)
                return new Rect();

            Vector3[] corners = new Vector3[4];
            _viewport.GetWorldCorners(corners);

            float minX = corners[0].x - _cullingMargin;
            float minY = corners[0].y - _cullingMargin;
            float maxX = corners[2].x + _cullingMargin;
            float maxY = corners[2].y + _cullingMargin;

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private bool IsInViewport(Bounds bounds)
        {
            // Convert bounds to viewport space
            Vector2 min = new Vector2(bounds.min.x, bounds.min.y);
            Vector2 max = new Vector2(bounds.max.x, bounds.max.y);

            // Check if bounds intersect with viewport
            return _viewportRect.Overlaps(new Rect(min, max - min));
        }

        public void OnDrawGizmos()
        {
            if (!_showDebugInfo || !Application.isPlaying)
                return;

            // Draw viewport bounds
            Gizmos.color = Color.green;
            Vector3 center = _viewportRect.center;
            Vector3 size = _viewportRect.size;
            Gizmos.DrawWireCube(center, size);
        }

        /// <summary>
        /// Gets culling statistics.
        /// </summary>
        public CullingStatistics GetStatistics()
        {
            return new CullingStatistics
            {
                TotalObjects = _cullableObjects.Count,
                VisibleObjects = _visibleCount,
                CulledObjects = _culledCount,
                CullingEnabled = _enableCulling
            };
        }
    }

    /// <summary>
    /// Interface for objects that can be culled.
    /// </summary>
    public interface ICullable
    {
        /// <summary>
        /// Gets the world-space bounds of the object.
        /// </summary>
        Bounds GetBounds();

        /// <summary>
        /// Sets whether the object should be visible.
        /// </summary>
        void SetVisible(bool visible);
    }

    /// <summary>
    /// Statistics about culling performance.
    /// </summary>
    public struct CullingStatistics
    {
        public int TotalObjects;
        public int VisibleObjects;
        public int CulledObjects;
        public bool CullingEnabled;

        public float CullingRatio => TotalObjects > 0 ? (float)CulledObjects / TotalObjects : 0f;

        public override string ToString()
        {
            return $"Culling: {VisibleObjects}/{TotalObjects} visible ({CullingRatio:P0} culled) | Enabled: {CullingEnabled}";
        }
    }
}
