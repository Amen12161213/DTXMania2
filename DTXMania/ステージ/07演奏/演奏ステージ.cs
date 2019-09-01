﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using SSTFormat.v4;

namespace DTXMania.演奏
{
    /// <summary>
    ///     ビュアーモードでは、SSTFファイルのみ再生可能。
    /// </summary>
    class 演奏ステージ : ステージ
    {
        public const float ヒット判定位置Ydpx = 847f;


        public enum フェーズ
        {
            フェードイン,
            表示,
            クリア,
            キャンセル通知,
            キャンセル時フェードアウト,
            キャンセル完了,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.キャンセル完了;

        /// <summary>
        ///     フェードインアイキャッチの遷移元画面。
        ///     活性化前に、外部から設定される。
        /// </summary>
        public Bitmap キャプチャ画面 { get; set; } = null;

        public 成績 成績 { get; protected set; } = null;



        // 生成と終了


        public 演奏ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void OnDispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                base.OnDispose();
            }
        }



        // 活性化と非活性化


        public override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._背景画像 = new 画像( @"$(System)images\演奏\演奏画面.png" );
                this._レーンフレーム = new レーンフレーム();
                this._曲名パネル = new 曲名パネル();
                this._ドラムキットとヒットバー = new ドラムキットとヒットバー();
                this._レーンフラッシュ = new レーンフラッシュ();
                this._ドラムチップ = new ドラムチップ();
                this._判定文字列 = new 判定文字列();
                this._チップ光 = new チップ光();
                this._左サイドクリアパネル = new 左サイドクリアパネル();
                this._右サイドクリアパネル = new 右サイドクリアパネル();
                this._判定パラメータ表示 = new 判定パラメータ表示();
                this._フェーズパネル = new フェーズパネル();
                this._コンボ表示 = new コンボ表示();
                this._カウントマップライン = new カウントマップライン();
                this._スコア表示 = new スコア表示();
                this._プレイヤー名表示 = new プレイヤー名表示();
                this._譜面スクロール速度 = new 譜面スクロール速度( App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );
                this._達成率表示 = new 達成率表示();
                this._曲別SKILL = new 曲別SKILL();
                this._エキサイトゲージ = new エキサイトゲージ();
                this._システム情報 = new システム情報();
                this._数字フォント中グレー48x64 = new 画像フォント(
                   @"$(System)images\数字フォント中ホワイト48x64.png",
                   @"$(System)images\数字フォント中48x64矩形リスト.yaml",
                   文字幅補正dpx: -16f,
                   不透明度: 0.3f );

                var dc = DXResources.Instance.既定のD2D1DeviceContext;

                this._小節線色 = new SolidColorBrush( dc, Color.White );
                this._小節線影色 = new SolidColorBrush( dc, Color.Blue );
                this._拍線色 = new SolidColorBrush( dc, Color.Gray );
                this._プレイヤー名表示.名前 = App進行描画.ユーザ管理.ログオン中のユーザ.ユーザ名;
                this._フェードインカウンタ = new Counter( 0, 100, 10 );

                this._演奏状態を初期化する();

                this.現在のフェーズ = フェーズ.フェードイン;

