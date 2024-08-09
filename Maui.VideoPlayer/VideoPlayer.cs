﻿using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.ComponentModel;

namespace Maui.VideoPlayer
{
    public class VideoPlayer : View, IVideoPlayerController
    {
        public event EventHandler UpdateStatus;

        public VideoPlayer()
        {
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                UpdateStatus?.Invoke(this, EventArgs.Empty);
                return true;
            });
        }

        // AreTransportControlsEnabled property
        public static readonly BindableProperty AreTransportControlsEnabledProperty =
            BindableProperty.Create(nameof(AreTransportControlsEnabled), typeof(bool), typeof(VideoPlayer), true);

        public bool AreTransportControlsEnabled
        {
            set { SetValue(AreTransportControlsEnabledProperty, value); }
            get { return (bool)GetValue(AreTransportControlsEnabledProperty); }
        }

        // Source property
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(VideoSource), typeof(VideoPlayer), null);

        [TypeConverter(typeof(VideoSourceConverter))]
        public VideoSource Source
        {
            set { SetValue(SourceProperty, value); }
            get { return (VideoSource)GetValue(SourceProperty); }
        }

        // AutoPlay property
        public static readonly BindableProperty AutoPlayProperty =
            BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(VideoPlayer), true);

        public bool AutoPlay
        {
            set { SetValue(AutoPlayProperty, value); }
            get { return (bool)GetValue(AutoPlayProperty); }
        }

        // Status read-only property
        private static readonly BindablePropertyKey StatusPropertyKey =
            BindableProperty.CreateReadOnly(nameof(Status), typeof(VideoStatus), typeof(VideoPlayer), VideoStatus.NotReady);

        public static readonly BindableProperty StatusProperty = StatusPropertyKey.BindableProperty;

        public VideoStatus Status
        {
            get { return (VideoStatus)GetValue(StatusProperty); }
        }

        VideoStatus IVideoPlayerController.Status
        {
            set { SetValue(StatusPropertyKey, value); }
            get { return Status; }
        }

        // Duration read-only property
        private static readonly BindablePropertyKey DurationPropertyKey =
            BindableProperty.CreateReadOnly(nameof(Duration), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan(),
                propertyChanged: (bindable, oldValue, newValue) => ((VideoPlayer)bindable).SetTimeToEnd());

        public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
        }

        TimeSpan IVideoPlayerController.Duration
        {
            set { SetValue(DurationPropertyKey, value); }
            get { return Duration; }
        }

        // Position property
        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan(),
                propertyChanged: (bindable, oldValue, newValue) => ((VideoPlayer)bindable).SetTimeToEnd());

        public TimeSpan Position
        {
            set { SetValue(PositionProperty, value); }
            get { return (TimeSpan)GetValue(PositionProperty); }
        }

        // TimeToEnd property
        private static readonly BindablePropertyKey TimeToEndPropertyKey =
            BindableProperty.CreateReadOnly(nameof(TimeToEnd), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan());

        public static readonly BindableProperty TimeToEndProperty = TimeToEndPropertyKey.BindableProperty;

        public TimeSpan TimeToEnd
        {
            private set { SetValue(TimeToEndPropertyKey, value); }
            get { return (TimeSpan)GetValue(TimeToEndProperty); }
        }

        void SetTimeToEnd()
        {
            try { TimeToEnd = Duration - Position; }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Duration: {Duration}");
                Debug.WriteLine("Position: {Position}");
            }
        }

        // IsBuffering property
        public static readonly BindableProperty IsBufferingProperty = BindableProperty.Create(
                                                         propertyName: "IsBuffering",
                                                         returnType: typeof(Boolean),
                                                         declaringType: typeof(VideoPlayer),
                                                         defaultValue: false,
                                                         defaultBindingMode: BindingMode.TwoWay);
        public Boolean IsBuffering
        {
            get { return (Boolean)GetValue(IsBufferingProperty); }
            set { SetValue(IsBufferingProperty, value); }
        }

        // Methods handled by renderers
        public event EventHandler PlayRequested;

        public void Play()
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PauseRequested;

        public void Pause()
        {
            PauseRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StopRequested;

        public void Stop()
        {
            StopRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowTransportControlsRequested;

        public void ShowTransportControls()
        {
            ShowTransportControlsRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler HideTransportControlsRequested;

        public void HideTransportControls()
        {
            HideTransportControlsRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PlayCompletion;

        public void OnPlayCompletion()
        {
            PlayCompletion?.Invoke(this, EventArgs.Empty);
        }

        public class PlayErrorEventArgs : EventArgs
        {
            public string ErrorMessage;
            public PlayErrorEventArgs(string errorMessage) { ErrorMessage = errorMessage; }
        }
        public delegate void PlayErrorEventHandler(object sender, PlayErrorEventArgs e);
        public event PlayErrorEventHandler PlayError;

        public void OnPlayError(object sender, PlayErrorEventArgs e)
        {
            PlayError?.Invoke(this, e);
        }

        public event EventHandler BufferingStart;

        public void OnBufferingStart()
        {
            if(IsBuffering == false) 
            {
                IsBuffering = true;
                BufferingStart?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler BufferingEnd;

        public void OnBufferingEnd()
        {
            if (IsBuffering)
            {
                IsBuffering = false;
                BufferingEnd?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
