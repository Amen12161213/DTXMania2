﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using Newtonsoft.Json.Linq;
using FDK;
using FDK.入力;
using FDK.メディア;
using FDK.メディア.サウンド;
using FDK.同期;
using SSTFormat.v3;
using DTXmatixx.ステージ;
using DTXmatixx.曲;
using DTXmatixx.設定;
using DTXmatixx.入力;
using DTXmatixx.Viewer;

namespace DTXmatixx
{
    [ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]   // サービスインターフェースをシングルスレッドで呼び出す。
    class App : ApplicationForm, IDTXManiaService, IDisposable
    {
        public static int リリース番号
            => int.TryParse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ), out int release ) ? release : throw new Exception( "アセンブリのプロダクトバージョンに記載ミスがあります。" );
        public static T 属性<T>() where T : Attribute
            => (T) Attribute.GetCustomAttribute( Assembly.GetExecutingAssembly(), typeof( T ) );

        public static App Instance
        {
            get;
            protected set;
        } = null;
        /// <remarks>
        ///		SharpDX.Mathematics パッケージを参照し、かつ SharpDX 名前空間を using しておくと、
        ///		SharpDX で定義する追加の拡張メソッド（NextFloatなど）を使えるようになる。
        /// </remarks>
        public static Random 乱数
        {
            get;
            protected set;
        } = null;
        public static システム設定 システム設定
        {
            get;
            protected set;
        } = null;
        public static 入力管理 入力管理
        {
            get;
            set;
        } = null;
        public static ステージ管理 ステージ管理
        {
            get;
            protected set;
        } = null;
        public static 曲ツリー 曲ツリー
        {
            get;
            protected set;
        } = null;
        public static SoundDevice サウンドデバイス
        {
            get;
            protected set;
        } = null;
        public static SoundTimer サウンドタイマ
        {
            get;
            protected set;
        } = null;
        public static ドラムサウンド ドラムサウンド
        {
            get;
            protected set;
        } = null;
        public static ユーザ管理 ユーザ管理
        {
            get;
            protected set;
        } = null;

        public static スコア 演奏スコア
        {
            get;
            set;
        } = null;
        public static WAV管理 WAV管理
        {
            get;
            set;
        } = null;

        public App()
            : base( 設計画面サイズ: new SizeF( 1920f, 1080f ), 物理画面サイズ: new SizeF( 1280f, 720f ), 深度ステンシルを使う: false )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
#if DEBUG
                SharpDX.Configuration.EnableReleaseOnFinalizer = true;          // ファイナライザの実行中、未解放のCOMを見つけたら解放を試みる。
                SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = true;  // その際には Trace にメッセージを出力する。
