using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Core
{
    public abstract class UILayer : MonoBehaviour
    {
        [SerializeField] private string layerId;
        [SerializeField] protected UIDocument uiDocument;

        public string LayerId => layerId;
        protected VisualElement Root => uiDocument?.rootVisualElement;
        private readonly Stack<int> _stackPositions = new();

        protected virtual void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }

        protected virtual void Start()
        {
            if (uiDocument == null)
            {
                Debug.LogError($"Cannot register UILayer {layerId} since it is missing a UIDocument component!");
                return;
            }

            SetupUI();
            UILayerManager.Instance?.RegisterLayer(this);
       }

        protected virtual void SetupUI()
        {
            // Child classes override this
        }

        public virtual void Show()
        {
            uiDocument.enabled = true;

            if (Root != null)
            {
                Root.style.display = DisplayStyle.Flex;
                Root.style.visibility = Visibility.Visible;
                Root.pickingMode = PickingMode.Position;
            }
        }

        public virtual void Hide()
        {
            if (uiDocument != null && Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.style.visibility = Visibility.Hidden;
                Root.pickingMode = PickingMode.Ignore;
            }
        }

        // Called when this layer is pushed onto the stack
        public virtual void OnLayerPushed()
        {
            int newStackPosition = UILayerManager.ActiveLayerCount;
            _stackPositions.Push(newStackPosition);
            uiDocument.sortingOrder = newStackPosition;
            Show();
        }

        // Called when this layer is popped from the stack
        public virtual void OnLayerPopped()
        {
            if (_stackPositions.TryPop(out int newStackPosition))
            {
                uiDocument.sortingOrder = newStackPosition;
            }
            else
            {
                Hide();
            }
        }

        // Called when another layer is pushed on top
        public virtual void OnLayerCovered()
        {
            if (Root != null)
            {
                Root.pickingMode = PickingMode.Ignore;
            }
        }

        // Called when a covering layer is popped
        public virtual void OnLayerUncovered()
        {
            if (Root != null)
            {
                Root.pickingMode = PickingMode.Position;
            }
        }
    }
}
