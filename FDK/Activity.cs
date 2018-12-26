﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    public class Activity
    {
        /// <summary>
        ///		子リストに Activity を登録すると、活性化と非活性化が親と連動するようになる。
        /// </summary>
        /// <remarks>
        ///		子リストには静的・動的の２種類があり、それぞれ以下のように使い分ける。
        /// 
        ///		(A) メンバとして定義する静的な子の場合：
        ///		　・子Activity の生成と子リストへの追加は、親Activity のコンストラクタで行う。
        ///		　・子リストからの削除は不要。
        ///	　
        ///		(B) 活性化時に生成する動的な子の場合：
        ///		　・子Activity の生成と子リストへの追加は、親Activity の On活性化() で行う。
        ///		　・子リストからの削除は、親Activity の On非活性化() で行う。
        /// </remarks>
        public IReadOnlyList<Activity> 子リスト
            => this._子リスト;

        public bool 活性化している
        {
            get;
            private set;    // 派生クラスからも設定は禁止。
        } = false;
        public bool 活性化していない
        {
            get
                => !( this.活性化している );

            // 派生クラスからも設定は禁止。
            private set
                => this.活性化している = !( value );
        }

        public Activity 親
        {
            get;
            protected set;
        } = null;

        public void 子を追加する( Activity 子 )
        {
            Debug.Assert( ( null == 子.親 ), "このActivityには、すでに親Activityが存在しています。" );

            子.親 = this;
            this._子リスト.Add( 子 );
        }
        public void 子を削除する( Activity 子 )
        {
            Debug.Assert( this._子リスト.Contains( 子 ), "指定されたActivityは子リストに存在していません。" );

            子.親 = null;
            this._子リスト.Remove( 子 );
        }
        public void 子リストをクリアする()
        {
            this._子リスト.Clear(); // Dispose はしない。
        }

        /// <summary>
        ///		この Activity を初期化し、進行や描画を行える状態にする。
        ///		これにはデバイス依存リソースの作成も含まれる。
        /// </summary>
        public void 活性化する()
        {
            if( this.活性化している )
                return;

            // (1) 自分を活性化する。
            this.On活性化();
            this.活性化している = true;

            // (2) すべての子Activityを活性化する。
            foreach( var child in this.子リスト )
                child.活性化する();
        }

        /// <summary>
        ///		この Activity を終了し、進行や描画を行わない状態に戻す。
        ///		これにはデバイス依存リソースの解放も含まれる。
        /// </summary>
        public void 非活性化する()
        {
            if( this.活性化していない )
                return;

            // (1) すべての子Activityを非活性化する。
            foreach( var child in this.子リスト )
                child.非活性化する();

            // (2) 自分を非活性化する。
            this.On非活性化();
            this.活性化していない = true;
        }

        /// <summary>
        ///     自身を活性化する。
        /// </summary>
        protected virtual void On活性化()
        {
        }

        /// <summary>
        ///     自身を非活性化する。
        /// </summary>
        protected virtual void On非活性化()
        {
        }


        private List<Activity> _子リスト = new List<Activity>();
    }
}
