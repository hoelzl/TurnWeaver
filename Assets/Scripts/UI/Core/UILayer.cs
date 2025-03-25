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

        protected virtual void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError($"UILayer {layerId} is missing UIDocument component!");
                    return;
                }
            }
        }

        protected virtual void Start()
        {
            // Register with manager and setup UI after all Awake calls are complete
            UILayerManager.Instance?.RegisterLayer(layerId, this);
            SetupUI();

            // IMPORTANT: Make sure we start hidden
            Hide();
        }

        protected virtual void SetupUI()
        {
            // Child classes override this
        }

        public virtual void Show()
        {
            if (uiDocument != null)
            {
                uiDocument.enabled = true;

                // Wait one frame to ensure root visual element is available
                StartCoroutine(ShowNextFrame());
            }
        }

        private System.Collections.IEnumerator ShowNextFrame()
        {
            yield return null;
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
            Show();
        }

        // Called when this layer is popped from the stack
        public virtual void OnLayerPopped()
        {
            Hide();
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
