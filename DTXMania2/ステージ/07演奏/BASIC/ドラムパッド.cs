using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace DTXMania2.演奏.BASIC
{
    class ドラムパッド : IDisposable
    {

        // 生成と終了


        public ドラムパッド()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            string mode = Global.App.ログオン中のユーザ.演奏モード.ToString();
            string layout = Global.App.ログオン中のユーザ.レーン配置.ToString();

            // 200115 OrzHighlight EXPERTフォルダがないため
            if (Global.App.ログオン中のユーザ.演奏モード == PlayMode.EXPERT)
                mode = PlayMode.BASIC.ToString();

            this._パッド画像 = new 画像(@"$(Images)\PlayStage\" + mode + @"\DrumPad.png");
            this._パッド絵の矩形リスト = new 矩形リスト(@"$(Images)\PlayStage\" + mode + @"\LaneType\" + layout + @"\DrumPad.yaml");
            this._レーンtoパッドContext = new Dictionary<表示レーン種別, パッドContext>();

            foreach (表示レーン種別 lane in Enum.GetValues(typeof(表示レーン種別)))
            {
                this._レーンtoパッドContext.Add(lane, new パッドContext()
                {
                    左上位置dpx = new Vector2(
                        x: レーンフレーム.領域.X + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[lane],
                        y: 840f),
                    転送元矩形 = this._パッド絵の矩形リスト[lane.ToString()]!.Value,
                    転送元矩形Flush = this._パッド絵の矩形リスト[lane.ToString() + "_Flush"]!.Value,
                    アニメカウンタ = new Counter(),
                });
            }
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._レーンtoパッドContext.Clear();
            this._パッド画像?.Dispose();
        }



        // ヒット


        public void ヒットする(表示レーン種別 lane)
        {
            this._レーンtoパッドContext[lane].アニメカウンタ.開始する(0, 100, 1);
        }



        // 進行と描画


        public void 進行描画する()
        {
            foreach (表示レーン種別 lane in Enum.GetValues(typeof(表示レーン種別)))
            {
                var drumContext = this._レーンtoパッドContext[lane];

                // ドラム側アニメーション
                float Yオフセットdpx = 0f;
                float フラッシュ画像の不透明度 = 0f;

                if (drumContext.アニメカウンタ.終了値に達していない)
                {
                    フラッシュ画像の不透明度 = (float)Math.Sin(Math.PI * drumContext.アニメカウンタ.現在値の割合);    // 0 → 1 → 0
                    Yオフセットdpx = (float)Math.Sin(Math.PI * drumContext.アニメカウンタ.現在値の割合) * 18f;     // 0 → 18 → 0
                }

                // ドラムパッド本体表示
                if (0 < drumContext.転送元矩形.Width && 0 < drumContext.転送元矩形.Height)
                {
                    this._パッド画像.描画する(
                        drumContext.左上位置dpx.X,
                        drumContext.左上位置dpx.Y + Yオフセットdpx,
                        不透明度0to1: 1.0f,
                        転送元矩形: drumContext.転送元矩形);
                }

                // ドラムフラッシュ表示
                if (0 < drumContext.転送元矩形Flush.Width && 0 < drumContext.転送元矩形Flush.Height && 0f < フラッシュ画像の不透明度)
                {
                    this._パッド画像.描画する(
                        drumContext.左上位置dpx.X,
                        drumContext.左上位置dpx.Y + Yオフセットdpx,
                        フラッシュ画像の不透明度,
                        転送元矩形: drumContext.転送元矩形Flush);
                }
            }
        }



        // private


        private readonly 画像 _パッド画像;

        private readonly 矩形リスト _パッド絵の矩形リスト;

        private struct パッドContext
        {
            public Vector2 左上位置dpx;
            public RectangleF 転送元矩形;
            public RectangleF 転送元矩形Flush;
            public Counter アニメカウンタ;
        };
        private readonly Dictionary<表示レーン種別, パッドContext> _レーンtoパッドContext;

    }
}