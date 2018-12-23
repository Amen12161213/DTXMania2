﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using Newtonsoft.Json.Linq;

namespace FDK
{
    /// <summary>
    ///		任意個の文字を格納した一枚の画像と、それぞれの文字領域の矩形リストから、文字列を連続するD2D画像で表示する。
    /// </summary>
    public class 画像フォント : Activity
    {
        /// <summary>
        ///		それぞれの文字矩形の幅に加算する補正値。
        /// </summary>
        public float 文字幅補正dpx { get; set; } = 0f;

        /// <summary>
        ///		透明: 0 ～ 1 :不透明
        /// </summary>
        public float 不透明度 { get; set; } = 1f;


        public 画像フォント( VariablePath 文字盤の画像ファイルパス, VariablePath 文字盤設定ファイルパス, float 文字幅補正dpx = 0f, float 不透明度 = 1f )
            : this( 文字盤の画像ファイルパス, JObject.Parse( File.ReadAllText( 文字盤設定ファイルパス.変数なしパス ) ), 文字幅補正dpx, 不透明度 )
        {
        }

        public 画像フォント( VariablePath 文字盤の画像ファイルパス, JObject 文字盤設定, float 文字幅補正dpx = 0f, float 不透明度 = 1f )
        {
            this.子を追加する( this._文字盤 = new 画像( 文字盤の画像ファイルパス ) );
            this.文字幅補正dpx = 文字幅補正dpx;
            this.不透明度 = 不透明度;

            // 高速化のために、JObject を Dictionary に変換。
            this._矩形リスト = new Dictionary<string, SharpDX.RectangleF>();
            foreach( JProperty rc in 文字盤設定[ "矩形リスト" ].Where(
                ( jtoken ) => ( jtoken is JProperty && jtoken.Values().Count() == 4 && ( (JProperty) jtoken ).Value.Type == JTokenType.Array ) ) )  // 要素4個の配列のみ抽出
            {
                this._矩形リスト.Add( rc.Name, FDKUtilities.JsonToRectangleF( rc.Value ) );
            }
        }

        /// <param name="基点のX位置">左揃えなら左端位置、右揃えなら右端位置のX座標。</param>
        /// <param name="右揃え">trueなら右揃え、falseなら左揃え。</param>
        public void 描画する( DeviceContext1 dc, float 基点のX位置, float 上位置, string 表示文字列, bool 右揃え = false )
        {
            if( 表示文字列.Nullまたは空である() )
                return;

            // 有効文字（矩形リストに登録されている文字）の矩形、文字数を抽出し、文字列全体のサイズを計算する。

            var 文字列全体のサイズ = Size2F.Empty;

            var 有効文字矩形リスト =
                from ch in 表示文字列
                where ( this._矩形リスト.ContainsKey( ch.ToString() ) )
                select ( this._矩形リスト[ ch.ToString() ] );

            int 有効文字数 = 有効文字矩形リスト.Count();
            if( 0 == 有効文字数 )
                return;

            foreach( var 文字矩形 in 有効文字矩形リスト )
            {
                文字列全体のサイズ.Width += ( 文字矩形.Width + this.文字幅補正dpx );

                if( 文字列全体のサイズ.Height < 文字矩形.Height )
                    文字列全体のサイズ.Height = 文字矩形.Height;  // 文字列全体の高さは、最大の文字高に一致。
            }

            // 描画する。

            if( 右揃え )
                基点のX位置 -= 文字列全体のサイズ.Width;

            for( int i = 0; i < 有効文字数; i++ )
            {
                var 文字矩形 = 有効文字矩形リスト.ElementAt( i );

                this._文字盤.描画する(
                    dc,
                    基点のX位置,
                    上位置 + ( 文字列全体のサイズ.Height - 文字矩形.Height ),
                    転送元矩形: 文字矩形,
                    不透明度0to1: this.不透明度 );

                基点のX位置 += ( 文字矩形.Width + this.文字幅補正dpx );
            }
        }

        
        private 画像 _文字盤 = null;

        private Dictionary<string, RectangleF> _矩形リスト = null;
    }
}
