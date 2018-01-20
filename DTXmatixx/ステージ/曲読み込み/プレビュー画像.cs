﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.曲読み込み
{
    class プレビュー画像 : Activity
    {
        public プレビュー画像()
        {
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 描画する( DeviceContext1 dc )
        {
            var 選択曲 = App.曲ツリー.フォーカス曲ノード;
            Debug.Assert( null != 選択曲 );

            var preimage = 選択曲.ノード画像 ?? Node.既定のノード画像;
            Debug.Assert( null != preimage );

            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
                -グラフィックデバイス.Instance.設計画面サイズ.Width / 2f,
                +グラフィックデバイス.Instance.設計画面サイズ.Height / 2f,
                0f );

            var 変換行列 =
                Matrix.Scaling( this._プレビュー画像表示サイズdpx ) *
                Matrix.Translation(
                    画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
                    画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
                    0f );

            preimage.描画する( 変換行列 );
        }

        private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 150f, 117f, 0f );
        private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 576f, 576f, 0f );
    }
}
