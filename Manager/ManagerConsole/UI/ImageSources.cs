using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ManagerConsole.UI
{
    public class ImageSources
    {
        private static ImageSources _Instance = new ImageSources();
        public static ImageSources Instance
        {
            get { return ImageSources._Instance; }
        }

        private BitmapImage _UnknownImage;
        private BitmapImage _ConnectingImage;

        private BitmapImage _UserConnectedImage;
        private BitmapImage _UserDisconnectedImage;

        private BitmapImage _ConnectedImage;
        private BitmapImage _DisconnectedImage;

        private ImageSources()
        {
            this._UnknownImage = new BitmapImage();
            this._UnknownImage.BeginInit();
            this._UnknownImage.UriSource = new Uri(@"..\Asset\Images\unknown.png", UriKind.RelativeOrAbsolute);
            this._UnknownImage.EndInit();

            this._ConnectingImage = new BitmapImage();
            this._ConnectingImage.BeginInit();
            this._ConnectingImage.UriSource = new Uri(@"..\Asset\Images\connecting.png", UriKind.RelativeOrAbsolute);
            this._ConnectingImage.EndInit();

            this._UserConnectedImage = new BitmapImage();
            this._UserConnectedImage.BeginInit();
            this._UserConnectedImage.UriSource = new Uri(@"..\Asset\Images\connected.png", UriKind.RelativeOrAbsolute);
            this._UserConnectedImage.EndInit();

            this._UserDisconnectedImage = new BitmapImage();
            this._UserDisconnectedImage.BeginInit();
            this._UserDisconnectedImage.UriSource = new Uri(@"..\Asset\Images\connect_no.png", UriKind.RelativeOrAbsolute);
            this._UserDisconnectedImage.EndInit();

            this._ConnectedImage = new BitmapImage();
            this._ConnectedImage.BeginInit();
            this._ConnectedImage.UriSource = new Uri(@"..\Asset\Images\connected2.png", UriKind.RelativeOrAbsolute);
            this._ConnectedImage.EndInit();

            this._DisconnectedImage = new BitmapImage();
            this._DisconnectedImage.BeginInit();
            this._DisconnectedImage.UriSource = new Uri(@"..\Asset\Images\connect_no2.png", UriKind.RelativeOrAbsolute);
            this._DisconnectedImage.EndInit();
        }

        public BitmapImage UnknownImage { get { return this._UnknownImage; } }
        public BitmapImage ConnectingImage { get { return this._ConnectingImage; } }
        public BitmapImage UserConnectedImage { get { return this._UserConnectedImage; } }
        public BitmapImage UserDisconnectedImage { get { return this._UserDisconnectedImage; } }
        public BitmapImage ConnectedImage { get { return this._ConnectedImage; } }
        public BitmapImage DisconnectedImage { get { return this._DisconnectedImage; } }
    }
}
