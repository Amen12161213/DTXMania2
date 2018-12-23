﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using SSTFormatCurrent = SSTFormat.v4;
using DTXMania.設定;

using Record = DTXMania.データベース.ユーザ.Record06;

namespace DTXMania.ステージ.演奏
{
    /// <summary>
    ///		現在の演奏状態や成績を保存するクラス。
    ///		描画は行わない。
    /// </summary>
    internal class 成績 : Record
    {
        public int Combo { get; protected set; } = 0;

        public int MaxCombo { get; protected set; } = 0;
        
        /// <summary>
        ///		0.0で0%、1.0で100%。
        /// </summary>
        public float エキサイトゲージ量 { get; protected set; }
        
        /// <summary>
        ///		現在の設定において、ヒット対象になるノーツの数を返す。
        /// </summary>
        public int 総ノーツ数 { get; protected set; } = 0;
        
        /// <summary>
        ///		判定種別ごとのヒット数。
        /// </summary>
        public IReadOnlyDictionary<判定種別, int> 判定toヒット数 => this._判定toヒット数;
        
        /// <summary>
        ///		現在の <see cref="判定toヒット数"/> から、判定種別ごとのヒット割合を算出して返す。
        ///		判定種別のヒット割合は、すべて合計すればちょうど 100 になる。
        /// </summary>
        public IReadOnlyDictionary<判定種別, int> 判定toヒット割合 => this._ヒット割合を算出して返す();


        public 成績()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.Score = 0;
                this.MaxCombo = 0;
                this.エキサイトゲージ量 = 0.75f;
                this.Achievement = 0.0f;
                this.総ノーツ数 = 0;

                this._判定toヒット数 = new Dictionary<判定種別, int>();
                this._最後にスコアを更新したときの判定toヒット数 = new Dictionary<判定種別, int>();
                foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
                {
                    this._判定toヒット数.Add( judge, 0 );
                    this._最後にスコアを更新したときの判定toヒット数.Add( judge, 0 );
                }

                // TODO: AutoPlay の状態から、達成率用のオプション補正を算出。
                this._オプション補正 = 1.0;

