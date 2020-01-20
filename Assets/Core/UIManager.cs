using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cobalt;
using UnityEngine;

namespace Cobalt.UI
{
    public class UIManager : MonoBehaviour
    {
        public Transform root;
        private Dictionary<Type, UIPanel> elements;

        private List<Relation> relations;
        private bool relationsIsDirty;

        public UIManager()
        {
            elements = new Dictionary<Type, UIPanel>();
            relations = new List<Relation>();
        }

        public void Start()
        {
            var elements = root.GetComponentsInChildren<UIPanel>();

            // Disable all active objects            
            foreach (var element in elements)
                if (element.gameObject.activeSelf)
                    element.gameObject.SetActive(false);
        }

        public T Get<T>() where T : UIPanel
        {
            var key = typeof(T);

            if (elements.ContainsKey(key) == false)
            {
                if (root == null)
                    throw new Exception("");

                var value = root.GetComponentInChildren<T>(true);
                if (value == null)
                    throw new Exception("");

                value.UIManager = this;
                elements[key] = value;
            }

            return (T)elements[key];
        }

        internal void Require(UIPanel panel, object token, int state)
        {
            var relation = relations.FirstOrDefault(x => x.panel == panel && x.token == token);
            if (relation == null && state != 0)
            {
                // Create new relation
                relationsIsDirty = true;
                relations.Add(new Relation()
                {
                    panel = panel,
                    token = token,
                    state = state
                });
            }
            else if (relation != null && relation.state != state)
            {
                // Update exist relation
                relationsIsDirty = true;
                relation.state = state;
            }
        }

        private void Update()
        {
            if (relationsIsDirty)
            {
                relationsIsDirty = false;

                // Calculate UIPanels strongest relation
                var dominants = new Dictionary<UIPanel, Relation>();
                foreach (var relation in relations)
                {
                    var needUpdate = !(dominants.ContainsKey(relation.panel))
                                  || !(dominants[relation.panel] > relation);  

                    if (needUpdate) dominants[relation.panel] = relation;
                }

                // Apply relations
                foreach (var relation in dominants.Values)
                    StartCoroutine(relation.Apply());
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
                {
                    panel.gameObject.SetActive(true);
                    panel.IsShow = true;
                    panel.IsTransit = true;
                    yield return panel.ShowInternal();
                    panel.IsTransit = false;
                }
                else if (state <= 0 && panel.IsShow)
                {
                    panel.IsShow = false;
                    panel.IsTransit = true;
                    yield return panel.HideInternal();
                    panel.IsTransit = false;
                    panel.gameObject.SetActive(false);
                }
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
        internal IEnumerator ShowInternal() { yield return Show(); }
        internal IEnumerator HideInternal() { yield return Hide(); }

        // Show/Hide Routines
        protected virtual IEnumerator Show() { yield break; }
        protected virtual IEnumerator Hide() { yield break; }

        // Open/Close
        public void Require(object token, int state) { UIManager.Require(this, token, state); }
        public void Open() { UIManager.Require(this, this, 1); }
        public void Close() { UIManager.Require(this, this, 0); }

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

		public IEnumerator WaitForOpen() { while (!IsShow || IsTransit) yield return null; }
		public IEnumerator WaitForClose() { while (IsShow || IsTransit) yield return null; }
    }
}
