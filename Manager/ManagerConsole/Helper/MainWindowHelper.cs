using Manager.Common;
using ManagerConsole.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ManagerConsole
{
    public class MediaManager
    {
        public static MediaSource _AlertPriceMediaSource = new MediaSource("Media/AlertPrice1.wma");
        public static MediaSource _EnquiryMediaSource = new MediaSource("Media/Enquiry.wma");
        public static MediaSource _LMTOrderPlacingMediaSource = new MediaSource("Media/LMTOrderPlacing.wma");
        public static MediaSource _LMTOrderConfirmedMediaSource = new MediaSource("Media/LMTOrderConfirmed.wma");
        public static MediaSource _SPTOrderPlacingMediaSource = new MediaSource("Media/SPTOrderPlacing.wma");
        public static MediaSource _SPTOrderConfirmedMediaSource = new MediaSource("Media/SPTOrderConfirmed.wma");

        private MediaManager() { }

        public static void PlayMedia(MediaElement mediaElement, MediaSource mediaSource)
        {
            try
            {
                if (mediaSource != null && mediaSource.Uri != null)
                {
                    mediaElement.Stop();
                    mediaElement.Source = mediaSource.Uri;
                    mediaElement.Play();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerConsole.Helper.MediaManager:.\r\n{0}", ex.ToString());
            }
        }

        public static void PlayMedia(MediaElement mediaElement, string soundPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(soundPath))
                {
                    Uri path = new Uri(soundPath, UriKind.Absolute);
                    mediaElement.Stop();
                    mediaElement.Source = path;
                    mediaElement.Play();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerConsole.Helper.MediaManager:.\r\n{0}", ex.ToString());
            }
        }

        public static void PlaySound(MediaElement mediaElement,string soundkey)
        {
            switch (soundkey)
            {
                case "Enquiry":
                    PlayMedia(mediaElement, _EnquiryMediaSource);
                    break;
                default:
                     PlayMedia(mediaElement, _EnquiryMediaSource);
                    break;

            }
        }

        public class MediaSource
        {
            private Uri _Uri = null;

            public Uri Uri
            {
                get { return this._Uri; }
                set { this._Uri = value; }
            }

            public MediaSource() { }

            public MediaSource(Uri uri)
                :this()
            {
                this._Uri = uri;
            }
            public MediaSource(string uriString)
                :this()
            {
                this.SetUri(uriString);
            }

            public void SetUri(string uriString)
            {
                if (!string.IsNullOrWhiteSpace(uriString))
                {
                    try
                    {
                        this._Uri = new Uri(uriString, UriKind.Relative);
                    }
                    catch (Exception ex)
                    {
                        Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerConsole.Helper.MediaSource:.\r\n{0}", ex.ToString());
                    }
                }
            }
        }

        
    }
}
