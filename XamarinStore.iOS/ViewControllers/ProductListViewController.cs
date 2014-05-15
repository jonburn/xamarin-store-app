using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Alliance.Carousel;
using PixateFreestyleLib;


namespace XamarinStore.iOS
{

	public class LinearDelegate : CarouselViewDelegate
	{
		ProductListViewController vc;
		readonly Action<Product> ProductSelected;
		public IReadOnlyList<Product> Products;

		public LinearDelegate(ProductListViewController vc,IReadOnlyList<Product> productSelected, Action<Product> product)
		{
			this.vc = vc;
			this.Products = productSelected;
			this.ProductSelected = product;
		}

		public override float ValueForOption(CarouselView carousel, CarouselOption option, float aValue)
		{
			if (option == CarouselOption.Spacing)
			{
				return aValue * 1.1f;
			}
			return aValue;
		}

		public override void DidSelectItem(CarouselView carousel, int index)
		{
			//Console.WriteLine("Selected: " + ++index);

			ProductSelected (Products [index]);


		}
	}


	public class LinearDataSource : CarouselViewDataSource
	{
		ProductListViewController vc;
		List<Product> product;

		public LinearDataSource(ProductListViewController vc, List<Product> p)
		{
			this.vc = vc;
			this.product = p;
		}

		public override uint NumberOfItems(CarouselView carousel)
		{
			return (uint)product.Count;
		}

		public override UIView ViewForItem(CarouselView carousel, uint index, UIView reusingView)
		{

			UILabel label;

			if (reusingView == null)
			{
				var imgView = new UIImageView(new RectangleF(0, 0, 200, 200))
				{

					Image = FromUrl( index > 1 ? product[0].ImageForSize(250) : product[(int)index].ImageForSize(250) ),
					ContentMode = UIViewContentMode.Center
				};


				label = new UILabel(imgView.Bounds)
				{
					BackgroundColor = UIColor.Clear,
					TextAlignment = UITextAlignment.Center,
					Tag = 1
				};
				label.Font = label.Font.WithSize(50);
				imgView.AddSubview(label);
				reusingView = imgView;
			}
			else
			{
				label = (UILabel)reusingView.ViewWithTag(1);
			}
				

			return reusingView;
		}

		static UIImage FromUrl (string uri)
		{
			using (var url = new NSUrl (uri))
			using (var data = NSData.FromUrl (url))
			return UIImage.LoadFromData (data);
		}


	}

	public class ProductListViewController : UIViewController
	{
		const int ProductCellRowHeight = 300;
		CarouselView carousel;
		public List<int> items;

		public event Action<Product> ProductTapped = delegate {};

		//ProductListViewSource source;

		UIButton btnRefresh;

		public ProductListViewController ()
		{
			Title = "Xamarin Store";

			// Hide the back button text when you leave this View Controller.
			NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler: null);

			btnRefresh = new UIButton ();//new RectangleF(10,400,200,30));
			//btnRefresh.SetTitle ("Refresh", UIControlState.Normal);
			btnRefresh.SetStyleId ("refresh");
			View.AddSubview (btnRefresh);

			GetData ();


			View.BackgroundColor = UIColor.White;

			btnRefresh.TouchUpInside += (object sender, EventArgs e) => {
				carousel.RemoveFromSuperview();
				GetData();
			};




		}

		async void GetData ()
		{
			//source.Products

			List<Product> PP = await WebService.Shared.GetProducts ();
			//Kicking off a task no need to await
			#pragma warning disable 4014
			WebService.Shared.PreloadImages (320 * UIScreen.MainScreen.Scale);
			#pragma warning restore 4014

			carousel = new CarouselView(new RectangleF(0,64,  View.Bounds.Width, View.Bounds.Height - 94));
			carousel.BackgroundColor = UIColor.White;
			carousel.DataSource = new LinearDataSource(this, PP);
			carousel.Delegate = new LinearDelegate(this, 
				PP,  (products => { ProductTapped(products); }));
			carousel.CarouselType = CarouselType.Rotary;
			carousel.ConfigureView();
			View.AddSubview(carousel);

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationItem.RightBarButtonItem = AppDelegate.Shared.CreateBasketButton ();
		}

	
	}
}