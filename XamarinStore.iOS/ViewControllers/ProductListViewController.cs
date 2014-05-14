using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Alliance.Carousel;

namespace XamarinStore.iOS
{

	public class LinearDelegate : CarouselViewDelegate
	{
		ProductListViewController vc;

		public LinearDelegate(ProductListViewController vc)
		{
			this.vc = vc;
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
			Console.WriteLine("Selected: " + ++index);
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

					Image = FromUrl( index > 1 ? product[0].ImageForSize(200) : product[(int)index].ImageForSize(200) ),
					ContentMode = UIViewContentMode.Center
				};


				//imgView.LoadUrl (index > 1 ? product[0].ImageUrl : product[(int)index].ImageUrl);


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

			label.Text = vc.items[(int)index].ToString();

			return reusingView;
		}

		static UIImage FromUrl (string uri)
		{
			using (var url = new NSUrl (uri))
			using (var data = NSData.FromUrl (url))
				return UIImage.LoadFromData (data);
			//return Scale2(UIImage.LoadFromData (data), new SizeF(200,200));
		}

		public static UIImage Scale (UIImage source, SizeF newSize)
		{
			UIGraphics.BeginImageContext (newSize);
			var context = UIGraphics.GetCurrentContext ();
			context.InterpolationQuality= MonoTouch.CoreGraphics.CGInterpolationQuality.High;
			context.TranslateCTM (0, newSize.Height);
			context.ScaleCTM (1f, -1f);

			context.DrawImage (new RectangleF (0, 0, newSize.Width, newSize.Height), source.CGImage);

			var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return scaledImage;         
		}

		public static UIImage Scale2 (UIImage source, SizeF newSize)
		{
			UIGraphics.BeginImageContext (newSize);
			var context = UIGraphics.GetCurrentContext ();
			context.TranslateCTM (0, newSize.Height);
			context.ScaleCTM (1f, -1f);

			context.DrawImage (new RectangleF (0, 0, newSize.Width, newSize.Height), source.CGImage);

			var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return scaledImage;         
		}
	}

	public class ProductListViewController : UITableViewController
	{
		const int ProductCellRowHeight = 300;
		static float ImageWidth = UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale;
		CarouselView carousel;
		public List<int> items;

		public event Action<Product> ProductTapped = delegate {};

		ProductListViewSource source;

		public ProductListViewController ()
		{
			Title = "Xamarin Store";

			// Hide the back button text when you leave this View Controller.
			NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler: null);
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.RowHeight = ProductCellRowHeight;
			TableView.Source = source = new ProductListViewSource (products => {
				ProductTapped (products);
			});

			items = Enumerable.Range(1, 100).ToList(); // Prettier than for (int i = 0; i < 100; i++)


			GetData ();


		}

		async void GetData ()
		{
			//source.Products

			List<Product> PP = await WebService.Shared.GetProducts ();
			//Kicking off a task no need to await
			#pragma warning disable 4014
			WebService.Shared.PreloadImages (320 * UIScreen.MainScreen.Scale);
			#pragma warning restore 4014

			carousel = new CarouselView(View.Bounds);
			carousel.DataSource = new LinearDataSource(this, PP);
			carousel.Delegate = new LinearDelegate(this);
			carousel.CarouselType = CarouselType.CoverFlow;
			carousel.ConfigureView();
			View.AddSubview(carousel);

			TableView.ReloadData ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationItem.RightBarButtonItem = AppDelegate.Shared.CreateBasketButton ();
		}

		class ProductListViewSource : UITableViewSource
		{
			readonly Action<Product> ProductSelected;
			public IReadOnlyList<Product> Products;

			public ProductListViewSource (Action<Product> productSelected)
			{
				ProductSelected = productSelected;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return Products == null ? 1 : Products.Count;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (Products == null)
					return;
				ProductSelected (Products [indexPath.Row]);
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				if (Products == null) {
					return new SpinnerCell ();
				}
							
				var cell = tableView.DequeueReusableCell (ProductListCell.CellId) as ProductListCell ?? new ProductListCell ();
				cell.Product = Products [indexPath.Row];
				return cell;
			}
		}

		class ProductListCell : UITableViewCell
		{
			public const string CellId = "ProductListCell";
			static readonly SizeF PriceLabelPadding = new SizeF (16, 6);
			Product product;
			TopAlignedImageView imageView;
			UILabel nameLabel, priceLabel;

			public Product Product {
				get { return product; }
				set {
					product = value;

					nameLabel.Text = product.Name;
					priceLabel.Text = product.PriceDescription.ToLower ();
					updateImage ();
				}
			}

			void updateImage()
			{
				var url = product.ImageForSize (ImageWidth);
				imageView.LoadUrl (url);
			}

			public ProductListCell ()
			{
				SelectionStyle = UITableViewCellSelectionStyle.None;
				ContentView.BackgroundColor = UIColor.LightGray;

				imageView = new TopAlignedImageView {
					ClipsToBounds = true,
				};

				nameLabel = new UILabel {
					TextColor = UIColor.White,
					TextAlignment = UITextAlignment.Left,
					Font = UIFont.FromName ("HelveticaNeue-Light", 22),
					//ShadowColor = UIColor.DarkGray,
					//ShadowOffset = new System.Drawing.SizeF(.5f,.5f),
					Layer = {
						ShadowRadius = 3,
						ShadowColor = UIColor.Black.CGColor,
						ShadowOffset = new System.Drawing.SizeF(0,1f),
						ShadowOpacity = .5f,
					}
				};

				priceLabel = new UILabel {
					Alpha = 0.95f,
					TextColor = UIColor.White,
					BackgroundColor = Color.Green,
					TextAlignment = UITextAlignment.Center,
					Font = UIFont.FromName ("HelveticaNeue", 16),
					ShadowColor = UIColor.LightGray,
					ShadowOffset = new SizeF(.5f, .5f),
				};

				var layer = priceLabel.Layer;
				layer.CornerRadius = 3;

				ContentView.AddSubviews (imageView, nameLabel, priceLabel);
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var bounds = ContentView.Bounds;

				imageView.Frame = bounds;
				nameLabel.Frame = new RectangleF (
					bounds.X + 12,
					bounds.Bottom - 58,
					bounds.Width,
					55
				);

				var priceSize = ((NSString)Product.PriceDescription).StringSize (priceLabel.Font);
				priceLabel.Frame = new RectangleF (
					bounds.Width - priceSize.Width - 2 * PriceLabelPadding.Width - 12,
					bounds.Bottom - priceSize.Height - 2 * PriceLabelPadding.Height - 14,
					priceSize.Width + 2 * PriceLabelPadding.Width,
					priceSize.Height + 2 * PriceLabelPadding.Height
				);
			}
		}
	}
}