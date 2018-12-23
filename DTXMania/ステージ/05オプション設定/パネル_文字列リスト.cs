﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.オプション設定
{
    /// <summary>
    ///		任意個の文字列から１つを選択できるパネル項目。
    ///		コンストラクタから活性化までの間に、<see cref="選択肢リスト"/> を設定すること。
    /// </summary>
    class パネル_文字列リスト : パネル
    {
        public int 現在選択されている選択肢の番号 { get; protected set; } = 0;
        public List<string> 選択肢リスト { get; protected set; } = new List<string>();

        public パネル_文字列リスト( string パネル名, int 初期選択肢番号 = 0, IEnumerable<string> 選択肢初期値s = null, Action<パネル> 値の変更処理 = null )
            : base( パネル名, 値の変更処理 )
        {
            //using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.現在選択されている選択肢の番号 = 初期選択肢番号;

                // 初期値があるなら設定する。
                if( null != 選択肢初期値s )
                {
                    foreach( var item in 選択肢初期値s )
                        this.選択肢リスト.Add( item );
                }

                Log.Info( $"文字列リストパネルを生成しました。[{this}]" );
            }
        }

        protected override void On活性化()
        {
            Trace.Assert( 0 < this.選択肢リスト.Count, "リストが空です。活性化するより先に設定してください。" );

            this._選択肢画像リスト = new Dictionary<string, 文字列画像>();

            for( int i = 0; i < this.選択肢リスト.Count; i++ )
            {
                var image = new 文字列画像() {
                    表示文字列 = this.選択肢リスト[ i ],
                    フォントサイズpt = 34f,
                    前景色 = Color4.White,
                };

                this._選択肢画像リスト.Add( this.選択肢リスト[ i ], image );

                this.子を追加する( image );
            }

            base.On活性化();   //忘れないこと
        }
        protected override void On非活性化()
        {
            foreach( var kvp in this._選択肢画像リスト )
                this.子を削除する( kvp.Value );

            this._選択肢画像リスト = null;

            base.On非活性化();   //忘れないこと
        }

        public override void 左移動キーが入力された()
        {
            this.現在選択されている選択肢の番号 = ( this.現在選択されている選択肢の番号 - 1 + this.選択肢リスト.Count ) % this.選択肢リスト.Count;
            this._値の変更処理?.Invoke( this );
        }
        public override void 右移動キーが入力された()
        {
            this.現在選択されている選択肢の番号 = ( this.現在選択されている選択肢の番号 + 1 ) % this.選択肢リスト.Count;
            this._値の変更処理?.Invoke( this );
        }
        public override void 確定キーが入力された()
            => this.右移動キーが入力された();

        public override void 進行描画する( DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // パネルの共通部分を描画。
            base.進行描画する( dc, left, top, 選択中 );

            // 以下、項目部分の描画。

            float 拡大率Y = (float) this._パネルの高さ割合.Value;
            float 項目の上下マージン = this.項目領域.Height * ( 1f - 拡大率Y ) / 2f;

            var 項目矩形 = new RectangleF(
                x: this.項目領域.X + left,
                y: this.項目領域.Y + top + 項目の上下マージン,
                width: this.項目領域.Width,
                height: this.項目領域.Height * 拡大率Y );

            var 項目画像 = this._選択肢画像リスト[ this.選択肢リスト[ this.現在選択されている選択肢の番号 ] ];

            float 拡大率X = Math.Min( 1f, ( 項目矩形.Width - 20f ) / 項目画像.画像サイズdpx.Width );    // -20 は左右マージンの最低値[dpx]

            項目画像.描画する(
                dc,
                項目矩形.Left + ( 項目矩形.Width - 項目画像.画像サイズdpx.Width * 拡大率X ) / 2f,
                項目矩形.Top + ( 項目矩形.Height - 項目画像.画像サイズdpx.Height * 拡大率Y ) / 2f,
                X方向拡大率: 拡大率X,
                Y方向拡大率: 拡大率Y );
        }

        public override string ToString()
            => $"{this.パネル名}, 選択肢: [{string.Join( ",", this.選択肢リスト )}]";


        private Dictionary<string, 文字列画像> _選択肢画像リスト = null;
    }
}
