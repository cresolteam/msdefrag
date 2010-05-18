﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MSDefragLib;
using System.Threading;
using System.Timers;

namespace MSDefrag
{
    public partial class MainForm : Form
    {
        #region Constructor

        public MainForm()
        {
            Initialize();
        }

        #endregion

        #region Initialization

        public void Initialize()
        {
            InitializeComponent();

            GuiRefreshTimer = new System.Timers.Timer(100);

            GuiRefreshTimer.Elapsed += new ElapsedEventHandler(OnRefreshGuiTimer);
            GuiRefreshTimer.Enabled = true;
        }

        private void InitializeBitmapDisplay()
        {
            diskBitmap = null;

            diskBitmap = new DiskBitmap(pictureBox1.Width, pictureBox1.Height, 10, m_defragmenter.NumClusters);
            pictureBox1.Image = diskBitmap.bitmap;
        }

        #endregion

        #region Graphics functions

        private void RefreshDisplay()
        {
            _inUse = true;

            if (diskBitmap != null)
            {
                diskBitmap.DrawAllMapSquares();
                pictureBox1.Invalidate();
            }

            _inUse = false;
        }

        private void AddChangedClustersToQueue(IList<MSDefragLib.ClusterState> changedClusters)
        {
            if (changedClusters == null || diskBitmap == null)
                return;

            diskBitmap.AddChangedClusters(changedClusters);
        }

        private void AddFilteredClustersToQueue(IList<MSDefragLib.MapClusterState> filteredClusters)
        {
            if (filteredClusters == null || diskBitmap == null)
                return;

            diskBitmap.AddFilteredClusters(filteredClusters);
        }

        #endregion

        #region Other

        private void StartDefragmentation(m_eDefragType mode)
        {
            switch (mode)
            {
                case m_eDefragType.defragTypeDefragmentation:
                    m_defragmenter = DefragmenterFactory.Create();
                    break;
                default:
                    m_defragmenter = DefragmenterFactory.CreateSimulation();
                    break;
            }

            m_defragmenter.StartDefragmentation("A");

            m_defragmenter.ProgressEvent += new ProgressHandler(UpdateProgress);
            //m_defragmenter.UpdateDiskMapEvent += new UpdateDiskMapHandler(UpdateDiskMap);
            m_defragmenter.UpdateFilteredDiskMapEvent += new UpdateFilteredDiskMapHandler(UpdateFilteredDiskMap);

            InitializeBitmapDisplay();
        }

        private void StopDefragmentation()
        {
            m_defragmenter.StopDefragmentation(4000);

            m_defragmenter.ProgressEvent -= new MSDefragLib.ProgressHandler(UpdateProgress);
            //m_defragmenter.UpdateDiskMapEvent -= new UpdateDiskMapHandler(UpdateDiskMap);
            m_defragmenter.UpdateFilteredDiskMapEvent -= new UpdateFilteredDiskMapHandler(UpdateFilteredDiskMap);
        }

        private void UpdateProgressBar(Double val)
        {
            progressBar.Value = (Int16)val;

            progressBarText.Text = String.Format("{0:P}", val * 0.01);
        }

        private void ShowStatistics()
        {
            progressBarStatistics.Text = "Frame skip: " + _skippedFrames;
        }

        #endregion

        #region Event Handling

        private void OnStartDefragmentation(object sender, EventArgs e)
        {
            toolButtonStartDefrag.Enabled = false;
            toolButtonStartSimulation.Enabled = false;
            toolButtonStopDefrag.Enabled = true;

            StartDefragmentation(m_eDefragType.defragTypeDefragmentation);
        }

        private void OnStartSimulation(object sender, EventArgs e)
        {
            toolButtonStartDefrag.Enabled = false;
            toolButtonStartSimulation.Enabled = false;
            toolButtonStopDefrag.Enabled = true;

            StartDefragmentation(m_eDefragType.defragTypeSimulation);
        }

        private void OnStopDefrag(object sender, EventArgs e)
        {
            toolButtonStartDefrag.Enabled = false;
            toolButtonStartSimulation.Enabled = false;
            toolButtonStopDefrag.Enabled = false;

            StopDefragmentation();

            toolButtonStartDefrag.Enabled = true;
            toolButtonStartSimulation.Enabled = true;
            toolButtonStopDefrag.Enabled = false;
        }

        private void UpdateDiskMap(object sender, EventArgs e)
        {
            if (e is ChangedClusterEventArgs)
            {
                ChangedClusterEventArgs ea = (ChangedClusterEventArgs)e;

                AddChangedClustersToQueue(ea.m_list);
            }
        }

        private void UpdateFilteredDiskMap(object sender, EventArgs e)
        {
            if (e is FilteredClusterEventArgs)
            {
                FilteredClusterEventArgs ea = (FilteredClusterEventArgs)e;

                AddFilteredClustersToQueue(ea.m_list);
            }
        }

        private void UpdateProgress(object sender, EventArgs e)
        {
            if (e is ProgressEventArgs)
            {
                ProgressEventArgs ea = (ProgressEventArgs)e;

                BeginInvoke(new MethodInvoker(delegate { UpdateProgressBar(ea.Progress); }));
            }
        }

        private void OnGuiClosing(object sender, FormClosingEventArgs e)
        {
            StopDefragmentation();
        }

        private void OnResizeBegin(object sender, EventArgs e)
        {
            ignoreEvent = true;
        }

        private void OnResizeEnd(object sender, EventArgs e)
        {
            ignoreEvent = false;
        }

        private bool _inUse = false;
        private int _skippedFrames = 0;

        private void OnRefreshGuiTimer(object source, ElapsedEventArgs e)
        {
            if (ignoreEvent)
                return;

            if (_inUse == false)
            {
                RefreshDisplay();
            }
            else
            {
                _skippedFrames++;

                BeginInvoke(new MethodInvoker(delegate { ShowStatistics(); })); 
            }
        }

        #endregion

        #region Variables

        Boolean ignoreEvent;

        System.Timers.Timer GuiRefreshTimer;

        private IDefragmenter m_defragmenter = null;

        public enum m_eDefragType
        {
            defragTypeDefragmentation = 0,
            defragTypeSimulation
        }

        DiskBitmap diskBitmap;

        #endregion
    }
}