﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;

namespace DTXMania.選曲
{
    /// <summary>
    ///		曲のリスト表示、選択、スクロール。
    /// </summary>
    /// <remarks>
    ///     <see cref="曲ツリー.フォーカスリスト"/> を表示する。
    ///		画面に表示される曲は8行だが、スクロールを勘案して上下に１行ずつ追加し、計10行として扱う。
    /// </remarks>
    class 曲リスト : IDisposable
    {

        // 生成と終了


        public 曲リスト()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App進行描画.曲ツリー.フォーカスノードが変更された += this.曲ツリー_フォーカスノードが変更された;

                this._カーソル位置 = 4;
                this._曲リスト全体のY軸移動オフセット = 0;

                this._選択ノードの表示オフセットdpx = null;
                this._選択ノードの表示オフセットのストーリーボード = null;

                this._ノードto曲名画像 = new Dictionary<Node, 文字列画像>();

                this._初めての進行描画 = true;
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var kvp in this._ノードto曲名画像 )
                    kvp.Value?.Dispose();
                this._ノードto曲名画像.Clear();

                foreach( var kvp in this._ノードtoサブタイトル画像 )
                    kvp.Value?.Dispose();
                this._ノードtoサブタイトル画像.Clear();

                this._選択ノードの表示オフセットのストーリーボード?.Abandon();
                this._選択ノードの表示オフセットdpx?.Dispose();
                this._選択ノードの表示オフセットのストーリーボード?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc )
        {
            // 進行

            if( this._初めての進行描画 )
            {
                this._スクロール用カウンタ = new 定間隔進行();     // 生成と同時にカウント開始。
                this._選択ノードのオフセットアニメをリセットする( DXResources.Instance.アニメーション );
                this._初めての進行描画 = false;
            }

            #region " 曲リストの縦方向スクロール進行残があれば進行する。"
            //----------------
            this._スクロール用カウンタ.経過時間の分だけ進行する( 1, () => {

                int オフセットの加減算速度 = 1;

                #region " カーソルが中央から遠いほど速くなるよう、オフセットの加減算速度（絶対値）を計算する。"
                //------------------
                int 距離 = Math.Abs( 4 - this._カーソル位置 );

                if( 2 > 距離 )
                    オフセットの加減算速度 = 1;
                else
                    オフセットの加減算速度 = 2;
                //------------------
                #endregion

                // オフセット と カーソル位置 を更新する。
                if( ( 4 > this._カーソル位置 ) ||
                  ( ( 4 == this._カーソル位置 ) && ( 0 > this._曲リスト全体のY軸移動オフセット ) ) )
                {
                    #region " (A) パネルは、上から下へ、移動する。"
                    //-----------------
                    this._曲リスト全体のY軸移動オフセット += オフセットの加減算速度;

                    // １行分移動した
                    if( 100 <= this._曲リスト全体のY軸移動オフセット )
                    {
                        this._曲リスト全体のY軸移動オフセット -= 100;  // 0 付近に戻る
                        this._カーソル位置++;
                    }
                    //-----------------
                    #endregion
                }
                else if( ( 4 < this._カーソル位置 ) ||
                       ( ( 4 == this._カーソル位置 ) && ( 0 < this._曲リスト全体のY軸移動オフセット ) ) )
                {
                    #region " (B) パネルは、下から上へ、移動する。"
                    //-----------------
                    this._曲リスト全体のY軸移動オフセット -= オフセットの加減算速度;

                    // １行分移動した
                    if( -100 >= this._曲リスト全体のY軸移動オフセット )
                    {
                        this._曲リスト全体のY軸移動オフセット += 100;  // 0 付近に戻る
                        this._カーソル位置--;
                    }
                    //-----------------
                    #endregion
                }

            } );
            //----------------
            #endregion


            // 描画

            // 現在のフォーカスノードを取得。
            var 描画するノード = App進行描画.曲ツリー.フォーカスノード;
            if( null == 描画するノード )
                return;

            lock( 描画するノード.子ノードリスト排他 )   // リスト描画中は lock
            {
                // 表示する最上行のノードまで戻る。
                for( int i = 0; i < this._カーソル位置; i++ )
                    描画するノード = 描画するノード.前のノード;

                // 10行描画。
                for( int i = 0; i < 10; i++ )
                {
                    this._ノードを描画する( dc, i, 描画するノード );
                    描画するノード = 描画するノード.次のノード;
                }
            }
        }

        /// <param name="行番号">
        ///		一番上:0 ～ 9:一番下。
        ///		「静止時の」可視範囲は 1～8。
        ///		4 がフォーカスノード。
        ///	</param>
        private void _ノードを描画する( DeviceContext dc, int 行番号, Node ノード )
        {
            Debug.Assert( 0 <= 行番号 && 9 >= 行番号 );
            Debug.Assert( null != ノード );
            Debug.Assert( ( ノード as RootNode ) is null );

            var ノード画像 = ノード.ノード画像 ?? Node.既定のノード画像;
            bool 選択ノードである = ( 4 == 行番号 );

            var 実数行番号 = 行番号 + ( this._曲リスト全体のY軸移動オフセット / 100f );
            var ノード左上dpx = new Vector3(
                // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。
                this._曲リストの基準左上隅座標dpx.X + ( ( 選択ノードである ) ? (float) this._選択ノードの表示オフセットdpx.Value : 0f ),
                this._曲リストの基準左上隅座標dpx.Y + ( 実数行番号 * _ノードの高さdpx ),
                0f );

            #region " 背景 "
            //----------------
            DXResources.Instance.D2DBatchDraw( dc, () => {

                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                if( ノード is BoxNode )
                {
                    #region " BOXノードの背景 "
                    //----------------
                    using( var brush = new SolidColorBrush( dc, new Color4( 0xffa3647c ) ) )
                    {
                        using( var pathGeometry = new PathGeometry( DXResources.Instance.D2D1Factory1 ) )
                        {
                            using( var sink = pathGeometry.Open() )
                            {
                                sink.SetFillMode( FillMode.Winding );
                                sink.BeginFigure( new Vector2( ノード左上dpx.X, ノード左上dpx.Y + 8f ), FigureBegin.Filled ); // 1
                                var points = new SharpDX.Mathematics.Interop.RawVector2[] {
                                        new Vector2( ノード左上dpx.X + 150f, ノード左上dpx.Y + 8f ),	// 2
										new Vector2( ノード左上dpx.X + 170f, ノード左上dpx.Y + 18f ),	// 3
										new Vector2( DXResources.Instance.設計画面サイズ.Width, ノード左上dpx.Y + 18f ),	// 4
										new Vector2( DXResources.Instance.設計画面サイズ.Width, ノード左上dpx.Y + _ノードの高さdpx ),	// 5
										new Vector2( ノード左上dpx.X, ノード左上dpx.Y + _ノードの高さdpx ),	// 6
										new Vector2( ノード左上dpx.X, ノード左上dpx.Y + 8f ),	// 1
									};
                                sink.AddLines( points );
                                sink.EndFigure( FigureEnd.Closed );
                                sink.Close();
                            }

                            dc.FillGeometry( pathGeometry, brush );
                        }
                    }
                    //----------------
                    #endregion
                }
                else if( ノード is BackNode || ノード is RandomSelectNode )
                {
                    #region " BACK/RandomSelectノードの背景 "
                    //----------------
                    using( var brush = new SolidColorBrush( dc, Color4.Black ) )
                    {
                        using( var pathGeometry = new PathGeometry( DXResources.Instance.D2D1Factory1 ) )
                        {
                            using( var sink = pathGeometry.Open() )
                            {
                                sink.SetFillMode( FillMode.Winding );
                                sink.BeginFigure( new Vector2( ノード左上dpx.X, ノード左上dpx.Y + 8f ), FigureBegin.Filled ); // 1
                                var points = new SharpDX.Mathematics.Interop.RawVector2[] {
                                        new Vector2( ノード左上dpx.X + 150f, ノード左上dpx.Y + 8f ),	// 2
										new Vector2( ノード左上dpx.X + 170f, ノード左上dpx.Y + 18f ),	// 3
										new Vector2( DXResources.Instance.設計画面サイズ.Width, ノード左上dpx.Y + 18f ),	// 4
										new Vector2( DXResources.Instance.設計画面サイズ.Width, ノード左上dpx.Y + _ノードの高さdpx ),	// 5
										new Vector2( ノード左上dpx.X, ノード左上dpx.Y + _ノードの高さdpx ),	// 6
										new Vector2( ノード左上dpx.X, ノード左上dpx.Y + 8f ),	// 1
									};
                                sink.AddLines( points );
                                sink.EndFigure( FigureEnd.Closed );
                                sink.Close();
                            }

                            dc.FillGeometry( pathGeometry, brush );
                        }
                    }
                    //----------------
                    #endregion
                }
                else
                {
                    #region " 既定の背景（半透明の黒）"
                    //----------------
                    using( var brush = new SolidColorBrush( dc, new Color4( 0f, 0f, 0f, 0.25f ) ) )
                        dc.FillRectangle( new RectangleF( ノード左上dpx.X, ノード左上dpx.Y, DXResources.Instance.設計画面サイズ.Width - ノード左上dpx.X, _ノードの高さdpx ), brush );
                    //----------------
                    #endregion
                }

            } );
            //----------------
            #endregion

            #region " ノード画像を縮小したサムネイル画像 "
            //----------------
            if( null != ノード画像 )
            {
                if( ノード is BoxNode )
                {
                    #region " BOXノードのサムネイル画像 → 少し小さく表示する（涙 "
                    //----------------
                    var ノード内サムネイルオフセットdpx = new Vector3( 58f, 4f, 0f );

                    var サムネイル表示中央dpx = new Vector3(
                        DXResources.Instance.画面左上dpx.X + ノード左上dpx.X + ( this._サムネイル表示サイズdpx.X / 2f ) + ノード内サムネイルオフセットdpx.X,
                        DXResources.Instance.画面左上dpx.Y - ノード左上dpx.Y - ( this._サムネイル表示サイズdpx.Y / 2f ) - ノード内サムネイルオフセットdpx.Y,
                        0f );

                    var 変換行列 =
                        Matrix.Scaling(
                            this._サムネイル表示サイズdpx.X / ノード画像.サイズ.Width,
                            this._サムネイル表示サイズdpx.Y / ノード画像.サイズ.Height,
                            0f ) *
                        Matrix.Scaling( 0.9f ) *                            // ちょっと小さく
                        Matrix.Translation( サムネイル表示中央dpx - 4f );   // ちょっと下へ

                    ノード.進行描画する( dc, 変換行列, キャプション表示: false );
                    //----------------
                    #endregion
                }
                else if( ノード is BackNode || ノード is RandomSelectNode )
                {
                    // BACK/RandomSelectノードはサムネイル画像なし
                }
                else
                {
                    #region " 既定のサムネイル画像 "
                    //----------------
                    var ノード内サムネイルオフセットdpx = new Vector3( 58f, 4f, 0f );

                    var サムネイル表示中央dpx = new Vector3(
                        DXResources.Instance.画面左上dpx.X + ノード左上dpx.X + ( this._サムネイル表示サイズdpx.X / 2f ) + ノード内サムネイルオフセットdpx.X,
                        DXResources.Instance.画面左上dpx.Y - ノード左上dpx.Y - ( this._サムネイル表示サイズdpx.Y / 2f ) - ノード内サムネイルオフセットdpx.Y,
                        0f );

                    var 変換行列 =
                        Matrix.Scaling(
                            this._サムネイル表示サイズdpx.X / ノード画像.サイズ.Width,
                            this._サムネイル表示サイズdpx.Y / ノード画像.サイズ.Height,
                            0f ) *
                        Matrix.Translation( サムネイル表示中央dpx );

                    ノード.進行描画する( dc, 変換行列, キャプション表示: false );
                    //----------------
                    #endregion
                }
            }
            //----------------
            #endregion

            #region " タイトル文字列 "
            //----------------
            {
                var 曲名画像 = (文字列画像) null;

                // 曲名画像を取得する。未生成なら生成する。
                if( !( this._ノードto曲名画像.ContainsKey( ノード ) ) )
                {
                    曲名画像 = new 文字列画像() {
                        表示文字列 = ノード.タイトル,
                        フォント名 = "HGMaruGothicMPRO", // "メイリオ",
                        フォント幅 = FontWeight.Regular,
                        フォントスタイル = FontStyle.Normal,
                        フォントサイズpt = 40f,
                        描画効果 = 文字列画像.効果.縁取り,
                        縁のサイズdpx = 6f,
                        前景色 = Color4.Black,
                        背景色 = Color4.White,
                    };

                    this._ノードto曲名画像.Add( ノード, 曲名画像 );
                }
                else
                {
                    曲名画像 = this._ノードto曲名画像[ ノード ];
                }

                // 拡大率を計算して描画する。
                float 最大幅dpx = DXResources.Instance.設計画面サイズ.Width - ノード左上dpx.X - 170f;
                曲名画像.描画する(
                    dc,
                    ノード左上dpx.X + 170f,
                    ノード左上dpx.Y + 30f,
                    X方向拡大率: ( 曲名画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / 曲名画像.画像サイズdpx.Width );
            }
            //----------------
            #endregion

            #region " サブタイトル文字列 "
            //----------------
            if( ノード == App進行描画.曲ツリー.フォーカスノード )  // フォーカスノードのみ表示する。
            {
                var サブタイトル画像 = (文字列画像) null;

                // ノードが SetNode ならユーザ希望難易度に応じた MusicNode が対象。
                var node = ( ノード is SetNode setnode ) ? App進行描画.曲ツリー.フォーカス曲ノード : ノード;

                // サブタイトル画像を取得する。未生成かつ指定があるなら生成する。
                if( !( this._ノードtoサブタイトル画像.ContainsKey( node ) ) )
                {
                    if( node.サブタイトル.Nullでも空でもない() )
                    {
                        サブタイトル画像 = new 文字列画像() {
                            表示文字列 = node.サブタイトル,
                            フォント名 = "HGMaruGothicMPRO", // "メイリオ",
                            フォント幅 = FontWeight.Regular,
                            フォントスタイル = FontStyle.Normal,
                            フォントサイズpt = 20f,
                            描画効果 = 文字列画像.効果.縁取り,
                            縁のサイズdpx = 4f,
                            前景色 = Color4.Black,
                            背景色 = Color4.White,
                        };

                        this._ノードtoサブタイトル画像.Add( node, サブタイトル画像 );
                    }
                    else
                    {
                        // 指定がない
                        サブタイトル画像 = null;
                    }
                }
                else
                {
                    サブタイトル画像 = this._ノードtoサブタイトル画像[ node ];
                }

                // 拡大率を計算して描画する。
                if( null != サブタイトル画像 )
                {
                    float 最大幅dpx = DXResources.Instance.設計画面サイズ.Width - ノード左上dpx.X - 170f;

                    サブタイトル画像.描画する(
                        dc,
                        ノード左上dpx.X + 190f,
                        ノード左上dpx.Y + 80f,
                        X方向拡大率: ( サブタイトル画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / サブタイトル画像.画像サイズdpx.Width );
                }
            }
            //----------------
            #endregion
        }



        // ノードの選択


        public void 前のノードを選択する()
        {
            this._カーソル位置--;     // 下限なし

            App進行描画.曲ツリー.前のノードをフォーカスする();

            this._選択ノードのオフセットアニメをリセットする( DXResources.Instance.アニメーション );
        }

        public void 次のノードを選択する()
        {
            this._カーソル位置++;     // 上限なし

            App進行描画.曲ツリー.次のノードをフォーカスする();

            this._選択ノードのオフセットアニメをリセットする( DXResources.Instance.アニメーション );
        }

        public void BOXに入る()
        {
            this._カーソル位置 = 4;

            this._曲リスト全体のY軸移動オフセット = 0;

            App進行描画.曲ツリー.フォーカスする( App進行描画.曲ツリー.フォーカスノード.子ノードリスト[ 0 ] );
        }

        public void BOXから出る()
        {
            this._カーソル位置 = 4;

            this._曲リスト全体のY軸移動オフセット = 0;

            App進行描画.曲ツリー.フォーカスする( App進行描画.曲ツリー.フォーカスノード.親ノード );
        }

        public void 難易度アンカをひとつ増やす()
        {
            App進行描画.曲ツリー.難易度アンカをひとつ増やす();
        }



        // private


        private bool _初めての進行描画 = true;
        
        /// <summary>
        ///		曲リスト（10行分！）の合計表示領域の左上隅の座標。
        ///		基準というのは、曲リストがスクロールしていないとき、という意味。
        /// </summary>
        private readonly Vector3 _曲リストの基準左上隅座標dpx = new Vector3( 1065f, 145f - _ノードの高さdpx, 0f );

        private readonly Vector3 _サムネイル表示サイズdpx = new Vector3( 100f, 100f, 0f );

        private const float _ノードの高さdpx = ( 913f / 8f );

        private Dictionary<Node, 文字列画像> _ノードto曲名画像 = new Dictionary<Node, 文字列画像>();

        private Dictionary<Node, 文字列画像> _ノードtoサブタイトル画像 = new Dictionary<Node, 文字列画像>();
        
        /// <summary>
        ///		静止時は 4 。曲リストがスクロールしているときは、4より大きい整数（下から上にスクロール中）か、
        ///		または 4 より小さい整数（上から下にスクロール中）になる。
        /// </summary>
        private int _カーソル位置 = 4;

        private 定間隔進行 _スクロール用カウンタ = null;
        
        /// <summary>
        ///		-100～100。曲リスト全体の表示位置を、負数は 上 へ、正数は 下 へずらす 。（正負と上下の対応に注意。）
        /// </summary>
        private int _曲リスト全体のY軸移動オフセット = 0;
        
        /// <summary>
        ///		選択中の曲ノードエリアを左にずらす度合い。
        ///		-50f ～ 0f [dpx] 。
        /// </summary>
        private Variable _選択ノードの表示オフセットdpx = null;

        private Storyboard _選択ノードの表示オフセットのストーリーボード = null;


        private void _選択ノードのオフセットアニメをリセットする( アニメーション am )
        {
            this._選択ノードの表示オフセットdpx?.Dispose();
            this._選択ノードの表示オフセットdpx = new Variable( am.Manager, initialValue: 0.0 );

            this._選択ノードの表示オフセットのストーリーボード?.Abandon();
            this._選択ノードの表示オフセットのストーリーボード?.Dispose();
            this._選択ノードの表示オフセットのストーリーボード = new Storyboard( am.Manager );

            using( var 維持 = am.TrasitionLibrary.Constant( 0.15 ) )
            using( var 左へ移動 = am.TrasitionLibrary.Linear( 0.07, finalValue: -50f ) )
            {
                this._選択ノードの表示オフセットのストーリーボード.AddTransition( this._選択ノードの表示オフセットdpx, 維持 );
                this._選択ノードの表示オフセットのストーリーボード.AddTransition( this._選択ノードの表示オフセットdpx, 左へ移動 );
            }

            this._選択ノードの表示オフセットのストーリーボード.Schedule( am.Timer.Time );
        }

        private void 曲ツリー_フォーカスノードが変更された( object sender, (Node 選択されたNode, Node 選択が解除されたNode) e )
        {
            // (1) 選択解除曲のプレビュー音声を止める。

            e.選択が解除されたNode?.プレビュー音声を停止する();


            // (2) 選択曲のプレビュー音声を生成し、再生を始める。

            e.選択されたNode?.プレビュー音声を再生する();
        }
    }
}
