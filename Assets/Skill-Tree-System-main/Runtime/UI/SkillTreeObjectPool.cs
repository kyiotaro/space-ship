using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.UI
{
    /// <summary>
    /// Object pool for skill tree UI elements (nodes, connections).
    /// Reduces garbage collection and improves performance when showing/hiding large skill trees.
    /// </summary>
    public class SkillTreeObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _container;
        private readonly Queue<T> _available = new Queue<T>();
        private readonly HashSet<T> _inUse = new HashSet<T>();
        private readonly int _initialSize;
        private readonly int _maxSize;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="container">Parent transform for pooled objects</param>
        /// <param name="initialSize">Number of objects to pre-instantiate</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        public SkillTreeObjectPool(T prefab, Transform container, int initialSize = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _container = container;
            _initialSize = initialSize;
            _maxSize = maxSize;

            PrewarmPool();
        }

        /// <summary>
        /// Gets an object from the pool. Creates a new one if pool is empty.
        /// </summary>
        public T Get()
        {
            T obj;

            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            _inUse.Add(obj);
            obj.gameObject.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        public void Return(T obj)
        {
            if (!_inUse.Remove(obj))
            {
                Debug.LogWarning($"Attempted to return object that wasn't from this pool: {obj.name}");
                return;
            }

            obj.gameObject.SetActive(false);

            // Check max size before returning to pool
            if (_maxSize == 0 || _available.Count < _maxSize)
            {
                _available.Enqueue(obj);
            }
            else
            {
                // Pool is full, destroy the object
                Object.Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// Returns all in-use objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            var inUseList = new List<T>(_inUse);
            foreach (var obj in inUseList)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// Clears the pool and destroys all objects.
        /// </summary>
        public void Clear()
        {
            // Destroy available objects
            while (_available.Count > 0)
            {
                var obj = _available.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            // Destroy in-use objects
            foreach (var obj in _inUse)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _inUse.Clear();
        }

        /// <summary>
        /// Current number of available objects in the pool.
        /// </summary>
        public int AvailableCount => _available.Count;

        /// <summary>
        /// Current number of objects in use.
        /// </summary>
        public int InUseCount => _inUse.Count;

        /// <summary>
        /// Total number of objects managed by this pool.
        /// </summary>
        public int TotalCount => _available.Count + _inUse.Count;

        private void PrewarmPool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        private T CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _container);
            obj.name = $"{_prefab.name}_{TotalCount}";
            return obj;
        }
    }

    /// <summary>
    /// Manages object pools for the skill tree UI system.
    /// Provides centralized pooling for nodes and connections.
    /// </summary>
    public class SkillTreePoolManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private SkillNodeUI _nodeUIPrefab;
        [SerializeField] private SkillConnectionUI _connectionUIPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int _initialNodePoolSize = 20;
        [SerializeField] private int _maxNodePoolSize = 100;
        [SerializeField] private int _initialConnectionPoolSize = 30;
        [SerializeField] private int _maxConnectionPoolSize = 200;

        [Header("Containers")]
        [SerializeField] private Transform _nodeContainer;
        [SerializeField] private Transform _connectionContainer;

        private SkillTreeObjectPool<SkillNodeUI> _nodePool;
        private SkillTreeObjectPool<SkillConnectionUI> _connectionPool;

        private void Awake()
        {
            InitializePools();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }

        private void InitializePools()
        {
            if (_nodeUIPrefab == null)
            {
                Debug.LogError("Node UI prefab not assigned to SkillTreePoolManager");
                return;
            }

            if (_connectionUIPrefab == null)
            {
                Debug.LogError("Connection UI prefab not assigned to SkillTreePoolManager");
                return;
            }

            // Create containers if not assigned
            if (_nodeContainer == null)
            {
                _nodeContainer = new GameObject("NodePool").transform;
                _nodeContainer.SetParent(transform);
            }

            if (_connectionContainer == null)
            {
                _connectionContainer = new GameObject("ConnectionPool").transform;
                _connectionContainer.SetParent(transform);
            }

            // Initialize pools
            _nodePool = new SkillTreeObjectPool<SkillNodeUI>(
                _nodeUIPrefab,
                _nodeContainer,
                _initialNodePoolSize,
                _maxNodePoolSize
            );

            _connectionPool = new SkillTreeObjectPool<SkillConnectionUI>(
                _connectionUIPrefab,
                _connectionContainer,
                _initialConnectionPoolSize,
                _maxConnectionPoolSize
            );

            Debug.Log($"Skill Tree pools initialized: {_nodePool.TotalCount} nodes, {_connectionPool.TotalCount} connections");
        }

        /// <summary>
        /// Gets a node UI from the pool.
        /// </summary>
        public SkillNodeUI GetNode()
        {
            return _nodePool.Get();
        }

        /// <summary>
        /// Returns a node UI to the pool.
        /// </summary>
        public void ReturnNode(SkillNodeUI node)
        {
            _nodePool.Return(node);
        }

        /// <summary>
        /// Gets a connection UI from the pool.
        /// </summary>
        public SkillConnectionUI GetConnection()
        {
            return _connectionPool.Get();
        }

        /// <summary>
        /// Returns a connection UI to the pool.
        /// </summary>
        public void ReturnConnection(SkillConnectionUI connection)
        {
            _connectionPool.Return(connection);
        }

        /// <summary>
        /// Returns all nodes and connections to their respective pools.
        /// </summary>
        public void ReturnAll()
        {
            _nodePool.ReturnAll();
            _connectionPool.ReturnAll();
        }

        /// <summary>
        /// Clears all pools and destroys all objects.
        /// </summary>
        public void ClearAllPools()
        {
            _nodePool?.Clear();
            _connectionPool?.Clear();
        }

        /// <summary>
        /// Gets pool statistics for debugging and optimization.
        /// </summary>
        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                NodesInUse = _nodePool?.InUseCount ?? 0,
                NodesAvailable = _nodePool?.AvailableCount ?? 0,
                ConnectionsInUse = _connectionPool?.InUseCount ?? 0,
                ConnectionsAvailable = _connectionPool?.AvailableCount ?? 0
            };
        }
    }

    /// <summary>
    /// Statistics about object pool usage.
    /// </summary>
    public struct PoolStatistics
    {
        public int NodesInUse;
        public int NodesAvailable;
        public int ConnectionsInUse;
        public int ConnectionsAvailable;

        public int TotalNodes => NodesInUse + NodesAvailable;
        public int TotalConnections => ConnectionsInUse + ConnectionsAvailable;

        public override string ToString()
        {
            return $"Nodes: {NodesInUse}/{TotalNodes} | Connections: {ConnectionsInUse}/{TotalConnections}";
        }
    }
}
