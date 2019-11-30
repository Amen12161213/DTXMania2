using System;
using System.Collections.Generic;
using System.Text;

namespace DTXMania2.演奏
{
    /// <summary>
    ///     演奏モード。
    ///     数値は DB に保存される値なので変更しないこと。
    /// </summary>
    enum PlayMode : int
    {
        // 191113 ひかり：やはり難易度？の順番に並び替えた
        BASIC = 0,
        ADVANCED = 1,
        EXPERT = 2
    }
}
