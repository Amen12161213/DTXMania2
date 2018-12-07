﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using DTXmatixx.設定;
using DTXmatixx.アイキャッチ;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.ステージ.結果
{
    class 結果ステージ : ステージ
    {
        public enum フェーズ
        {
            表示,
            フェードアウト,
            確定,
        }
        public フェーズ 現在のフェーズ
        {
            get;
            protected set;
        }

        // 外部依存アクション; ステージ管理クラスで接続。
        internal Func<成績> 結果を取得する = null;
        internal Action BGMを停止する = null;

        public 結果ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子を追加する( this._背景 = new 舞台画像() );
                this.子を追加する( this._曲名パネル = new 画像( @"$(System)images\結果\曲名パネル.png" ) );
                this.子を追加する( this._曲名画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 40f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 6f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                } );
                this.子を追加する( this._サブタイトル画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 25f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 5f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                } );
                this.子を追加する( this._演奏パラメータ結果 = new 演奏パラメータ結果() );
            }
        }
        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 選択曲 = App.曲ツリー.フォーカス曲ノード;
                Debug.Assert( null != 選択曲 );

                this._結果 = this.結果を取得する();

                // 成績をDBに記録。
                曲DB.成績を追加または更新する( this._結果, App.ユーザ管理.ログオン中のユーザ.ユーザID, 選択曲.曲ファイルハッシュ );

                this._曲名画像.表示文字列 = 選択曲.タイトル;
                this._サブタイトル画像.表示文字列 = 選択曲.サブタイトル;

                this._黒マスクブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, new Color4( Color3.Black, 0.75f ) );
                this._プレビュー枠ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, new Color4( 0xFF209292 ) );
                this.現在のフェーズ = フェーズ.表示;
                this._初めての進行描画 = true;
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._結果 = null;

                this._黒マスクブラシ?.Dispose();
                this._黒マスクブラシ = null;

                this._プレビュー枠ブラシ?.Dispose();
                this._プレビュー枠ブラシ = null;

                this.BGMを停止する();

                App.WAV管理?.Dispose();
                App.WAV管理 = null;
            }
        }
        public override void 進行描画する( DeviceContext1 dc )
        {
            if( this._初めての進行描画 )
            {
                this._背景.ぼかしと縮小を適用する( 0.0 );    // 即時適用
                this._初めての進行描画 = false;
            }

            this._背景.進行描画する( dc );
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.FillRectangle( new RectangleF( 0f, 36f, グラフィックデバイス.Instance.設計画面サイズ.Width, グラフィックデバイス.Instance.設計画面サイズ.Height - 72f ), this._黒マスクブラシ );
            } );
            this._プレビュー画像を描画する( dc );
            this._曲名パネル.描画する( dc, 660f, 796f );
            this._曲名を描画する( dc );
            this._サブタイトルを描画する( dc );
            this._演奏パラメータ結果.描画する( dc, 1317f, 716f, this._結果 );

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:
                    if( App.入力管理.確定キーが入力された() )
                    {
                        App.ステージ管理.アイキャッチを選択しクローズする( nameof( シャッター ) );
                        this.現在のフェーズ = フェーズ.フェードアウト;
                    }
                    break;

                case フェーズ.フェードアウト:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                    {
                        this.現在のフェーズ = フェーズ.確定;
                    }
                    break;

                case フェーズ.確定:
                    break;
            }
        }

        private bool _初めての進行描画 = true;
        private 成績 _結果 = null;
        private 舞台画像 _背景 = null;
        private 画像 _曲名パネル = null;
        private 文字列画像 _曲名画像 = null;
        private 文字列画像 _サブタイトル画像 = null;
        private 演奏パラメータ結果 _演奏パラメータ結果 = null;
        private SolidColorBrush _黒マスクブラシ = null;
        private SolidColorBrush _プレビュー枠ブラシ = null;
        private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 668f, 194f, 0f );
        private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 574f, 574f, 0f );

        private void _プレビュー画像を描画する( DeviceContext1 dc )
        {
            var 選択曲 = App.曲ツリー.フォーカス曲ノード;
            Debug.Assert( null != 選択曲 );

            var preimage = 選択曲.ノード画像 ?? Node.既定のノード画像;
            Debug.Assert( null != preimage );

            // 枠

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                const float 枠の太さdpx = 5f;
                dc.FillRectangle(
                    new RectangleF(
                        this._プレビュー画像表示位置dpx.X - 枠の太さdpx,
                        this._プレビュー画像表示位置dpx.Y - 枠の太さdpx,
                        this._プレビュー画像表示サイズdpx.X + 枠の太さdpx * 2f,
                        this._プレビュー画像表示サイズdpx.Y + 枠の太さdpx * 2f ),
                    this._プレビュー枠ブラシ );
            } );

            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
                -グラフィックデバイス.Instance.設計画面サイズ.Width / 2f,
                +グラフィックデバイス.Instance.設計画面サイズ.Height / 2f,
                0f );

            var 変換行列 =
                Matrix.Scaling( this._プレビュー画像表示サイズdpx ) *
                Matrix.Translation(
                    画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
                    画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
                    0f );

            preimage.描画する( 変換行列 );
        }
        private void _曲名を描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 690f, 820f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = 545f;

            this._曲名画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._曲名画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._曲名画像.画像サイズdpx.Width );
        }
        private void _サブタイトルを描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 690f, 820f + 60f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = 545f;

            this._サブタイトル画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._サブタイトル画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._サブタイトル画像.画像サイズdpx.Width );
        }
    }
}
