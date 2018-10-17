// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ptv.XServer.Demo.UseCases
{
    public class IntroManager
    {
        private readonly IntroControl[] introControls;
        private readonly string[] handlerIds;
        private const int index = 0;
        private readonly Grid location;
        public Action<string, bool> HandlerCallback;

        public IntroManager(IEnumerable<Intro> intros, Grid location)
        {
            var ctrls = new List<IntroControl>();
            var cnxts = new List<string>();

            foreach (var intro in intros)
            {
                var ctrl = new IntroControl
                {
                    Width = 500,
                    Height = 330,
                    Title = intro.Title,
                    Text = intro.Text,
                    BackButton = true,
                    NextButtonText = "Next"
                };

                switch (intro.Type)
                {
                    case Intro.PageType.First: ctrl.BackButton = false; break;
                    case Intro.PageType.Last: ctrl.NextButtonText = "Start"; break;
                    case Intro.PageType.Single: ctrl.BackButton = false; ctrl.NextButtonText = "Start"; break;
                }

                ctrls.Add(ctrl);
                cnxts.Add((String.IsNullOrEmpty(intro.HandlerId)) ? "" : intro.HandlerId);
            }

            introControls = ctrls.ToArray();
            handlerIds = cnxts.ToArray();
            
            this.location = location;
        }

        public void StartIntro()
        {
            ShowIntro(index);
        }

        private void ShowIntro(int introIndex)
        {
            if (introIndex < 0)
                return;

            location.Children.Add(introControls[introIndex]);

            if (HandlerCallback != null && handlerIds.Length > introIndex)
                HandlerCallback(handlerIds[introIndex], true);

            introControls[introIndex].Forwarded = () =>
            {
                if (HandlerCallback != null && handlerIds.Length > introIndex)
                    HandlerCallback(handlerIds[introIndex], false);

                if (introControls.Length > introIndex)
                    location.Children.Remove(introControls[introIndex]);

                introIndex++;

                if (introIndex < introControls.Length)
                    ShowIntro(introIndex);
                else
                    HandlerCallback?.Invoke("_SkippedIntro", false);
            };

            introControls[introIndex].Backwarded = () =>
            {
                if (HandlerCallback != null && handlerIds.Length > introIndex)
                    HandlerCallback(handlerIds[introIndex], false);

                if(introControls.Length > introIndex)
                    location.Children.Remove(introControls[introIndex]);

                introIndex--;
                ShowIntro(introIndex);
            };

            introControls[introIndex].Skipped = yes =>
            {
                HandlerCallback?.Invoke(handlerIds[introIndex], false);

                location.Children.Remove(introControls[introIndex]);
                HandlerCallback?.Invoke("_SkippedIntro", !yes);
            };
        }
    }

    public class Intro
    {
        public PageType Type { get; private set; }
        public string Title { get; private set; }
        public string Text { get; private set; }
        public string HandlerId { get; private set; }

        public Intro(PageType type, string handlerId, string title, string text)
        {
            Type = type;
            Title = title.ToUpper();
            Text = text;
            HandlerId = handlerId;
        }

        public enum PageType
        {
            First,
            Normal,
            Last,
            Single
        }
    }
}