                this._譜面レベル = 0.5;
            }
        }

        public void スコアと設定を反映する( SSTFormatCurrent.スコア 譜面, ユーザ設定 設定 )
        {
            this.総ノーツ数 = ( null != 譜面 && null != 設定 ) ? this._総ノーツ数を算出して返す( 譜面, 設定 ) : 0;
            this._譜面レベル = 譜面?.難易度 ?? 0.5;
        }

        /// <summary>
        ///		ヒット数を加算し、各プロパティを更新する。
        /// </summary>
        public void ヒット数を加算する( 判定種別 judge, int 加算値 = 1 )
        {
            // (1) ヒット数を加算する。
            this._判定toヒット数[ judge ] += 加算値;

            // (2) コンボを加算する。
            {
                bool コンボ切れ = ( judge == 判定種別.OK || judge == 判定種別.MISS );

                if( コンボ切れ )
                {
                    this.Combo = 0;
                }
                else
                {
                    this.Combo++;
                    this.MaxCombo = Math.Max( this.Combo, this.MaxCombo );
                }
            }

            // (3) スコアを加算する。
            {
                double 基礎点 = 1000000.0 / ( 1275.0 + 50.0 * ( this.総ノーツ数 - 50 ) );
                int コンボ数 = Math.Min( this.Combo, 50 );

                this.Score += (int) Math.Floor( 基礎点 * コンボ数 * this._判定値表[ judge ] );
            }

            // (4) 達成率を更新する。
            {
                double 判定値 = Math.Floor( 100.0 * ( ( this._判定toヒット数[ 判定種別.PERFECT ] * 85.0 + this._判定toヒット数[ 判定種別.GREAT ] * 35.0 ) / this.総ノーツ数 ) ) / 100.0;    // 小数第3位以下切り捨て
                double フレーズコンボ成功率 = 0.0;    // 未対応
                double COMBO値 = Math.Floor( 100.0 * ( ( this.MaxCombo * 5.0 / this.総ノーツ数 ) + ( フレーズコンボ成功率 * 10.0 ) ) ) / 100.0; // 小数第3位以下切り捨て

                this.Achievement = (float) ( Math.Floor( 100.0 * ( ( 判定値 + COMBO値 ) * this._オプション補正 ) ) / 100.0 );    // 小数第3位以下切り捨て
            }

            // (5) SKILL値を更新する。
            this.Skill = (float) ( Math.Floor( 100.0 * ( ( this.Achievement * this._譜面レベル * 20.0 ) / 100.0 ) ) / 100.0 );       // 小数第3位以下切り捨て

            // (6) エキサイトゲージ → ここからは呼び出さない。（AutoPlayと区別がつかないため）
        }

        /// <summary>
        ///		判定に応じてエキサイトゲージを加減する。
        /// </summary>
        public void エキサイトゲージを加算する( 判定種別 judge )
        {
            switch( judge )
            {
                case 判定種別.PERFECT: this.エキサイトゲージ量 += 0.025f; break;
                case 判定種別.GREAT: this.エキサイトゲージ量 += 0.01f; break;
                case 判定種別.GOOD: this.エキサイトゲージ量 += 0.005f; break;
                case 判定種別.OK: this.エキサイトゲージ量 += 0f; break;
                case 判定種別.MISS: this.エキサイトゲージ量 -= 0.08f; break;
            }

            this.エキサイトゲージ量 = Math.Max( Math.Min( this.エキサイトゲージ量, 1.0f ), 0.0f );
        }


        private Dictionary<判定種別, int> _判定toヒット数 = null;
        private Dictionary<判定種別, int> _最後にスコアを更新したときの判定toヒット数 = null;
        private readonly Dictionary<判定種別, double> _判定値表 = new Dictionary<判定種別, double>() {
            { 判定種別.PERFECT, 1.0 },
            { 判定種別.GREAT,   0.5 },
            { 判定種別.GOOD,    0.2 },
            { 判定種別.OK,      0.0 },
            { 判定種別.MISS,    0.0 },
        };
        private double _オプション補正 = 1.0;
        private double _譜面レベル = 5.0;


        private IReadOnlyDictionary<判定種別, int> _ヒット割合を算出して返す()
        {
            // HACK: ヒット割合の計算式は、本家とは一致していない。

            int ヒット数の合計 = 0;
            var ヒット割合_実数 = new Dictionary<判定種別, double>();  // 実値（0～100）
            var ヒット割合_整数 = new Dictionary<判定種別, int>(); // 実値を整数にしてさらに補正した値（0～100）
            var ヒット数リスト = new List<(判定種別 judge, int hits)>();
            var 切り捨てした = new Dictionary<判定種別, bool>();
            判定種別 判定;

            // ヒット数の合計を算出。
            foreach( var kvp in this._判定toヒット数 )
                ヒット数の合計 += kvp.Value;

            // 各判定のヒット割合（実数）を算出。
            foreach( var kvp in this._判定toヒット数 )
            {
                ヒット割合_実数.Add( kvp.Key, ( 100.0 * kvp.Value ) / ヒット数の合計 );
                切り捨てした.Add( kvp.Key, false );   // ついでに初期化。
            }

            // ヒット数の大きいもの順（降順）に、リストを作成。
            foreach( 判定種別 j in Enum.GetValues( typeof( 判定種別 ) ) )
                ヒット数リスト.Add( (j, this.判定toヒット数[ j ]) );

            ヒット数リスト.Sort( ( x, y ) => ( y.hits - x.hits ) );    // 降順

            // ヒット数が一番大きい判定は、ヒット割合の小数部を切り捨てる。
            判定 = ヒット数リスト[ 0 ].judge;
            ヒット割合_整数.Add( 判定, (int) Math.Floor( ヒット割合_実数[ 判定 ] ) );
            切り捨てした[ 判定 ] = true;

            // 以下、二番目以降についてヒット割合（整数）を算出する。
            int 整数割合の合計 = ヒット割合_整数[ 判定 ];

            for( int i = 1; i < ヒット数リスト.Count; i++ )
            {
                判定 = ヒット数リスト[ i ].judge;

                // まずは四捨五入する。
                ヒット割合_整数.Add( 判定, (int) Math.Round( ヒット割合_実数[ 判定 ], MidpointRounding.AwayFromZero ) );

                // 合計が100になり、かつ、まだ後続に非ゼロがいるなら、値を -1 する。
                // → まだ非ゼロの後続がいる場合は、ここで100になってはならない。逆に、後続がすべてゼロなら、ここで100にならなければならない。
                if( 100 <= ( 整数割合の合計 + ヒット割合_整数[ 判定 ] ) )
                {
                    bool 後続にまだ非ゼロがいる = false;
                    for( int n = ( i + 1 ); n < ヒット数リスト.Count; n++ )
                    {
                        if( ヒット数リスト[ n ].hits > 0 )
                        {
                            後続にまだ非ゼロがいる = true;
                            break;
                        }
                    }
                    if( 後続にまだ非ゼロがいる )
                    {
                        ヒット割合_整数[ 判定 ]--;
                        切り捨てした[ 判定 ] = true;
                    }
                }

                // 合計に加算して、次へ。
                整数割合の合計 += ヒット割合_整数[ 判定 ];
            }

            // 合計が100に足りない場合は、「四捨五入した値と実数値との差の絶対値」が一番大きい判定に +1 する。
            // ただし、「ヒット数が一番大きい判定」と、「切り捨てした判定」は除外する。
            if( 100 > 整数割合の合計 )
            {
                var 差の絶対値リスト = new List<(判定種別 judge, double 差の絶対値)>();

                for( int i = 1; i < ヒット数リスト.Count; i++ )    // i = 0 （ヒット数が一番大きい判定）は除く
                {
                    判定 = ヒット数リスト[ i ].judge;
                    差の絶対値リスト.Add( (判定, Math.Abs( ヒット割合_実数[ 判定 ] - ヒット割合_整数[ 判定 ] )) );
                }

                差の絶対値リスト.Sort( ( x, y ) => (int) ( y.差の絶対値 * 1000.0 - x.差の絶対値 * 1000.0 ) );     // 降順; 0.xxxx だと (int) で詰むが、1000倍したらだいたいOk

                // 余るときはたいてい 99 だと思うが、念のため、100になるまで降順に+1していく。
                for( int i = 0; i < 差の絶対値リスト.Count; i++ )
                {
                    判定 = 差の絶対値リスト[ i ].judge;

                    if( 切り捨てした[ 判定 ] )
                        continue;   // 切り捨てした判定は除く

                    ヒット割合_整数[ 判定 ]++;
                    整数割合の合計++;

                    if( 100 <= 整数割合の合計 )
                        break;
                }
            }

            return ヒット割合_整数;
        }

        private int _総ノーツ数を算出して返す( SSTFormatCurrent.スコア score, ユーザ設定 options )
        {
            int 総ノーツ数 = 0;

            foreach( var chip in score.チップリスト )
            {
                var ドラムチッププロパティ = options.ドラムチッププロパティ管理[ chip.チップ種別 ];

                // AutoPlay ON のチップは、
                if( options.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ] )
                {
                    if( !( options.AutoPlayがすべてONである ) )
                        continue;   // すべてがONである場合を除いて、カウントしない。
                }

                // AutoPlay OFF 時でもユーザヒットの対象にならないチップはカウントしない。
                if( !( ドラムチッププロパティ.AutoPlayOFF_ユーザヒット ) )
                    continue;

                総ノーツ数++;
            }

            return 総ノーツ数;
        }
    }
}
