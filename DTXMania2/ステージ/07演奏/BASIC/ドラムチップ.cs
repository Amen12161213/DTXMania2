using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SSTFormat.v004;

namespace DTXMania2.演奏.BASIC
{
    class ドラムチップ : IDisposable
    {

        // 生成と終了


        public ドラムチップ()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._ドラムチップ画像 = new 画像(@"$(Images)\PlayStage\BASIC\DrumChip.png");
            this._ドラムチップの矩形リスト = new 矩形リスト(@"$(Images)\PlayStage\BASIC\LaneType\" + Global.App.ログオン中のユーザ.レーン配置 + @"\DrumChip.yaml");
            this._ドラムチップアニメ = new LoopCounter(0, 200, 3);
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._ドラムチップ画像?.Dispose();
        }



        // 進行と描画


        /// <returns>クリアしたらtrueを返す。</returns>
        public bool 進行描画する(ref int 描画開始チップ番号, チップの演奏状態 state, チップ chip, int index, double ヒット判定バーとの距離dpx)
        {
            float たて中央位置dpx = (float)(演奏ステージ.ヒット判定位置Ydpx + ヒット判定バーとの距離dpx);
            float 消滅割合 = 0f;

            #region " 消滅割合を算出; チップがヒット判定バーを通過したら徐々に消滅する。"
            //----------------
            const float 消滅を開始するヒット判定バーからの距離dpx = 20f;
            const float 消滅開始から完全消滅するまでの距離dpx = 70f;

            if (消滅を開始するヒット判定バーからの距離dpx < ヒット判定バーとの距離dpx)   // 通過した
            {
                // 通過距離に応じて 0→1の消滅割合を付与する。0で完全表示、1で完全消滅、通過してなければ 0。
                消滅割合 = Math.Min(1f, (float)((ヒット判定バーとの距離dpx - 消滅を開始するヒット判定バーからの距離dpx) / 消滅開始から完全消滅するまでの距離dpx));
            }
            //----------------
            #endregion

            #region " チップが描画開始チップであり、かつ、そのY座標が画面下端を超えたなら、描画開始チップ番号を更新する。"
            //----------------
            if ((index == 描画開始チップ番号) &&
                (Global.設計画面サイズ.Height + 40.0 < たて中央位置dpx))   // +40 はチップが隠れるであろう適当なマージン。
            {
                描画開始チップ番号++;

                // 描画開始チップがチップリストの末尾に到達したら、演奏を終了する。
                if (Global.App.演奏スコア.チップリスト.Count <= 描画開始チップ番号)
                {
                    描画開始チップ番号 = -1;    // 演奏完了。
                    return true;                // クリア。
                }

                return false;
            }
            //----------------
            #endregion

            if (state.不可視)
                return false;

            // BASICモードでは、チップの大きさは変化しない。ADVANCEDは変化する。
            float 大きさ0to1 = 1.0f;
            var userConfig = Global.App.ログオン中のユーザ;

            #region " 音量からチップの大きさを計算する。"
            //----------------
            if (userConfig.演奏モード > PlayMode.BASIC && userConfig.音量に応じてチップサイズを変更する)
            {
                if (chip.チップ種別 != チップ種別.Snare_Ghost)   // Ghost は対象外
                {
                    // 191130 OrzHighlight チップのY幅を音量に応じて3/9、4/9、…、8/9、9/9、1.2にします
                    大きさ0to1 = chip.音量 <= チップ.既定音量 ? ((chip.音量 + 2) / (float)(チップ.既定音量 + 2)) : 1.2f;
                    //大きさ0to1 = chip.音量 <= チップ.既定音量 ? Math.Max((chip.音量 + 1) / (float)(チップ.既定音量 + 1), 0.375f) : 1.25f;
                }
            }
            //----------------
            #endregion

            // チップ種別 から、表示レーン種別 と 表示チップ種別 を取得。
            var 表示レーン種別 = userConfig.ドラムチッププロパティリスト[chip.チップ種別].表示レーン種別;
            var 表示チップ種別 = userConfig.ドラムチッププロパティリスト[chip.チップ種別].表示チップ種別;

            if ((表示レーン種別 != 表示レーン種別.Unknown) &&   // Unknwon ならチップを表示しない。
                (表示チップ種別 != 表示チップ種別.Unknown))     //
            {
                var 左端位置dpx = レーンフレーム.領域.Left + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[表示レーン種別];
                var 中央位置Xdpx = 左端位置dpx + レーンフレーム.現在のレーン配置.表示レーンの幅dpx[表示レーン種別] / 2f;

                #region " チップ背景（あれば）を描画する。"
                //----------------
                {
                    var 矩形 = this._ドラムチップの矩形リスト[表示チップ種別.ToString() + "_back"]!.Value;

                    if ((null != 矩形) && (0 < 矩形.Width && 0 < 矩形.Height))
                    {
                        var 矩形中央 = new Vector2(矩形.Width / 2f, 矩形.Height / 2f);
                        var アニメ割合 = this._ドラムチップアニメ.現在値の割合;   // 0→1のループ

                        var 変換行列 = (0 >= 消滅割合) ? Matrix.Identity : Matrix.Scaling(1f - 消滅割合, 1f, 0f);

                        // 変換(1) 拡大縮小、回転
                        // → 現在は、どの表示チップ種別の背景がどのアニメーションを行うかは、コード内で名指しする（固定）。
                        switch (表示チップ種別)
                        {
                            case 表示チップ種別.LeftCymbal:
                            case 表示チップ種別.RightCymbal:
                            case 表示チップ種別.HiHat:
                            case 表示チップ種別.HiHat_Open:
                            case 表示チップ種別.HiHat_HalfOpen:
                            case 表示チップ種別.Foot:
                            case 表示チップ種別.Tom3:
                            case 表示チップ種別.Tom3_Rim:
                            case 表示チップ種別.LeftRide:
                            case 表示チップ種別.RightRide:
                            case 表示チップ種別.LeftRide_Cup:
                            case 表示チップ種別.RightRide_Cup:
                            case 表示チップ種別.LeftChina:
                            case 表示チップ種別.RightChina:
                            case 表示チップ種別.LeftSplash:
                            case 表示チップ種別.RightSplash:
                                #region " 縦横に伸び縮み "
                                //----------------
                                {
                                    float v = (float)(Math.Sin(2 * Math.PI * アニメ割合) * 0.2);    // -0.2～0.2 の振動

                                    変換行列 = 変換行列 * Matrix.Scaling((float)(1 + v), (float)(1 - v) * 1.0f, 0f);       // チップ背景は大きさを変えない
                                }
                                //----------------
                                #endregion
                                break;

                            case 表示チップ種別.Bass:
                            case 表示チップ種別.LeftBass:
                                #region " 左右にゆらゆら回転 "
                                //----------------
                                {
                                    float r = (float)(Math.Sin(2 * Math.PI * アニメ割合) * 0.2);    // -0.2～0.2 の振動
                                    変換行列 = 変換行列 *
                                        Matrix.Scaling(1f, 1f, 0f) * // チップ背景は大きさを変えない
                                        Matrix.RotationZ((float)(r * Math.PI));
                                }
                                //----------------
                                #endregion
                                break;
                        }

                        // 変換(2) 移動
                        変換行列 = 変換行列 *
                            //Matrix.Scaling(1f, 大きさ0to1, 0f) *
                            Matrix.Translation(
                                Global.画面左上dpx.X + 中央位置Xdpx,
                                Global.画面左上dpx.Y - たて中央位置dpx,
                                0f);

                        // 描画。
                        //if( 表示チップ種別 != 表示チップ種別.HiHat &&         // 暫定処置：これらでは背景画像を表示しない 
                        //    表示チップ種別 != 表示チップ種別.LeftRide &&      //
                        //    表示チップ種別 != 表示チップ種別.RightRide &&     //
                        //    表示チップ種別 != 表示チップ種別.LeftRide_Cup &&  // 
                        //    表示チップ種別 != 表示チップ種別.RightRide_Cup )
                        {
                            this._ドラムチップ画像.描画する(
                                変換行列,
                                転送元矩形: 矩形);
                        }
                    }
                }
                //----------------
                #endregion

                #region " チップ本体を描画する。"
                //----------------
                {
                    var 矩形 = this._ドラムチップの矩形リスト[表示チップ種別.ToString()]!.Value;

                    if ((null != 矩形) && ((0 < 矩形.Width && 0 < 矩形.Height)))
                    {
                        var sx = (0 >= 消滅割合) ? 1f : 1f - 消滅割合;
                        var sy = 大きさ0to1;

                        var 変換行列 =
                            Matrix.Scaling(sx, sy, 0f) *
                            Matrix.Translation(
                                Global.画面左上dpx.X + 中央位置Xdpx,
                                Global.画面左上dpx.Y - たて中央位置dpx,
                                0f);

                        this._ドラムチップ画像.描画する(
                            変換行列,
                            転送元矩形: 矩形);
                    }
                }
                //----------------
                #endregion
            }

            return false;
        }



        // private


        private readonly 画像 _ドラムチップ画像;

        private readonly 矩形リスト _ドラムチップの矩形リスト;

        private readonly LoopCounter _ドラムチップアニメ;
    }
}