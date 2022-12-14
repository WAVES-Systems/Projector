using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WavesSystems
{
    /// <summary>
    /// UI Element that reproduces an animation based on sprite sheet file.
    /// </summary>
    public partial class Projector : UserControl, IDisposable
    {
        private int _repeatCounter = 0;
        private TimeSpan _frameRateInterval = TimeSpan.FromMilliseconds(1000 / 60d);
        private Point _currentPosition = new Point(0, 0);
        private int _rowCount = 1;
        private int _currentFrame = 0;
        private int _repeatBehavior = 0;
        private Stopwatch _watcher = new Stopwatch();

        public Projector()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        /// Gets or Sets the frame per second rate of animation.
        /// </summary>
        public int FrameRate
        {
            get { return (int)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(int), typeof(Projector), new PropertyMetadata(60, new PropertyChangedCallback(OnFrameRatePropertyUpdated)));

        /// <summary>
        /// Gets or Sets the amount of frames in the sprite animation file.
        /// </summary>
        public int FrameCount
        {
            get { return (int)GetValue(FrameCountProperty); }
            set
            { SetValue(FrameCountProperty, value); }
        }
        public static readonly DependencyProperty FrameCountProperty =
            DependencyProperty.Register("FrameCount", typeof(int), typeof(Projector), new PropertyMetadata(1, new PropertyChangedCallback(OnSpriteSheetPropertiesUpdate)));

        /// <summary>
        /// Gets or Sets the amount of columns in the sprite animation sheet.
        /// </summary>
        public int ColumnCount
        {
            get { return (int)GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof(int), typeof(Projector), new PropertyMetadata(1, new PropertyChangedCallback(OnSpriteSheetPropertiesUpdate)));

        /// <summary>
        /// Gets or sets a value that indicates whether the animation is running.
        /// </summary>
        public bool IsPlaying
        {
            get => _watcher.IsRunning;
            set
            {
                if (value)
                    Begin();
                else
                    Stop();
            }
        }

        /// <summary>
        /// Gets of sets a value that indicates whether the animation starts automatically after loading.
        /// </summary>
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }
        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(Projector), new PropertyMetadata(true));

        /// <summary>
        /// Defines the number of times the animation will be played.
        /// </summary>
        public string RepeatBehavior
        {
            get { return (string)GetValue(RepeatBehaviorProperty); }
            set
            { SetValue(RepeatBehaviorProperty, value); }
        }

        public static readonly DependencyProperty RepeatBehaviorProperty =
            DependencyProperty.Register("RepeatBehavior", typeof(string), typeof(Projector), new PropertyMetadata("Forever", new PropertyChangedCallback(OnRepeatBehaviorUpdate)));

        /// <summary>
        /// Gets or Sets the animated image displayed in the view./>
        /// </summary>
        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Projector), new PropertyMetadata(null, new PropertyChangedCallback(OnSourcePropertyUpdated)));

        #endregion

        #region Methods

        /// <summary>
        /// Initiates the animation.
        /// </summary>
        public void Begin()
        {
            if (Source != null)
                _watcher.Start();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            _watcher.Reset();
            _watcher.Stop();
            _currentFrame = 0;
            _currentPosition = new Point(0, 0);
            _repeatCounter = 0;
        }

        #endregion

        #region Event handlers

        private static void OnSourcePropertyUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var parent = (Projector)dependencyObject;
            try
            {
                parent.IsPlaying = false;

                if (eventArgs.NewValue != null)
                {
                    OnSpriteSheetPropertiesUpdate(dependencyObject, eventArgs);
                    parent.ImageBrushSprite.ImageSource = (ImageSource)eventArgs.NewValue;
                    if (parent.AutoStart)
                        parent.IsPlaying = true;
                }
            }
            catch
            {
                parent.IsPlaying = false;
                parent.ImageBrushSprite.Viewport = new Rect(0, 0, 1, 1);
                parent.ImageBrushSprite.ImageSource = null;
            }
        }

        private static void OnFrameRatePropertyUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var parent = (Projector)dependencyObject;
            parent._frameRateInterval = TimeSpan.FromMilliseconds(1000 / (double)eventArgs.NewValue);
        }

        private static void OnSpriteSheetPropertiesUpdate(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            Projector parent = (Projector)dependencyObject;
            parent._rowCount = (int)Math.Ceiling(parent.FrameCount / (double)parent.ColumnCount);
            double desiredWidth = parent.Width * parent.ColumnCount;
            double desiredHeight = parent.Height * parent._rowCount;
            parent.ImageBrushSprite.Viewport = new Rect(0, 0, desiredWidth, desiredHeight);
            if (parent.ImageBrushSprite.ImageSource != null)
            {
                var bitmapSource = (BitmapSource)parent.ImageBrushSprite.ImageSource;
                parent.SpriteSheetScale.ScaleX = (desiredWidth / bitmapSource.PixelWidth) * (bitmapSource.DpiX / 96d);
                parent.SpriteSheetScale.ScaleY = (desiredHeight / bitmapSource.PixelHeight) * (bitmapSource.DpiY / 96d);
            }
        }

        private static void OnRepeatBehaviorUpdate(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            Projector parent = (Projector)dependencyObject;
            string newValue = (string)eventArgs.NewValue;
            newValue = newValue.ToLower();
            if (newValue == "forever")
            {
                parent._repeatBehavior = 0;
            }
            else
            {
                parent._repeatBehavior = 0;
                int.TryParse(newValue.Replace("x", string.Empty), out parent._repeatBehavior);
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;

            if (AutoStart)
            {
                IsPlaying = true;
            }
        }

        private void OnUpdate(object sender, object e)
        {
            if (IsPlaying)
            {
                if (_repeatBehavior > 0 && _repeatCounter >= _repeatBehavior)
                {
                    Stop();
                    return;
                }

                if (_watcher.Elapsed >= _frameRateInterval)
                {
                    _watcher.Restart();

                    this.SpriteSheetOffset.X = -_currentPosition.X * this.ActualWidth;
                    this.SpriteSheetOffset.Y = -_currentPosition.Y * this.ActualHeight;
                    this._currentFrame++;

                    if (_currentFrame == FrameCount)
                    {
                        _currentPosition = new Point(0, 0);
                        _currentFrame = 0;
                        _repeatCounter++;
                    }
                    else
                    {
                        _currentPosition.X++;

                        if (_currentPosition.X == ColumnCount)
                        {
                            _currentPosition.X = 0;
                            _currentPosition.Y++;
                            if (_currentPosition.Y == _rowCount)
                                _currentPosition.Y = 0;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Source = null;
            GC.Collect();
        }

        private void Usr_ani_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= this.OnUpdate;
        }

        private void Usr_ani_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += this.OnUpdate;
        }

        #endregion

        private void Usr_ani_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnSpriteSheetPropertiesUpdate(this, new DependencyPropertyChangedEventArgs());
        }
    }
}
