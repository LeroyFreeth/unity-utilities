using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScrollUtilities
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Creates a UI element that can be scrolled infinitely
    /// Resuses the same game objects by transfering data between those objects while scrolling
    /// </summary>
    /// <typeparam name="X">Needs to be a graphic, as the element needs to draw something within the UI</typeparam>
    /// <typeparam name="Y">Data type which is linked to the graphic element</typeparam>
    [RequireComponent(typeof(Mask))]
    [RequireComponent(typeof(Image))]
    public abstract class ElementScroll<X, Y> : MonoBehaviour where X : Graphic
    {
        [Header("Components")]
        [SerializeField] private Mask _mask;
        [SerializeField] private X _prefab;

        [Header("Content")]
        [SerializeField] private Y[] _content;

        [Header("Layout")]
        [SerializeField] private int _visibleElements = 3;
        [SerializeField] private float _spacing;

        [Header("Settings")]
        [SerializeField] private Orientation _orientation;
        [SerializeField] private bool _infinite;

        [SerializeField][Range(0.0f, 1.0f)] private float _scrollValue;

        private X[] _elements;
        public X[] Elements => _elements;

        private float _start;
        private float _diff;

        /// <summary>
        /// Sets the scroll value and updates the scroll positions for specified value
        /// </summary>
        /// <param name="value">Will be modulated to looping normalized value</param>
        public void SetScrollValue(float value)
        {
            // Modifies value depending on infinity. If not infinite, the last content value will be a factor in the maximum scroll value
            var t_scroll_value = _infinite ? value : value * ((float)_content.Length - (float)_visibleElements) / (float)_content.Length;
            // Convert to 0-1
            t_scroll_value = (1 + (t_scroll_value % 1)) % 1;

            float step = t_scroll_value * _content.Length;
            int l = _elements.Length;
            for (int i = 0; i < l; i++)
            {
                Vector3 pos = CalculatePositionForStep(step + i, l);
                _elements[i].rectTransform.localPosition = pos;
                ApplyData(i, _content[CalculateIndexForStep(step, i, l, _content.Length)]);
            }
            _scrollValue = value;
        }

        /// <summary>
        /// Uses 
        /// </summary>
        /// <param name="step"></param>
        /// <param name="elementCount"></param>
        /// <returns></returns>
        private Vector3 CalculatePositionForStep(float step, int elementCount)
        {
           /* float normalizedDistance = ((step % elementCount) / elementCount);
            switch (_orientation)
            {
                case Orientation.Horizontal:
                    
                    return new Vector3(_start - normalizedDistance * _diff, 0, 0);
                case Orientation.Vertical:

                    return new Vector3(_start - normalizedDistance * _diff, 0, 0);
            }
            return Vector3.zero;*/
            float normalizedDistance = (step % elementCount) / elementCount;
            float position = _start - normalizedDistance * _diff;
            return _orientation == Orientation.Horizontal
                ? new Vector3(position, 0, 0)
                : new Vector3(0, position, 0);
        }

        /// <summary>
        /// Calculates which index for content is associated with the parameters
        /// </summary>
        /// <param name="step"></param>
        /// <param name="offset"></param>
        /// <param name="elementCount"></param>
        /// <returns></returns>
        private int CalculateIndexForStep(float step, int offset, int elementCount, int contentCount)
        {
            int index = Mathf.FloorToInt((step + offset) / elementCount);
            return (contentCount + index * elementCount + (elementCount - 1 - offset)) % contentCount;
        }

        /// <summary>
        /// Applies data to element at index. This needs to be specified by the Graphic extender specified
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        protected abstract void ApplyData(int index, Y data);

        /// <summary>
        /// Set to index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pivot"></param>
        public void SetToContentIndex(int index, float pivot = 0.0f)
        {
            index = index % _content.Length;
            float value = 1.0f / _content.Length * ((index) - pivot * (_visibleElements - 1));
            SetScrollValue(value);
        }

        /// <summary>
        /// Allows to set new content array
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(Y[] content)
        {
            bool createVisibleElements = this._content.Length != content.Length;
            this._content = content;
            if (createVisibleElements) SetVisibleElementCount(_visibleElements);
        }

        /// <summary>
        /// Spawns/Despawns elements for required visible element count
        /// </summary>
        /// <param name="elementCount"></param>
        public void SetVisibleElementCount(int elementCount)
        {
            // This could be considered to remove all. For now, it litteraly does nothing.
            if (elementCount == 0)
            {
                Debug.LogWarning($"{gameObject.name}: Requires atleast 1 visisble element.");
                return;
            }

            // Cannot spawn if there is no prefab
            if (_prefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: Cannot modify elements. Prefab == null");
                return;
            }

            elementCount = Math.Clamp(elementCount + 1, 1, _content.Length + 1);

            // Spawn/remove elements
            var children = GetComponentsInChildren<X>();
            if (children.Length == elementCount) return;
            for (int i = elementCount; i < children.Length; i++) DestroyImmediate(children[i].gameObject); // Remove all the extra elements
            for (int i = children.Length; i < elementCount; i++) Instantiate(_prefab, _mask.rectTransform); // Add all extra elements

            // Position all elements
            float maskEdge = _orientation == Orientation.Horizontal ? _mask.rectTransform.rect.width : _mask.rectTransform.rect.height;
            float positionOffset = maskEdge / _visibleElements;
            _start = positionOffset * (elementCount / 2.0f);
            _diff = positionOffset * elementCount;
            _elements = GetComponentsInChildren<X>(); 
            for (int i = 0; i < elementCount; i++)
            {
                _elements[i].rectTransform.localPosition = CalculatePositionForStep(i, elementCount);
                ApplyData(i, _content[CalculateIndexForStep(0, i, elementCount, _content.Length)]);
            }
            
            // Position for current scroll value
            SetScrollValue(_scrollValue);
            _visibleElements = Math.Max(elementCount - 1, 0);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                // Run all functionality of "SetVisibleElementCount" without the checks
                var elementCount = _visibleElements;
                elementCount = Math.Clamp(elementCount + 1, 1, _content.Length + 1);

                // Spawn/remove elements
                var children = GetComponentsInChildren<X>();
                for (int i = elementCount; i < children.Length; i++) DestroyImmediate(children[i].gameObject); // Remove all the extra elements
                for (int i = children.Length; i < elementCount; i++) Instantiate(_prefab, _mask.rectTransform); // Add all extra elements

                // Position all elements
                float maskEdge = _orientation == Orientation.Horizontal ? _mask.rectTransform.rect.width : _mask.rectTransform.rect.height;
                float positionOffset = maskEdge / _visibleElements;
                _start = positionOffset * (elementCount / 2.0f);
                _diff = positionOffset * elementCount;
                _elements = GetComponentsInChildren<X>();
                for (int i = 0; i < elementCount; i++)
                {
                    _elements[i].rectTransform.localPosition = CalculatePositionForStep(i, elementCount);
                    ApplyData(i, _content[CalculateIndexForStep(0, i, elementCount, _content.Length)]);
                }

                // Position for current scroll value
                SetScrollValue(_scrollValue);
                _visibleElements = Math.Max(elementCount - 1, 0);
            };
            if (_elements != null && _elements.Length > 0)
                SetScrollValue(_scrollValue);
        }

        private void Reset()
        {
            _mask = GetComponent<Mask>();
            // Hard coded warning to instruct user to set a different mask texture
            if (_mask.graphic.mainTexture.name == "UnityWhite") Debug.LogWarning($"{gameObject.name}: Image component requires a mask. You are adviced to manually add the build-in \"UIMask\".");
            if (_content.Length == 0) Debug.LogWarning($"{gameObject.name}: No content specified");
        }
#endif
    }
}
