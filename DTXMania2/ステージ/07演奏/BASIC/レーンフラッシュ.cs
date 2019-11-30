using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace DTXMania2.演奏.BASIC
{
    class レーンフラッシュ : IDisposable
    {

        // 生成と終了


        public レーンフラッシュ()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._レーンフラッシュ画像 = new 画像(@"$(Images)\PlayStage\BASIC\LaneFlush.png");
            this._レーンフラッシュの矩形リスト = new 矩形リスト(@"$(Images)\PlayStage\BASIC\LaneType\" + Global.App.ログオン中のユーザ.レーン配置 + @"\LaneFlush.yaml");

            this._レーンtoレーンContext = new Dictionary<表示レーン種別, レーンContext>();

            foreach (表示レーン種別 lane in Enum.GetValues(typeof(表示レーン種別)))
            {
                this._レーンtoレーンContext.Add(lane, new レーンContext()
                {
                    開始位置dpx = new Vector2(
                        x: レーンフレーム.領域.X + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[lane],
                        y: レーンフレーム.領域.Bottom),
                    転送元矩形 = this._レーンフラッシュの矩形リスト[lane.ToString()],
                    アニメカウンタ = new Counter(),
                });
            }
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._レーンtoレーンContext.Clear();
            this._レーンフラッシュ画像.Dispose();
        }



        // フラッシュ開始


        public void 開始する(表示レーン種別 lane)
        {
            this._レーンtoレーンContext[lane].アニメカウンタ.開始する(0, 250, 1);
        }



        // 進行と描画


        public void 進行描画する()
        {
            foreach (表示レーン種別 lane in Enum.GetValues(typeof(表示レーン種別)))
            {
                var laneContext = this._レーンtoレーンContext[lane];
                if (laneContext.アニメカウンタ.動作中である && laneContext.アニメカウンタ.終了値に達していない)
                {
                    this._レーンフラッシュ画像.描画する(
                        laneContext.開始位置dpx.X,
                        laneContext.開始位置dpx.Y - laneContext.アニメカウンタ.現在値の割合 * BASIC.レーンフレーム.領域.Height,
                        不透明度0to1: 1f - laneContext.アニメカウンタ.現在値の割合,
                        転送元矩形: laneContext.転送元矩形);
                }
            }
        }



        // private


        private struct レーンContext
        {
            public Vector2 開始位置dpx;
            public RectangleF? 転送元矩形;
            public Counter アニメカウンタ;
        };

        private Dictionary<表示レーン種別, レーンContext> _レーンtoレーンContext = null!;

        private readonly 画像 _レーンフラッシュ画像;

        private readonly 矩形リスト _レーンフラッシュの矩形リスト;

        //private class YAMLマップ
        //{
        //    public Dictionary<string, float[]> 矩形リスト { get; set; } = null!;
        //}
    }
}