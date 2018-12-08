﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXmatixx.ステージ.演奏
{
    class 曲別SKILL : Activity
    {
        public 曲別SKILL()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子を追加する( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜.json", 文字幅補正dpx: -6f ) );
                this.子を追加する( this._ロゴ画像 = new 画像( @"$(System)images\曲別SKILLアイコン.png" ) );
            }
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

        public void 進行描画する( DeviceContext1 dc, double? スキル値 )
        {
            if( null == スキル値 )
                return;

            var skill = (double) スキル値;
            var 描画領域 = new RectangleF( 108f, 780f, 275f, 98f );
            string スキル値文字列 = skill.ToString( "0.00" ).PadLeft( 6 ).Replace( ' ', 'o' );  // 右詰め、余白は'o'。

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                var pretrans = dc.Transform;

                // 曲別SKILLアイコンを描画する
                dc.Transform =
                    Matrix3x2.Scaling( 0.375f, 0.5f ) *
                    Matrix3x2.Translation( 描画領域.X, 描画領域.Y ) *
                    pretrans;

                this._ロゴ画像.描画する( dc, 0f, 0f );

                // 小数部を描画する
                dc.Transform =
                    Matrix3x2.Scaling( 0.65f, 0.8f ) *
                    Matrix3x2.Translation( 描画領域.X + 90f + 105f, 描画領域.Y + ( 描画領域.Height * 0.2f ) ) *
                    pretrans;

                this._数字画像.描画する( dc, 0f, 0f, スキル値文字列.Substring( 4 ) );

                // 整数部を描画する（'.'含む）
                dc.Transform =
                    Matrix3x2.Scaling( 0.65f, 1.0f ) *
                    Matrix3x2.Translation( 描画領域.X + 90f, 描画領域.Y ) *
                    pretrans;

                this._数字画像.描画する( dc, 0f, 0f, スキル値文字列.Substring( 0, 4 ) );

            } );
        }


        private 画像フォント _数字画像 = null;
        private 画像 _ロゴ画像 = null;
    }
}