#endif
                this.Text = Application.ProductName + " " + App.リリース番号.ToString( "000" );

                var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
                VariablePath.フォルダ変数を追加または更新する( "Exe", $@"{exePath}\" );
                VariablePath.フォルダ変数を追加または更新する( "System", Path.Combine( exePath, @"System\" ) );
                VariablePath.フォルダ変数を追加または更新する( "AppData", Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMatixx\" ) );

                if( !( Directory.Exists( VariablePath.フォルダ変数の内容を返す( "AppData" ) ) ) )
                    Directory.CreateDirectory( VariablePath.フォルダ変数の内容を返す( "AppData" ) );  // なければ作成。

                App.Instance = this;

                App.乱数 = new Random( DateTime.Now.Millisecond );

                App.システム設定 = システム設定.復元する();

                App.入力管理 = new 入力管理( this.Handle ) {
                    キーバインディングを取得する = () => App.システム設定.キーバインディング,
                    キーバインディングを保存する = () => App.システム設定.保存する(),
                };
                App.入力管理.初期化する();

                App.ステージ管理 = new ステージ管理();

                App.曲ツリー = new 曲ツリー();

                App.演奏スコア = null;

                App.WAV管理 = null;

                App.サウンドデバイス = new SoundDevice( CSCore.CoreAudioAPI.AudioClientShareMode.Shared );

                App.サウンドタイマ = new SoundTimer( App.サウンドデバイス );

                App.ドラムサウンド = new ドラムサウンド();

                App.ユーザ管理 = new ユーザ管理();
                App.ユーザ管理.ユーザリスト.SelectItem( ( user ) => ( user.ユーザID == "AutoPlayer" ) );  // ひとまずAutoPlayerを選択。

                this._活性化する();

                base.全画面モード = App.ユーザ管理.ログオン中のユーザ.全画面モードである;

                // 最初のステージへ遷移する。
                App.ステージ管理.ステージを遷移する( App.ステージ管理.最初のステージ名 );
            }
        }
        public new void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._非活性化する();

                App.ユーザ管理?.Dispose();
                App.ユーザ管理 = null;

                App.ドラムサウンド?.Dispose();
                App.ドラムサウンド = null;

                App.WAV管理?.Dispose();   // サウンドデバイスより先に開放すること
                App.WAV管理 = null;

                App.サウンドタイマ?.Dispose();
                App.サウンドタイマ = null;

                App.サウンドデバイス?.Dispose();
                App.サウンドデバイス = null;

                App.演奏スコア?.Dispose();
                App.演奏スコア = null;

                App.曲ツリー.Dispose();
                App.曲ツリー = null;

                App.ステージ管理.Dispose();
                App.ステージ管理 = null;

                App.入力管理.Dispose();
                App.入力管理 = null;

                App.システム設定.保存する();
                App.システム設定 = null;

                App.Instance = null;

                base.Dispose();
            }
        }
        public override void Run()
        {
            RenderLoop.Run( this, () => {

                if( this.FormWindowState == FormWindowState.Minimized )
                    return;

                switch( this._AppStatus )
                {
                    case AppStatus.開始:

                        // 高速進行タスク起動。
                        this._高速進行ステータス = new TriStateEvent( TriStateEvent.状態種別.OFF );
                        Task.Factory.StartNew( this._高速進行タスクエントリ );

                        // 描画タスク起動。
                        this._AppStatus = AppStatus.実行中;

                        break;

                    case AppStatus.実行中:
                        this._進行と描画を行う();
                        break;

                    case AppStatus.終了:
                        Thread.Sleep( 500 );    // 終了待機中。
                        break;
                }

            } );
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                lock( this._高速進行と描画の同期 )
                {
                    // 通常は進行タスクから終了するが、Alt+F4でここに来た場合はそれが行われてないので、行う。
                    if( this._AppStatus != AppStatus.終了 )
                    {
                        this._アプリを終了する();
                    }

                    base.OnClosing( e );
                }
            }
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            if( e.KeyCode == Keys.F11 )
            {
                this.全画面モード = !( this.全画面モード );
                App.ユーザ管理.ログオン中のユーザ.全画面モードである = this.全画面モード;
            }

            base.OnKeyDown( e );
        }

        #region " IDTXManiaService の実装 "
        //----------------
        // このアセンブリ（exe）は、WCF で IDTXManiaService を公開する。
        // ・このサービスインターフェースは、シングルスレッド（GUIスレッド）で同期実行される。（Appクラスの ServiceBehavior属性を参照。）
        // ・このサービスホストはシングルトンであり、すべてのクライアントセッションは同一（単一）のサービスインスタンスへ接続される。（Program.Main() を参照。）

        /// <summary>
        ///		曲を読み込み、演奏を開始する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        /// <param name="path">曲ファイルパス</param>
        /// <param name="startPart">演奏開始小節番号(0～)</param>
        /// <param name="drumsSound">ドラムチップ音を発声させるなら true。</param>
        public void ViewerPlay( string path, int startPart = 0, bool drumsSound = true )
        {
            // TODO: ViewerPlay メソッドを実装する。
            throw new NotImplementedException();
        }

        /// <summary>
        ///		現在の演奏を停止する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        public void ViewerStop()
        {
            // TODO: ViewerStop メソッドを実装する。
            throw new NotImplementedException();
        }

        /// <summary>
        ///		サウンドデバイスの発声遅延[ms]を返す。
        /// </summary>
        /// <returns>遅延量[ms]</returns>
        public float GetSoundDelay()
            => (float) ( App.サウンドデバイス?.再生遅延sec ?? 0.0 ) * 1000.0f;
        //----------------
        #endregion

        // ※ Form イベントの override メソッドは描画スレッドで実行されるため、処理中に進行タスクが呼び出されると困る場合には、進行タスクとの lock を忘れないこと。
        private readonly object _高速進行と描画の同期 = new object();

        private enum AppStatus { 開始, 実行中, 終了 };
        private AppStatus _AppStatus = AppStatus.開始;

        /// <summary>
        ///		進行タスクの状態。
        ///		OFF:タスク起動前、ON:タスク実行中、無効:タスク終了済み
        /// </summary>
        private TriStateEvent _高速進行ステータス;

        /// <summary>
        ///		グローバルリソースのうち、グラフィックリソースを持つものについて、活性化がまだなら活性化する。
        /// </summary>
        private void _活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.ステージ管理.活性化する();
                App.曲ツリー.活性化する();
            }
        }

        /// <summary>
        ///		グローバルリソースのうち、グラフィックリソースを持つものについて、活性化中なら非活性化する。
        /// </summary>
        private void _非活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.ステージ管理.非活性化する();
                App.曲ツリー.非活性化する();
            }
        }

        /// <summary>
        ///		高速進行ループの処理内容。
        /// </summary>
        private void _高速進行タスクエントリ()
        {
            Log.現在のスレッドに名前をつける( "高速進行" );
            Log.Header( "高速進行タスクを開始します。" );

            this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.ON;

            while( true )
            {
                lock( this._高速進行と描画の同期 )
                {
                    if( this._高速進行ステータス.現在の状態 != TriStateEvent.状態種別.ON )    // lock してる間に状態が変わることがあるので注意。
                        break;

                    //App.入力管理.すべての入力デバイスをポーリングする();
                    // --> 入力ポーリングの挙動はステージごとに異なるので、それぞれのステージ内で行う。

                    App.ステージ管理.現在のステージ.高速進行する();
                }

                Thread.Sleep( App.システム設定.入力発声スレッドのスリープ量ms );  // ウェイト。
            }

            this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.無効;

            Log.Header( "高速進行タスクを終了しました。" );
        }

        /// <summary>
        ///		進行描画ループの処理内容。
        /// </summary>
        private void _進行と描画を行う()
        {
            bool vsync = true;

            lock( this._高速進行と描画の同期 )
            {
                if( this._AppStatus != AppStatus.実行中 )  // 上記lock中に終了されている場合があればそれをはじく。
                    return;

                グラフィックデバイス.Instance.D3DDeviceを取得する( ( d3dDevice ) => {

                    #region " D2D, D3Dレンダリングの前処理を行う。"
                    //----------------
                    グラフィックデバイス.Instance.D2DDeviceContext.Transform = グラフィックデバイス.Instance.拡大行列DPXtoPX;
                    グラフィックデバイス.Instance.D2DDeviceContext.PrimitiveBlend = SharpDX.Direct2D1.PrimitiveBlend.SourceOver;

                    // 既定のD3Dレンダーターゲットビューを黒でクリアする。
                    d3dDevice.ImmediateContext.ClearRenderTargetView( グラフィックデバイス.Instance.D3DRenderTargetView, Color4.Black );

                    // 深度バッファを 1.0f でクリアする。
                    d3dDevice.ImmediateContext.ClearDepthStencilView(
                        グラフィックデバイス.Instance.D3DDepthStencilView,
                        SharpDX.Direct3D11.DepthStencilClearFlags.Depth,
                        depth: 1.0f,
                        stencil: 0 );
                    //----------------
                    #endregion

                    // Windows Animation を進行。
                    グラフィックデバイス.Instance.Animation.進行する();

                    // 現在のステージを進行＆描画。
                    App.ステージ管理.現在のステージ.進行描画する( グラフィックデバイス.Instance.D2DDeviceContext );

                    // 現在のUIツリーを描画する。
                    グラフィックデバイス.Instance.UIFramework.描画する( グラフィックデバイス.Instance.D2DDeviceContext );

                    // ステージの進行描画の結果（フェーズの状態など）を受けての後処理。
                    switch( App.ステージ管理.現在のステージ )
                    {
                        case ステージ.曲ツリー構築.曲ツリー構築ステージ stage:
                            #region " 確定 → タイトルステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.曲ツリー構築.曲ツリー構築ステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.タイトル.タイトルステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.タイトル.タイトルステージ stage:
                            #region " キャンセル → 終了ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.キャンセル )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.終了.終了ステージ ) );
                            }
                            //----------------
                            #endregion
                            #region " 確定 → 認証ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.認証.認証ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.認証.認証ステージ stage:
                            #region " キャンセル → 終了ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.キャンセル )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.終了.終了ステージ ) );
                            }
                            //----------------
                            #endregion
                            #region " 確定 → 選曲ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.選曲.選曲ステージ stage:
                            #region " キャンセル → タイトルステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.キャンセル )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.タイトル.タイトルステージ ) );
                            }
                            //----------------
                            #endregion
                            #region " 確定_選曲 → 曲読み込みステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.確定_選曲 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.曲読み込み.曲読み込みステージ ) );
                            }
                            //----------------
                            #endregion
                            #region " 確定_設定 → 設定ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.確定_設定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.オプション設定.オプション設定ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.オプション設定.オプション設定ステージ stage:
                            #region " キャンセル, 確定 → 選曲ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.オプション設定.オプション設定ステージ.フェーズ.キャンセル ||
                                stage.現在のフェーズ == ステージ.オプション設定.オプション設定ステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.曲読み込み.曲読み込みステージ stage:
                            #region " 確定 → 演奏ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.曲読み込み.曲読み込みステージ.フェーズ.完了 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.演奏.演奏ステージ ) );

                                // 曲読み込みステージ画面をキャプチャする（演奏ステージのクロスフェードで使う）
                                var 演奏ステージ = App.ステージ管理.ステージリスト[ nameof( ステージ.演奏.演奏ステージ ) ] as ステージ.演奏.演奏ステージ;
                                演奏ステージ.キャプチャ画面 = 画面キャプチャ.取得する();
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.演奏.演奏ステージ stage:
                            #region " キャンセル → 選曲ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.キャンセル完了 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                            }
                            //----------------
                            #endregion
                            #region " クリア → 結果ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.クリア )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.結果.結果ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.結果.結果ステージ stage:
                            #region " 確定 → 選曲ステージへ "
                            //----------------
                            if( stage.現在のフェーズ == ステージ.結果.結果ステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                            }
                            //----------------
                            #endregion
                            break;

                        case ステージ.終了.終了ステージ stage:
                            #region " 確定 → アプリを終了する。"
                            //----------------
                            if( stage.現在のフェーズ == ステージ.終了.終了ステージ.フェーズ.確定 )
                            {
                                App.ステージ管理.ステージを遷移する( null );
                                this._アプリを終了する();
                            }
                            //----------------
                            #endregion
                            break;
                    }

                    // コマンドフラッシュ。
                    if( vsync )
                        d3dDevice.ImmediateContext.Flush();
                } );
            }

            // スワップチェーン表示。
            グラフィックデバイス.Instance.SwapChain.Present( ( vsync ) ? 1 : 0, SharpDX.DXGI.PresentFlags.None );
        }

        /// <summary>
        ///		進行タスクを終了し、ウィンドウを閉じ、アプリを終了する。
        /// </summary>
        private void _アプリを終了する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( this._AppStatus != AppStatus.終了 )
                {
                    this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.OFF;

                    // ユーザ情報を保存する。
                    App.ユーザ管理.ログオン中のユーザ?.保存する();

                    // _AppStatus を変更してから、GUI スレッドで非同期実行を指示する。
                    this._AppStatus = AppStatus.終了;
                    this.BeginInvoke( new Action( () => { this.Close(); } ) );
                }
            }
        }
    }
}
