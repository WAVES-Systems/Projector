using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private Stopwatch _watcher = new Stopwatch();
        private Stopwatch _repeatWatcher = new Stopwatch();
        AutoResetEvent _taskCompleteTimer = new AutoResetEvent(false);

        public Projector()
        {
            InitializeComponent();
        }

        #region Properties

        private EventHandler _completed;

        /// <summary>
        /// Occurs when this timeline has completely finished playing: it will no longer enter its active period.
        /// </summary>
        public event EventHandler Completed
        {
            add
            {
                _completed += value;
            }
            remove
            {
                _completed -= value;
            }
        }

        private void OnCompleted()
        {
            this._completed?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets or sets the repeating behavior of this animation.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or sets the repeating behavior of this animation.")]
        public RepeatBehavior RepeatBehavior
        {
            get { return (RepeatBehavior)GetValue(RepeatBehaviorProperty); }
            set { SetValue(RepeatBehaviorProperty, value); }
        }

        public static readonly DependencyProperty RepeatBehaviorProperty =
            DependencyProperty.Register("RepeatBehavior", typeof(RepeatBehavior), typeof(Projector), new PropertyMetadata(RepeatBehavior.Forever));

        /// <summary>
        /// Gets or sets a value that specifies how the animation behaves after it reaches the end of its active period.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or sets a value that specifies how the animation behaves after it reaches the end of its active period.")]
        public FillBehavior FillBehavior
        {
            get { return (FillBehavior)GetValue(FillBehaviorProperty); }
            set { SetValue(FillBehaviorProperty, value); }
        }

        public static readonly DependencyProperty FillBehaviorProperty =
            DependencyProperty.Register("FillBehavior", typeof(FillBehavior), typeof(Projector), new PropertyMetadata(FillBehavior.HoldEnd, new PropertyChangedCallback(OnFillBehaviorPropertyUpdated)));

        /// <summary>
        /// Gets or Sets the frame per second rate of animation.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or Sets the frame per second rate of animation.")]
        [TypeConverter(typeof(Int16Converter))]
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
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or Sets the frame per second rate of animation.")]
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
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or Sets the amount of columns in the sprite animation sheet.")]
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
        [Browsable(false)]
        public bool IsPlaying
        {
            get => _watcher.IsRunning;
            set
            {
                if (value)
                    BeginAnimation();
                else
                    StopAnimation();
            }
        }

        /// <summary>
        /// Gets of sets a value that indicates whether the animation starts automatically after loading.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets of sets a value that indicates whether the animation starts automatically after loading.")]
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }
        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(Projector), new PropertyMetadata(true,new PropertyChangedCallback(OnAutoStartPropertyUpdated)));

        /// <summary>
        /// Gets or Sets the animated image displayed in the view.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or Sets the animated image displayed in the view.")]
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
        /// Starts the animation.
        /// </summary>
        public void BeginAnimation()
        {
            _watcher.Start();
            if (RepeatBehavior.HasDuration)
                _repeatWatcher.Start();
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public Task BeginAnimationTask()
        {
            return new Task(() =>
            {
                this.Completed += (o, a) => { _taskCompleteTimer.Set(); };

                BeginAnimation();

                _taskCompleteTimer.WaitOne();
            });
        }


        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void StopAnimation()
        {
            _taskCompleteTimer.Set();
            _watcher.Stop();
            _repeatWatcher.Stop();
            _watcher.Reset();
            _repeatWatcher.Reset();
            _repeatCounter = 0;

            if (FillBehavior == FillBehavior.Stop)
            {
                _currentFrame = 0;
                _currentPosition = new Point(0, 0);
                this.SpriteSheetOffset.X = -_currentPosition.X * this.ActualWidth;
                this.SpriteSheetOffset.Y = -_currentPosition.Y * this.ActualHeight;
            }
        }

        #endregion

        #region Event handlers

        private static void OnAutoStartPropertyUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var parent = (Projector)dependencyObject;
            if (DesignerProperties.GetIsInDesignMode(parent))
            {
                parent.IsPlaying = (bool)eventArgs.NewValue;
            }

        }
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

        private static void OnFillBehaviorPropertyUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var parent = (Projector)dependencyObject;
            if (!parent.IsPlaying && (FillBehavior)eventArgs.NewValue == FillBehavior.Stop)
            {
                parent.StopAnimation();
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

        private void Rect_stage_Loaded(object sender, RoutedEventArgs e)
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
                if ((RepeatBehavior.HasDuration && _repeatWatcher.Elapsed >= RepeatBehavior.Duration)
                    || (RepeatBehavior.HasCount && _repeatCounter >= RepeatBehavior.Count))
                {
                    StopAnimation();
                    OnCompleted();
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
