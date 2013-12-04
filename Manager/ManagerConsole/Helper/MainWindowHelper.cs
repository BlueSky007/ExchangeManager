using Manager.Common;
using ManagerConsole.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ManagerConsole
{
    public class MainWindowHelper
    {
        private const string PaneNamePrefix = "module";

        public static string GetPaneName(int moduleType)
        {
            return string.Format("{0}{1}", MainWindowHelper.PaneNamePrefix, moduleType);
        }

        public static int GetModuleType(string paneName)
        {
            return int.Parse(paneName.Substring(MainWindowHelper.PaneNamePrefix.Length));
        }

        public static UserControl GetControl(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.UserManager:
                    return new UserManagerControl();
                case ModuleType.RoleManager:
                    return new RoleManagerControl();
                case ModuleType.InstrumentManager:
                    break;
                case ModuleType.QuotePolicy:
                    break;
                case ModuleType.QuotationSource:
                    return new QuotationSourceControl();
                case ModuleType.SourceQuotation:
                    return new SourceQuotationControl(); 
                case ModuleType.QuotationMonitor:
                    return new QuotationMonitorControl();
                case ModuleType.AbnormalQuotation:
                    break;
                case ModuleType.Quote:
                    return new QutePriceControl();
                case ModuleType.OrderProcess:
                    return new OrderTaskControl();
                case ModuleType.LimitBatchProcess:
                    return new LMTProcess();
                case ModuleType.MooMocProcess:
                    return new MooMocOrderTask();
                case ModuleType.LogAuditQuery:
                    return new LogAuditControl();
                case ModuleType.OrderSearch:
                    return new OrderSearchControl();
                case ModuleType.ExecutedOrder:
                    return new ExecutedOrders();
                case ModuleType.OpenInterest:
                    return new OpenInterestControl();
                default:
                    break;
            }
            return null;
        }
    }

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
                        //Trace...
                    }
                }
            }
        }
    }
}
