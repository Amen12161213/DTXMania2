﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace DTXMania2.演奏
{
    class 右サイドクリアパネル : IDisposable
    {

        // プロパティ


        public 描画可能画像 クリアパネル { get; }



        // 生成と終了


        public 右サイドクリアパネル()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._背景 = new 画像D2D( @"$(Images)\PlayStage\RightSideClearPanel.png" );
            this.クリアパネル = new 描画可能画像( new Size2F( 500, 990 ) );  // this._背景.サイズはまだ設定されていない。
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this.クリアパネル.Dispose();
            this._背景.Dispose();
        }



        // クリア


        /// <summary>
        ///		クリアパネルにそれまで描かれていた内容を消去する。
        /// </summary>
        public void クリアする()
        {
            this.クリアパネル.画像へ描画する( ( dcp ) => {

                dcp.Transform = Matrix3x2.Identity;  // 等倍描画(DPXtoDPX)
                dcp.PrimitiveBlend = PrimitiveBlend.Copy;

                dcp.Clear( new Color4( Color3.Black, 0f ) );
                dcp.DrawBitmap( this._背景.Bitmap, opacity: 1f, interpolationMode: InterpolationMode.Linear );

            } );
        }



        // 進行と描画


        public void 描画する()
        {
            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 変換行列 =
                Matrix.RotationY( MathUtil.DegreesToRadians( +48f ) ) *
                Matrix.Translation( Global.画面左上dpx.X + 1630f, Global.画面左上dpx.Y - 530f, 0f );

            this.クリアパネル.描画する( 変換行列 );
        }



        // ローカル


        private readonly 画像D2D _背景;
    }
}
