﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;

namespace FDK
{
    /// <summary>
    ///     動画ファイルのビデオストリームからフレームを生成するビデオソース。
    /// </summary>
    public class MediaFoundationFileVideoSource : IVideoSource
    {
        public double 総演奏時間sec { get; protected set; }

        public Size2F フレームサイズ { get; protected set; }

        public bool ループ再生する { get; set; } = false;

        public double 再生速度 { get; protected set; } = 1.0;


        public MediaFoundationFileVideoSource( VariablePath ファイルパス, double 再生速度 = 1.0 )
        {
            this.再生速度 = Math.Max( 0.01, Math.Min( 10.0, 再生速度 ) );

            #region " フレームキューを生成。"
            //----------------
            // キューサイズは3フレームとする。
            this._FrameQueue = new BlockingQueue<VideoFrame>( 3 );
            //----------------
            #endregion

            #region " ファイルから SourceReaderEx を生成する。"
            //----------------
            using( var ビデオ属性 = new MediaAttributes() )
            {
                // DXVAに対応しているGPUの場合には、それをデコードに利用するよう指定する。
                ビデオ属性.Set( SourceReaderAttributeKeys.D3DManager, グラフィックデバイス.Instance.MFDXGIDeviceManager );

                // 追加のビデオプロセッシングを有効にする。
                ビデオ属性.Set( SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true );  // 真偽値が bool だったり

                // 追加のビデオプロセッシングを有効にしたら、こちらは無効に。
                ビデオ属性.Set( SinkWriterAttributeKeys.ReadwriteDisableConverters, 0 );           // int だったり

                // 属性を使って、SourceReaderEx を生成。
                using( var sourceReader = new SourceReader( ファイルパス.変数なしパス, ビデオ属性 ) )    // パスは URI 扱い
                    this._SourceReaderEx = sourceReader.QueryInterface<SourceReaderEx>();
            }

            // 最初のビデオストリームだけを選択。
            this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.AllStreams, false );
            this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.FirstVideoStream, true );
            //----------------
            #endregion

            #region " ビデオの長さを取得する。"
            //----------------
            this.総演奏時間sec = 
                FDKUtilities.変換_100ns単位からsec単位へ(
                    this._SourceReaderEx.GetPresentationAttribute( SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration ) ) /
                    this.再生速度;
            //----------------
            #endregion

            #region " デコーダを選択し、完全メディアタイプを取得する。"
            //----------------
            // 部分メディアタイプを設定する。
            using( var videoMediaType = new MediaType() )
            {
                // フォーマットは ARGB32 で固定とする。（SourceReaderEx を使わない場合、H264 では ARGB32 が選べないので注意。）
                videoMediaType.Set( MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video );
                videoMediaType.Set( MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32 );

                // 部分メディアタイプを SourceReaderEx にセットする。SourceReaderEx は、必要なデコーダをロードするだろう。
                this._SourceReaderEx.SetCurrentMediaType( SourceReaderIndex.FirstVideoStream, videoMediaType );
            }

            // 完成されたメディアタイプを取得する。
            this._MediaType = this._SourceReaderEx.GetCurrentMediaType( SourceReaderIndex.FirstVideoStream );
            //----------------
            #endregion

