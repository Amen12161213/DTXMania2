﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using SSTFormat.v3;

namespace DTXmatixx.ステージ.曲読み込み
{
    class 曲読み込みステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            完了,
            キャンセル,
        }
        public フェーズ 現在のフェーズ
        {
            get;
            protected set;
        }

        public 曲読み込みステージ()
        {
            this.子を追加する( this._舞台画像 = new 舞台画像() );
            this.子を追加する( this._注意文 = new 画像( @"$(System)images\ご注意ください.png" ) );
            this.子を追加する( this._曲名画像 = new 文字列画像() {
                フォント名 = "HGMaruGothicMPRO",
                フォントサイズpt = 70f,
                フォント幅 = FontWeight.Regular,
                フォントスタイル = FontStyle.Normal,
                描画効果 = 文字列画像.効果.縁取り,
                縁のサイズdpx = 10f,
                前景色 = Color4.Black,
                背景色 = Color4.White,
            } );
            this.子を追加する( this._サブタイトル画像 = new 文字列画像() {
                フォント名 = "HGMaruGothicMPRO",
                フォントサイズpt = 45f,
                フォント幅 = FontWeight.Regular,
                フォントスタイル = FontStyle.Normal,
                描画効果 = 文字列画像.効果.縁取り,
                縁のサイズdpx = 7f,
                前景色 = Color4.Black,
                背景色 = Color4.White,
            } );
            this.子を追加する( this._プレビュー画像 = new プレビュー画像() );
            this.子を追加する( this._難易度 = new 難易度() );
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 選択曲 = App.曲ツリー.フォーカス曲ノード;
                Debug.Assert( null != 選択曲 );

                this._曲名画像.表示文字列 = 選択曲.タイトル;
                this._サブタイトル画像.表示文字列 = 選択曲.サブタイトル;

                this.現在のフェーズ = フェーズ.フェードイン;
                this._初めての進行描画 = true;
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            if( this._初めての進行描画 )
            {
                this._舞台画像.ぼかしと縮小を適用する( 0.0 );
                App.ステージ管理.現在のアイキャッチ.オープンする();
                this._初めての進行描画 = false;
            }

            this._舞台画像.進行描画する( dc );
            this._注意文.描画する( dc, 0f, 760f );
            this._プレビュー画像.描画する( dc );
            this._難易度.描画する( dc );
            this._曲名を描画する( dc );
            this._サブタイトルを描画する( dc );

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
                    {
                        this.現在のフェーズ = フェーズ.表示;
                    }
                    break;

                case フェーズ.表示:
                    this._スコアを読み込む();
                    App.入力管理.すべての入力デバイスをポーリングする();  // 先行入力があったらここでキャンセル
                    this.現在のフェーズ = フェーズ.完了;
                    break;

                case フェーズ.完了:
                case フェーズ.キャンセル:
                    break;
            }
        }

        private bool _初めての進行描画 = true;
        private 舞台画像 _舞台画像 = null;
        private 画像 _注意文 = null;
        private 文字列画像 _曲名画像 = null;
        private 文字列画像 _サブタイトル画像 = null;
        private プレビュー画像 _プレビュー画像 = null;
        private 難易度 _難易度 = null;

        private void _曲名を描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 782f, 409f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = グラフィックデバイス.Instance.設計画面サイズ.Width - 表示位置dpx.X;

            this._曲名画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._曲名画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._曲名画像.画像サイズdpx.Width );
        }
        private void _サブタイトルを描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 782f, 520f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = グラフィックデバイス.Instance.設計画面サイズ.Width - 表示位置dpx.X;

            this._サブタイトル画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._サブタイトル画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._サブタイトル画像.画像サイズdpx.Width );
        }
        private void _スコアを読み込む()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 曲ファイルを読み込む。

                var 選択曲 = App.曲ツリー.フォーカス曲ノード;
                Debug.Assert( null != 選択曲 );

                var 選択曲ファイルパス = 選択曲.曲ファイルパス;
                Debug.Assert( 選択曲ファイルパス?.変数なしパス.Nullでも空でもない() ?? false );

                var 拡張子名 = Path.GetExtension( 選択曲ファイルパス.変数なしパス );

                if( ".sstf" == 拡張子名 )
                {
                    App.演奏スコア = new スコア( 選択曲ファイルパス.変数なしパス );
                }
                else if( ".dtx" == 拡張子名 )
                {
                    App.演奏スコア = DTXReader.ReadFromFile( 選択曲ファイルパス.変数なしパス );
                }
                else
                {
                    throw new Exception( $"未対応のフォーマットファイルです。[{選択曲ファイルパス.変数付きパス}]" );
                }

                // 全チップの発声時刻を修正する。

                //long 早める時間ms = (long) ( App.サウンドデバイス.再生遅延sec * 1000 - App.演奏スコア.サウンドデバイス遅延ms );
                long 早める時間ms = (long) ( App.サウンドデバイス.再生遅延sec * 1000 );  // SoundDevice.Delay 不要論

                foreach( var chip in App.演奏スコア.チップリスト )
                {
                    chip.発声時刻ms -= (long) 早める時間ms;
                }

                // 完了。

                Log.Info( $"曲ファイルを読み込みました。" );
                Log.Info( $"曲名: {App.演奏スコア.曲名}" );
            }
        }
    }
}
