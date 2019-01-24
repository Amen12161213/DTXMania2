﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.演奏
{
    class 判定文字列 : Activity
    {
        public 判定文字列()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._判定文字列画像 = new テクスチャ( @"$(System)images\演奏\判定文字列.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\判定文字列.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._判定文字列の矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._判定文字列の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._レーンtoステータス = new Dictionary<表示レーン種別, 表示レーンステータス>() {
                    { 表示レーン種別.Unknown,      new 表示レーンステータス( 表示レーン種別.Unknown ) },
                    { 表示レーン種別.LeftCymbal,   new 表示レーンステータス( 表示レーン種別.LeftCymbal ) },
                    { 表示レーン種別.HiHat,        new 表示レーンステータス( 表示レーン種別.HiHat ) },
                    { 表示レーン種別.Foot,         new 表示レーンステータス( 表示レーン種別.Foot ) },
                    { 表示レーン種別.Snare,        new 表示レーンステータス( 表示レーン種別.Snare ) },
                    { 表示レーン種別.Bass,         new 表示レーンステータス( 表示レーン種別.Bass ) },
                    { 表示レーン種別.Tom1,         new 表示レーンステータス( 表示レーン種別.Tom1 ) },
                    { 表示レーン種別.Tom2,         new 表示レーンステータス( 表示レーン種別.Tom2 ) },
                    { 表示レーン種別.Tom3,         new 表示レーンステータス( 表示レーン種別.Tom3 ) },
                    { 表示レーン種別.RightCymbal,  new 表示レーンステータス( 表示レーン種別.RightCymbal ) },
                };
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var kvp in this._レーンtoステータス )
                    kvp.Value.Dispose();

                this._レーンtoステータス = null;
            }
        }

        public void 表示を開始する( 表示レーン種別 lane, 判定種別 judge )
        {
            var status = this._レーンtoステータス[ lane ];

            status.判定種別 = judge;
            status.現在の状態 = 表示レーンステータス.状態.表示開始;  // 描画スレッドへ通知。
        }

        public void 進行描画する()
        {
            foreach( 表示レーン種別 レーン in Enum.GetValues( typeof( 表示レーン種別 ) ) )
            {
                var status = this._レーンtoステータス[ レーン ];

                switch( status.現在の状態 )
                {
                    case 表示レーンステータス.状態.表示開始:
                        #region " 開始処理 "
                        //----------------
                        {
                            status.アニメ用メンバを解放する();

                            var animation = グラフィックデバイス.Instance.Animation;

                            #region " (1) 光 アニメーションを構築 "
                            //----------------
                            if( status.判定種別 == 判定種別.PERFECT )   // 今のところ、光はPERFECT時のみ表示。
                            {
                                // 初期状態
                                status.光の回転角 = new Variable( animation.Manager, initialValue: 0 );
                                status.光のX方向拡大率 = new Variable( animation.Manager, initialValue: 1.2 );
                                status.光のY方向拡大率 = new Variable( animation.Manager, initialValue: 0.25 );
                                status.光のストーリーボード = new Storyboard( animation.Manager );

                                double 期間sec;

                                // シーン1. 小さい状態からすばやく展開
                                期間sec = 0.03;
                                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: -100.0 ) )       // [degree]
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 1.0 ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 1.0 ) )
                                {
                                    status.光のストーリーボード.AddTransition( status.光の回転角, 回転角の遷移 );
                                    status.光のストーリーボード.AddTransition( status.光のX方向拡大率, X方向拡大率の遷移 );
                                    status.光のストーリーボード.AddTransition( status.光のY方向拡大率, Y方向拡大率の遷移 );
                                }

                                // シーン2. 大きい状態でゆっくり消える
                                期間sec = 0.29;
                                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: -140.0 ) )       // [degree]
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 0.0 ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                {
                                    status.光のストーリーボード.AddTransition( status.光の回転角, 回転角の遷移 );
                                    status.光のストーリーボード.AddTransition( status.光のX方向拡大率, X方向拡大率の遷移 );
                                    status.光のストーリーボード.AddTransition( status.光のY方向拡大率, Y方向拡大率の遷移 );
                                }

                                // 開始
                                status.光のストーリーボード.Schedule( animation.Timer.Time );
                            }
                            //----------------
                            #endregion

                            #region " (2) 判定文字（影）アニメーションを構築 "
                            //----------------
                            {
                                // 初期状態
                                status.文字列影の相対Y位置dpx = new Variable( animation.Manager, initialValue: +40.0 );
                                status.文字列影の不透明度 = new Variable( animation.Manager, initialValue: 0.0 );
                                status.文字列影のストーリーボード = new Storyboard( animation.Manager );

                                double 期間sec;

                                // シーン1. 完全透明のまま下から上に移動。
                                期間sec = 0.05;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: -5.0 ) )
                                using( var 透明度の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                {
                                    status.文字列影のストーリーボード.AddTransition( status.文字列影の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列影のストーリーボード.AddTransition( status.文字列影の不透明度, 透明度の遷移 );
                                }

                                // シーン2. 透明になりつつ上に消える
                                期間sec = 0.15;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: -10.0 ) )
                                using( var 透明度の遷移1 = animation.TrasitionLibrary.Linear( duration: 0.0, finalValue: 0.5 ) )
                                using( var 透明度の遷移2 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 0.0 ) )
                                {
                                    status.文字列影のストーリーボード.AddTransition( status.文字列影の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列影のストーリーボード.AddTransition( status.文字列影の不透明度, 透明度の遷移1 );
                                    status.文字列影のストーリーボード.AddTransition( status.文字列影の不透明度, 透明度の遷移2 );
                                }

                                // 開始
                                status.文字列影のストーリーボード.Schedule( animation.Timer.Time );
                            }
                            //----------------
                            #endregion

                            #region " (3) 判定文字（本体）アニメーションを構築 "
                            //----------------
                            {
                                // 初期状態
                                status.文字列本体の相対Y位置dpx = new Variable( animation.Manager, initialValue: +40.0 );
                                status.文字列本体のX方向拡大率 = new Variable( animation.Manager, initialValue: 1.0 );
                                status.文字列本体のY方向拡大率 = new Variable( animation.Manager, initialValue: 1.0 );
                                status.文字列本体の不透明度 = new Variable( animation.Manager, initialValue: 0.0 );
                                status.文字列本体のストーリーボード = new Storyboard( animation.Manager );

                                double 期間sec;

                                // シーン1. 透明から不透明になりつつ下から上に移動。
                                期間sec = 0.05;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: -5.0 ) )
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var 不透明度の遷移 = animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間sec, finalValue: 1.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
                                {
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のX方向拡大率, X方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のY方向拡大率, Y方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の不透明度, 不透明度の遷移 );
                                }

                                // シーン2. ちょっと下に跳ね返る
                                期間sec = 0.05;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: +5.0 ) )
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var 不透明度の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                {
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のX方向拡大率, X方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のY方向拡大率, Y方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の不透明度, 不透明度の遷移 );
                                }

                                // シーン3. また上に戻る
                                期間sec = 0.05;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: +0.0 ) )
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var 不透明度の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                {
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のX方向拡大率, X方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のY方向拡大率, Y方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の不透明度, 不透明度の遷移 );
                                }

                                // シーン4. 静止
                                期間sec = 0.15;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var 不透明度の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                {
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のX方向拡大率, X方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のY方向拡大率, Y方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の不透明度, 不透明度の遷移 );
                                }

                                // シーン5. 横に広がり縦につぶれつつ消える
                                期間sec = 0.05;
                                using( var 相対Y位置の遷移 = animation.TrasitionLibrary.Constant( duration: 期間sec ) )
                                using( var X方向拡大率の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 2.0 ) )
                                using( var Y方向拡大率の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 0.0 ) )
                                using( var 不透明度の遷移 = animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 0.0 ) )
                                {
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の相対Y位置dpx, 相対Y位置の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のX方向拡大率, X方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体のY方向拡大率, Y方向拡大率の遷移 );
                                    status.文字列本体のストーリーボード.AddTransition( status.文字列本体の不透明度, 不透明度の遷移 );
                                }

                                // 開始
                                status.文字列本体のストーリーボード.Schedule( animation.Timer.Time );
                            }
                            //----------------
                            #endregion

                            status.現在の状態 = 表示レーンステータス.状態.表示中;
                        }
                        //----------------
                        #endregion
                        break;

                    case 表示レーンステータス.状態.表示中:
                        #region " 開始完了、表示中 "
                        //----------------
                        {
                            #region " (1) 光 の進行描画 "
                            //----------------
                            if( null != status.光のストーリーボード )
                            {
                                var 転送元矩形 = this._判定文字列の矩形リスト[ "PERFECT光" ];

                                var sx = (float) status.光のX方向拡大率.Value;
                                var sy = (float) status.光のY方向拡大率.Value;

                                var 変換行列 =
                                    Matrix.Scaling( sx, sy, 0f ) *
                                    Matrix.RotationZ(
                                        MathUtil.DegreesToRadians( (float) status.光の回転角.Value ) ) *
                                    Matrix.Translation(
                                        グラフィックデバイス.Instance.画面左上dpx.X + ( status.表示中央位置dpx.X ),
                                        グラフィックデバイス.Instance.画面左上dpx.Y - ( status.表示中央位置dpx.Y ),
                                        0f );

                                this._判定文字列画像.描画する( 変換行列, 転送元矩形: 転送元矩形 );
                            }
                            //----------------
                            #endregion

                            #region " (2) 判定文字列（影）の進行描画"
                            //----------------
                            if( null != status.文字列影のストーリーボード )
                            {
                                var 転送元矩形 = this._判定文字列の矩形リスト[ status.判定種別.ToString() ];

                                this._判定文字列画像.描画する(
                                    status.表示中央位置dpx.X - 転送元矩形.Width / 2f,
                                    status.表示中央位置dpx.Y - 転送元矩形.Height / 2f + (float) status.文字列影の相対Y位置dpx.Value,
                                    (float) status.文字列影の不透明度.Value,
                                    転送元矩形: 転送元矩形 );
                            }
                            //----------------
                            #endregion

                            #region " (3) 判定文字列（本体）の進行描画 "
                            //----------------
                            if( null != status.文字列本体のストーリーボード )
                            {
                                var 転送元矩形 = this._判定文字列の矩形リスト[ status.判定種別.ToString() ];

                                var sx = (float) status.文字列本体のX方向拡大率.Value;
                                var sy = (float) status.文字列本体のY方向拡大率.Value;

                                this._判定文字列画像.描画する(
                                    status.表示中央位置dpx.X - sx * 転送元矩形.Width / 2f,
                                    status.表示中央位置dpx.Y - sy * 転送元矩形.Height / 2f + (float) status.文字列本体の相対Y位置dpx.Value,
                                    X方向拡大率: sx,
                                    Y方向拡大率: sy,
                                    不透明度0to1: (float) status.文字列本体の不透明度.Value,
                                    転送元矩形: 転送元矩形 );
                            }
                            //----------------
                            #endregion

                            // 全部終わったら非表示へ。
                            if( ( ( null == status.文字列影のストーリーボード ) || ( status.文字列影のストーリーボード.Status == StoryboardStatus.Ready ) ) &&
                                ( ( null == status.文字列本体のストーリーボード ) || ( status.文字列本体のストーリーボード.Status == StoryboardStatus.Ready ) ) &&
                                ( ( null == status.光のストーリーボード ) || ( status.光のストーリーボード.Status == StoryboardStatus.Ready ) ) )
                            {
                                status.現在の状態 = 表示レーンステータス.状態.非表示;
                            }
                        }
                        //----------------
                        #endregion
                        break;

                    default:
                        continue;   // 非表示
                }
            }
        }


        private テクスチャ _判定文字列画像 = null;

        private Dictionary<string, RectangleF> _判定文字列の矩形リスト = null;

        /// <summary>
        ///		以下の画像のアニメ＆表示管理を行うクラス。
        ///		・判定文字列（本体）
        ///		・判定文字列（影）
        ///		・光（今のところPERFECTのみ）
        /// </summary>
        private class 表示レーンステータス : IDisposable
        {
            public enum 状態
            {
                非表示,
                表示開始,   // 高速進行スレッドが設定
                表示中,        // 描画スレッドが設定
            }
            public 状態 現在の状態 = 状態.非表示;

            public 判定種別 判定種別 = 判定種別.PERFECT;

            public readonly Vector2 表示中央位置dpx;
            
            /// <summary>
            ///		判定文字列（本体）の表示されるY座標のオフセット。
            ///		表示中央位置dpx.Y からの相対値[dpx]。
            /// </summary>
            public Variable 文字列本体の相対Y位置dpx = null;
            
            /// <summary>
            ///		判定文字列（本体）の不透明度。
            ///		0 で完全透明、1 で完全不透明。
            /// </summary>
            public Variable 文字列本体の不透明度 = null;

            public Variable 文字列本体のX方向拡大率 = null;

            public Variable 文字列本体のY方向拡大率 = null;

            public Storyboard 文字列本体のストーリーボード = null;
            
            /// <summary>
            ///		判定文字列（影）の表示されるY座標のオフセット。
            ///		表示中央位置dpx.Y からの相対値[dpx]。
            /// </summary>
            public Variable 文字列影の相対Y位置dpx = null;

            /// <summary>
            ///		判定文字列（影）の不透明度。
            ///		0 で完全透明、1 で完全不透明。
            /// </summary>
            public Variable 文字列影の不透明度 = null;

            public Storyboard 文字列影のストーリーボード = null;

            /// <summary>
            ///		単位は度（degree）、時計回りを正とする。
            /// </summary>
            public Variable 光の回転角 = null;

            public Variable 光のX方向拡大率 = null;

            public Variable 光のY方向拡大率 = null;

            public Storyboard 光のストーリーボード = null;


            public 表示レーンステータス( 表示レーン種別 lane )
            {
                this.現在の状態 = 状態.非表示;

                // 表示中央位置は、レーンごとに固定。
                float x = BASIC.レーンフレーム.領域.Left + BASIC.レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[ lane ] + BASIC.レーンフレーム.現在のレーン配置.表示レーンの幅dpx[ lane ] / 2f;
                switch( lane )
                {
                    case 表示レーン種別.LeftCymbal: this.表示中央位置dpx = new Vector2( x, 530f ); break;
                    case 表示レーン種別.HiHat: this.表示中央位置dpx = new Vector2( x, 597f ); break;
                    case 表示レーン種別.Foot: this.表示中央位置dpx = new Vector2( x, 636f ); break;
                    case 表示レーン種別.Snare: this.表示中央位置dpx = new Vector2( x, 597f ); break;
                    case 表示レーン種別.Bass: this.表示中央位置dpx = new Vector2( x, 635f ); break;
                    case 表示レーン種別.Tom1: this.表示中央位置dpx = new Vector2( x, 561f ); break;
                    case 表示レーン種別.Tom2: this.表示中央位置dpx = new Vector2( x, 561f ); break;
                    case 表示レーン種別.Tom3: this.表示中央位置dpx = new Vector2( x, 600f ); break;
                    case 表示レーン種別.RightCymbal: this.表示中央位置dpx = new Vector2( x, 533f ); break;
                    default: this.表示中央位置dpx = new Vector2( x, -100f ); break;
                }
            }

            public void Dispose()
            {
                this.アニメ用メンバを解放する();
                this.現在の状態 = 状態.非表示;
            }

            public void アニメ用メンバを解放する()
            {
                this.文字列本体のストーリーボード?.Abandon();

                this.文字列本体のストーリーボード?.Dispose();
                this.文字列本体のストーリーボード = null;

                this.文字列本体の不透明度?.Dispose();
                this.文字列本体の不透明度 = null;

                this.文字列本体の相対Y位置dpx?.Dispose();
                this.文字列本体の相対Y位置dpx = null;

                this.文字列影のストーリーボード?.Abandon();

                this.文字列影のストーリーボード?.Dispose();
                this.文字列影のストーリーボード = null;

                this.文字列影の不透明度?.Dispose();
                this.文字列影の不透明度 = null;

                this.文字列影の相対Y位置dpx?.Dispose();
                this.文字列影の相対Y位置dpx = null;

                this.光のストーリーボード?.Abandon();

                this.光のストーリーボード?.Dispose();
                this.光のストーリーボード = null;

                this.光のY方向拡大率?.Dispose();
                this.光のY方向拡大率 = null;

                this.光のX方向拡大率?.Dispose();
                this.光のX方向拡大率 = null;

                this.光の回転角?.Dispose();
                this.光の回転角 = null;
            }
        }

        private Dictionary<表示レーン種別, 表示レーンステータス> _レーンtoステータス = null;


        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