                base.On活性化();
            }
        }

        public override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._演奏状態を終了する();

                #region " 現在の譜面スクロール速度をDBに保存。"
                //----------------
                using( var userdb = new UserDB() )
                {
                    var user = userdb.Users.Where( ( r ) => ( r.Id == App進行描画.ユーザ管理.ログオン中のユーザ.ユーザID ) ).SingleOrDefault();
                    if( null != user )
                    {
                        user.ScrollSpeed = App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度;
                        userdb.DataContext.SubmitChanges();
                        Log.Info( $"現在の譜面スクロール速度({App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度})をDBに保存しました。[{user}]" );
                    }
                }
                //----------------
                #endregion

                this._拍線色?.Dispose();
                this._小節線影色?.Dispose();
                this._小節線色?.Dispose();
                this.キャプチャ画面?.Dispose();

                this._背景画像?.Dispose();
                this._レーンフレーム?.Dispose();
                this._曲名パネル?.Dispose();
                this._ドラムキットとヒットバー?.Dispose();
                this._レーンフラッシュ?.Dispose();
                this._ドラムチップ?.Dispose();
                this._判定文字列?.Dispose();
                this._チップ光?.Dispose();
                this._左サイドクリアパネル?.Dispose();
                this._右サイドクリアパネル?.Dispose();
                this._判定パラメータ表示?.Dispose();
                this._フェーズパネル?.Dispose();
                this._コンボ表示?.Dispose();
                this._カウントマップライン?.Dispose();
                this._スコア表示?.Dispose();
                this._プレイヤー名表示?.Dispose();
                this._譜面スクロール速度?.Dispose();
                this._達成率表示?.Dispose();
                this._曲別SKILL?.Dispose();
                this._エキサイトゲージ?.Dispose();
                this._システム情報?.Dispose();
                this._数字フォント中グレー48x64?.Dispose();

                base.On非活性化();
            }
        }

        /// <summary>
        ///     <see cref="AppForm.演奏スコア"/> に対して、ステージを初期化する。
        /// </summary>
        private void _演奏状態を初期化する()
        {
            // スコアに依存するデータを初期化する。

            this.成績 = new 成績();
            this.成績.スコアと設定を反映する( App進行描画.演奏スコア, App進行描画.ユーザ管理.ログオン中のユーザ );

            this._カウントマップライン?.Dispose();
            this._カウントマップライン = new カウントマップライン();

            this._描画開始チップ番号 = -1;

            this._チップの演奏状態 = new Dictionary<チップ, チップの演奏状態>();
            foreach( var chip in App進行描画.演奏スコア.チップリスト )
                this._チップの演奏状態.Add( chip, new チップの演奏状態( chip ) );

            this._スコア指定の背景画像?.Dispose();
            this._スコア指定の背景画像 = (App進行描画.演奏スコア.背景画像ファイル名.Nullまたは空である() ) ? null : 
                new 画像( Path.Combine( App進行描画.演奏スコア.PATH_WAV, App進行描画.演奏スコア.背景画像ファイル名 ) );


            // WAVを生成する。

            App進行描画.WAVキャッシュレンタル.世代を進める();

            App進行描画.WAV管理?.Dispose();
            App進行描画.WAV管理 = new WAV管理();

            foreach( var kvp in App進行描画.演奏スコア.WAVリスト )
            {
                var wavInfo = kvp.Value;

                var path = Path.Combine( App進行描画.演奏スコア.PATH_WAV, wavInfo.ファイルパス );
                App進行描画.WAV管理.登録する( kvp.Key, path, wavInfo.多重再生する, wavInfo.BGMである );
            }


            // AVIを生成する。（必要があれば）

            App進行描画.AVI管理?.Dispose();
            App進行描画.AVI管理 = null;

            if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
            {
                App進行描画.AVI管理 = new AVI管理();

                foreach( var kvp in App進行描画.演奏スコア.AVIリスト )
                {
                    var path = Path.Combine( App進行描画.演奏スコア.PATH_WAV, kvp.Value );
                    App進行描画.AVI管理.登録する( kvp.Key, path, App進行描画.ユーザ管理.ログオン中のユーザ.再生速度 );
                }
            }
        }

        private void _演奏状態を終了する()
        {
            this._描画開始チップ番号 = -1;

            //App進行描画.WAV管理?.Dispose();	
            //App進行描画.WAV管理 = null;
            App進行描画.AVI管理?.Dispose();
            App進行描画.AVI管理 = null;
        }



        // BGM停止

        /// <remarks>
        ///		演奏クリア時には、次の結果ステージに入ってもBGMが鳴り続ける。
        ///		そのため、後からBGMだけを別個に停止するためのメソッドが必要になる。
        /// </remarks>
        public void BGMを停止する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App進行描画.WAV管理.すべての発声を停止する();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
            try
            {
                this._システム情報.FPSをカウントしプロパティを更新する();

                switch( this.現在のフェーズ )
                {
                    case フェーズ.フェードイン:
                        if( this._フェードインカウンタ.終了値に達した )
                        {
                            #region " フェードインが終わったので、演奏開始。 "
                            //----------------
                            Log.Info( "演奏を開始します。" );

                            this._描画開始チップ番号 = 0; // -1 から 0 に変われば演奏開始。

                            App進行描画.サウンドタイマ.リセットする();

                            this.現在のフェーズ = フェーズ.表示;

                            // ここで break; すると、次の表示フェーズまで１フレーム分の時間（数ミリ秒）が空いてしまう。
                            // なので、フレームが空かないように、ここですぐさま最初の表示フェーズを実行する。
                            this.進行する();
                            //----------------
                            #endregion
                        }
                        break;

                    case フェーズ.表示:

                        // ※注:クリアや失敗の判定は、ここではなく描画側で行っている。

                        double 現在の演奏時刻sec = this._演奏開始からの経過時間secを返す();
                        long 現在の演奏時刻qpc = QPCTimer.生カウント;

                        #region " 自動ヒット処理。"
                        //----------------
                        this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

                            var ユーザ設定 = App進行描画.ユーザ管理.ログオン中のユーザ;
                            var ドラムチッププロパティ = ユーザ設定.ドラムチッププロパティ管理[ chip.チップ種別 ];

                            bool AutoPlayである = ユーザ設定.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];
                            bool チップはヒット済みである = this._チップの演奏状態[ chip ].ヒット済みである;
                            bool チップはまだヒットされていない = !( チップはヒット済みである );
                            bool チップはMISSエリアに達している = ( ヒット判定バーと描画との時間sec > ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] );
                            bool チップは描画についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと描画との時間sec );
                            bool チップは発声についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと発声との時間sec );

                            if( チップはまだヒットされていない && チップはMISSエリアに達している )
                            {
                                #region " MISS判定。"
                                //----------------
                                if( AutoPlayである && ドラムチッププロパティ.AutoPlayON_Miss判定 )
                                {
                                    this._チップのヒット処理を行う(
                                        chip,
                                        判定種別.MISS,
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_再生,
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_判定,
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_非表示,
                                        ヒット判定バーと発声との時間sec,
                                        ヒット判定バーと描画との時間sec );
                                    return;
                                }
                                else if( !AutoPlayである && ドラムチッププロパティ.AutoPlayOFF_Miss判定 )
                                {
                                    this._チップのヒット処理を行う(
                                        chip,
                                        判定種別.MISS,
                                        ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_再生,
                                        ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_判定,
                                        ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_非表示,
                                        ヒット判定バーと発声との時間sec,
                                        ヒット判定バーと描画との時間sec );

                                    this.成績.エキサイトゲージを更新する( 判定種別.MISS ); // 手動演奏なら MISS はエキサイトゲージに反映。
                                    return;
                                }
                                else
                                {
                                    // 通過。
                                }
                                //----------------
                                #endregion
                            }

                            // ヒット処理(1) 発声時刻
                            if( チップは発声についてヒット判定バーを通過した )    // ヒット済みかどうかには関係ない
                            {
                                #region " 自動ヒット判定（再生）"
                                //----------------
                                if( ( AutoPlayである && ドラムチッププロパティ.AutoPlayON_自動ヒット_再生 ) ||
                                    ( !AutoPlayである && ドラムチッププロパティ.AutoPlayOFF_自動ヒット_再生 ) )
                                {
                                    // チップの発声がまだなら発声を行う。

                                    if( !( this._チップの演奏状態[ chip ].発声済みである ) )
                                    {
                                        this.チップの発声を行う( chip, ユーザ設定.ドラムの音を発声する );
                                        this._チップの演奏状態[ chip ].発声済みである = true;
                                    }
                                }
                                //----------------
                                #endregion
                            }

                            // ヒット処理(2) 描画時刻
                            if( チップはまだヒットされていない && チップは描画についてヒット判定バーを通過した )
                            {
                                #region " 自動ヒット判定（判定）"
                                //----------------
                                if( AutoPlayである && ドラムチッププロパティ.AutoPlayON_自動ヒット )
                                {
                                    this._チップのヒット処理を行う(
                                        chip,
                                        判定種別.PERFECT,   // AutoPlay 時は Perfect 扱い。
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_再生,
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_判定,
                                        ドラムチッププロパティ.AutoPlayON_自動ヒット_非表示,
                                        ヒット判定バーと発声との時間sec,
                                        ヒット判定バーと描画との時間sec );

                                    //this.成績.エキサイトゲージを加算する( 判定種別.PERFECT ); -> エキサイトゲージには反映しない。

                                    this._ドラムキットとヒットバー.ヒットアニメ開始( ドラムチッププロパティ.表示レーン種別 );

                                    return;
                                }
                                else if( !AutoPlayである && ドラムチッププロパティ.AutoPlayOFF_自動ヒット )
                                {
                                    this._チップのヒット処理を行う(
                                        chip,
                                        判定種別.PERFECT,   // AutoPlay OFF でも自動ヒットする場合は Perfect 扱い。
                                        ドラムチッププロパティ.AutoPlayOFF_自動ヒット_再生,
                                        ドラムチッププロパティ.AutoPlayOFF_自動ヒット_判定,
                                        ドラムチッププロパティ.AutoPlayOFF_自動ヒット_非表示,
                                        ヒット判定バーと発声との時間sec,
                                        ヒット判定バーと描画との時間sec );

                                    //this.成績.エキサイトゲージを加算する( 判定種別.PERFECT ); -> エキサイトゲージには反映しない。

                                    this._ドラムキットとヒットバー.ヒットアニメ開始( ドラムチッププロパティ.表示レーン種別 );

                                    return;
                                }
                                else
                                {
                                    // 通過。
                                }
                                //----------------
                                #endregion
                            }

                        } );
                        //----------------
                        #endregion

                        App進行描画.入力管理.すべての入力デバイスをポーリングする( 入力履歴を記録する: false );

                        #region " 手動ヒット処理。"
                        //----------------
                        {
                            var ヒット処理済み入力 = new List<ドラム入力イベント>(); // ヒット処理が終わった入力は、二重処理しないよう、この中に追加しておく。

                            var ユーザ設定 = App進行描画.ユーザ管理.ログオン中のユーザ;

                            #region " 描画範囲内のすべてのチップについて、対応する入力があればヒット処理を行う。"
                            //----------------
                            this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離 ) => {

                                var ドラムチッププロパティ = ユーザ設定.ドラムチッププロパティ管理[ chip.チップ種別 ];

                                #region " チップがヒット対象外であれば無視する。"
                                //----------------
                                bool チップはAutoPlayONである = ユーザ設定.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];
                                bool チップはMISSエリアに達している = ( ヒット判定バーと描画との時間sec > ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] );
                                bool チップはOKエリアに達していない = ヒット判定バーと描画との時間sec < -( ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] );
                                bool チップはすでにヒット済みである = this._チップの演奏状態[ chip ].ヒット済みである;
                                bool チップはAutoPlayOFFのときゆユーザヒットの対象にならない = !( ドラムチッププロパティ.AutoPlayOFF_ユーザヒット );

                                if( チップはAutoPlayONである ||
                                    チップはすでにヒット済みである ||
                                    チップはAutoPlayOFFのときゆユーザヒットの対象にならない ||
                                    チップはMISSエリアに達している ||
                                    チップはOKエリアに達していない )
                                    return;
                                //----------------
                                #endregion


                                ドラム入力イベント チップのヒットエリア内にある入力 = null;
                                
                                #region " チップのヒットエリア内にある入力を探す。"
                                //----------------
                                チップのヒットエリア内にある入力 = App進行描画.入力管理.ポーリング結果.FirstOrDefault( ( 入力 ) => {

                                    if( !入力.InputEvent.押された ||                   // 押下入力じゃないなら無視。
                                        ヒット処理済み入力.Contains( 入力 ) ||         // すでに今回のターンで処理済み（＝処理済み入力リストに追加済み）なら無視。
                                        入力.Type == ドラム入力種別.HiHat_Control )    // HiHat_Control 入力はここでは無視。
                                        return false;

                                    var チップの入力グループ = ドラムチッププロパティ.入力グループ種別;

                                    // (A) 入力グループ種別 が Unknown の場合 → ドラム入力種別で比較
                                    if( チップの入力グループ == 入力グループ種別.Unknown )
                                    {
                                        return ( ドラムチッププロパティ.ドラム入力種別 == 入力.Type );          // 一致すればヒット
                                    }
                                    // (B) 入力グループ種別が Unknown ではない場合　→　入力グループ種別で比較
                                    else
                                    {
                                        var 入力の入力グループ種別リスト =
                                            from kvp in ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ
                                            where ( kvp.Value.ドラム入力種別 == 入力.Type )
                                            select kvp.Value.入力グループ種別;

                                        return 入力の入力グループ種別リスト.Any( ( groupType ) => ( groupType == チップの入力グループ ) );  // どれかが一致すればヒット
                                    }

                                } );
                                //----------------
                                #endregion


                                if( null != チップのヒットエリア内にある入力 ) // チップにヒットした入力があった場合
                                {
                                    #region " チップの手動ヒット処理。"
                                    //----------------
                                    ヒット処理済み入力.Add( チップのヒットエリア内にある入力 );    // この入力はこのチップでヒット処理した。

                                    double 入力とチップの時間sec = 0.0;  // チップより入力が早ければ負数、遅ければ正数。

                                    #region " 入力とチップとの時間差を算出。"
                                    //----------------
                                    {
                                        // 入力時刻とはヒット判定バーの位置における時刻であり、理想値は 0 である。
                                        // しかし実際には、入力遅延（MIDI入力のポーリング時刻と現在時刻のズレなど）が発生することがあるので、0 または正数となる。
                                        double 入力時刻sec = QPCTimer.生カウント相対値を秒へ変換して返す( 現在の演奏時刻qpc - チップのヒットエリア内にある入力.InputEvent.TimeStamp );

                                        // そこで、システム設定の「判定位置調整」量を加えることで、この入力遅延を相殺する。
                                        // 具体的には、ヒット判定バーの位置、すなわち入力時刻を、判定位置調整の値で調整する。
                                        // 例えば、判定位置調整が -50ms である場合、各入力はそれぞれ 50ms 早く発生したものとみなされる。
                                        // 注意：この機能を、タイミングが常に早い方向か遅い方向にずれまくる人のための「人為的な入力遅延」として使うことは推奨されない。
                                        入力時刻sec += App進行描画.システム設定.判定位置調整ms * 0.001;

                                        入力とチップの時間sec = 入力時刻sec - ヒット判定バーと描画との時間sec;
                                    }
                                    //----------------
                                    #endregion


                                    // 時刻差から判定を算出。
                                    double 入力とチップの時間差の絶対値sec = Math.Abs( 入力とチップの時間sec );
                                    var 判定 =
                                        ( 入力とチップの時間差の絶対値sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.PERFECT ] ) ? 判定種別.PERFECT :
                                        ( 入力とチップの時間差の絶対値sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.GREAT ] ) ? 判定種別.GREAT :
                                        ( 入力とチップの時間差の絶対値sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.GOOD ] ) ? 判定種別.GOOD : 判定種別.OK;

                                    // ヒット処理。
                                    this._チップのヒット処理を行う(
                                        chip,
                                        判定,
                                        ( ユーザ設定.ドラムの音を発声する ) ? ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_再生 : false,   // ヒットすれば再生する？
                                        ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_判定,                                                 // ヒットすれば判定する？
                                        ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_非表示,                                               // ヒットすれば非表示にする？
                                        ヒット判定バーと発声との時間sec,
                                        入力とチップの時間sec );

                                    // エキサイトゲージに反映する。
                                    this.成績.エキサイトゲージを更新する( 判定 );
                                    //----------------
                                    #endregion
                                }

                            } );
                            //----------------
                            #endregion

                            #region " ヒットしてようがしてまいが起こすアクションを実行。"
                            //----------------
                            {
                                var アクション済み入力 = new List<ドラム入力イベント>();  // ヒット処理が終わった入力は、二重処理しないよう、この中に追加しておく。

                                foreach( var 入力 in App進行描画.入力管理.ポーリング結果 )
                                {
                                    // 押下入力じゃないなら無視。
                                    if( 入力.InputEvent.離された )
                                        continue;

                                    var プロパティs = ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ.Where( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) );

                                    if( プロパティs.Count() > 0 )
                                    {
                                        //for( int i = 0; i < プロパティs.Count(); i++ )
                                        int i = 0;  // １つの入力で処理するのは、１つの表示レーン種別のみ。
                                        {
                                            var laneType = プロパティs.ElementAt( i ).Value.表示レーン種別;

                                            this._ドラムキットとヒットバー.ヒットアニメ開始( laneType );
                                            this._レーンフラッシュ.開始する( laneType );
                                        }
                                    }
                                }
                            }
                            //----------------
                            #endregion

                            #region " どのチップにもヒットしなかった入力は空打ちとみなし、空打ち音を再生する。"
                            //----------------
                            if( ユーザ設定.ドラムの音を発声する )
                            {
                                foreach( var 入力 in App進行描画.入力管理.ポーリング結果 )
                                {
                                    if( ヒット処理済み入力.Contains( 入力 ) ||  // ヒット済みなら無視。
                                        入力.InputEvent.離された )              // 押下じゃないなら無視。
                                        continue;

                                    var プロパティs = ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ.Where( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) );

                                    for( int i = 0; i < プロパティs.Count(); i++ )
                                    {
                                        var prop = プロパティs.ElementAt( i ).Value;

                                        if( 0 < App進行描画.演奏スコア.空打ちチップマップ.Count )
                                        {
                                            #region " DTX他の場合（空うちチップマップ使用）"
                                            //----------------
                                            int zz = App進行描画.演奏スコア.空打ちチップマップ[ prop.レーン種別 ];

                                            // (A) 空打ちチップの指定があるなら、それを発声する。
                                            if( 0 != zz )
                                                App進行描画.WAV管理.発声する( zz, prop.チップ種別, prop.発声前消音, prop.消音グループ種別, true );

                                            // (B) 空打ちチップの指定がないなら、一番近いチップを検索し、それを発声する。
                                            else
                                            {
                                                var chip = this.指定された時刻に一番近いチップを返す( 現在の演奏時刻sec, 入力.Type );

                                                if( null != chip )
                                                {
                                                    this.チップの発声を行う( chip, true );
                                                    break;  // 複数のチップが該当する場合でも、最初のチップの発声のみ行う。
                                                }
                                            }
                                            //----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " SSTFの場合（空うちチップマップ未使用）"
                                            //----------------
                                            App進行描画.ドラムサウンド.発声する( prop.チップ種別, 0, prop.発声前消音, prop.消音グループ種別 );
                                            //----------------
                                            #endregion
                                        }
                                    }
                                }
                            }
                            //----------------
                            #endregion

                            ヒット処理済み入力 = null;
                        }
                        //----------------
                        #endregion

                        #region " その他の手動入力の処理。"
                        //----------------
                        // ※演奏中なのでドラム入力は無視。

                        if( App進行描画.入力管理.キーボード.キーが押された( 0, Keys.Escape ) )
                        {
                            #region " ESC → 演奏中断 "
                            //----------------
                            Log.Info( "ESC キーが押されました。演奏を中断します。" );

                            this.BGMを停止する();
                            App進行描画.WAV管理.すべての発声を停止する();    // DTXでのBGMサウンドはこっちに含まれる。

                            // 進行描画スレッドへの通知フェーズを挟む。
                            this.現在のフェーズ = フェーズ.キャンセル通知;
                            //----------------
                            #endregion
                        }
                        if( App進行描画.入力管理.キーボード.キーが押された( 0, Keys.Up ) )
                        {
                            if( App進行描画.入力管理.キーボード.キーが押されている( 0, Keys.ShiftKey ) )
                            {
                                #region " Shift+上 → BGMAdjust 増加 "
                                //----------------
                                App進行描画.演奏曲ノード.BGMAdjust += 10; // ms

                                App進行描画.WAV管理.すべてのBGMの再生位置を移動する( +10.0 / 1000.0 );  // sec

                                this._BGMAdjustをデータベースに保存する( App進行描画.演奏曲ノード );
                                //----------------
                                #endregion
                            }
                            else
                            {
                                #region " 上 → 譜面スクロールを加速 "
                                //----------------
                                const double 最大倍率 = 8.0;
                                App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Min( App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 + 0.5, 最大倍率 );
                                //----------------
                                #endregion
                            }
                        }
                        if( App進行描画.入力管理.キーボード.キーが押された( 0, Keys.Down ) )
                        {
                            if( App進行描画.入力管理.キーボード.キーが押されている( 0, Keys.ShiftKey ) )
                            {
                                #region " Shift+下 → BGMAdjust 減少 "
                                //----------------
                                App進行描画.演奏曲ノード.BGMAdjust -= 10; // ms
                                App進行描画.WAV管理.すべてのBGMの再生位置を移動する( -10.0 / 1000.0 );  // sec

                                this._BGMAdjustをデータベースに保存する( App進行描画.演奏曲ノード );
                                //----------------
                                #endregion
                            }
                            else
                            {
                                #region " 下 → 譜面スクロールを減速 "
                                //----------------
                                const double 最小倍率 = 0.5;
                                App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Max( App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 - 0.5, 最小倍率 );
                                //----------------
                                #endregion
                            }
                        }

                        #region " ハイハットの開閉 "
                        //----------------
                        foreach( var ev in App進行描画.入力管理.MIDI入力.入力イベントリスト.Where( ( ie ) => ( 255 == ie.Key ) ) )
                        {
                            this._ドラムキットとヒットバー.ハイハットの開度を設定する( ev.Velocity );
                        }
                        //----------------
                        #endregion

                        if( App進行描画.入力管理.ドラムが入力された( ドラム入力種別.Pause_Resume ) )
                        {
                            #region " Pause/Resumu パッド → 演奏の一時停止または再開 "
                            //----------------
                            this._演奏を一時停止または再開する();
                            //----------------
                            #endregion
                        }
                        //----------------
                        #endregion

                        break;

                    case フェーズ.キャンセル完了:
                    case フェーズ.キャンセル時フェードアウト:
                    case フェーズ.キャンセル通知:
                    case フェーズ.クリア:
                        break;
                }
            }
            catch( Exception e )
            {
                Log.ERROR( $"!!! 進行スレッドで例外が発生しました !!! [{Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }
        }

        public override void 描画する()
        {
            var dc = DXResources.Instance.既定のD2D1DeviceContext;
            dc.Transform = DXResources.Instance.拡大行列DPXtoPX;

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                case フェーズ.キャンセル完了:
                    {
                        if( App進行描画.ユーザ管理.ログオン中のユーザ.スコア指定の背景画像を表示する )
                        {
                            this._スコア指定の背景画像?.描画する( dc, 0f, 0f,
                                X方向拡大率: DXResources.Instance.設計画面サイズ.Width / this._スコア指定の背景画像.サイズ.Width,
                                Y方向拡大率: DXResources.Instance.設計画面サイズ.Height / this._スコア指定の背景画像.サイズ.Height );
                        }

                        this._左サイドクリアパネル.クリアする();
                        this._左サイドクリアパネル.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                            this._プレイヤー名表示.進行描画する( dcp );
                            this._スコア表示.進行描画する( dcp, DXResources.Instance.アニメーション, new Vector2( +280f, +120f ), this.成績 );
                            this._達成率表示.描画する( dcp, (float) this.成績.Achievement );
                            this._判定パラメータ表示.描画する( dcp, +118f, +372f, this.成績 );
                            this._曲別SKILL.進行描画する( dcp, 0f );
                        } );
                        this._左サイドクリアパネル.描画する();

                        this._右サイドクリアパネル.クリアする();
                        this._右サイドクリアパネル.描画する();

                        this._レーンフレーム.描画する( dc, App進行描画.ユーザ管理.ログオン中のユーザ.レーンの透明度 );

                        this._背景画像.描画する( dc, 0f, 0f );
                        this._譜面スクロール速度.描画する( dc, App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );
                        this._エキサイトゲージ.進行描画する( dc, this.成績.エキサイトゲージ量 );
                        this._カウントマップライン.進行描画する( dc );
                        this._フェーズパネル.進行描画する( dc );
                        this._曲名パネル.描画する( dc );

                        this._ドラムキットとヒットバー.ヒットバーを進行描画する();
                        this._ドラムキットとヒットバー.ドラムキットを進行描画する();

                        this._キャプチャ画面を描画する( dc, ( 1.0f - this._フェードインカウンタ.現在値の割合 ) );
                    }
                    break;

                case フェーズ.クリア:
                    this._背景画像.描画する( dc, 0f, 0f );
                    break;

                case フェーズ.表示:
                case フェーズ.キャンセル時フェードアウト:
                    {
                        double 演奏時刻sec = this._演奏開始からの経過時間secを返す() + DXResources.Instance.次のDComp表示までの残り時間sec;

                        this._譜面スクロール速度.進行する( App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );  // チップの表示より前に進行だけ行う

                        if( App進行描画.ユーザ管理.ログオン中のユーザ.スコア指定の背景画像を表示する )
                        {
                            this._スコア指定の背景画像?.描画する( dc, 0f, 0f,
                                X方向拡大率: DXResources.Instance.設計画面サイズ.Width / this._スコア指定の背景画像.サイズ.Width,
                                Y方向拡大率: DXResources.Instance.設計画面サイズ.Height / this._スコア指定の背景画像.サイズ.Height );
                        }
                        if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
                        {
                            #region " AVI（動画）の進行描画を行う。"
                            //----------------
                            foreach( var kvp in App進行描画.AVI管理.動画リスト )
                            {
                                int zz = kvp.Key;
                                var video = kvp.Value;

                                if( video.再生中 )
                                {
                                    switch( App進行描画.ユーザ管理.ログオン中のユーザ.動画の表示サイズ )
                                    {
                                        case 動画の表示サイズ.全画面:
                                            {
                                                // 100%全体表示
                                                float w = DXResources.Instance.設計画面サイズ.Width;
                                                float h = DXResources.Instance.設計画面サイズ.Height;
                                                video.描画する( dc, new RectangleF( 0f, 0f, w, h ) );
                                            }
                                            break;

                                        case 動画の表示サイズ.中央寄せ:
                                            {
                                                // 75%縮小表示
                                                float w = DXResources.Instance.設計画面サイズ.Width;
                                                float h = DXResources.Instance.設計画面サイズ.Height;

                                                // (1) 画面いっぱいに描画。
                                                video.描画する( dc, new RectangleF( 0f, 0f, w, h ), 0.2f );    // 不透明度は 0.2 で暗くする。

                                                // (2) ちょっと縮小して描画。
                                                float 拡大縮小率 = 0.75f;
                                                float 上移動 = 100.0f;
                                                video.最後のフレームを再描画する( dc, new RectangleF(   // 直前に取得したフレームをそのまま描画。
                                                    w * ( 1f - 拡大縮小率 ) / 2f,
                                                    h * ( 1f - 拡大縮小率 ) / 2f - 上移動,
                                                    w * 拡大縮小率,
                                                    h * 拡大縮小率 ) );
                                            }
                                            break;
                                    }
                                }
                            }
                            //----------------
                            #endregion
                        }

                        this._左サイドクリアパネル.クリアする();
                        this._左サイドクリアパネル.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                            this._プレイヤー名表示.進行描画する( dcp );
                            this._スコア表示.進行描画する( dcp, DXResources.Instance.アニメーション, new Vector2( +280f, +120f ), this.成績 );
                            this._達成率表示.描画する( dcp, (float) this.成績.Achievement );
                            this._判定パラメータ表示.描画する( dcp, +118f, +372f, this.成績 );
                            this._曲別SKILL.進行描画する( dcp, this.成績.Skill );
                        } );
                        this._左サイドクリアパネル.描画する();

                        this._右サイドクリアパネル.クリアする();
                        this._右サイドクリアパネル.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                            this._コンボ表示.進行描画する( dcp, DXResources.Instance.アニメーション, new Vector2( +228f + 264f / 2f, +234f ), this.成績 );
                        } );
                        this._右サイドクリアパネル.描画する();

                        this._レーンフレーム.描画する( dc, App進行描画.ユーザ管理.ログオン中のユーザ.レーンの透明度 );

                        this._レーンフラッシュ.進行描画する();

                        this._小節線拍線を描画する( dc, 演奏時刻sec );

                        this._背景画像.描画する( dc, 0f, 0f );

                        this._譜面スクロール速度.描画する( dc, App進行描画.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );

                        this._エキサイトゲージ.進行描画する( dc, this.成績.エキサイトゲージ量 );

                        double 曲の長さsec = App進行描画.演奏スコア.チップリスト[ App進行描画.演奏スコア.チップリスト.Count - 1 ].描画時刻sec;
                        float 現在位置 = (float) ( 1.0 - ( 曲の長さsec - 演奏時刻sec ) / 曲の長さsec );

                        this._カウントマップライン.カウント値を設定する( 現在位置, this.成績.判定toヒット数 );
                        this._カウントマップライン.進行描画する( dc );

                        this._フェーズパネル.現在位置 = 現在位置;
                        this._フェーズパネル.進行描画する( dc );

                        this._曲名パネル.描画する( dc );

                        this._ドラムキットとヒットバー.ヒットバーを進行描画する();
                        this._ドラムキットとヒットバー.ドラムキットを進行描画する();

                        this._描画範囲内のすべてのチップに対して( 演奏時刻sec, ( チップ chip, int index, double ヒット判定バーと描画との時間sec, double ヒット判定バーと発声との時間sec, double ヒット判定バーとの距離dpx ) => {

                            // クリア判定はこの中。
                            if( this._ドラムチップ.進行描画する(
                                this._レーンフレーム,
                                演奏時刻sec, ref this._描画開始チップ番号, this._チップの演奏状態[ chip ],
                                chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) )
                            {
                                this.現在のフェーズ = フェーズ.クリア;
                            }

                        } );

                        this._チップ光.進行描画する( dc );

                        this._判定文字列.進行描画する();

                        this._システム情報.VPSをカウントする();
                        this._システム情報.描画する( dc, $"BGMAdjust: {App進行描画.演奏曲ノード.BGMAdjust}" );

                        if( this.現在のフェーズ == フェーズ.キャンセル時フェードアウト )
                        {
                            App進行描画.アイキャッチ管理.現在のアイキャッチ.進行描画する( dc );

                            if( App進行描画.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
                            {
                                this.現在のフェーズ = フェーズ.キャンセル完了;
                            }
                        }
                    }
                    break;

                case フェーズ.キャンセル通知:
                    App進行描画.アイキャッチ管理.アイキャッチを選択しクローズする( nameof( 半回転黒フェード ) );
                    this.現在のフェーズ = フェーズ.キャンセル時フェードアウト;
                    break;
            }
        }


       
        // 画面を構成するもの


        private 画像 _背景画像 = null;

        private 画像 _スコア指定の背景画像 = null;

        private 曲名パネル _曲名パネル = null;

        private システム情報 _システム情報 = null;

        private レーンフレーム _レーンフレーム = null;

        private ドラムキットとヒットバー _ドラムキットとヒットバー = null;

        private ドラムチップ _ドラムチップ = null;

        private 譜面スクロール速度 _譜面スクロール速度 = null;

        private エキサイトゲージ _エキサイトゲージ = null;

        private フェーズパネル _フェーズパネル = null;

        private カウントマップライン _カウントマップライン = null;

        private 左サイドクリアパネル _左サイドクリアパネル = null;

        private 右サイドクリアパネル _右サイドクリアパネル = null;

        

        // 左サイドクリアパネル内に表示されるもの


        private スコア表示 _スコア表示 = null;

        private プレイヤー名表示 _プレイヤー名表示 = null;

        private 判定パラメータ表示 _判定パラメータ表示 = null;

        private 達成率表示 _達成率表示 = null;

        private 曲別SKILL _曲別SKILL = null;



        // 右サイドクリアパネル内に表示されるもの


        private コンボ表示 _コンボ表示 = null;



        // 譜面上に表示されるもの


        private レーンフラッシュ _レーンフラッシュ = null;

        private 判定文字列 _判定文字列 = null;

        private チップ光 _チップ光 = null;

        private 画像フォント _数字フォント中グレー48x64 = null;



        // 小節線・拍線


        private SolidColorBrush _小節線色 = null;

        private SolidColorBrush _小節線影色 = null;

        private SolidColorBrush _拍線色 = null;

        private void _小節線拍線を描画する( DeviceContext dc, double 現在の演奏時刻sec )
        {
            // 小節線・拍線 と チップ は描画階層（奥行き）が異なるので、別々のメソッドに分ける。

            DXResources.Instance.D2DBatchDraw( dc, () => {

                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

                    if( chip.チップ種別 == チップ種別.小節線 )
                    {
                        float 上位置dpx = (float) ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx - 1f );   // -1f は小節線の厚みの半分。

                        // 小節線
                        if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に小節線と拍線を表示する )
                        {
                            float x = 441f;
                            float w = 780f;
                            dc.DrawLine( new Vector2( x, 上位置dpx + 0f ), new Vector2( x + w, 上位置dpx + 0f ), this._小節線色 );
                            dc.DrawLine( new Vector2( x, 上位置dpx + 1f ), new Vector2( x + w, 上位置dpx + 1f ), this._小節線影色 );
                        }

                        // 小節番号
                        if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に小節番号を表示する )
                        {
                            float 右位置dpx = 441f + 780f - 24f;   // -24f は適当なマージン。
                            this._数字フォント中グレー48x64.描画する( dc, 右位置dpx, 上位置dpx - 84f, chip.小節番号.ToString(), 右揃え: true );    // -84f は適当なマージン。
                        }
                    }
                    else if( chip.チップ種別 == チップ種別.拍線 )
                    {
                        // 拍線
                        if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に小節線と拍線を表示する )
                        {
                            float 上位置dpx = (float) ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx - 1f );   // -1f は拍線の厚みの半分。
                            dc.DrawLine( new Vector2( 441f, 上位置dpx ), new Vector2( 441f + 780f, 上位置dpx ), this._拍線色, strokeWidth: 1f );
                        }
                    }

                } );

            } );
        }



        // 演奏状態


        /// <summary>
        ///		<see cref="スコア表示.チップリスト"/> のうち、描画を始めるチップのインデックス番号。
        ///		未演奏時・演奏終了時は -1 。
        /// </summary>
        /// <remarks>
        ///		演奏開始直後は 0 で始まり、対象番号のチップが描画範囲を流れ去るたびに +1 される。
        ///		このメンバの更新は、高頻度進行タスクではなく、進行描画メソッドで行う。（低精度で構わないので）
        /// </remarks>
        private int _描画開始チップ番号 = -1;

        private Dictionary<チップ, チップの演奏状態> _チップの演奏状態 = null;

        private double _演奏開始からの経過時間secを返す()
            => App進行描画.サウンドタイマ.現在時刻sec;



        // ステージ切り替え（特別にアイキャッチを使わないパターン）


        /// <summary>
        ///		読み込み画面: 0 ～ 1: 演奏画面
        /// </summary>
        private Counter _フェードインカウンタ = null;



        // private


        /// <summary>
        ///		<see cref="_描画開始チップ番号"/> から画面上端にはみ出すまでの間の各チップに対して、指定された処理を適用する。
        /// </summary>
        /// <param name="適用する処理">
        ///		引数は、順に、対象のチップ, チップ番号, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx。
        ///		時間と距離はいずれも、負数ならバー未達、0でバー直上、正数でバー通過。
        ///	</param>
        private void _描画範囲内のすべてのチップに対して( double 現在の演奏時刻sec, Action<チップ, int, double, double, double> 適用する処理 )
        {
            var スコア = App進行描画.演奏スコア;

            if( null == スコア )
                return;

            // 描画開始チップから後方のチップに向かって……

            for( int i = this._描画開始チップ番号; ( 0 <= i ) && ( i < スコア.チップリスト.Count ); i++ )
            {
                var チップ = スコア.チップリスト[ i ];

                // ヒット判定バーとチップの間の、時間 と 距離 を算出。→ いずれも、負数ならバー未達、0でバー直上、正数でバー通過。

                double ヒット判定バーと描画との時間sec = 現在の演奏時刻sec - チップ.描画時刻sec;
                double ヒット判定バーと発声との時間sec = 現在の演奏時刻sec - チップ.発声時刻sec;
                double 倍率 = this._譜面スクロール速度.補間付き速度;
                double ヒット判定バーとの距離dpx = this._指定された時間secに対応する符号付きピクセル数を返す( 倍率, ヒット判定バーと描画との時間sec );


                // 演奏終了？

                bool チップは画面上端より上に出ている = ( ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx ) < -40.0 );   // -40 はチップが隠れるであろう適当なマージン。

                if( チップは画面上端より上に出ている )
                    break;


                // 適用する処理を呼び出す。開始判定（描画開始チップ番号の更新）もこの中で。

                適用する処理( チップ, i, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx );
            }
        }

        private double _指定された時間secに対応する符号付きピクセル数を返す( double speed, double 指定時間sec )
        {
            const double _1秒あたりのピクセル数 = 0.14625 * 2.25 * 1000.0;    // これを変えると、speed あたりの速度が変わる。

            return ( 指定時間sec * _1秒あたりのピクセル数 * speed );
        }

        /// <summary>
        ///     入力とヒットしたと判定されたチップの処理（再生、判定、非表示化など）を行う。
        /// </summary>
        /// <param name="chip"></param>
        /// <param name="judge">ヒットしたチップの<see cref="判定種別"/>。</param>
        /// <param name="再生">チップを再生するならtrue。</param>
        /// <param name="判定">チップを判定するならtrue。</param>
        /// <param name="非表示">チップを非表示化するならtrue。</param>
        /// <param name="ヒット判定バーと発声との時間sec">負数でバー未達、0で直上、正数でバー通過。</param>
        /// <param name="入力とチップとの間隔sec">チップより入力が早ければ負数、遅ければ正数。</param>
        private void _チップのヒット処理を行う( チップ chip, 判定種別 judge, bool 再生, bool 判定, bool 非表示, double ヒット判定バーと発声との時間sec, double 入力とチップとの間隔sec )
        {
            this._チップの演奏状態[ chip ].ヒット済みである = true;

            if( 再生 && ( judge != 判定種別.MISS ) )
            {
                #region " チップの発声がまだなら行う。"
                //----------------
                // チップの発声時刻は、描画時刻と同じかそれより過去に位置するので、ここに来た時点で未発声なら発声していい。
                // というか発声時刻が過去なのに未発声というならここが最後のチャンスなので、必ず発声しないといけない。
                if( !( this._チップの演奏状態[ chip ].発声済みである ) )
                {
                    this.チップの発声を行う( chip, 再生 );
                    this._チップの演奏状態[ chip ].発声済みである = true;
                }
                //----------------
                #endregion
            }
            if( 判定 )
            {
                #region " チップの判定処理を行う。"
                //----------------
                var 対応表 = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ];

                if( judge != 判定種別.MISS )
                {
                    // 判定処理(1) チップ光アニメ開始
                    this._チップ光.表示を開始する( 対応表.表示レーン種別 );

                    // 判定処理(2) レーンフラッシュアニメ開始
                    this._レーンフラッシュ.開始する( 対応表.表示レーン種別 );
                }

                // 判定処理(4) 判定文字列アニメ開始
                this._判定文字列.表示を開始する( 対応表.表示レーン種別, judge, 入力とチップとの間隔sec );

                var ドラムチッププロパティ = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ];
                var AutoPlay = App進行描画.ユーザ管理.ログオン中のユーザ.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];

                // 判定処理(5) 成績更新
                this.成績.成績を更新する( judge, AutoPlay );
                //----------------
                #endregion
            }
            if( 非表示 )
            {
                #region " チップを非表示にする。"
                //----------------
                if( judge == 判定種別.MISS )
                {
                    // MISSチップは最後まで表示し続ける。
                }
                else
                {
                    // MISS以外なら、指定通り非表示に。
                    this._チップの演奏状態[ chip ].可視 = false;
                }
                //----------------
                #endregion
            }
        }

        private void _キャプチャ画面を描画する( DeviceContext dc, float 不透明度 = 1.0f )
        {
            Debug.Assert( null != this.キャプチャ画面, "キャプチャ画面が設定されていません。" );

            DXResources.Instance.D2DBatchDraw( dc, () => {

                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                dc.DrawBitmap(
                    this.キャプチャ画面,
                    new RectangleF( 0f, 0f, DXResources.Instance.設計画面サイズ.Width, DXResources.Instance.設計画面サイズ.Height ),
                    不透明度,
                    BitmapInterpolationMode.Linear );
            } );
        }

        private void _BGMAdjustをデータベースに保存する( MusicNode musicNode )
        {
            Task.Run( () => {

                using( var songdb = new SongDB() )
                {
                    var 曲レコード = songdb.Songs.Where( ( song ) => ( song.HashId == musicNode.曲ファイルハッシュ ) ).SingleOrDefault();

                    if( null != 曲レコード )
                    {
                        曲レコード.BGMAdjust = musicNode.BGMAdjust;  // 更新

                        songdb.DataContext.SubmitChanges(); // 更新完了
                    }
                }

            } );
        }


        private bool _一時停止中 = false;

        private void _演奏を一時停止または再開する()
        {
            if( !this._一時停止中 )
            {
                this._一時停止中 = true;

                App進行描画.サウンドタイマ.一時停止する();

                App進行描画.AVI管理?.再生中の動画をすべて一時停止する();
                App進行描画.WAV管理?.再生中の音声をすべて一時停止する();
            }
            else
            {
                this._一時停止中 = false;

                App進行描画.AVI管理?.一時停止中の動画をすべて再開する();
                App進行描画.WAV管理?.一時停止中の音声をすべて再開する();

                App進行描画.サウンドタイマ.再開する();
            }
        }
    }
}
