using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _roots = null;

    private Dictionary<Type, UIBehaviour> _elements;
    private Dictionary<UIBehaviour, Coroutine> _elementsCoroutine;
    private List<Relation> _relations;
    private bool _relationsIsDirty;

    public UIManager()
    {
        _elements = new Dictionary<Type, UIBehaviour>();
        _elementsCoroutine = new Dictionary<UIBehaviour, Coroutine>();
        _relations = new List<Relation>();
    }

    private void Start()
    {
        foreach (var root in _roots)
            RegisterAll(root);
    }

    public void RegisterAll(Transform root)
    {
        var components = root.GetComponentsInChildren<UIBehaviour>(true);

        foreach (var element in components)
            Register(element);
    }

    public void Register(UIBehaviour element)
    {
        Debug.Assert(
            element.transform.parent == null ||
            element.transform.parent.GetComponentInParent<UIPanel>() == null,
            $"UIPanel '{element.name}' mustn't be nested"
        );

        _elements.Add(element.GetType(), element);
        ((IUIBehaviour)element).UI = this;
        element.gameObject.SetActive(false);
    }

    public T Get<T>() where T : UIBehaviour
    {
        var elementType = typeof(T);

        if (_elements.ContainsKey(elementType) == false)
            throw new Exception($"{nameof(UIPanel)} with type '{elementType.Name}' not found");

        return (T)_elements[elementType];
    }

    private void Require(UIBehaviour element, object token, int state)
    {
        var relation = _relations.FirstOrDefault(x => x.element == element && x.token == token);
        if (relation == null && state != 0)
        {
            // Create new relation
            _relationsIsDirty = true;
            _relations.Add(new Relation()
            {
                element = element,
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

            // Calculate UIBehaviours strongest relation
            var dominants = new Dictionary<UIBehaviour, Relation>();
            foreach (var relation in _relations)
            {
                var needUpdate = !(dominants.ContainsKey(relation.element))
                                || !(dominants[relation.element] > relation);  

                if (needUpdate) dominants[relation.element] = relation;
            }

            // Apply relations
            foreach (var relation in dominants.Values)
            {
                // Cancel previous coroutine (if it's not completed)
                var hasCoroutine = _elementsCoroutine.TryGetValue(relation.element, out Coroutine coroutine);
                if (hasCoroutine && coroutine != null)
                    StopCoroutine(coroutine);
                
                _elementsCoroutine[relation.element] = StartCoroutine(relation.Apply());
            }
        }
    }

    // Tool class for toggle UIBehaviour state
    private class Relation
    {
        public object token;
        public UIBehaviour element;
        public int state;

        public IEnumerator Apply()
        {
            if (state > 0 && !element.IsShow)
                yield return ((IUIBehaviour)element).Show();
            else if (state <= 0 && element.IsShow)
                yield return ((IUIBehaviour)element).Hide();
        }

        public static bool operator <(Relation r1, Relation r2) { return !(r1 > r2); }
        public static bool operator >(Relation r1, Relation r2)
        {
            if (r1.token != r2.token) return false;
            if (r1.element != r2.element) return false;
            return Mathf.Abs(r1.state) > Mathf.Abs(r2.state);
        }
    }

    // Hidden API for UIManager <-> UIBehaviour interaction
    private interface IUIBehaviour
    {
        UIManager UI { get; set; }
        IEnumerator Show();
        IEnumerator Hide();
    }

    // Base class for any UI element
    public abstract class UIBehaviour : MonoBehaviour, IUIBehaviour
    {
        // Public API

        public bool IsShow { get; private set; }
        public bool IsTransit { get; private set; }

        public T Get<T>() where T : UIBehaviour => ((IUIBehaviour)this).UI.Get<T>();
        public void Require(object token, int state) { ((IUIBehaviour)this).UI.Require(this, token, state); }
        public void Show() { Require(this, 1); }
        public void Hide() { Require(this, 0); }

        // Protected template methods

        protected virtual IEnumerator OnShow() { yield break; }
        protected virtual IEnumerator OnHide() { yield break; }

        // IUIBehaviour
        
        UIManager IUIBehaviour.UI { get; set; }

        IEnumerator IUIBehaviour.Show()
        {
            gameObject.SetActive(true);
            IsShow = true;
            IsTransit = true;
            yield return OnShow();
            IsTransit = false;
        }

        IEnumerator IUIBehaviour.Hide()
        {
            IsShow = false;
            IsTransit = true;
            yield return OnHide();
            IsTransit = false;
            gameObject.SetActive(false);
        }
    }
}

[RequireComponent(typeof(RectTransform))]
public class UIPanel : UIManager.UIBehaviour
{
    #if UNITY_EDITOR
    public void OnValidate() { gameObject.name = GetType().Name; }
    #endif

    //
    // Tools
    //
    public IEnumerator WaitForShow() { while (!IsShow || IsTransit) yield return null; }
    public IEnumerator WaitForHide() { while (IsShow || IsTransit) yield return null; }

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
}