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

        private BitmapImage _CreateTaskImage;
        private BitmapImage _EditorTaskImage;
        private BitmapImage _DeleteTaskImage;
        private BitmapImage _RefreshTaskImage;
        private BitmapImage _RunTaskImage;
        private BitmapImage _EnableTaskImage;
        private BitmapImage _DisabledTaskImage;
        private BitmapImage _StopTaskImage;


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

            this._CreateTaskImage = new BitmapImage();
            this._CreateTaskImage.BeginInit();
            this._CreateTaskImage.UriSource = new Uri(@"..\..\Asset\Images\add.png", UriKind.RelativeOrAbsolute);
            this._CreateTaskImage.EndInit();

            this._EditorTaskImage = new BitmapImage();
            this._EditorTaskImage.BeginInit();
            this._EditorTaskImage.UriSource = new Uri(@"..\..\Asset\Images\edit.png", UriKind.RelativeOrAbsolute);
            this._EditorTaskImage.EndInit();

            this._DeleteTaskImage = new BitmapImage();
            this._DeleteTaskImage.BeginInit();
            this._DeleteTaskImage.UriSource = new Uri(@"..\..\Asset\Images\delete.png", UriKind.RelativeOrAbsolute);
            this._DeleteTaskImage.EndInit();

            this._RefreshTaskImage = new BitmapImage();
            this._RefreshTaskImage.BeginInit();
            this._RefreshTaskImage.UriSource = new Uri(@"..\..\Asset\Images\Refresh.gif", UriKind.RelativeOrAbsolute);
            this._RefreshTaskImage.EndInit();

            this._RunTaskImage = new BitmapImage();
            this._RunTaskImage.BeginInit();
            this._RunTaskImage.UriSource = new Uri(@"..\..\Asset\Images\play.png", UriKind.RelativeOrAbsolute);
            this._RunTaskImage.EndInit();

            this._EnableTaskImage = new BitmapImage();
            this._EnableTaskImage.BeginInit();
            this._EnableTaskImage.UriSource = new Uri(@"..\..\Asset\Images\forward.png", UriKind.RelativeOrAbsolute);
            this._EnableTaskImage.EndInit();

            this._DisabledTaskImage = new BitmapImage();
            this._DisabledTaskImage.BeginInit();
            this._DisabledTaskImage.UriSource = new Uri(@"..\..\Asset\Images\pause.png", UriKind.RelativeOrAbsolute);
            this._DisabledTaskImage.EndInit();

            this._StopTaskImage = new BitmapImage();
            this._StopTaskImage.BeginInit();
            this._StopTaskImage.UriSource = new Uri(@"..\..\Asset\Images\Stop.png", UriKind.RelativeOrAbsolute);
            this._StopTaskImage.EndInit();
        }

        public BitmapImage UnknownImage { get { return this._UnknownImage; } }
        public BitmapImage ConnectingImage { get { return this._ConnectingImage; } }
        public BitmapImage UserConnectedImage { get { return this._UserConnectedImage; } }
        public BitmapImage UserDisconnectedImage { get { return this._UserDisconnectedImage; } }
        public BitmapImage ConnectedImage { get { return this._ConnectedImage; } }
        public BitmapImage DisconnectedImage { get { return this._DisconnectedImage; } }

        public BitmapImage CreateTaskImage { get { return this._CreateTaskImage; } }
        public BitmapImage EditorTaskImage { get { return this._EditorTaskImage; } }
        public BitmapImage DeleteTaskImage { get { return this._DeleteTaskImage; } }
        public BitmapImage RefreshTaskImage { get { return this._RefreshTaskImage; } }
        public BitmapImage RunTaskImage { get { return this._RunTaskImage; } }
        public BitmapImage EnableTaskImage { get { return this._EnableTaskImage; } }
        public BitmapImage DisabledTaskImage { get { return this._DisabledTaskImage; } }
        public BitmapImage StopTaskImage { get { return this._StopTaskImage; } }
    }
}
