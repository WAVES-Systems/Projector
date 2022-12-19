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

namespace Waves.Visual
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
        CancellationTokenSource _completeTokenSource = new CancellationTokenSource();
        bool _isProgressing = true;

        public Projector()
        {
            InitializeComponent();
        }

        #region Properties

        private EventHandler _completed;

        /// <summary>
        /// Occurs when this animation has completely finished playing and will no longer enter its active period.
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

        /// <summary>
        /// Gets the length of time for which this Animation plays, not counting repetitions.
        /// </summary>
        public TimeSpan Duration => TimeSpan.FromMilliseconds(FrameRate * (_frameRateInterval.TotalMilliseconds + 0.5d));

        /// <summary>
        /// Gets the length of time for which this Animation plays, counting repetitions.
        /// </summary>
        public TimeSpan TotalDuration
        {
            get
            {
                if (RepeatBehavior == RepeatBehavior.Forever)
                    return TimeSpan.MaxValue;
                else if (RepeatBehavior.HasDuration)
                    return RepeatBehavior.Duration;

                return TimeSpan.FromMilliseconds(Duration.TotalMilliseconds * RepeatBehavior.Count);
            }
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

        /// <summary>
        /// Identifies the <see cref="RepeatBehavior"/> dependency property.
        /// </summary>
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

        /// <summary>
        /// Identifies the <see cref="FillBehavior"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillBehaviorProperty =
            DependencyProperty.Register("FillBehavior", typeof(FillBehavior), typeof(Projector), new PropertyMetadata(FillBehavior.HoldEnd, new PropertyChangedCallback(OnFillBehaviorPropertyUpdated)));

        /// <summary>
        /// Gets or Sets the frame per second rate of animation.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or Sets the frame per second rate of animation.")]
        public double FrameRate
        {
            get { return (double)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FrameRate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(double), typeof(Projector), new PropertyMetadata(60d, new PropertyChangedCallback(OnFrameRatePropertyUpdated)));

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

        /// <summary>
        /// Identifies the <see cref="FrameCount"/> dependency property.
        /// </summary>
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

        /// <summary>
        /// Identifies the <see cref="ColumnCount"/> dependency property.
        /// </summary>
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
        /// Gets or sets a value that indicates whether the animation plays in reverse after it completes a forward iteration.
        /// </summary>
        [Category("Animation")]
        [Browsable(true)]
        [Description("Gets or sets a value that indicates whether the animation plays in reverse after it completes a forward iteration.")]
        public bool AutoReverse
        {
            get { return (bool)GetValue(AutoReverseProperty); }
            set { SetValue(AutoReverseProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AutoReverse"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoReverseProperty =
            DependencyProperty.Register("AutoReverse", typeof(bool), typeof(Projector), new PropertyMetadata(false));

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

        /// <summary>
        /// Identifies the <see cref="AutoStart"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(Projector), new PropertyMetadata(true, new PropertyChangedCallback(OnAutoStartPropertyUpdated)));

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

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Projector), new PropertyMetadata(null, new PropertyChangedCallback(OnSourcePropertyUpdated)));

        #endregion

        #region Methods

        /// <summary>
        /// Starts or resumes the animation.
        /// </summary>
        public void BeginAnimation()
        {
            _watcher.Start();
            if (RepeatBehavior.HasDuration)
                _repeatWatcher.Start();
        }

        /// <summary>
        /// Starts or resumes the animation and holds the execution until the animation completes its active period.
        /// </summary>
        /// <returns>An asynchronous object that represents the operation.</returns>
        public async Task BeginAnimationAsync()
        {
            _completeTokenSource = new CancellationTokenSource();
            BeginAnimation();
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(int.MaxValue), _completeTokenSource.Token);
            }
            catch (TaskCanceledException) { }
        }

        /// <summary>
        /// Reset the Viewport element to the initial position.
        /// </summary>
        private void ResetViewportPosition()
        {
            _currentFrame = 0;
            _currentPosition = new Point(0, 0);
            this.SpriteSheetOffset.X = -_currentPosition.X * this.ActualWidth;
            this.SpriteSheetOffset.Y = -_currentPosition.Y * this.ActualHeight;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void StopAnimation()
        {
            _completeTokenSource.Cancel();
            _watcher.Stop();
            _repeatWatcher.Stop();
            _watcher.Reset();
            _repeatWatcher.Reset();
            _repeatCounter = 0;

            if (FillBehavior == FillBehavior.Stop)
            {
                ResetViewportPosition();
            }
        }

        #endregion

        #region Event handlers

        private void OnCompleted()
        {
            this._completed?.Invoke(this, new EventArgs());
        }

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
                bool previousState = parent.IsPlaying;
                parent.IsPlaying = false;
                parent.ResetViewportPosition();
                parent.ImageBrushSprite.ImageSource = eventArgs.NewValue as ImageSource;
                parent.UpdateSpriteSheetProperties(false);

                if (parent.AutoStart || previousState)
                    parent.IsPlaying = true;
            }
            catch
            {
                parent.IsPlaying = false;
                parent.ImageBrushSprite.Viewport = new Rect(0, 0, 1, 1);
                parent.ImageBrushSprite.ImageSource = null;
            }
        }

        private void UpdateSpriteSheetProperties(bool resetAnimation)
        {
            bool previousState = this.IsPlaying;
            if (resetAnimation)
            {
                this.IsPlaying = false;
                ResetViewportPosition();
            }

            this._rowCount = (int)Math.Ceiling(this.FrameCount / (double)this.ColumnCount);
            double desiredWidth = this.ActualWidth * this.ColumnCount;
            double desiredHeight = this.ActualHeight * this._rowCount;
            this.ImageBrushSprite.Viewport = new Rect(0, 0, desiredWidth, desiredHeight);

            if (this.ImageBrushSprite.ImageSource != null)
            {
                var bitmapSource = (BitmapSource)this.ImageBrushSprite.ImageSource;
                this.SpriteSheetScale.ScaleX = (desiredWidth / bitmapSource.PixelWidth) * (bitmapSource.DpiX / 96d);
                this.SpriteSheetScale.ScaleY = (desiredHeight / bitmapSource.PixelHeight) * (bitmapSource.DpiY / 96d);
                if (resetAnimation && (AutoStart || previousState))
                    this.IsPlaying = true;
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
            parent.UpdateSpriteSheetProperties(true);
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

                    if (_isProgressing)
                    {
                        this._currentFrame++;
                        if (_currentFrame >= FrameCount)
                        {
                            _repeatCounter++;
                            if (!AutoReverse)
                            {
                                _currentPosition = new Point(0, 0);
                                _currentFrame = 0;
                            }
                            else
                            {
                                _isProgressing = false;
                                this._currentFrame--;
                            }
                        }
                        else
                        {
                            _currentPosition.X++;

                            if (_currentPosition.X >= ColumnCount)
                            {
                                _currentPosition.X = 0;
                                _currentPosition.Y++;
                            }
                        }
                    }
                    else
                    {
                        this._currentFrame--;
                        if (_currentFrame < 0)
                        {
                            _currentPosition = new Point(0, 0);
                            _currentFrame = 0;
                            _isProgressing = true;
                        }
                        else
                        {
                            _currentPosition.X--;

                            if (_currentPosition.X < 0)
                            {
                                _currentPosition.X = ColumnCount - 1;
                                _currentPosition.Y--;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Source = null;
            _completeTokenSource.Cancel();
            _watcher.Stop();
            _repeatWatcher.Stop();
        }

        private void Usr_ani_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= this.OnUpdate;
        }

        private void Usr_ani_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += this.OnUpdate;
        }

        private void Usr_ani_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSpriteSheetProperties(true);
        }

        #endregion
    }
}
