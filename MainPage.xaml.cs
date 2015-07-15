﻿using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Stuart
{
    // UI codebehind for the main application page.
    public sealed partial class MainPage : Page
    {
        Photo photo = new Photo();

#if DEBUG
        int drawCount;
#endif


        public MainPage()
        {
            this.InitializeComponent();

            DataContext = photo;

            photo.PropertyChanged += Photo_PropertyChanged;
        }


        void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }


        async Task CreateResourcesAsync(CanvasControl sender)
        {
            await photo.Load(sender.Device, "bran.jpg");

            // Convert the photo size from pixels to dips.
            var photoSize = photo.Size;

            photoSize.X = sender.ConvertPixelsToDips((int)photoSize.X);
            photoSize.Y = sender.ConvertPixelsToDips((int)photoSize.Y);

            // Size the CanvasControl to exactly fit the image.
            sender.Width = photoSize.X;
            sender.Height = photoSize.Y;

            // Zoom so the whole image is visible.
            var viewSize = new Vector2((float)scrollView.ActualWidth, (float)scrollView.ActualHeight);
            var sizeRatio =  viewSize / photoSize;
            var zoomFactor = Math.Min(sizeRatio.X, sizeRatio.Y) * 0.95f;

            scrollView.ChangeView(null, null, zoomFactor, disableAnimation: true);
        }


        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            photo.Draw(args.DrawingSession);

#if DEBUG
            args.DrawingSession.DrawText((++drawCount).ToString(), 0, 0, Colors.Cyan);
#endif
        }


        void Photo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedEffect":
                    break;

                default:
                    canvas.Invalidate();
                    break;
            }
        }


        void NewEdit_Click(object sender, RoutedEventArgs e)
        {
            photo.Edits.Add(new EditGroup(photo));
        }


        void EditList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties.Add("DragItems", e.Items.ToArray());
        }


        void TrashCan_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }


        void TrashCan_Drop(object sender, DragEventArgs e)
        {
            var items = (object[])e.Data.GetView().Properties["DragItems"];

            foreach (IDisposable item in items)
            {
                item.Dispose();
            }
        }


        void Background_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            photo.SelectedEffect = null;
        }
    }
}
