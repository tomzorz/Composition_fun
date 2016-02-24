using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using FlickrNet;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Composition_Hotchkiss
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
	    private Compositor _compositor;
	    private Visual _canvasVisual;
	    private ExpressionAnimation _followScroll;

	    public MainPage()
        {
            this.InitializeComponent();
			Loaded += MainPage_Loaded;
        }

		private async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
		    _canvasVisual = ElementCompositionPreview.GetElementVisual(Canvas);
		    _compositor = _canvasVisual.Compositor;

			var f = new Flickr("3a68f22971d8d66b521b362c312c175c");

			//api key lifted from https://github.com/samjudson/flickrnet-samples/blob/master/UniversalWindowsApp/MainPage.xaml.cs, sorry... :)

			var albums = await f.PhotosetsGetListAsync("24662369@N07", 0, 200);

			var template = (DataTemplate)Resources["ImageTemplate"];
			var width = Canvas.ActualWidth;
			var itemsPerRow = Convert.ToInt32(Math.Floor(width / 300.0)); //270 is template size

			_followScroll = _compositor.CreateExpressionAnimation("scrollProps.Translation.Y");
			_followScroll.SetReferenceParameter("scrollProps", ElementCompositionPreview.GetScrollViewerManipulationPropertySet(FakeScrollViewer));

			var expandAnimations = new Dictionary<int, Vector3KeyFrameAnimation>();
			var contractAnimations = new Dictionary<int, Vector3KeyFrameAnimation>();
			for (int j = 0; j < 5; j++)
			{
				var a = _compositor.CreateVector3KeyFrameAnimation();
				a.Duration = TimeSpan.FromSeconds(0.5);
				a.InsertKeyFrame(0.0f, new Vector3(0.0f, 0.0f, j * 2.0f));
				a.InsertKeyFrame(1.0f, new Vector3(0.0f, 0.0f, j * 40.0f));
				expandAnimations[j] = a;
				var b = _compositor.CreateVector3KeyFrameAnimation();
				b.Duration = TimeSpan.FromSeconds(0.5);
				b.InsertKeyFrame(0.0f, new Vector3(0.0f, 0.0f, j * 40.0f));
				b.InsertKeyFrame(1.0f, new Vector3(0.0f, 0.0f, j * 2.0f));
				contractAnimations[j] = b;
			}

			var rnd = new Random();

			// ReSharper disable PossibleLossOfFraction
			var i = 0;
			foreach (var album in albums.Take(200))
			{
				if (i == itemsPerRow + 1) MoveIntoPosition();
				var ims = await f.PhotosetsGetPhotosAsync(album.PhotosetId);
				var r = new Rectangle
				{
					Width = 300.0,
					Height = 300.0,
					Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255))
				};
				Canvas.SetLeft(r, i % itemsPerRow * 300.0);
				Canvas.SetTop(r, i / itemsPerRow * 300.0);
				var inner = 0;
				foreach (var source in ims.Take(5))
				{
					var fwe = (FrameworkElement)template.LoadContent();
					fwe.DataContext = source;
					Canvas.SetLeft(fwe, i % itemsPerRow * 300.0);
					Canvas.SetTop(fwe, i / itemsPerRow * 300.0);
					InnerCanvas.Children.Add(fwe);
					var fweVisual = ElementCompositionPreview.GetElementVisual(fwe);
					var inner1 = inner;
					r.PointerEntered += (o, args) =>
					{
						fweVisual.StartAnimation(nameof(fweVisual.Offset), expandAnimations[inner1]);
					};
					r.PointerExited += (o, args) =>
					{
						fweVisual.StartAnimation(nameof(fweVisual.Offset), contractAnimations[inner1]);
					};
					fweVisual.Offset = new Vector3(0.0f, 0.0f, 2.0f * inner);
					fweVisual.RotationAngleInDegrees = ((float) rnd.NextDouble()*10.0f) - 5.0f;
					inner++;
				}
				InnerCanvas.Children.Add(r);
				ContentHeightPlaceholder.Height = i/itemsPerRow*300.0;
				i++;
			}
		}

	    private void MoveIntoPosition()
	    {
			ElementCompositionPreview.GetElementVisual(InnerCanvas).StartAnimation("Offset.Y", _followScroll);
			_canvasVisual.CenterPoint = new Vector3((float)Canvas.ActualWidth / 2.0f, (float)Canvas.ActualHeight / 2.0f, 0.0f);
		    var orientationAnimation = _compositor.CreateQuaternionKeyFrameAnimation();
			orientationAnimation.Duration = TimeSpan.FromSeconds(1.0);
			orientationAnimation.InsertKeyFrame(0.0f, Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, 0.0f));
			orientationAnimation.InsertKeyFrame(1.0f, Quaternion.CreateFromYawPitchRoll((float)((Math.PI * 2)/360.0f) * 0.0f , (float)((Math.PI * 2) / 360.0f) * 70.0f, (float)((Math.PI * 2) / 360.0f) * 45.0f));
		    var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
			offsetAnimation.Duration = TimeSpan.FromSeconds(1.0);
			offsetAnimation.InsertKeyFrame(0.0f, new Vector3(0.0f, 0.0f, 0.0f));
			offsetAnimation.InsertKeyFrame(0.0f, new Vector3(0.0f, 150.0f, 0.0f));
			_canvasVisual.StartAnimation(nameof(_canvasVisual.Orientation), orientationAnimation);
			_canvasVisual.StartAnimation(nameof(_canvasVisual.Offset), offsetAnimation);
		}
	}
}