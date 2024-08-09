﻿using System;
using System.ComponentModel;

using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
[assembly: ExportRenderer(typeof(Xam.Forms.VideoPlayer.VideoPlayer),
                          typeof(Xam.Forms.VideoPlayer.UWP.VideoPlayerRenderer))]

namespace Xam.Forms.VideoPlayer.UWP
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, // TODO Microsoft.UI.Xaml.Controls.MediaElement is not yet supported in WindowsAppSDK. For more details see https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/what-is-supported
MediaElement>
    {
        public static void Init() { }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    // TODO Microsoft.UI.Xaml.Controls.MediaElement is not yet supported in WindowsAppSDK. For more details see https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/what-is-supported
                    MediaElement mediaElement = new MediaElement();
                    SetNativeControl(mediaElement);

                    mediaElement.MediaOpened += OnMediaElementMediaOpened;
                    mediaElement.CurrentStateChanged += OnMediaElementCurrentStateChanged;
                    mediaElement.MediaEnded += OnMediaElement_MediaEnded;
                    mediaElement.MediaFailed += MediaElement_MediaFailed;
                }

                SetAreTransportControlsEnabled();
                SetSource();
                SetAutoPlay();

                args.NewElement.UpdateStatus += OnUpdateStatus;
                args.NewElement.PlayRequested += OnPlayRequested;
                args.NewElement.PauseRequested += OnPauseRequested;
                args.NewElement.StopRequested += OnStopRequested;
                args.NewElement.ShowTransportControlsRequested += OnShowTransportControls;
                args.NewElement.HideTransportControlsRequested += OnHideTransportControls;
            }

            if (args.OldElement != null)
            {
                args.OldElement.UpdateStatus -= OnUpdateStatus;
                args.OldElement.PlayRequested -= OnPlayRequested;
                args.OldElement.PauseRequested -= OnPauseRequested;
                args.OldElement.StopRequested -= OnStopRequested;
                args.OldElement.ShowTransportControlsRequested -= OnShowTransportControls;
                args.OldElement.HideTransportControlsRequested -= OnHideTransportControls;
            }
        }

        private void OnMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Element.OnPlayCompletion();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Element.OnPlayError(sender, new VideoPlayer.PlayErrorEventArgs(e.ErrorMessage));
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                Control.MediaOpened -= OnMediaElementMediaOpened;
                Control.CurrentStateChanged -= OnMediaElementCurrentStateChanged;
            }

            base.Dispose(disposing);
        }

        void OnMediaElementMediaOpened(object sender, RoutedEventArgs args)
        {
            ((IVideoPlayerController)Element).Duration = Control.NaturalDuration.TimeSpan;
        }

        void OnMediaElementCurrentStateChanged(object sender, RoutedEventArgs args)
        {
            VideoStatus videoStatus = VideoStatus.NotReady;

            switch (Control.CurrentState)
            {
                case MediaElementState.Buffering:
                    Element.OnBufferingStart();
                    break;
                case MediaElementState.Playing:
                    Element.OnBufferingEnd();
                    videoStatus = VideoStatus.Playing;
                    break;
                case MediaElementState.Paused:
                case MediaElementState.Stopped:
                    Element.OnBufferingEnd();
                    videoStatus = VideoStatus.Paused;
                    break;
            }

            ((IVideoPlayerController)Element).Status = videoStatus;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VideoPlayer.AreTransportControlsEnabledProperty.PropertyName)
            {
                SetAreTransportControlsEnabled();
            }
            else if (args.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
            else if (args.PropertyName == VideoPlayer.AutoPlayProperty.PropertyName)
            {
                SetAutoPlay();
            }
            else if (args.PropertyName == VideoPlayer.PositionProperty.PropertyName)
            {
                if (!Control.CanSeek)
                    return;
                if (Math.Abs((Control.Position - Element.Position).TotalSeconds) > 1)
                {
                    if (Element.AreTransportControlsEnabled)
                    {
                        Control.TransportControls.Show();
                        // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                        Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                        {
                            Control.TransportControls.Hide();
                            return false;
                        });
                    }
                    Control.Position = Element.Position;
                }
            }
        }

        private void OnShowTransportControls(object sender, EventArgs args)
        {
            if (Element.AreTransportControlsEnabled)
            {
                    Control.TransportControls.Show();
            }
        }

        private void OnHideTransportControls(object sender, EventArgs args)
        {
            if (Element.AreTransportControlsEnabled)
            {
                    Control.TransportControls.Hide();
            }
        }

        void SetAreTransportControlsEnabled()
        {
            Control.AreTransportControlsEnabled = Element.AreTransportControlsEnabled;
        }

        async void SetSource()
        {
            bool hasSetSource = false;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    Control.Source = new Uri(uri);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                // Code requires Pictures Library in Package.appxmanifest Capabilities to be enabled
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    StorageFolder storageFolder = KnownFolders.VideosLibrary;
                    StorageFile storageFile = await storageFolder.GetFileAsync(System.IO.Path.GetFileName(filename));
                    //StorageFile storageFile = await StorageFile.GetFileFromPathAsync(filename);
                    IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();
                    Control.SetSource(stream, storageFile.ContentType);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string path = "ms-appx:///" + (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    Control.Source = new Uri(path);
                    hasSetSource = true;
                }
            }

            if (!hasSetSource)
            {
                Control.Source = null;
            }
        }

        void SetAutoPlay()
        {
            Control.AutoPlay = Element.AutoPlay;
        }

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, Control.Position);
        }

        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            Control.Play();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            Control.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            Control.Stop();
        }
    }
}
