﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using MSDefragLib;
using System.Windows.Forms;

namespace MSDefrag
{
    class DiskBitmap : PictureBox
    {
        #region Constructor

        public DiskBitmap()
        {
            BitmapSettings = new DiskBitmapSettings();
        }

        #endregion

        #region Initialization

        public void Initialize(GuiSettings guiSettings)
        {
            lock (BitmapSettings)
            {
                BitmapSettings.Initialize(Width, Height, guiSettings.SquareSize);

                InitializeDiskMap();

                DrawMapSquares(0, BitmapSettings.NumSquares);
            }
        }

        public void InitializeDiskMap()
        {
            Image = BitmapSettings.bitmap;

            graphics = Graphics.FromImage(BitmapSettings.bitmap);

            Rectangle drawingArea = BitmapSettings.DrawingArea;

            LinearGradientBrush brush = new LinearGradientBrush(drawingArea, Color.Black, Color.White, LinearGradientMode.ForwardDiagonal);
            graphics.FillRectangle(brush, drawingArea);

            Pen hiPen = Pens.White;
            Pen loPen = Pens.DarkGray;

            // Outside border
            Draw3DRectangle(BitmapSettings.OutsideBorder, hiPen, loPen);

            // Inside border
            Draw3DRectangle(BitmapSettings.InsideBorder, loPen, hiPen);

            Invalidate();
        }

        private void Draw3DRectangle(Rectangle rectangle, Pen highlightPen, Pen shadowPen)
        {
            graphics.DrawLine(highlightPen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
            graphics.DrawLine(shadowPen, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom);
            graphics.DrawLine(shadowPen, rectangle.Right, rectangle.Bottom, rectangle.Left, rectangle.Bottom);
            graphics.DrawLine(highlightPen, rectangle.Left, rectangle.Bottom, rectangle.Left, rectangle.Top);
        }

        #endregion

        /// <summary>
        /// This function parses whole cluster list and updates square information.
        /// </summary>
        public void AddFilteredClusters(IList<MapClusterState> clusters)
        {
            if (SystemBusy == true)
            {
                return;
            }

            lock (BitmapSettings)
            {
                BitmapSettings.AddFilteredClusters(clusters);
                DrawMapSquares(0, BitmapSettings.NumSquares);
            }
        }

        #region Drawing

        private void DrawMapSquares(Int32 indexBegin, Int32 indexEnd)
        {
            if (SystemBusy == true)
            {
                return;
            }

            BitmapSettings.DrawMapSquares(graphics, indexBegin, indexEnd);

            Invalidate();
        }

        public void DrawAllMapSquares()
        {
            if (SystemBusy == true)
            {
                return;
            }

            lock (BitmapSettings)
            {
                DrawMapSquares(0, BitmapSettings.NumSquares);
            }
        }

        public void SetBusy(Boolean busy)
        {
            SystemBusy = busy;
        }


        public Int32 NumSquares
        {
            get { return BitmapSettings.NumSquares; }
        }

        #endregion

        #region Variables

        private DiskBitmapSettings BitmapSettings;

        private Graphics graphics;

        private Boolean SystemBusy;

        #endregion
    }
}
