﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.入力;
using DTXmatixx.ステージ.演奏;
using DTXmatixx.設定;

using チップ種別 = SSTFormat.v3.チップ種別;
using レーン種別 = SSTFormat.v3.レーン種別;

namespace DTXmatixx.設定
{
    class ドラムチッププロパティ管理
    {
        /// <summary>
        ///		チップ種別をキーとする対応表。
        /// </summary>
        public Dictionary<チップ種別, ドラムチッププロパティ> チップtoプロパティ
        {
            get;
            protected set;
        }

        /// <summary>
        ///     インデクサによるプロパティの取得。
        /// </summary>
        public ドラムチッププロパティ this[ チップ種別 chipType ]
            => this.チップtoプロパティ[ chipType ];


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public ドラムチッププロパティ管理( PlayMode 演奏モード, 表示レーンの左右 表示レーンの左右, 入力グループプリセット種別 入力グループプリセット種別 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._演奏モード = 演奏モード;
                this._表示レーンの左右 = 表示レーンの左右;
                this._入力グループプリセット種別 = 入力グループプリセット種別;

                this.チップtoプロパティ = new Dictionary<チップ種別, ドラムチッププロパティ>() {

                    // ※以下、コメントアウトされている初期化子は、「後で上書きする」の意。

                    #region " チップ種別.Unknown "
                    //----------------
                    [ チップ種別.Unknown ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Unknown,
                        レーン種別 = レーン種別.Unknown,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.LeftCrash "
                    //----------------
                    [ チップ種別.LeftCrash ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.LeftCrash,
                        レーン種別 = レーン種別.LeftCrash,
                        表示レーン種別 = 表示レーン種別.LeftCymbal,
                        表示チップ種別 = 表示チップ種別.LeftCymbal,
                        ドラム入力種別 = ドラム入力種別.LeftCrash,
                        AutoPlay種別 = AutoPlay種別.LeftCrash,
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.LeftCymbal,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Ride "
                    //----------------
                    [ チップ種別.Ride ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Ride,
                        レーン種別 = レーン種別.Ride,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Ride,
                        //AutoPlay種別 = ...
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        //消音グループ種別 = ...
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Ride_Cup "
                    //----------------
                    [ チップ種別.Ride_Cup ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Ride_Cup,
                        レーン種別 = レーン種別.Ride,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Ride,
                        //AutoPlay種別 = ...
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        //消音グループ種別 = ...
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.China "
                    //----------------
                    [ チップ種別.China ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.China,
                        レーン種別 = レーン種別.China,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.China,
                        //AutoPlay種別 = ...
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        //消音グループ種別 = ...
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Splash "
                    //----------------
                    [ チップ種別.Splash ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Splash,
                        レーン種別 = レーン種別.Splash,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Splash,
                        //AutoPlay種別 = ...
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        //消音グループ種別 = ...
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.HiHat_Open "
                    //----------------
                    [ チップ種別.HiHat_Open ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.HiHat_Open,
                        レーン種別 = レーン種別.HiHat,
                        表示レーン種別 = 表示レーン種別.HiHat,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.HiHat_Open,
                        AutoPlay種別 = AutoPlay種別.HiHat,
                        //入力グループ種別 = ...
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.HiHat,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.HiHat_HalfOpen "
                    //----------------
                    [ チップ種別.HiHat_HalfOpen ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.HiHat_HalfOpen,
                        レーン種別 = レーン種別.HiHat,
                        表示レーン種別 = 表示レーン種別.HiHat,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.HiHat_Open,
                        AutoPlay種別 = AutoPlay種別.HiHat,
                        //入力グループ種別 = ...
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.HiHat,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.HiHat_Close "
                    //----------------
                    [ チップ種別.HiHat_Close ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.HiHat_Close,
                        レーン種別 = レーン種別.HiHat,
                        表示レーン種別 = 表示レーン種別.HiHat,
                        表示チップ種別 = 表示チップ種別.HiHat,
                        ドラム入力種別 = ドラム入力種別.HiHat_Close,
                        AutoPlay種別 = AutoPlay種別.HiHat,
                        //入力グループ種別 = ...
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.HiHat,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.HiHat_Foot "
                    //----------------
                    [ チップ種別.HiHat_Foot ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.HiHat_Foot,
                        レーン種別 = レーン種別.Foot,
                        表示レーン種別 = 表示レーン種別.Foot,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.HiHat_Foot,
                        AutoPlay種別 = AutoPlay種別.Foot,
                        //入力グループ種別 = ...
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.HiHat,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Snare "
                    //----------------
                    [ チップ種別.Snare ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Snare,
                        レーン種別 = レーン種別.Snare,
                        表示レーン種別 = 表示レーン種別.Snare,
                        表示チップ種別 = 表示チップ種別.Snare,
                        ドラム入力種別 = ドラム入力種別.Snare,
                        AutoPlay種別 = AutoPlay種別.Snare,
                        入力グループ種別 = 入力グループ種別.Snare,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Snare_OpenRim "
                    //----------------
                    [ チップ種別.Snare_OpenRim ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Snare_OpenRim,
                        レーン種別 = レーン種別.Snare,
                        表示レーン種別 = 表示レーン種別.Snare,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Snare_OpenRim,
                        AutoPlay種別 = AutoPlay種別.Snare,
                        入力グループ種別 = 入力グループ種別.Snare,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Snare_ClosedRim "
                    //----------------
                    [ チップ種別.Snare_ClosedRim ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Snare_ClosedRim,
                        レーン種別 = レーン種別.Snare,
                        表示レーン種別 = 表示レーン種別.Snare,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Snare_ClosedRim,
                        AutoPlay種別 = AutoPlay種別.Snare,
                        入力グループ種別 = 入力グループ種別.Snare,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Snare_Ghost "
                    //----------------
                    [ チップ種別.Snare_Ghost ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Snare_Ghost,
                        レーン種別 = レーン種別.Snare,
                        表示レーン種別 = 表示レーン種別.Snare,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Snare,
                        AutoPlay種別 = AutoPlay種別.Snare,
                        入力グループ種別 = 入力グループ種別.Snare,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Bass "
                    //----------------
                    [ チップ種別.Bass ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Bass,
                        レーン種別 = レーン種別.Bass,
                        表示レーン種別 = 表示レーン種別.Bass,
                        表示チップ種別 = 表示チップ種別.Bass,
                        ドラム入力種別 = ドラム入力種別.Bass,
                        AutoPlay種別 = AutoPlay種別.Bass,
                        入力グループ種別 = 入力グループ種別.Bass,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.LeftBass "
                    //----------------
                    [ チップ種別.LeftBass ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.LeftBass,
                        レーン種別 = レーン種別.Bass,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Bass,
                        AutoPlay種別 = AutoPlay種別.Bass,
                        入力グループ種別 = 入力グループ種別.Bass,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom1 "
                    //----------------
                    [ チップ種別.Tom1 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom1,
                        レーン種別 = レーン種別.Tom1,
                        表示レーン種別 = 表示レーン種別.Tom1,
                        表示チップ種別 = 表示チップ種別.Tom1,
                        ドラム入力種別 = ドラム入力種別.Tom1,
                        AutoPlay種別 = AutoPlay種別.Tom1,
                        入力グループ種別 = 入力グループ種別.Tom1,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom1_Rim "
                    //----------------
                    [ チップ種別.Tom1_Rim ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom1_Rim,
                        レーン種別 = レーン種別.Tom1,
                        表示レーン種別 = 表示レーン種別.Tom1,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Tom1_Rim,
                        AutoPlay種別 = AutoPlay種別.Tom1,
                        入力グループ種別 = 入力グループ種別.Tom1,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom2 "
                    //----------------
                    [ チップ種別.Tom2 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom2,
                        レーン種別 = レーン種別.Tom2,
                        表示レーン種別 = 表示レーン種別.Tom2,
                        表示チップ種別 = 表示チップ種別.Tom2,
                        ドラム入力種別 = ドラム入力種別.Tom2,
                        AutoPlay種別 = AutoPlay種別.Tom2,
                        入力グループ種別 = 入力グループ種別.Tom2,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom2_Rim "
                    //----------------
                    [ チップ種別.Tom2_Rim ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom2_Rim,
                        レーン種別 = レーン種別.Tom2,
                        表示レーン種別 = 表示レーン種別.Tom2,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Tom2,
                        AutoPlay種別 = AutoPlay種別.Tom2,
                        入力グループ種別 = 入力グループ種別.Tom2,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom3 "
                    //----------------
                    [ チップ種別.Tom3 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom3,
                        レーン種別 = レーン種別.Tom3,
                        表示レーン種別 = 表示レーン種別.Tom3,
                        表示チップ種別 = 表示チップ種別.Tom3,
                        ドラム入力種別 = ドラム入力種別.Tom3,
                        AutoPlay種別 = AutoPlay種別.Tom3,
                        入力グループ種別 = 入力グループ種別.Tom3,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.Tom3_Rim "
                    //----------------
                    [ チップ種別.Tom3_Rim ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.Tom3_Rim,
                        レーン種別 = レーン種別.Tom3,
                        表示レーン種別 = 表示レーン種別.Tom3,
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Tom3_Rim,
                        AutoPlay種別 = AutoPlay種別.Tom3,
                        入力グループ種別 = 入力グループ種別.Tom3,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.RightCrash "
                    //----------------
                    [ チップ種別.RightCrash ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.RightCrash,
                        レーン種別 = レーン種別.RightCrash,
                        表示レーン種別 = 表示レーン種別.RightCymbal,
                        表示チップ種別 = 表示チップ種別.RightCymbal,
                        ドラム入力種別 = ドラム入力種別.RightCrash,
                        AutoPlay種別 = AutoPlay種別.RightCrash,
                        //入力グループ種別 = ...
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.RightCymbal,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = true,
                        AutoPlayON_Miss判定 = true,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = true,
                        AutoPlayOFF_ユーザヒット_非表示 = true,
                        AutoPlayOFF_ユーザヒット_判定 = true,
                        AutoPlayOFF_Miss判定 = true,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.BPM "
                    //----------------
                    [ チップ種別.BPM ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.BPM,
                        レーン種別 = レーン種別.BPM,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.小節線 "
                    //----------------
                    [ チップ種別.小節線 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.小節線,
                        レーン種別 = レーン種別.Unknown,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.PartLine,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.拍線 "
                    //----------------
                    [ チップ種別.拍線 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.拍線,
                        レーン種別 = レーン種別.Unknown,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.BeatLine,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.背景動画 "
                    //----------------
                    [ チップ種別.背景動画 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.背景動画,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.小節メモ "
                    //----------------
                    [ チップ種別.小節メモ ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.小節メモ,
                        レーン種別 = レーン種別.Unknown,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.LeftCymbal_Mute "
                    //----------------
                    [ チップ種別.LeftCymbal_Mute ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.LeftCymbal_Mute,
                        レーン種別 = レーン種別.LeftCrash,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.LeftCrash,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.LeftCymbal,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.RightCymbal_Mute "
                    //----------------
                    [ チップ種別.RightCymbal_Mute ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.RightCymbal_Mute,
                        レーン種別 = レーン種別.RightCrash,
                        //表示レーン種別 = ...
                        //表示チップ種別 = ...
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.RightCrash,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = true,
                        消音グループ種別 = 消音グループ種別.RightCymbal,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.小節の先頭 "
                    //----------------
                    [ チップ種別.小節の先頭 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.小節の先頭,
                        レーン種別 = レーン種別.Unknown,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = false,
                        AutoPlayON_自動ヒット_非表示 = false,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = false,
                        AutoPlayOFF_自動ヒット_非表示 = false,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.BGM "
                    //----------------
                    [ チップ種別.BGM ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.BGM,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.SE1 "
                    //----------------
                    [ チップ種別.SE1 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.SE1,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.SE2 "
                    //----------------
                    [ チップ種別.SE2 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.SE2,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.SE3 "
                    //----------------
                    [ チップ種別.SE3 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.SE3,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.SE4 "
                    //----------------
                    [ チップ種別.SE4 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.SE4,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.SE5 "
                    //----------------
                    [ チップ種別.SE5 ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.SE5,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.GuitarAuto "
                    //----------------
                    [ チップ種別.GuitarAuto ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.GuitarAuto,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                    #region " チップ種別.BassAuto "
                    //----------------
                    [ チップ種別.BassAuto ] = new ドラムチッププロパティ() {
                        チップ種別 = チップ種別.BassAuto,
                        レーン種別 = レーン種別.Song,
                        表示レーン種別 = 表示レーン種別.Unknown,
                        表示チップ種別 = 表示チップ種別.Unknown,
                        ドラム入力種別 = ドラム入力種別.Unknown,
                        AutoPlay種別 = AutoPlay種別.Unknown,
                        入力グループ種別 = 入力グループ種別.Unknown,
                        発声前消音 = false,
                        消音グループ種別 = 消音グループ種別.Unknown,
                        AutoPlayON_自動ヒット_再生 = true,
                        AutoPlayON_自動ヒット_非表示 = true,
                        AutoPlayON_自動ヒット_判定 = false,
                        AutoPlayON_Miss判定 = false,
                        AutoPlayOFF_自動ヒット_再生 = true,
                        AutoPlayOFF_自動ヒット_非表示 = true,
                        AutoPlayOFF_自動ヒット_判定 = false,
                        AutoPlayOFF_ユーザヒット_再生 = false,
                        AutoPlayOFF_ユーザヒット_非表示 = false,
                        AutoPlayOFF_ユーザヒット_判定 = false,
                        AutoPlayOFF_Miss判定 = false,
                    },
                    //----------------
                    #endregion
                };

                this.反映する( 演奏モード );
                this.反映する( 表示レーンの左右 );
                this.反映する( 入力グループプリセット種別 );
            }
        }

        /// <summary>
        ///     演奏モードに依存するメンバに対して一括設定を行う。
        ///     依存しないメンバには何もしない。
        /// </summary>
        public void 反映する( PlayMode mode )
        {
            this._演奏モード = mode;

            foreach( var kvp in this.チップtoプロパティ )
            {
                switch( kvp.Key )
                {
                    case チップ種別.Ride:
                        kvp.Value.表示チップ種別 =
                            ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Ride :
                            ( this._表示レーンの左右.Rideは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.Ride_Cup:
                        kvp.Value.表示チップ種別 =
                            ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Ride_Cup :
                            ( this._表示レーンの左右.Rideは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.China:
                        kvp.Value.表示チップ種別 =
                            ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.China :
                            ( this._表示レーンの左右.Chinaは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.Splash:
                        kvp.Value.表示チップ種別 =
                            ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Splash :
                            ( this._表示レーンの左右.Splashは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.HiHat_Open:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.HiHat_Open : 表示チップ種別.HiHat;
                        break;

                    case チップ種別.HiHat_HalfOpen:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.HiHat_HalfOpen : 表示チップ種別.HiHat;
                        break;

                    case チップ種別.HiHat_Foot:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Foot : 表示チップ種別.LeftPedal;
                        break;

                    case チップ種別.Snare_OpenRim:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Snare_OpenRim : 表示チップ種別.Snare;
                        break;

                    case チップ種別.Snare_ClosedRim:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Snare_ClosedRim : 表示チップ種別.Snare;
                        break;

                    case チップ種別.Snare_Ghost:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Snare_Ghost : 表示チップ種別.Unknown;
                        break;

                    case チップ種別.LeftBass:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Bass : 表示チップ種別.LeftBass;
                        kvp.Value.表示レーン種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示レーン種別.Bass : 表示レーン種別.Foot;
                        break;

                    case チップ種別.Tom1_Rim:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Tom1_Rim : 表示チップ種別.Tom1;
                        break;

                    case チップ種別.Tom2_Rim:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Tom2_Rim : 表示チップ種別.Tom2;
                        break;

                    case チップ種別.Tom3_Rim:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.Tom3_Rim : 表示チップ種別.Tom3;
                        break;

                    case チップ種別.LeftCymbal_Mute:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.LeftCymbal_Mute : 表示チップ種別.Unknown;
                        kvp.Value.表示レーン種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示レーン種別.LeftCymbal : 表示レーン種別.Unknown;
                        break;

                    case チップ種別.RightCymbal_Mute:
                        kvp.Value.表示チップ種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示チップ種別.RightCymbal_Mute : 表示チップ種別.Unknown;
                        kvp.Value.表示レーン種別 = ( this._演奏モード == PlayMode.MANIAC ) ? 表示レーン種別.RightCymbal : 表示レーン種別.Unknown;
                        break;
                }
            }
        }

        /// <summary>
        ///     表示レーンの左右に依存するメンバに対して一括設定を行う。
        ///     依存しないメンバには何もしない。
        /// </summary>
        public void 反映する( 表示レーンの左右 position )
        {
            this._表示レーンの左右 = position;

            foreach( var kvp in this.チップtoプロパティ )
            {
                switch( kvp.Key )
                {
                    case チップ種別.Ride:
                    case チップ種別.Ride_Cup:
                        kvp.Value.表示レーン種別 = ( this._表示レーンの左右.Rideは左 ) ? 表示レーン種別.LeftCymbal : 表示レーン種別.RightCymbal;
                        kvp.Value.AutoPlay種別 = ( this._表示レーンの左右.Rideは左 ) ? AutoPlay種別.LeftCrash : AutoPlay種別.RightCrash;
                        kvp.Value.消音グループ種別 = ( this._表示レーンの左右.Rideは左 ) ? 消音グループ種別.LeftCymbal : 消音グループ種別.RightCymbal;
                        if( this._演奏モード == PlayMode.BASIC )
                            kvp.Value.表示チップ種別 = ( this._表示レーンの左右.Rideは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.China:
                        kvp.Value.表示レーン種別 = ( this._表示レーンの左右.Chinaは左 ) ? 表示レーン種別.LeftCymbal : 表示レーン種別.RightCymbal;
                        kvp.Value.AutoPlay種別 = ( this._表示レーンの左右.Chinaは左 ) ? AutoPlay種別.LeftCrash : AutoPlay種別.RightCrash;
                        kvp.Value.消音グループ種別 = ( this._表示レーンの左右.Chinaは左 ) ? 消音グループ種別.LeftCymbal : 消音グループ種別.RightCymbal;
                        if( this._演奏モード == PlayMode.BASIC )
                            kvp.Value.表示チップ種別 = ( this._表示レーンの左右.Chinaは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;

                    case チップ種別.Splash:
                        kvp.Value.表示レーン種別 = ( this._表示レーンの左右.Splashは左 ) ? 表示レーン種別.LeftCymbal : 表示レーン種別.RightCymbal;
                        kvp.Value.AutoPlay種別 = ( this._表示レーンの左右.Splashは左 ) ? AutoPlay種別.LeftCrash : AutoPlay種別.RightCrash;
                        kvp.Value.消音グループ種別 = ( this._表示レーンの左右.Splashは左 ) ? 消音グループ種別.LeftCymbal : 消音グループ種別.RightCymbal;
                        if( this._演奏モード == PlayMode.BASIC )
                            kvp.Value.表示チップ種別 = ( this._表示レーンの左右.Splashは左 ) ? 表示チップ種別.LeftCymbal : 表示チップ種別.RightCymbal;
                        break;
                }
            }
        }

        /// <summary>
        ///     指定されたプリセットに依存する入力グループ種別を一括設定する。
        ///     依存しないメンバには何もしない。
        /// </summary>
        public void 反映する( 入力グループプリセット種別 preset )
        {
            this._入力グループプリセット種別 = preset;

            foreach( var kvp in this.チップtoプロパティ )
            {
                switch( this._入力グループプリセット種別 )
                {
                    case 入力グループプリセット種別.シンバルフリー:

                        switch( kvp.Key )
                        {
                            case チップ種別.LeftCrash:
                            case チップ種別.Ride:
                            case チップ種別.Ride_Cup:
                            case チップ種別.China:
                            case チップ種別.Splash:
                            case チップ種別.HiHat_Open:
                            case チップ種別.HiHat_HalfOpen:
                            case チップ種別.HiHat_Close:
                            case チップ種別.HiHat_Foot:
                            case チップ種別.RightCrash:
                                kvp.Value.入力グループ種別 = 入力グループ種別.Cymbal;
                                break;
                        }
                        break;

                    case 入力グループプリセット種別.基本形:

                        switch( kvp.Key )
                        {
                            case チップ種別.LeftCrash:
                                kvp.Value.入力グループ種別 = 入力グループ種別.LeftCymbal;
                                break;

                            case チップ種別.Ride:
                            case チップ種別.Ride_Cup:
                                kvp.Value.入力グループ種別 = 入力グループ種別.Ride;
                                break;

                            case チップ種別.China:
                                kvp.Value.入力グループ種別 = 入力グループ種別.China;
                                break;

                            case チップ種別.Splash:
                                kvp.Value.入力グループ種別 = 入力グループ種別.Splash;
                                break;

                            case チップ種別.HiHat_Open:
                            case チップ種別.HiHat_HalfOpen:
                            case チップ種別.HiHat_Close:
                            case チップ種別.HiHat_Foot:
                                kvp.Value.入力グループ種別 = 入力グループ種別.HiHat;
                                break;

                            case チップ種別.RightCrash:
                                kvp.Value.入力グループ種別 = 入力グループ種別.RightCymbal;
                                break;
                        }
                        break;

                    default:
                        throw new Exception( $"未知の入力グループプリセット種別です。[{this._入力グループプリセット種別.ToString()}]" );
                }
            }
        }

        private PlayMode _演奏モード;
        private 表示レーンの左右 _表示レーンの左右;
        private 入力グループプリセット種別 _入力グループプリセット種別;
    }
}
