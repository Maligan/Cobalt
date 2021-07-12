using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Cobalt.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _roots = null;

        private Dictionary<Type, UIPanel> _panels;
        private Dictionary<UIPanel, Coroutine> _coroutines;
        private List<Relation> _relations;
        private bool _relationsIsDirty;

        public UIManager()
        {
            _panels = new Dictionary<Type, UIPanel>();
            _coroutines = new Dictionary<UIPanel, Coroutine>();
            _relations = new List<Relation>();
        }

        private void Start()
        {
            foreach (var root in _roots)
                RegisterAll(root);
        }

        public void RegisterAll(Transform root)
        {
            var components = root.GetComponentsInChildren<UIPanel>(true);

            foreach (var panel in components)
               Register(panel);
        }

        public void Register(UIPanel panel)
        {
            Debug.Assert(
                panel.transform.parent?.GetComponentInParent<UIPanel>() == null,
                $"UIPanel '{panel.name}' mustn't be nested"
            );

            _panels.Add(panel.GetType(), panel);
            panel.UIManager = this;
            panel.gameObject.SetActive(false);
        }

        public T Get<T>() where T : UIPanel
        {
            var panelType = typeof(T);

            if (_panels.ContainsKey(panelType) == false)
                throw new Exception($"{nameof(UIPanel)} with type '{panelType.Name}' not found");

            return (T)_panels[panelType];
        }

        internal void Require(UIPanel panel, object token, int state)
        {
            var relation = _relations.FirstOrDefault(x => x.panel == panel && x.token == token);
            if (relation == null && state != 0)
            {
                // Create new relation
                _relationsIsDirty = true;
                _relations.Add(new Relation()
                {
                    panel = panel,
                    token = token,
                    state = state
                });
            }
            else if (relation != null && relation.state != state)
            {
                // Update exist relation
                _relationsIsDirty = true;
                relation.state = state;
            }
        }

        private void Update()
        {
            if (_relationsIsDirty)
            {
                _relationsIsDirty = false;

                // Calculate UIPanels strongest relation
                var dominants = new Dictionary<UIPanel, Relation>();
                foreach (var relation in _relations)
                {
                    var needUpdate = !(dominants.ContainsKey(relation.panel))
                                  || !(dominants[relation.panel] > relation);  

                    if (needUpdate) dominants[relation.panel] = relation;
                }

                // Apply relations
                foreach (var relation in dominants.Values)
                {
                    // Cancel previous coroutine (if it's not completed)
                    var hasCoroutine = _coroutines.TryGetValue(relation.panel, out Coroutine coroutine);
                    if (hasCoroutine && coroutine != null)
                        StopCoroutine(coroutine);
                    
                    _coroutines[relation.panel] = StartCoroutine(relation.Apply());
                }
            }
        }

        private class Relation
        {
            public object token;
            public UIPanel panel;
            public int state;

            public IEnumerator Apply()
            {
                if (state > 0 && !panel.IsShow)
                    yield return panel.ShowRoutine();
                else if (state <= 0 && panel.IsShow)
                    yield return panel.HideRoutine();
            }

            public static bool operator <(Relation r1, Relation r2) { return !(r1 > r2); }
            public static bool operator >(Relation r1, Relation r2)
            {
                if (r1.token != r2.token) return false;
                if (r1.panel != r2.panel) return false;
                return Mathf.Abs(r1.state) > Mathf.Abs(r2.state);
            }
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public class UIPanel : MonoBehaviour
    {
        // Internal API
        internal UIManager UIManager { get; set; }

        internal IEnumerator ShowRoutine()
        {
            gameObject.SetActive(true);
            IsShow = true;
            IsTransit = true;
            yield return OnShow();
            IsTransit = false;
        }

        internal IEnumerator HideRoutine()
        {
            IsShow = false;
            IsTransit = true;
            yield return OnHide();
            IsTransit = false;
            gameObject.SetActive(false);
        }

        // Show/Hide Routines
        protected virtual IEnumerator OnShow() { yield break; }
        protected virtual IEnumerator OnHide() { yield break; }

        // Open/Close
        public void Show() { Require(this, 1); }
        public void Hide() { Require(this, 0); }
        public void Require(object token, int state) { UIManager.Require(this, token, state); }

        public bool IsShow { get; internal set; }
        public bool IsTransit { get; internal set; }

        #if UNITY_EDITOR
        public void OnValidate() { gameObject.name = GetType().Name; }
		#endif

        // Tools
        protected IEnumerator PlayAndAwait(string state)
        {
            var animator = GetComponent<Animator>();
            if (animator != null && animator.enabled)
            {
                animator.Play(state);
                yield return null;

                var layer = animator.GetCurrentAnimatorStateInfo(0);
                var layerTime = layer.length;
                yield return new WaitForSeconds(layerTime);
            }
        }

        protected IEnumerator PlayAndAwait(AnimationClip clip)
        {
            if (clip != null)
            {
                var animator = GetComponent<Animator>();
                if (animator == null)
                    animator = gameObject.AddComponent<Animator>();

                if (animator.enabled)
                {
                    AnimationPlayableUtilities.PlayClip(animator, clip, out PlayableGraph graph);
                    yield return new WaitForSeconds(clip.length);
                    graph.Destroy();
                }
            }
            else
            {
                Debug.LogWarning("AnimationClip can't be null");
            }
        }

		public IEnumerator WaitForShow() { while (!IsShow || IsTransit) yield return null; }
		public IEnumerator WaitForHide() { while (IsShow || IsTransit) yield return null; }
    }
}
