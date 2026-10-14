using UnityEngine;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using SkillTree.Core.Events;

#if VCONTAINER_INSTALLED
using VContainer;
using VContainer.Unity;
#endif

namespace SkillTree.Integration.VContainer
{
    /// <summary>
    /// VContainer installer for the Skill Tree system.
    /// Registers all core services for dependency injection.
    ///
    /// Usage:
    /// 1. Add this component to a GameObject in your scene
    /// 2. Assign a SkillTreeGraph to the inspector
    /// 3. The installer will automatically register services with VContainer
    ///
    /// Note: Requires VContainer package to be installed.
    /// If VContainer is not installed, this class will be a no-op MonoBehaviour.
    /// </summary>
#if VCONTAINER_INSTALLED
    public class SkillTreeInstaller : LifetimeScope
#else
    public class SkillTreeInstaller : MonoBehaviour
#endif
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("The skill tree graph to use for this installer")]
        private SkillTreeGraph _skillTreeGraph;

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [SerializeField, Tooltip("Whether to use a singleton event bus (shared across all skill trees)")]
        private bool _useSingletonEventBus = true;

        [SerializeField, Tooltip("Automatically load state on start")]
        private bool _autoLoadState = false;

        [SerializeField, Tooltip("Save key for auto-load/save")]
        private string _saveKey = "default_skill_tree";
#pragma warning restore CS0414

#if VCONTAINER_INSTALLED
        protected override void Configure(IContainerBuilder builder)
        {
            if (_skillTreeGraph == null)
            {
                Debug.LogError("[SkillTreeInstaller] No SkillTreeGraph assigned! Cannot register services.");
                return;
            }

            // Register the skill tree graph as an instance
            builder.RegisterInstance(_skillTreeGraph).AsSelf();

            // Register event bus
            if (_useSingletonEventBus)
            {
                builder.Register<ISkillTreeEventBus, SkillTreeEventBus>(Lifetime.Singleton);
            }
            else
            {
                builder.Register<ISkillTreeEventBus, SkillTreeEventBus>(Lifetime.Scoped);
            }

            // Register core services
            builder.Register<SkillTreeManager>(Lifetime.Singleton)
                .As<ISkillTreeManager>()
                .AsSelf();

            builder.Register<SkillUnlockValidator>(Lifetime.Singleton).AsSelf();
            builder.Register<SkillTreeTraverser>(Lifetime.Singleton).AsSelf();

            Debug.Log($"[SkillTreeInstaller] Registered skill tree services for graph: {_skillTreeGraph.DisplayName}");
        }

        protected override void Awake()
        {
            base.Awake();

            if (_autoLoadState && !string.IsNullOrEmpty(_saveKey))
            {
                // Auto-load will be implemented when save system integration is added
                Debug.Log($"[SkillTreeInstaller] Auto-load is enabled but not yet implemented");
            }
        }
#else
        private void Awake()
        {
            Debug.LogWarning("[SkillTreeInstaller] VContainer is not installed. This installer will not register any services.\n" +
                           "Install VContainer from the Unity Package Manager to enable dependency injection.");
        }
#endif

        /// <summary>
        /// Gets the configured skill tree graph.
        /// </summary>
        public SkillTreeGraph GetSkillTreeGraph()
        {
            return _skillTreeGraph;
        }

        /// <summary>
        /// Sets the skill tree graph at runtime.
        /// Note: This will not affect already-resolved dependencies.
        /// </summary>
        public void SetSkillTreeGraph(SkillTreeGraph graph)
        {
            _skillTreeGraph = graph;
        }
    }
}
