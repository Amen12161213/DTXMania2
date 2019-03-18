﻿using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using FDK32;

namespace DTXMania.データベース.ユーザ
{
    /// <summary>
    ///		ユーザテーブルのエンティティクラス。
    /// </summary>
    [Table( Name = "Users" )]   // テーブル名は複数形
    class User09 : ICloneable
    {
        /// <summary>
        ///		ユーザを一意に識別する文字列。主キー。
        ///		変更不可。
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = true, IsPrimaryKey = true )]
        public string Id { get; set; }

        /// <summary>
        ///		ユーザ名。
        ///		変更可。
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = false )]
        public string Name { get; set; }

        /// <summary>
        ///		譜面スクロール速度の倍率。1.0で等倍。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double ScrollSpeed { get; set; }

        /// <summary>
        ///		起動直後の表示モード。
        ///		0: ウィンドウモード、その他: 全画面モードで。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int Fullscreen { get; set; }

        // AutoPlay

        /// <summary>
        ///		左シンバルレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_LeftCymbal { get; set; }

        /// <summary>
        ///		ハイハットレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_HiHat { get; set; }

        /// <summary>
        ///		左ペダルレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_LeftPedal { get; set; }

        /// <summary>
        ///		スネアレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_Snare { get; set; }

        /// <summary>
        ///		バスレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_Bass { get; set; }

        /// <summary>
        ///		ハイタムレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_HighTom { get; set; }

        /// <summary>
        ///		ロータムレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_LowTom { get; set; }

        /// <summary>
        ///		フロアタムレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_FloorTom { get; set; }

        /// <summary>
        ///		右シンバルレーンの AutoPlay 。
        ///		0: OFF, その他: ON。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int AutoPlay_RightCymbal { get; set; }

        // 最大ヒット距離

        /// <summary>
        ///		Perfect の最大ヒット距離[秒]。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double MaxRange_Perfect { get; set; }

        /// <summary>
        ///		Great の最大ヒット距離[秒]。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double MaxRange_Great { get; set; }

        /// <summary>
        ///		Good の最大ヒット距離[秒]。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double MaxRange_Good { get; set; }

        /// <summary>
        ///		Ok の最大ヒット距離[秒]。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double MaxRange_Ok { get; set; }


        /// <summary>
        ///		シンバルフリーモード。
        ///		0: OFF, その他: ON
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int CymbalFree { get; set; }

        /// <summary>
        ///		演奏モード。
        ///		0: Basic, 1: Expert
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int PlayMode { get; set; }

        /// <summary>
        ///		Ride の表示位置。
        ///		0: 右, 1: 左
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int RideLeft { get; set; }

        /// <summary>
        ///		China の表示位置。
        ///		0: 右, 1: 左
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int ChinaLeft { get; set; }

        /// <summary>
        ///		Splash の表示位置。
        ///		0: 右, 1: 左
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int SplashLeft { get; set; }

        /// <summary>
        ///		ユーザ入力時にドラム音を発声するか？
        ///		0: OFF, その他: ON
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int DrumSound { get; set; }

        /// <summary>
        ///     レーンタイプ名。
        ///     $(System)images\演奏\レーン配置\*.json の '*' に当たる部分の名前を指定できる。
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = false )]
        public string LaneType { get; set; }

        /// <summary>
        ///     レーンの透過度[%]。
        ///     0:完全不透明 ～ 100:完全透明
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int LaneTrans { get; set; }

        /// <summary>
        ///		演奏時に再生される背景動画を表示するか？
        ///		0: OFF, その他: ON
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int BackgroundMovie { get; set; }

        /// <summary>
        ///     演奏速度。
        ///     1.0 で通常速度。
        /// </summary>
        [Column( DbType = "REAL", CanBeNull = false )]
        public double PlaySpeed { get; set; }

        /// <summary>
        ///		小節線・拍線の表示
        ///		0: OFF, その他: ON
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int ShowPartLine { get; set; }

        /// <summary>
        ///		小節番号の表示
        ///		0: OFF, その他: ON
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int ShowPartNumber { get; set; }


        ///////////////////////////

        /// <summary>
        ///		既定値で初期化。
        /// </summary>
        public User09()
        {
            this.Id = "Anonymous";
            this.Name = "Anonymous";
            this.ScrollSpeed = 1.0;
            this.Fullscreen = 0;
            this.AutoPlay_LeftCymbal = 1;
            this.AutoPlay_HiHat = 1;
            this.AutoPlay_LeftPedal = 1;
            this.AutoPlay_Snare = 1;
            this.AutoPlay_Bass = 1;
            this.AutoPlay_HighTom = 1;
            this.AutoPlay_LowTom = 1;
            this.AutoPlay_FloorTom = 1;
            this.AutoPlay_RightCymbal = 1;
            this.MaxRange_Perfect = 0.034;
            this.MaxRange_Great = 0.067;
            this.MaxRange_Good = 0.084;
            this.MaxRange_Ok = 0.117;
            this.CymbalFree = 1;
            this.PlayMode = 0;
            this.RideLeft = 0;
            this.ChinaLeft = 0;
            this.SplashLeft = 1;
            this.DrumSound = 1;
            this.LaneType = "TypeA";
            this.LaneTrans = 40;
            this.BackgroundMovie = 1;
            this.PlaySpeed = 1.0;
            this.ShowPartLine = 1;
            this.ShowPartNumber = 1;
        }

        // ICloneable 実装
        public User09 Clone()
        {
            return (User09) this.MemberwiseClone();
        }
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public override string ToString()
            => $"Id:{this.Id}, Name:{this.Name}";


        ///////////////////////////

        /// <summary>
        ///		テーブルのカラム部分を列挙したSQL。
        /// </summary>
        public static readonly string ColumnsList =
            @"( Id NVARCHAR NOT NULL PRIMARY KEY" +
            @", Name NVARCHAR NOT NULL" +
            @", ScrollSpeed REAL NOT NULL" +
            @", Fullscreen INTEGER NOT NULL" +
            @", AutoPlay_LeftCymbal INTEGER NOT NULL" +
            @", AutoPlay_HiHat INTEGER NOT NULL" +
            @", AutoPlay_LeftPedal INTEGER NOT NULL" +
            @", AutoPlay_Snare INTEGER NOT NULL" +
            @", AutoPlay_Bass INTEGER NOT NULL" +
            @", AutoPlay_HighTom INTEGER NOT NULL" +
            @", AutoPlay_LowTom INTEGER NOT NULL" +
            @", AutoPlay_FloorTom INTEGER NOT NULL" +
            @", AutoPlay_RightCymbal INTEGER NOT NULL" +
            @", MaxRange_Perfect REAL NOT NULL" +
            @", MaxRange_Great REAL NOT NULL" +
            @", MaxRange_Good REAL NOT NULL" +
            @", MaxRange_Ok REAL NOT NULL" +
            @", CymbalFree INTEGER NOT NULL" +
            @", PlayMode INTEGER NOT NULL" +
            @", RideLeft INTEGER NOT NULL" +
            @", ChinaLeft INTEGER NOT NULL" +
            @", SplashLeft INTEGER NOT NULL" +
            @", DrumSound INTEGER NOT NULL" +
            @", LaneType NVARCHAR NOT NULL" +
            @", LaneTrans INTEGER NOT NULL" +
            @", BackgroundMovie INTEGER NOT NULL" +
            @", PlaySpeed REAL NOT NULL" +
            @", ShowPartLine INTEGER NOT NULL" +
            @", ShowPartNumber INTEGER NOT NULL" +
            @")";
    }
}
