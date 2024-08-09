﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Maui.VideoPlayer.Android;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBar))]

namespace Maui.VideoPlayer.Android
{
    public class StatusBar : IStatusBar
    {

        #region IStatusBar implementation

        public void HideStatusBar()
        {
            var activity = MainActivity.Current as Activity;
            activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
        }

        public void ShowStatusBar()
        {
            if (MainActivity.Current == null)
                return;
            var activity = MainActivity.Current as Activity;
            activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);
        }

        #endregion
    }
}