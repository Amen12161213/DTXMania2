using System;

namespace DTXMania2.演奏.BASIC
{
    /// <summary>
    ///     ヒットバー ... チップがこの位置に来たら叩け！という線。
    /// </summary>
    class ヒットバー : IDisposable
    {
        public const float ヒット判定バーの中央Y座標dpx = 847f;



        // 生成と終了


        public ヒットバー()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._ヒットバー画像 = new 画像(@"$(Images)\PlayStage\BASIC\HitBar.png");
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock(Log.現在のメソッド名);

            this._ヒットバー画像?.Dispose();
        }



        // 進行と描画


        public void 描画する()
        {
            const float バーの左端Xdpx = 441f;
            const float バーの中央Ydpx = 演奏ステージ.ヒット判定位置Ydpx;
            const float バーの厚さdpx = 8f;

            this._ヒットバー画像.描画する(バーの左端Xdpx, バーの中央Ydpx - バーの厚さdpx / 2f);
        }



        // private


        private readonly 画像 _ヒットバー画像;
    }
}