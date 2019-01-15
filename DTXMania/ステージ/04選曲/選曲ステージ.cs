﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;
using SSTFormat.v4;
using DTXMania.アイキャッチ;
using DTXMania.曲;

namespace DTXMania.ステージ.選曲
{
    class 選曲ステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            フェードアウト,
            確定_選曲,
            確定_設定,
            キャンセル,
        }
        public フェーズ 現在のフェーズ { get; protected set; }


        public 選曲ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._舞台画像 = new 舞台画像( @"$(System)images\舞台_暗.jpg" ) );
                this.子Activityを追加する( this._曲リスト = new 曲リスト() );
                this.子Activityを追加する( this._難易度と成績 = new 難易度と成績() );
                this.子Activityを追加する( this._曲ステータスパネル = new 曲ステータスパネル() );
                this.子Activityを追加する( this._ステージタイマー = new 画像( @"$(System)images\選曲\ステージタイマー.png" ) );
                this.子Activityを追加する( this._青い線 = new 青い線() );
                this.子Activityを追加する( this._選択曲枠ランナー = new 選択曲枠ランナー() );
                this.子Activityを追加する( this._BPMパネル = new BPMパネル() );
                this.子Activityを追加する( this._曲別SKILL = new 曲別SKILL() );
                this.子Activityを追加する( this._表示方法選択パネル = new 表示方法選択パネル() );
				this.子Activityを追加する( this._SongNotFound = new 文字列画像() {
					表示文字列 =
					"Song not found...\n" +
					"Hit BDx2 (in default SPACEx2) to select song folders."
                } );

                // 外部接続。
                this._難易度と成績.青い線を取得する = () => this._青い線;
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var dc = グラフィックデバイス.Instance.D2DDeviceContext;

                this._白 = new SolidColorBrush( dc, Color4.White );
                this._黒 = new SolidColorBrush( dc, Color4.Black );
                this._黒透過 = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) );
                this._灰透過 = new SolidColorBrush( dc, new Color4( 0x80535353 ) );

                this._上に伸びる導線の長さdpx = null;
                this._左に伸びる導線の長さdpx = null;
                this._プレビュー枠の長さdpx = null;
                this._導線のストーリーボード = null;

                App.システムサウンド.再生する( 設定.システムサウンド種別.選曲ステージ_開始音 );

                this.現在のフェーズ = フェーズ.フェードイン;
                this._初めての進行描画 = true;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._上に伸びる導線の長さdpx?.Dispose();
                this._上に伸びる導線の長さdpx = null;

                this._左に伸びる導線の長さdpx?.Dispose();
                this._左に伸びる導線の長さdpx = null;

                this._プレビュー枠の長さdpx?.Dispose();
                this._プレビュー枠の長さdpx = null;

                this._導線のストーリーボード?.Dispose();
                this._導線のストーリーボード = null;

                this._白?.Dispose();
                this._白 = null;

                this._黒?.Dispose();
                this._黒 = null;

                this._黒透過?.Dispose();
                this._黒透過 = null;

                this._灰透過?.Dispose();
                this._灰透過 = null;
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            if( this._初めての進行描画 )
            {
                App.ステージ管理.現在のアイキャッチ.オープンする();
                this._導線アニメをリセットする();
                this._初めての進行描画 = false;
            }

            
            // 進行描画

            if( null != App.曲ツリー.フォーカスノード )
            {
                // (A) 曲がある場合

                this._舞台画像.進行描画する( dc );
                this._曲リスト.進行描画する( dc );
                this._その他パネルを描画する( dc );
                this._表示方法選択パネル.進行描画する( dc );
                this._難易度と成績.描画する( dc, App.曲ツリー.フォーカス難易度 );
                this._曲ステータスパネル.描画する( dc );
                this._プレビュー画像を描画する( dc, App.曲ツリー.フォーカスノード );
                this._BPMパネル.描画する( dc );
                this._曲別SKILL.進行描画する( dc );
                this._選択曲を囲む枠を描画する( dc );
                this._選択曲枠ランナー.進行描画する( dc );
                this._導線を描画する( dc );
                this._ステージタイマー.描画する( dc, 1689f, 37f );
            }
            else
            {
                // (B) 曲が１つもない場合

                this._舞台画像.進行描画する( dc );
                this._表示方法選択パネル.進行描画する( dc );
                this._ステージタイマー.描画する( dc, 1689f, 37f );
                this._SongNotFound.描画する( dc, 1150f, 400f );
            }

        
            // 入力

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.オープン完了 )
                    {
                        this.現在のフェーズ = フェーズ.表示;
                    }
                    break;

                case フェーズ.表示:
                    if( App.入力管理.確定キーが入力された() )
                    {
                        #region " 確定 "
                        //----------------
                        if( App.曲ツリー.フォーカスノード is BoxNode boxNode )
                        {
                            App.システムサウンド.再生する( 設定.システムサウンド種別.決定音 );
                            this._曲リスト.BOXに入る();
                        }
                        else if( App.曲ツリー.フォーカスノード is BackNode backNode )
                        {
                            App.システムサウンド.再生する( 設定.システムサウンド種別.決定音 );
                            this._曲リスト.BOXから出る();
                        }
                        else if( null != App.曲ツリー.フォーカスノード )
                        {
                            // 選曲する

                            App.曲ツリー.フォーカスノード?.プレビュー音声を停止する();
                            App.システムサウンド.再生する( 設定.システムサウンド種別.選曲ステージ_曲決定音 );

                            App.ステージ管理.アイキャッチを選択しクローズする( nameof( GO ) );
                            this.現在のフェーズ = フェーズ.フェードアウト;
                        }
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.キャンセルキーが入力された() )
                    {
                        #region " キャンセル "
                        //----------------
                        {
                            App.曲ツリー.フォーカスノード?.プレビュー音声を停止する();
                            App.システムサウンド.再生する( 設定.システムサウンド種別.取消音 );

                            if( App.曲ツリー.フォーカスノード.親ノード != App.曲ツリー.ルートノード )
                            {
                                this._曲リスト.BOXから出る();
                            }
                            else
                            {
                                this.現在のフェーズ = フェーズ.キャンセル;
                            }
                        }
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.上移動キーが入力された() )
                    {
                        #region " 上移動 "
                        //----------------
                        if( null != App.曲ツリー.フォーカスノード )
                        {
                            App.システムサウンド.再生する( 設定.システムサウンド種別.カーソル移動音 );

                            //App.曲ツリー.前のノードをフォーカスする();	--> 曲リストへ委譲
                            this._曲リスト.前のノードを選択する();
                            this._導線アニメをリセットする();
                        }
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.下移動キーが入力された() )
                    {
                        #region " 下移動 "
                        //----------------
                        if( null != App.曲ツリー.フォーカスノード )
                        {
                            App.システムサウンド.再生する( 設定.システムサウンド種別.カーソル移動音 );

                            //App.曲ツリー.次のノードをフォーカスする();	--> 曲リストへ委譲
                            this._曲リスト.次のノードを選択する();
                            this._導線アニメをリセットする();
                        }
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.左移動キーが入力された() )
                    {
                        #region " 左移動 "
                        //----------------
                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._表示方法選択パネル.前のパネルを選択する();
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.右移動キーが入力された() )
                    {
                        #region " 右移動 "
                        //----------------
                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._表示方法選択パネル.次のパネルを選択する();
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.シーケンスが入力された( new[] { レーン種別.HiHat, レーン種別.HiHat }, App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理 ) )
                    {
                        #region " HH×2 → 難易度変更 "
                        //----------------
                        App.曲ツリー.フォーカスノード?.プレビュー音声を停止する();

                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._曲リスト.難易度アンカをひとつ増やす();

                        App.曲ツリー.フォーカスノード?.プレビュー音声を再生する();
                        //----------------
                        #endregion
                    }
                    else if( App.入力管理.シーケンスが入力された( new[] { レーン種別.Bass, レーン種別.Bass }, App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理 ) )
                    {
                        #region " BD×2 → オプション設定 "
                        //----------------
                        App.曲ツリー.フォーカスノード?.プレビュー音声を停止する();
                        App.システムサウンド.再生する( 設定.システムサウンド種別.決定音 );
                        this.現在のフェーズ = フェーズ.確定_設定;
                        //----------------
                        #endregion
                    }
                    break;

                case フェーズ.フェードアウト:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                    {
                        this.現在のフェーズ = フェーズ.確定_選曲;
                    }
                    break;

                case フェーズ.確定_選曲:
                case フェーズ.キャンセル:
                    break;
            }
        }


        private bool _初めての進行描画 = true;
        private 舞台画像 _舞台画像 = null;
        private 曲リスト _曲リスト = null;
        private 難易度と成績 _難易度と成績 = null;
        private 曲ステータスパネル _曲ステータスパネル = null;
        private 青い線 _青い線 = null;
        private 選択曲枠ランナー _選択曲枠ランナー = null;
        private BPMパネル _BPMパネル = null;
        private 曲別SKILL _曲別SKILL = null;
        private 表示方法選択パネル _表示方法選択パネル = null;
        private 文字列画像 _SongNotFound = null;
        private SolidColorBrush _白 = null;
        private SolidColorBrush _黒 = null;
        private SolidColorBrush _黒透過 = null;
        private SolidColorBrush _灰透過 = null;
        private 画像 _ステージタイマー = null;
        private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 471f, 61f, 0f );
        private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 444f, 444f, 0f );

        private void _その他パネルを描画する( DeviceContext1 dc )
        {
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                using( var ソートタブ上色 = new SolidColorBrush( dc, new Color4( 0xFF121212 ) ) )
                using( var ソートタブ下色 = new SolidColorBrush( dc, new Color4( 0xFF1f1f1f ) ) )
                {
                    // 曲リストソートタブ
                    dc.FillRectangle( new RectangleF( 927f, 50f, 993f, 138f ), ソートタブ上色 );
                    dc.FillRectangle( new RectangleF( 927f, 142f, 993f, 46f ), ソートタブ下色 );
                }

                // インフォメーションバー
                dc.FillRectangle( new RectangleF( 0f, 0f, 1920f, 50f ), this._黒 );
                dc.DrawLine( new Vector2( 0f, 50f ), new Vector2( 1920f, 50f ), this._白, strokeWidth: 1f );

                // ボトムバー
                dc.FillRectangle( new RectangleF( 0f, 1080f - 43f, 1920f, 1080f ), this._黒 );

                // プレビュー領域
                dc.FillRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._黒透過 );
                dc.DrawRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._灰透過, strokeWidth: 1f );
                dc.DrawLine( new Vector2( 1f, 442f ), new Vector2( 925f, 442f ), this._灰透過, strokeWidth: 1f );

            } );
        }
        private void _プレビュー画像を描画する( DeviceContext1 dc, Node ノード )
        {
            var 画像 = ノード.ノード画像 ?? Node.既定のノード画像;

            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 変換行列 =
                Matrix.Scaling(
                    this._プレビュー画像表示サイズdpx.X / 画像.サイズ.Width,
                    this._プレビュー画像表示サイズdpx.Y / 画像.サイズ.Height, 
                    0f ) *
                Matrix.Translation(
                    グラフィックデバイス.Instance.画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
                    グラフィックデバイス.Instance.画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
                    0f );

            画像.描画する( 変換行列 );
        }
        private void _選択曲を囲む枠を描画する( DeviceContext1 dc )
        {
            var 矩形 = new RectangleF( 1015f, 485f, 905f, 113f );

            this._青い線.描画する( dc, new Vector2( 矩形.Left - this._青枠のマージンdpx, 矩形.Top ), 幅dpx: 矩形.Width + this._青枠のマージンdpx * 2f );
            this._青い線.描画する( dc, new Vector2( 矩形.Left - this._青枠のマージンdpx, 矩形.Bottom ), 幅dpx: 矩形.Width + this._青枠のマージンdpx * 2f );
            this._青い線.描画する( dc, new Vector2( 矩形.Left, 矩形.Top - this._青枠のマージンdpx ), 高さdpx: 矩形.Height + this._青枠のマージンdpx * 2f );
        }

        private Variable _上に伸びる導線の長さdpx = null;
        private Variable _左に伸びる導線の長さdpx = null;
        private Variable _プレビュー枠の長さdpx = null;
        private Storyboard _導線のストーリーボード = null;
        private readonly float _青枠のマージンdpx = 8f;

        private void _導線アニメをリセットする()
        {
            var animation = グラフィックデバイス.Instance.Animation;

            this._選択曲枠ランナー.リセットする();

            this._上に伸びる導線の長さdpx?.Dispose();
            this._上に伸びる導線の長さdpx = new Variable( animation.Manager, initialValue: 0.0 );

            this._左に伸びる導線の長さdpx?.Dispose();
            this._左に伸びる導線の長さdpx = new Variable( animation.Manager, initialValue: 0.0 );

            this._プレビュー枠の長さdpx?.Dispose();
            this._プレビュー枠の長さdpx = new Variable( animation.Manager, initialValue: 0.0 );

            this._導線のストーリーボード?.Abandon();
            this._導線のストーリーボード?.Dispose();
            this._導線のストーリーボード = new Storyboard( animation.Manager );

            double 期間 = 0.3;
            using( var 上に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 左に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 枠が広がる = animation.TrasitionLibrary.Constant( 期間 ) )
            {
                this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
                this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
                this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
            }

            期間 = 0.07;
            using( var 上に伸びる = animation.TrasitionLibrary.Linear( 期間, finalValue: 209.0 ) )
            using( var 左に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 枠が広がる = animation.TrasitionLibrary.Constant( 期間 ) )
            {
                this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
                this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
                this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
            }

            期間 = 0.06;
            using( var 上に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 左に伸びる = animation.TrasitionLibrary.Linear( 期間, finalValue: 129.0 ) )
            using( var 枠が広がる = animation.TrasitionLibrary.Constant( 期間 ) )
            {
                this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
                this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
                this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
            }

            期間 = 0.07;
            using( var 維持 = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 上に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 左に伸びる = animation.TrasitionLibrary.Constant( 期間 ) )
            using( var 枠が広がる = animation.TrasitionLibrary.Linear( 期間, finalValue: 444.0 + this._青枠のマージンdpx * 2f ) )
            {
                this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
                this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
                this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
            }

            this._導線のストーリーボード.Schedule( animation.Timer.Time );
        }
        private void _導線を描画する( DeviceContext1 dc )
        {
            var h = (float) this._上に伸びる導線の長さdpx.Value;
            this._青い線.描画する( dc, new Vector2( 1044f, 485f - h ), 高さdpx: h );

            var w = (float) this._左に伸びる導線の長さdpx.Value;
            this._青い線.描画する( dc, new Vector2( 1046f - w, 278f ), 幅dpx: w );

            var z = (float) this._プレビュー枠の長さdpx.Value;   // マージン×2 込み
            var 上 = this._プレビュー画像表示位置dpx.Y;
            var 下 = this._プレビュー画像表示位置dpx.Y + this._プレビュー画像表示サイズdpx.Y;
            var 左 = this._プレビュー画像表示位置dpx.X;
            var 右 = this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X;
            this._青い線.描画する( dc, new Vector2( 右 + this._青枠のマージンdpx - z, 上 ), 幅dpx: z ); // 上辺
            this._青い線.描画する( dc, new Vector2( 右 + this._青枠のマージンdpx - z, 下 ), 幅dpx: z ); // 下辺
            this._青い線.描画する( dc, new Vector2( 左, 下 + this._青枠のマージンdpx - z ), 高さdpx: z ); // 左辺
            this._青い線.描画する( dc, new Vector2( 右, 下 + this._青枠のマージンdpx - z ), 高さdpx: z ); // 右辺
        }
    }
}
