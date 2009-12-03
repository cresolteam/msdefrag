﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MSDefragLib
{
    public class DiskStruct : IDisposable
    {
        public DiskStruct()
        {
            VolumeHandle = IntPtr.Zero;
        }

        private IntPtr VolumeHandle
        {
            get;
            set;
        }

        private void Open(String path)
        {
            Close();
            VolumeHandle = IO.IOWrapper.OpenVolume(path);
        }

        public Boolean IsOpen
        {
            get
            {
                return VolumeHandle != IntPtr.Zero;
            }
        }

        private String _mountPoint;

        /* Example: "c:" */
        public String MountPoint
        {
            get
            {
                return _mountPoint;
            }
            set
            {
                _mountPoint = value;
                String root = Path.GetPathRoot(value);
                Open(root.Replace(@"\",""));
            }
        }
        
        public String MountPointSlash
        {
            // Example: "c:\"
            get { return MountPoint + @"\"; }
        }
        
        private String VolumeName;          /* Example: "\\?\Volume{08439462-3004-11da-bbca-806d6172696f}" */

        public String VolumeNameSlash
        {
            // Example: "\\?\Volume{08439462-3004-11da-bbca-806d6172696f}\"
            get { return VolumeName + @"\"; }
        }

        /// <summary>
        /// Returns the filesystem of this volume
        /// </summary>
        public FS.Filesystem Filesystem
        {
            get { return BootSector.Filesystem; }
        }

        public UInt64 MftLockedClusters;    /* Number of clusters at begin of MFT that cannot be moved. */

        /// <summary>
        /// Read data from this disk starting at the given LCN
        /// </summary>
        /// <param name="lcn"></param>
        /// <param name="buffer">Buffer to copy the data into</param>
        /// <param name="start">Start index inside buffer</param>
        /// <param name="count">Number of bytes to read</param>
        public void ReadFromCluster(UInt64 lcn, Byte[] buffer, int start, int count)
        {
            Overlapped overlapped = IO.OverlappedBuilder.Get(lcn);
            int bytesRead = IO.IOWrapper.Read(VolumeHandle, buffer, start, count, overlapped);
            if (bytesRead != count)
            {
                String errorMessage = IO.IOWrapper.GetMessage(Marshal.GetLastWin32Error());
                //ShowDebug(2, String.Format("      Error while reading Inode {0:G}: " + errorMessage, RefInode));
                throw new Exception("Could not read the data from disk!");
            }
        }


        private FS.IBootSector _bootSector;

        public FS.IBootSector BootSector
        {
            get
            {
                if (_bootSector == null)
                {
                    FS.Volume volume = new FS.Volume(VolumeHandle);
                    _bootSector = volume.BootSector;
                }
                return _bootSector;
            }
        }

        public IO.IOWrapper.BitmapData VolumeBitmap
        {
            get
            {
                return IO.IOWrapper.GetVolumeMap(VolumeHandle);
            }
        }

        public IO.IOWrapper.NTFS_VOLUME_DATA_BUFFER NtfsVolumeData
        {
            get
            {
                return IO.IOWrapper.GetNtfsInfo(VolumeHandle);
            }
        }



        public void Close()
        {
            if (IsOpen)
                IO.IOWrapper.CloseHandle(VolumeHandle);
            VolumeHandle = IntPtr.Zero;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    };

}
