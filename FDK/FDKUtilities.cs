﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml;
using SharpDX;

namespace FDK
{
    public static class FDKUtilities
    {
        // 数学、計算

        /// <summary>
        ///		深度から射影行列（定数）を計算して返す。
        ///		Direct2D 用。
        /// </summary>
        public static Matrix D2DPerspectiveProjection( float depth )
        {
            var mat = Matrix.Identity;
            mat.M34 = ( 0 != depth ) ? -( 1.0f / depth ) : 0.0f;
            return mat;
        }

        public static int 最大公約数を返す( int m, int n )
        {
            if( ( 0 > m ) || ( 0 > n ) )
                throw new Exception( "引数に負数は指定できません。" );

            // 片方が 0 ならもう片方を返す。
            if( 0 == m ) return n;
            if( 0 == n ) return m;

            // ユーグリッドの互除法
            int r;
            while( ( r = m % n ) != 0 )
            {
                m = n;
                n = r;
            }

            return n;
        }
        public static int 最小公倍数を返す( int m, int n )
        {
            if( ( 0 >= m ) || ( 0 >= n ) )
                throw new Exception( "引数に0以下の数は指定できません。" );

            return ( m * n / FDKUtilities.最大公約数を返す( m, n ) );
        }

        public static (int 分子, int 分母) 約分する( int 分子, int 分母 )
        {
            if( 0 == 分子 )
                return (0, 1);

            int 最大公約数 = 1;

            while( 1 != ( 最大公約数 = FDKUtilities.最大公約数を返す( 分子, 分母 ) ) )
            {
                分子 /= 最大公約数;
                分母 /= 最大公約数;
            }

            return (分子, 分母);
        }

        public static double 変換_100ns単位からsec単位へ( long 数値100ns )
        {
            return 数値100ns / 10_000_000.0;
        }
        public static long 変換_sec単位から100ns単位へ( double 数値sec )
        {
            return (long) ( 数値sec * 10_000_000.0 + 0.5 ); // +0.5 で四捨五入できる。
        }

        public static float 変換_pt単位からpx単位へ( float dpi, float 数値pt )
        {
            return dpi * 数値pt / 72f;
        }
        public static float 変換_px単位からpt単位へ( float dpi, float 数値px )
        {
            return 72f * 数値px / dpi;
        }


        /// <summary>
        ///		指定された位置を、それを超えないブロック境界に揃えて返す。
        /// </summary>
        /// <param name="position">位置[byte]。</param>
        /// <param name="blockAlign">ブロック境界[byte]。</param>
        /// <returns></returns>
        public static int 位置をブロック境界単位にそろえて返す( int position, int blockAlign )
        {
            return ( position - ( position % blockAlign ) );
        }

        /// <summary>
        ///		指定された位置を、それを超えないブロック境界に揃えて返す。
        /// </summary>
        /// <param name="position">位置[byte]。</param>
        /// <param name="blockAlign">ブロック境界[byte]。</param>
        /// <returns></returns>
        public static long 位置をブロック境界単位にそろえて返す( long position, long blockAlign )
        {
            return ( position - ( position % blockAlign ) );
        }


        // グラフィック

        /// <summary>
        ///		画像ファイルからシェーダリソースビューを作成して返す。
        /// </summary>
        /// <remarks>
        ///		（参考: http://qiita.com/oguna/items/c516e09ee57d931892b6 ）
        /// </remarks>
        public static (SharpDX.Direct3D11.ShaderResourceView srv, Size2F viewSize, SharpDX.Direct3D11.Texture2D texture) CreateShaderResourceViewFromFile(
            SharpDX.Direct3D11.Device d3dDevice,
            SharpDX.Direct3D11.BindFlags bindFlags,
            VariablePath 画像ファイルパス )
        {
            var 出力 = (
                srv: (SharpDX.Direct3D11.ShaderResourceView) null,
                viewSize: new Size2F( 0, 0 ),
                texture: (SharpDX.Direct3D11.Texture2D) null);

            using( var image = new System.Drawing.Bitmap( 画像ファイルパス.変数なしパス ) )
            {
                var 画像の矩形 = new System.Drawing.Rectangle( 0, 0, image.Width, image.Height );

                using( var bitmap = image.Clone( 画像の矩形, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) )
                {
                    var ロック領域 = bitmap.LockBits( 画像の矩形, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat );
                    var dataBox = new[] { new DataBox( ロック領域.Scan0, bitmap.Width * 4, bitmap.Height ) };
                    var textureDesc = new SharpDX.Direct3D11.Texture2DDescription() {
                        ArraySize = 1,
                        BindFlags = bindFlags,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Height = bitmap.Height,
                        Width = bitmap.Width,
                        MipLevels = 1,
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                        SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default
                    };
                    var texture = new SharpDX.Direct3D11.Texture2D( d3dDevice, textureDesc, dataBox );
                    bitmap.UnlockBits( ロック領域 );
                    出力.srv = new SharpDX.Direct3D11.ShaderResourceView( d3dDevice, texture );
                    出力.texture = texture;
                }

                出力.viewSize = new Size2F( 画像の矩形.Width, 画像の矩形.Height );
            }

            return 出力;
        }