            #region " ビデオのフレームサイズを取得する。"
            //----------------
            long packedFrameSize = this._MediaType.Get( MediaTypeAttributeKeys.FrameSize ); // 動画の途中でのサイズ変更には対応しない。
            this.フレームサイズ = new Size2F( ( packedFrameSize >> 32 ) & 0xFFFFFFFF, ( packedFrameSize ) & 0xFFFFFFFF );
            //----------------
            #endregion
        }

        public void Dispose()
        {
            // デコードを停止。
            this.Stop();

            this._デコードキャンセル?.Dispose();
            this._デコードキャンセル = null;

            this._FrameQueue?.Dispose();
            this._FrameQueue = null;

            this._MediaType?.Dispose();
            this._MediaType = null;

            this._SourceReaderEx?.Dispose();
            this._SourceReaderEx = null;
        }

        /// <summary>
        ///     指定した時刻からデコードを開始する。
        /// </summary>
        public void Start( double 再生開始時刻sec )
        {
            if( 再生開始時刻sec >= this.総演奏時間sec )
                return;

            this._デコードキャンセル = new CancellationTokenSource();
            this._デコード起動完了通知 = new ManualResetEventSlim( false );
            this._一時停止解除通知 = new ManualResetEventSlim( true );

            // (1) デコードタスク起動、デコード開始。
            this._デコードタスク = Task.Factory.StartNew(  // Task.Factory.StartNew は常に MTAThreaas
                this._デコードタスクエントリ, 
                (再生開始時刻sec, this.再生速度), 
                this._デコードキャンセル.Token );

            // (2) デコードから完了通知がくるまでブロック。
            this._デコード起動完了通知.Wait();
        }

        /// <summary>
        ///     デコードを終了する。
        /// </summary>
        public void Stop()
        {
            if( ( null != this._デコードタスク ) && !( this._デコードタスク.IsCompleted ) )   // デコードタスク起動中？
            {
                // (1) デコードタスクにキャンセルを通知。
                this._デコードキャンセル.Cancel();

                // (2) デコードタスクがキューでブロックしてたら解除する。
                this._FrameQueue.Cancel();

                // (3) デコードタスクが一時停止中でブロックしてたら解除する。
                this._一時停止解除通知.Set();

                // (4) デコードタスクが終了するまで待つ。
                if( !( this._デコードタスク.Wait( 5000 ) ) )   // 最大5秒
                    Log.ERROR( $"デコードタスクの終了がタイムアウトしました。" );

                this._デコードタスク = null;
            }
        }

        /// <summary>
        ///     デコードを一時停止する。
        /// </summary>
        public void Pause()
        {
            this._一時停止解除通知.Reset();
        }

        /// <summary>
        ///     デコードを再開する。
        ///     <see cref="Pause"/>で停止しているときのみ有効。
        /// </summary>
        public void Resume()
        {
            this._一時停止解除通知.Set();   // 解除
        }


        /// <summary>
        ///     次に読みだされるフレームがあれば、その表示予定時刻[100ns単位]を返す。
        ///     フレームがなければ、ブロックせずにすぐ 負数 を返す。
        /// </summary>
        public long Peek()
        {
            this._FrameQueue.Peek( out VideoFrame frame );
            return frame?.表示時刻100ns ?? -1;
        }

        /// <summary>
        ///     フレームを１つ読みだす。
        ///     再生中の場合、フレームが取得できるまでブロックする。
        ///     再生が終了している場合は null を返す。
        ///     取得したフレームは、使用が終わったら、呼び出し元で Dispose すること。
        /// </summary>
        public VideoFrame Read()
        {
            return this._FrameQueue.Take();
        }

        /// <summary>
        ///     MediaFoundation のビデオサンプルを D2DBitmap に変換して返す。
        /// </summary>
        public unsafe Bitmap サンプルからビットマップを取得する( Sample Sample )
        {
            // 1. Sample → MediaBuffer
            using( var mediaBuffer = Sample.ConvertToContiguousBuffer() )
            {
                // 2. MediaBuffer → DXGIBuffer
                using( var dxgiBuffer = mediaBuffer.QueryInterface<DXGIBuffer>() )
                {
                    // 3. DXGIBuffer → DXGISurface(IntPtr)
                    dxgiBuffer.GetResource( typeof( SharpDX.DXGI.Surface ).GUID, out IntPtr vDxgiSurface );

                    // 4.  DXGISurface(IntPtr) → DXGISurface
                    using( var dxgiSurface = new SharpDX.DXGI.Surface( vDxgiSurface ) )
                    {
                        // 5. DXGISurface → Bitmap
                        return new Bitmap(
                            グラフィックデバイス.Instance.既定のD2D1DeviceContext,
                            dxgiSurface,
                            new BitmapProperties( new PixelFormat( dxgiSurface.Description.Format, AlphaMode.Ignore ) ) );
                    }
                }
            }
        }


        /// <summary>
        ///     デコードされたビデオフレームを格納しておくキュー。
        /// </summary>
        private BlockingQueue<VideoFrame> _FrameQueue = null;

        private MediaType _MediaType = null;

        private SourceReaderEx _SourceReaderEx = null;


        // デコード関連

        private Task _デコードタスク = null;

        private CancellationTokenSource _デコードキャンセル = null;

        private ManualResetEventSlim _デコード起動完了通知 = null;

        private ManualResetEventSlim _一時停止解除通知 = null;


        private void _デコードタスクエントリ( object obj引数 )
        {
            Log.現在のスレッドに名前をつける( "ビデオデコード" );

            var 引数 = ((double 再生開始時刻sec, double 再生速度)) obj引数;

            double 再生速度 = Math.Max( 0.01, Math.Min( 10.0, 引数.再生速度 ) );
            double 再生開始時刻sec = Math.Max( 0.0, 引数.再生開始時刻sec ) / 再生速度;

            long 再生開始時刻100ns = FDKUtilities.変換_sec単位から100ns単位へ( 再生開始時刻sec );


            #region " 再生開始時刻までシーク(1)。"
            //----------------
            if( 0.0 < 再生開始時刻sec )
            {
                if( this.総演奏時間sec <= 再生開始時刻sec )
                {
                    Log.Info( $"再生開始時刻が総演奏時間を超えています。タスクを終了します。" );
                    return;
                }

                // 再生開始時刻 が 0 なら、これを呼び出さないこと（ガタつきの原因になるため）。
                this._SourceReaderEx.SetCurrentPosition( 再生開始時刻100ns );
            }
            //----------------
            #endregion

            // シーク(1)では、SetCurrentPosition() の仕様により、「指定された位置を超えない一番近いキーフレーム」までしか移動できない。
            // なので、残りのシーク（キーフレームから再生開始時刻まで）を、メインループ内で引き続き行う。
            // （残りのシークが終わって、ようやく デコード起動完了通知 がセットされる。）

            bool シーク中である = ( 0.0 < 再生開始時刻sec );

            if( !シーク中である)
                this._デコード起動完了通知.Set(); // シークはしない


            // メインループ。

            while( true )
            {
                // キャンセル通知があれば、ループを抜けてタスクを終了。
                if( this._デコードキャンセル.Token.IsCancellationRequested )
                {
                    Log.Info( $"キャンセル通知を受信しました。" );
                    break;
                }

                // 一時停止中なら、解除通知がくるまでここでタスクをブロック。
                this._一時停止解除通知.Wait();

                if( !this._サンプルをひとつデコードしてフレームをキューへ格納する( 再生速度 ) )
                    break;  // エラーまたは再生終了


                if( シーク中である )
                {
                    #region " 再生開始時刻までシーク(2)。"
                    //----------------
                    var frame = this._FrameQueue.Peek();    // 今格納されたフレームを覗く

                    if( frame.表示時刻100ns >= 再生開始時刻100ns )
                    {
                        // シーク終了；今回のフレームから再生の対象（なのでTakeしない）。
                        シーク中である = false;    

                        // シークが終わったので、呼び出し元に起動完了を通知する。
                        this._デコード起動完了通知.Set();
                    }
                    else
                    {
                        // 取り出して、すぐに破棄。
                        frame = this._FrameQueue.Take();
                        frame.Dispose();
                        frame = null;
                    }
                    //----------------
                    #endregion
                }
            }

            Log.Info( $"タスクを終了します。" );
        }

        /// <returns>
        ///		格納できた場合は true、エラーあるいは再生終了なら false。
        ///	</returns>
        private bool _サンプルをひとつデコードしてフレームをキューへ格納する( double 再生速度 )
        {
            // SourceReaderEx から次のサンプルをひとつデコードする。

            var サンプル = this._SourceReaderEx.ReadSample(
                SourceReaderIndex.FirstVideoStream,
                SourceReaderControlFlags.None,
                out int ストリーム番号,
                out var ストリームフラグ,
                out long サンプルの表示時刻100ns );

            if( ストリームフラグ.HasFlag( SourceReaderFlags.Endofstream ) )
            {
                Log.Info( $"ストリームが終了しました。" );

                if( this.ループ再生する )                                                       // ループするなら、
                {
                    Log.Info( $"ループ再生します。" );
                    this._SourceReaderEx.SetCurrentPosition( 0 );                               // 先頭に戻って
                    return _サンプルをひとつデコードしてフレームをキューへ格納する( 再生速度 ); // もう一回。
                }
                else
                {
                    return false;  // ループしないなら再生終了
                }
            }
            if( ストリームフラグ.HasFlag( SourceReaderFlags.Error ) ||
                ストリームフラグ.HasFlag( SourceReaderFlags.AllEffectsremoved ) ||
                //ストリームフラグ.HasFlag( SourceReaderFlags.Currentmediatypechanged ) ||
                ストリームフラグ.HasFlag( SourceReaderFlags.Nativemediatypechanged ) ||
                ストリームフラグ.HasFlag( SourceReaderFlags.Newstream ) ||
                ストリームフラグ.HasFlag( SourceReaderFlags.StreamTick ) )
            {
                Log.ERROR( $"デコード中にエラーが発生または未対応の状態変化が発生しました。" );
                return false;   // エラー
            }


            // ビデオフレームを生成してキューに格納する。
            this._FrameQueue.Add(   // キューがいっぱいなら、キューが空くまでブロックするので注意。
                new VideoFrame() {
                    Sample = サンプル,
                    Bitmap = this.サンプルからビットマップを取得する( サンプル ),
                    表示時刻100ns = (long)(サンプルの表示時刻100ns / 再生速度 ),
                } );

            return true;    // 格納した
        }
    }
}
