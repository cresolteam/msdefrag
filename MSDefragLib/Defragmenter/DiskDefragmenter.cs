﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSDefragLib.Defragmenter
{
    internal class DiskDefragmenter : BaseDefragmenter
    {
        private DefragEventDispatcher m_defragEventDispatcher = new DefragEventDispatcher();

        public override DefragEventDispatcher defragEventDispatcher
        {
            get
            {
                return m_defragEventDispatcher;
            }
            set
            {
                m_defragEventDispatcher = value;
            }
        }

        private MSDefragLib lib = null;

        public DiskDefragmenter()
        {
            lib = new MSDefragLib(m_defragEventDispatcher);
        }

        #region IDefragmenter Members

        //public override event LogMessageHandler LogMessage
        //{
        //    add
        //    {
        //        lib.LogMessageEvent += value;
        //    }
        //    remove
        //    {
        //        lib.LogMessageEvent -= value;
        //    }
        //}

        public override void Start(string parameter)
        {
            lib.RunJkDefrag(@"C:\*", 2, 10, null, null);
            //lib.RunJkDefrag(@"T:\*", 2, 10, null, null);
        }

        public override void Stop(Int32 timeoutMs)
        {
            if ((lib.Data != null) && (lib.Data.Running == RunningState.RUNNING))
            {
                lib.StopJkDefrag(timeoutMs);
            }
        }

        public override void ResendAllClusters()
        {
            lib.ResendAllClusters();
        }

        public override UInt64 NumClusters
        {
            get
            {
                if (lib.Data != null)
                    return lib.Data.TotalClusters;
                else
                    return 0;
            }

            set {}
        }
        #endregion
    }
}