﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SSTFormat.v4;
using FDK;

namespace DTXMania.曲
{
    /// <summary>
    ///		BOX定義。
    /// </summary>
    class BoxDef
    {
        /// <summary>
        ///		BOX名。
        /// </summary>
        public string TITLE { get; set; } = null;

        /// <summary>
        ///		制作者名など。
        /// </summary>
        public string ARTIST { get; set; } = null;


        public BoxDef()
        {
        }

        public static BoxDef 復元する( VariablePath Box定義ファイルパス )
        {
            var boxDef = new BoxDef();

            using( var sr = new StreamReader( Box定義ファイルパス.変数なしパス, Encoding.GetEncoding( 932/*Shift-JIS*/ ) ) )
            {
                string 行;

                while( ( 行 = sr.ReadLine() ) != null )
                {
                    try
                    {
                        string パラメータ = "";

                        #region " TITLE コマンド "
                        //---------------------
                        if( Utilities.コマンドのパラメータ文字列部分を返す( 行, @"TITLE", out パラメータ ) )
                        {
                            boxDef.TITLE = パラメータ;
                            continue;
                        }
                        //---------------------
                        #endregion
                        #region " ARTIST コマンド "
                        //---------------------
                        if( Utilities.コマンドのパラメータ文字列部分を返す( 行, @"ARTIST", out パラメータ ) )
                        {
                            boxDef.ARTIST = パラメータ;
                            continue;
                        }
                        //---------------------
                        #endregion
                    }
                    catch
                    {
                        // 例外は無視。
                    }
                }
            }

            return boxDef;
        }
    }
}
