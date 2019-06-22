using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Cobalt.UI
{
    public class UIManager : MonoBehaviour
    {
        public Transform root;
        private Dictionary<Type, UIElement> elements;
        private List<UIPopup> queue;

        private List<Relation> relations;
        private bool relationsIsDirty;

        public UIManager()
        {
            elements = new Dictionary<Type, UIElement>();
            queue = new List<UIPopup>();
            relations = new List<Relation>();
        }

        public T Get<T>() where T : UIElement
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

        internal void Enqueue(UIPopup popup, bool enqueue)
        {
            if (queue.IndexOf(popup) == -1)
            {
                if (enqueue) queue.Add(popup);
                else queue.Insert(0, popup);
            }
        }

        internal void Dequeue(UIPopup popup)
        {
            var indexOf = queue.IndexOf(popup);
            if (indexOf != -1) queue.RemoveAt(indexOf);
        }

        internal bool IsAwait(UIPopup popup)
        {
            return queue.IndexOf(popup) != -1   // Попап в очереди
                || popup.IsShow == true         // или сейчас отображается
                || popup.IsTransit == true;     // или сейчас проигрывается анимация закрытия
        }

        internal bool IsTopmost(UIPopup popup)
        {
            return queue.Count > 1
                && queue[0] == popup;
        }

        private void Update()
        {
            if (queue.Count > 0)
            {
                var topmost = queue[0];
                if (topmost.IsShow) return;
                StartCoroutine(UpdateCoroutine());
            }
        }

        private IEnumerator UpdateCoroutine()
        {
            if (queue.Count > 0)
            {
                var topmost = queue[0];
                if (topmost.IsShow) yield break;

                // Shade
                var shaded = queue.Count > 1 && queue[1].IsShow ? queue[1] : null;
                if (shaded != null) shaded.ShadeOn();

                // Show
                topmost.gameObject.SetActive(true);
                topmost.IsShow = true;
                topmost.IsTransit = true;
                yield return topmost.Show();
                topmost.IsTransit = false;

                // Await for Dequeue()
                while (queue.IndexOf(topmost) != -1) yield return null;

                // Hide
                topmost.IsShow = false;
                topmost.IsTransit = true;
                yield return topmost.Hide();
                topmost.IsTransit = false;
                topmost.gameObject.SetActive(false);

                // Unshade
                if (shaded != null) shaded.ShadeOff();
            }
        }

        private void LateUpdate()
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
                    yield return panel.Show();
                    panel.IsTransit = false;
                }
                else if (state <= 0 && panel.IsShow)
                {
                    panel.IsShow = false;
                    panel.IsTransit = true;
                    yield return panel.Hide();
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
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement : MonoBehaviour
    {
        internal UIManager UIManager { get; set; }
        internal protected virtual IEnumerator Show() { yield break; }
        internal protected virtual IEnumerator Hide() { yield break; }

        public bool IsShow { get; internal set; }
        public bool IsTransit { get; internal set; }
    }

    public class UIPanel : UIElement
    {
        public void Require(object token, int state) { UIManager.Require(this, token, state); }
    }

    public class UIPopup : UIElement
    {
        public void Open(bool enqueue = false) { UIManager.Enqueue(this, enqueue); }
        public void Close() { UIManager.Dequeue(this); }

        public CustomYieldInstruction Await() { return new WaitWhile(AwaitPredicate); }
        private bool AwaitPredicate() { return UIManager.IsAwait(this); }

        internal protected virtual void ShadeOn() { }
        internal protected virtual void ShadeOff() { }

        public bool IsTopmost { get { return UIManager.IsTopmost(this); } }
    }
}