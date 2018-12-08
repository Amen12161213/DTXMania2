﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;

namespace FDK
{
    public class VideoFrame : IDisposable
    {
        public long 表示時刻100ns { get; set; }
        public Sample Sample { get; set; }  // Bitmap への変換前。
        public Bitmap Bitmap { get; set; }  // Sample からの変換後。Sample とビデオメモリを共有しているので注意。

        public void Dispose()
        {
            this.Bitmap?.Dispose();
            this.Sample?.Dispose();
        }
    }

}
