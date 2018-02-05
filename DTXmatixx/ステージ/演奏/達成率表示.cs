﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
    class 達成率表示 : Activity
    {
        public 達成率表示()
        {
            this.子を追加する( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大矩形.xml", 文字幅補正dpx: 0f ) );
            this.子を追加する( this._達成率ロゴ画像 = new 画像( @"$(System)images\達成率ロゴ.png" ) );
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

        public void 描画する( DeviceContext1 dc, float 達成率 )
        {
            var 描画領域 = new RectangleF( 220f, 650f, 165f, 80f );
            達成率 = Math.Max( Math.Min( 達成率, 99.99f ), 0f );  // 0～99.99にクリッピング

            string 達成率文字列 = ( 達成率.ToString( "0.00" ) + '%' ).PadLeft( 6 ).Replace( ' ', 'o' );  // 右詰め、余白は'o'。例:"99.00%", "o8.12%", "o0.00%"

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                var pretrans = dc.Transform;

                // 達成率ロゴを描画する
                dc.Transform =
                    Matrix3x2.Scaling( 0.4f, 0.5f ) *
                    Matrix3x2.Translation( 描画領域.X - 30f, 描画領域.Y - 60f ) *
                    pretrans;

                this._達成率ロゴ画像.描画する( dc, 0f, 0f );

                // 小数部を描画する（'%'含む）
                dc.Transform =
                    Matrix3x2.Scaling( 0.5f, 0.8f ) *
                    Matrix3x2.Translation( 描画領域.X + 65f, 描画領域.Y + ( 描画領域.Height * 0.2f ) ) *
                    pretrans;

                this._数字画像.描画する( dc, 0f, 0f, 達成率文字列.Substring( 3 ) );

                // 整数部を描画する（'.'含む）
                dc.Transform =
                    Matrix3x2.Scaling( 0.5f, 1.0f ) *
                    Matrix3x2.Translation( 描画領域.X, 描画領域.Y ) *
                    pretrans;

                this._数字画像.描画する( dc, 0f, 0f, 達成率文字列.Substring( 0, 3 ) );

            } );
        }

        private 画像フォント _数字画像 = null;
        private 画像 _達成率ロゴ画像 = null;
    }
}
