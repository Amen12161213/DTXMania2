using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DTXMania2.演奏
{
    enum 入力グループ種別
    {
        // 200115 OrzHighlight 現状では、同じレーン上のチップならいずれを入力しても判定する。
        Unknown,
        Cymbal,     // シンバルフリーモード
        LeftCymbal,
        RightCymbal,
        Ride,
        China,
        Splash,
        HiHat,
        Snare,
        Bass,
        Tom1,
        Tom2,
        Tom3,
    }
}
