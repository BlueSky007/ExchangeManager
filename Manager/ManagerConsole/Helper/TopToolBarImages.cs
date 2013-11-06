﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ManagerConsole.Helper
{
    public class TopToolBarImages
    {
        private BitmapImage _UpdateImg;
        private BitmapImage _ModifyImg;
        private BitmapImage _CancelImg;
        private BitmapImage _ExecuteImg;

        public BitmapImage UpdateImg
        {
            get { return this._UpdateImg; }
            set { this._UpdateImg = value; }
        }

        public BitmapImage ModifyImg
        {
            get { return this._ModifyImg; }
            set { this._ModifyImg = value; }
        }

        public BitmapImage CancelImg
        {
            get { return this._CancelImg; }
            set { this._CancelImg = value; }
        }

        public BitmapImage ExecuteImg
        {
            get { return this._ExecuteImg; }
            set { this._ExecuteImg = value; }
        }

        public TopToolBarImages()
        {
            this._UpdateImg = new BitmapImage(new Uri("../Images/TopToolBar/Update.png", UriKind.Relative));
            this._ModifyImg = new BitmapImage(new Uri("../Images/TopToolBar/Modify.png", UriKind.Relative));
            this._CancelImg = new BitmapImage(new Uri("../Images/TopToolBar/CancelOrder.png", UriKind.Relative));
            this.ExecuteImg = new BitmapImage(new Uri("../Images/TopToolBar/Execute.png", UriKind.Relative));
        }
    }
}