using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VPM.Services
{
    /// <summary>
    /// Helper class for creating and managing .NET 10 native animations using Storyboards
    /// Optimized for performance with fade-in/out effects and snappy easing functions
    /// </summary>
    public static class AnimationHelper
    {
        // Track active storyboards per element for proper cancellation and cleanup
        private static Dictionary<UIElement, Storyboard> _activeStoryboards = new Dictionary<UIElement, Storyboard>();

        /// <summary>
        /// Stops and cleans up any active storyboard on the element
        /// </summary>
        private static void StopActiveStoryboard(UIElement element)
        {
            if (element == null)
                return;

            if (_activeStoryboards.TryGetValue(element, out var storyboard))
            {
                storyboard.Stop();
                _activeStoryboards.Remove(element);
            }
        }
        /// <summary>
        /// Creates a fade-in animation for opacity
        /// </summary>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        /// <param name="fromOpacity">Starting opacity (default 0)</param>
        /// <param name="toOpacity">Ending opacity (default 1)</param>
        /// <returns>DoubleAnimation configured for fade-in</returns>
        public static DoubleAnimation CreateFadeInAnimation(int durationMilliseconds = 300, double fromOpacity = 0, double toOpacity = 1)
        {
            var animation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            return animation;
        }

        /// <summary>
        /// Creates a fade-out animation for opacity
        /// </summary>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        /// <param name="fromOpacity">Starting opacity (default 1)</param>
        /// <param name="toOpacity">Ending opacity (default 0)</param>
        /// <returns>DoubleAnimation configured for fade-out</returns>
        public static DoubleAnimation CreateFadeOutAnimation(int durationMilliseconds = 300, double fromOpacity = 1, double toOpacity = 0)
        {
            var animation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            return animation;
        }

        /// <summary>
        /// Applies a professional fade-in animation using storyboard
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        /// <param name="completedCallback">Optional callback when animation completes</param>
        public static void FadeIn(UIElement element, int durationMilliseconds = 300, EventHandler completedCallback = null)
        {
            if (element == null)
                return;

            // Stop any existing animation
            StopActiveStoryboard(element);

            element.Opacity = 0;
            
            // Create fade-in animation with QuadraticEase for smooth feel
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Create storyboard
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            if (completedCallback != null)
            {
                storyboard.Completed += completedCallback;
            }

            // Track and begin storyboard
            _activeStoryboards[element] = storyboard;
            storyboard.Begin();
        }

        /// <summary>
        /// Applies a fade-out animation using storyboard
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        /// <param name="completedCallback">Optional callback when animation completes</param>
        public static void FadeOut(UIElement element, int durationMilliseconds = 300, EventHandler completedCallback = null)
        {
            if (element == null)
                return;

            // Stop any existing animation
            StopActiveStoryboard(element);

            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // Create storyboard
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            if (completedCallback != null)
            {
                storyboard.Completed += completedCallback;
            }

            // Track and begin storyboard
            _activeStoryboards[element] = storyboard;
            storyboard.Begin();
        }

        /// <summary>
        /// Applies a fade animation with automatic visibility management using storyboard
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="fadeIn">True for fade-in, false for fade-out</param>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        public static void FadeWithVisibility(UIElement element, bool fadeIn, int durationMilliseconds = 300)
        {
            if (element == null)
                return;

            if (fadeIn)
            {
                element.Visibility = Visibility.Visible;
                FadeIn(element, durationMilliseconds);
            }
            else
            {
                FadeOut(element, durationMilliseconds, (s, e) =>
                {
                    element.Visibility = Visibility.Collapsed;
                });
            }
        }

        /// <summary>
        /// Creates a professional staggered fade-in with scale animation for multiple elements
        /// </summary>
        /// <param name="elements">Array of elements to animate</param>
        /// <param name="durationMilliseconds">Duration of each animation in milliseconds</param>
        /// <param name="staggerDelayMilliseconds">Delay between each element animation</param>
        public static void StaggeredFadeIn(UIElement[] elements, int durationMilliseconds = 300, int staggerDelayMilliseconds = 50)
        {
            if (elements == null || elements.Length == 0)
                return;

            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var delay = i * staggerDelayMilliseconds;

                // Schedule the fade-in with delay
                _ = System.Threading.Tasks.Task.Delay(delay).ContinueWith(_ =>
                {
                    FadeIn(element, durationMilliseconds);
                });
            }
        }

        /// <summary>
        /// Creates a snappy snap-in animation with fade-in effect
        /// Uses native .NET 10 storyboard animations optimized for performance
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        public static void SnapIn(UIElement element, int durationMilliseconds = 250)
        {
            if (element == null)
                return;

            // Ensure we're on the UI thread
            if (element.Dispatcher.CheckAccess())
            {
                PerformSnapIn(element, durationMilliseconds);
            }
            else
            {
                element.Dispatcher.Invoke(() => PerformSnapIn(element, durationMilliseconds));
            }
        }

        /// <summary>
        /// Creates a snap-in animation that avoids flickering during rapid updates
        /// Only animates if element is not already visible or if animation is in progress
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="durationMilliseconds">Duration of the animation in milliseconds</param>
        public static void SnapInSmooth(UIElement element, int durationMilliseconds = 250)
        {
            if (element == null)
                return;

            // Ensure we're on the UI thread
            if (element.Dispatcher.CheckAccess())
            {
                PerformSnapInSmooth(element, durationMilliseconds);
            }
            else
            {
                element.Dispatcher.Invoke(() => PerformSnapInSmooth(element, durationMilliseconds));
            }
        }

        /// <summary>
        /// Internal method to perform snap-in animation with flicker prevention
        /// </summary>
        private static void PerformSnapInSmooth(UIElement element, int durationMilliseconds)
        {
            // If element is already fully visible and no animation is running, skip animation
            if (element.Opacity >= 0.99 && !_activeStoryboards.ContainsKey(element))
            {
                return;
            }

            // Stop any existing animation
            StopActiveStoryboard(element);

            // Only set opacity to 0 if it's not already animating or visible
            if (element.Opacity < 0.5)
            {
                element.Opacity = 0;
            }

            // Create snappy fade-in animation with CubicEase for responsive feel
            var fadeAnimation = new DoubleAnimation
            {
                From = element.Opacity, // Start from current opacity to avoid flicker
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Create and configure storyboard
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);

            // Set animation target
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            // Track storyboard for potential cancellation
            _activeStoryboards[element] = storyboard;

            // Begin the storyboard animation
            storyboard.Begin();
        }

        /// <summary>
        /// Internal method to perform snap-in animation with snappy fade-in using storyboard
        /// </summary>
        private static void PerformSnapIn(UIElement element, int durationMilliseconds)
        {
            // Stop any existing animation
            StopActiveStoryboard(element);

            // Set initial opacity to 0
            element.Opacity = 0;

            // Create snappy fade-in animation with CubicEase for responsive feel
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Create and configure storyboard
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);

            // Set animation target
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            // Track storyboard for potential cancellation
            _activeStoryboards[element] = storyboard;

            // Begin the storyboard animation
            storyboard.Begin();
        }
    }
}
