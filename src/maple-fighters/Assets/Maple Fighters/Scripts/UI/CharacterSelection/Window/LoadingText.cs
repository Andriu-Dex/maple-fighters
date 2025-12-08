using System;
using UI;
using UnityEngine;

namespace Scripts.UI.CharacterSelection
{
    [RequireComponent(typeof(UIFadeAnimation))]
    public class LoadingText : UIElement, ILoadingView, ILoadingAnimation
    {
        public ILoadingAnimation LoadingAnimation => this;

        public event Action Finished;

        private UIFadeAnimation uiFadeAnimation;

        private void Awake()
        {
            uiFadeAnimation = GetComponent<UIFadeAnimation>();
            if (uiFadeAnimation != null)
            {
                uiFadeAnimation.FadeInCompleted += OnFadeInCompleted;
                uiFadeAnimation.FadeOutCompleted += OnFadeOutCompleted;
            }
            else
            {
                Debug.LogWarning("[LoadingText] UIFadeAnimation no encontrado en el objeto");
            }
        }

        private void OnDestroy()
        {
            if (uiFadeAnimation != null)
            {
                uiFadeAnimation.FadeInCompleted -= OnFadeInCompleted;
                uiFadeAnimation.FadeOutCompleted -= OnFadeOutCompleted;
            }
        }

        private void OnFadeInCompleted()
        {
            Finished?.Invoke();
        }

        private void OnFadeOutCompleted()
        {
            Finished?.Invoke();
        }
    }
}