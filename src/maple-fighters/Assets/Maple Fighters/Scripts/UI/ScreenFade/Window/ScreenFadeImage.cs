using System;
using UI;
using UnityEngine;

namespace Scripts.UI.ScreenFade
{
    [RequireComponent(typeof(UIFadeAnimation))]
    public class ScreenFadeImage : UIElement, IScreenFadeView
    {
        public event Action FadeInCompleted;
        public event Action FadeOutCompleted;

        private UIFadeAnimation uiFadeAnimation;

        private void Start()
        {
            SubscribeToUIFadeAnimation();
        }

        private void OnDestroy()
        {
            UnsubscribeFromUIFadeAnimation();
        }

        private void SubscribeToUIFadeAnimation()
        {
            uiFadeAnimation = GetComponent<UIFadeAnimation>();
            if (uiFadeAnimation != null)
            {
                uiFadeAnimation.FadeInCompleted += OnFadeInCompleted;
                uiFadeAnimation.FadeOutCompleted += OnFadeOutCompleted;
            }
        }

        private void UnsubscribeFromUIFadeAnimation()
        {
            if (uiFadeAnimation != null)
            {
                uiFadeAnimation.FadeInCompleted -= OnFadeInCompleted;
                uiFadeAnimation.FadeOutCompleted -= OnFadeOutCompleted;
            }
        }

        private void OnFadeInCompleted()
        {
            FadeInCompleted?.Invoke();
        }

        private void OnFadeOutCompleted()
        {
            FadeOutCompleted?.Invoke();
        }
    }
}