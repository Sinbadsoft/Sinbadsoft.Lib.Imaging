// <copyright file="StreamAdapter.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2010
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by 
// applicable law or agreed to in writing, software distributed under the License
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// </copyright>
// <author>Chaker Nakhli</author>
// <email>chaker.nakhli@sinbadsoft.com</email>
// <date>2010/11/04</date>
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sinbadsoft.Lib.Imaging.InteropServices
{
    internal class StreamAdapter : IStream
    {
        private readonly Stream stream;

        public StreamAdapter(Stream stream)
        {
            this.stream = stream;
        }

        void IStream.Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            Marshal.WriteInt64(pcbRead, this.stream.Read(pv, 0, cb));
        }

        void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            this.stream.Write(pv, 0, cb);
            Marshal.WriteInt64(pcbWritten, cb);
        }

        void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            SeekOrigin seekOrigin;
            switch (dwOrigin)
            {
                case 0:
                    seekOrigin = SeekOrigin.Begin;
                    break;
                case 1:
                    seekOrigin = SeekOrigin.Current;
                    break;
                default:
                    seekOrigin = SeekOrigin.End;
                    break;
            }

            var seek = this.stream.Seek(dlibMove, seekOrigin);
            if (plibNewPosition != IntPtr.Zero)
            {
                Marshal.WriteInt64(plibNewPosition, seek);
            }
        }

        void IStream.SetSize(long libNewSize)
        {
            this.stream.SetLength(libNewSize);
        }

        void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            var bytes = new byte[cb];
            Marshal.WriteInt64(pcbRead, this.stream.Read(bytes, 0, (int)cb));
            Marshal.WriteInt64(pcbWritten, cb);
            this.stream.Write(bytes, 0, (int)cb);
        }

        void IStream.Commit(int grfCommitFlags)
        {
            this.stream.Flush();
        }

        void IStream.Revert()
        {
        }

        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        void IStream.Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG { type = 2 };
        }

        void IStream.Clone(out IStream ppstm)
        {
            ppstm = (IStream)MemberwiseClone();
        }
    }
}