        /// <summary>
        ///		空のテクスチャとそのシェーダーリソースビューを作成し、返す。
        /// </summary>
        public static (SharpDX.Direct3D11.ShaderResourceView srv, SharpDX.Direct3D11.Texture2D texture) CreateShaderResourceView(
            SharpDX.Direct3D11.Device d3dDevice,
            SharpDX.Direct3D11.BindFlags bindFlags,
            Size2 サイズ )
        {
            var textureDesc = new SharpDX.Direct3D11.Texture2DDescription() {
                ArraySize = 1,
                BindFlags = bindFlags,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Height = サイズ.Height,
                Width = サイズ.Width,
                MipLevels = 1,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default
            };

            var 出力 = (srv: (SharpDX.Direct3D11.ShaderResourceView) null, texture: (SharpDX.Direct3D11.Texture2D) null);
            出力.texture = new SharpDX.Direct3D11.Texture2D( d3dDevice, textureDesc );
            出力.srv = new SharpDX.Direct3D11.ShaderResourceView( d3dDevice, 出力.texture );

            return 出力;
        }


        // デバッグ

        /// <summary>
        ///		このメソッドの 呼び出し元のメソッド名 を返す。デバッグログ用。
        /// </summary>
        public static string 現在のメソッド名
        {
            get
            {
                // 1つ前のスタックフレームを取得。
                var prevFrame = new StackFrame( skipFrames: 1, fNeedFileInfo: false );

                var クラス名 = prevFrame.GetMethod().ReflectedType.ToString();
                var メソッド名 = prevFrame.GetMethod().Name;

                return $"{クラス名}.{メソッド名}()";
            }
        }


        // シリアライズ

        /// <summary>
        ///		DataContract オブジェクトをシリアル化してファイルに保存する。
        /// </summary>
        public static void 保存する( object 保存するオブジェクト, VariablePath ファイルパス, bool UseSimpleDictionaryFormat = true )
        {
            using( var stream = File.Open( ファイルパス.変数なしパス, FileMode.Create ) )
            {
                var culture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                    using( var writer = JsonReaderWriterFactory.CreateJsonWriter( stream, Encoding.UTF8, true, true ) )
                    {
                        var serializer = new DataContractJsonSerializer( 保存するオブジェクト.GetType(), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = UseSimpleDictionaryFormat } );
                        serializer.WriteObject( writer, 保存するオブジェクト );
                        writer.Flush();
                    }
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                }
            }
        }

        /// <summary>
        ///		ファイルを逆シリアル化して、DataContract オブジェクトを生成する。
        /// </summary>
        /// <remarks>
        ///		DataContract 属性を持つ型の逆シリアル時には、コンストラクタは呼び出されないので注意。
        ///		各メンバは 0 または null になるが、それがイヤなら OnDeserializing コールバックを用意して、逆シリアル化の前に初期化すること。
        /// </remarks>
        public static T 復元する<T>( VariablePath ファイルパス, bool UseSimpleDictionaryFormat = true ) where T : class, new()
        {
            T dataContract;

            using( var stream = File.OpenRead( ファイルパス.変数なしパス ) )
            {
                // BOM (0xEF,0xBB,0xBFの3バイト）があれば飛ばす。
                stream.Position = ( 0xEF == stream.ReadByte() ) ? 3 : 0;

                var culture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    var serialier = new DataContractJsonSerializer( typeof( T ), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = UseSimpleDictionaryFormat } );
                    dataContract = (T) serialier.ReadObject( stream );
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                }
            }

            return dataContract;
        }

        /// <summary>
        ///     JToken に含まれる４つの要素から矩形を抽出して返す。
        /// </summary>
        public static SharpDX.RectangleF JsonToRectangleF( Newtonsoft.Json.Linq.JToken jTokens )
        {
            return new SharpDX.RectangleF(
                (float) jTokens[ 0 ],        // x
                (float) jTokens[ 1 ],        // y
                (float) jTokens[ 2 ],        // w
                (float) jTokens[ 3 ] );      // h
        }
    }
}
