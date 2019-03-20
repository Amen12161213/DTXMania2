﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using SharpDX;

namespace DTXMania
{
    /// <summary>
    ///		BOXを表すノード。
    /// </summary>
    class BoxNode : Node
    {
        public BoxNode()
        {
        }

        /// <summary>
        ///		box.def ファイルからBOXノードを生成する。
        /// </summary>
        public BoxNode( VariablePath BOX定義ファイルパス, Node 親ノード )
            : this()
        {
            var box = BoxDef.復元する( BOX定義ファイルパス );

            this.タイトル = box.TITLE;
            this.親ノード = 親ノード;
        }

        /// <summary>
        ///		タイトル名からBOXノードを生成する。
        /// </summary>
        /// <remarks>
        ///		「DTXFiles.」で始まるBOXフォルダの場合はこちらで初期化。
        /// </remarks>
        public BoxNode( string BOX名, Node 親ノード )
            : this()
        {
            this.タイトル = BOX名;
            this.親ノード = 親ノード;
        }
    }
}
