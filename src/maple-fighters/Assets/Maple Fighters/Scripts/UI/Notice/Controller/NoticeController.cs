using System;
using UI;
using UnityEngine;

namespace Scripts.UI.Notice
{
    public class NoticeController : MonoBehaviour
    {
        private INoticeView noticeView;
        private Action onOkButtonClicked;

        public void Show(string message, Action onClicked = null, bool background = true)
        {
            var noticeWindow = CreateOrShowNoticeView();
            if (noticeWindow != null)
            {
                noticeWindow.Message = message;

                if (onClicked != null)
                {
                    onOkButtonClicked = onClicked;

                    noticeWindow.OkButtonClicked += onOkButtonClicked;
                }
                else
                {
                    onOkButtonClicked = null;
                }

                if (background == false)
                {
                    noticeWindow.HideBackground();
                }
            }
        }

        private void Hide()
        {
            SubscribeToUIFadeAnimation();

            HideNoticeWindow();
        }

        private void HideNoticeWindow()
        {
            if (noticeView != null)
            {
                if (onOkButtonClicked != null)
                {
                    noticeView.OkButtonClicked -= onOkButtonClicked;
                }

                noticeView.Hide();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromNoticeWindow();
            
            // Destruir la ventana de notificación para evitar objetos huérfanos
            if (noticeView != null)
            {
                var viewGameObject = (noticeView as MonoBehaviour)?.gameObject;
                if (viewGameObject != null)
                {
                    Destroy(viewGameObject);
                }
                noticeView = null;
            }
        }

        private void UnsubscribeFromNoticeWindow()
        {
            if (noticeView != null)
            {
                noticeView.OkButtonClicked -= Hide;
            }
        }

        private void SubscribeToUIFadeAnimation()
        {
            if (noticeView != null)
            {
                noticeView.FadeOutCompleted += OnFadeOutCompleted;
            }
        }

        private void UnsubscribeFromUIFadeAnimation()
        {
            if (noticeView != null)
            {
                noticeView.FadeOutCompleted -= OnFadeOutCompleted;
            }
        }

        private void OnFadeOutCompleted()
        {
            UnsubscribeFromUIFadeAnimation();
        }

        private INoticeView CreateOrShowNoticeView()
        {
            if (noticeView == null)
            {
                CreateAndSubscribeToNoticeWindow();
            }

            noticeView.Show();

            return noticeView;
        }

        private void CreateAndSubscribeToNoticeWindow()
        {
            noticeView = UICreator
                .GetInstance()
                .Create<NoticeWindow>(UICanvasLayer.Foreground, UIIndex.End);

            if (noticeView != null)
            {
                noticeView.OkButtonClicked += Hide;
            }
        }
    }
}