﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.曲ツリー構築
{
	class 曲ツリー構築ステージ : ステージ
	{
		public enum フェーズ
		{
			開始,
			構築中,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 曲ツリー構築ステージ()
		{
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.開始;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}
		public override void 進行描画する( グラフィックデバイス gd )
		{
			App.入力管理.すべての入力デバイスをポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.開始:

					// 状態チェック； ここの時点で曲ツリーが初期状態であること。
					Debug.Assert( null != App.曲ツリー.ルートノード );
					Debug.Assert( null == App.曲ツリー.フォーカスノード );
					Debug.Assert( null == App.曲ツリー.フォーカスリスト );

					// OK なら構築へ
					this.現在のフェーズ = フェーズ.構築中;
					break;

				case フェーズ.構築中:
					App.曲ツリー.非活性化する( gd );

					foreach( var varpath in App.システム設定.曲検索フォルダ )
						App.曲ツリー.曲を検索して親ノードに追加する( App.曲ツリー.ルートノード, varpath );

					App.曲ツリー.活性化する( gd );

					this.現在のフェーズ = フェーズ.確定;
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}

			if( App.入力管理.キャンセルキーが入力された() )
			{
				this.現在のフェーズ = フェーズ.キャンセル;
			}

		}
	}
}
