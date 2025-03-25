using UnityEngine;

namespace UI.Core
{
    public abstract class UILayer : MonoBehaviour
    {
        [SerializeField] private string layerId;
        [SerializeField] private bool blockRaycastsWhenActive = true;

        public string LayerId => layerId;

        protected virtual void Awake()
        {
            UILayerManager.Instance?.RegisterLayer(layerId, this);
        }

        // Called when this layer is pushed onto the stack
        public virtual void OnLayerPushed()
        {
            SetRaycastBlockingState(blockRaycastsWhenActive);
        }

        // Called when this layer is popped from the stack
        public virtual void OnLayerPopped()
        {
            gameObject.SetActive(false);
            SetRaycastBlockingState(false);
        }

        // Called when another layer is pushed on top
        public virtual void OnLayerCovered()
        {
            SetRaycastBlockingState(false); // Disable interaction
        }

        // Called when a covering layer is popped
        public virtual void OnLayerUncovered()
        {
            SetRaycastBlockingState(blockRaycastsWhenActive); // Re-enable interaction
        }

        protected void SetRaycastBlockingState(bool state)
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.blocksRaycasts = state;
                group.interactable = state;
            }
        }
    }
}
