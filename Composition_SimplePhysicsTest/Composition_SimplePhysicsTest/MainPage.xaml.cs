using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Composition_SimplePhysicsTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
	    private Visual _time;
	    private Compositor _compositor;
	    private ScalarKeyFrameAnimation _clock;

	    public MainPage()
        {
            this.InitializeComponent();
			this.Loaded += MainPage_Loaded;
        }

	    private const float Gravity = 9.82f;

		private void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			/* time */
			_time = ElementCompositionPreview.GetElementVisual(Time);
			_time.CenterPoint = new Vector3(50.0f, 50.0f, 0.0f);
			_compositor = _time.Compositor;
			_clock = _compositor.CreateScalarKeyFrameAnimation();
			_clock.Duration = TimeSpan.FromSeconds(60*60); // 1 hr should be enough for all intents and purposes :)
			_clock.InsertKeyFrame(0.0f, 0.0f);
			_clock.InsertKeyFrame(1.0f, (float)(3.0f * _clock.Duration.TotalSeconds)); // timescale = 1.0f / second

			/* the falling object */

			var obj = ElementCompositionPreview.GetElementVisual(TheFallingObject);

			//                                                                     1/2 * 'a' * 't'^2
			var physicsAnimation = _compositor.CreateExpressionAnimation("0.5 * acceleration * time.RotationAngleInDegrees * time.RotationAngleInDegrees");

			physicsAnimation.SetReferenceParameter("time", _time);
			physicsAnimation.SetScalarParameter("acceleration", Gravity);

			obj.StartAnimation("Offset.Y", physicsAnimation);

			/* the springy object */

			//todo...
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	    {
		    _time.StartAnimation(nameof(_time.RotationAngleInDegrees), _clock);
	    }
    }
}
