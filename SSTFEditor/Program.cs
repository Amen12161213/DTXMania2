﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace SSTFEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            try
            {
                Application.Run( new メインフォーム() );
            }
#if !DEBUG
            catch( Exception e )
            {
                //using( var dlg = new 未処理例外検出ダイアログ() )
                {
                    Trace.WriteLine( "" );
                    Trace.WriteLine( "====> 未処理の例外が検出されました。" );
                    Trace.WriteLine( "" );
                    Trace.WriteLine( e.ToString() );

                    //dlg.ShowDialog();
                }
            }
#else
            finally
            {
                // DEBUG 時には、未処理の例外が発出されてもcatchしない。（デバッガでキャッチすることを想定。）
            }
#endif
        }
    }
}
