using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTXMania2
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // ������

                timeBeginPeriod( 1 );

                #region " AppData/DTXMania2 �t�H���_���Ȃ���΍쐬����B"
                //----------------
                //var AppData�t�H���_�� = Application.UserAppDataPath;  // %USERPROFILE%/AppData/<��Ж�>/DTXMania2/
                var AppData�t�H���_�� = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), "DTXMania2" ); // %USERPROFILE%/AppData/DTXMania2/

                if( !( Directory.Exists( AppData�t�H���_�� ) ) )
                    Directory.CreateDirectory( AppData�t�H���_�� );
                //----------------
                #endregion

                #region " ���O�t�@�C���ւ̃��O�̕����o�͊J�n�B"
                //----------------
                {
                    const int ���O�t�@�C���̍ő�ۑ����� = 30;
                    Trace.AutoFlush = true;

                    var ���O�t�@�C���� = Log.���O�t�@�C�����𐶐�����( Path.Combine( AppData�t�H���_��, "Logs" ), "Log.", TimeSpan.FromDays( ���O�t�@�C���̍ő�ۑ����� ) );

                    // ���O�t�@�C����Trace���X�i�Ƃ��Ēǉ��B
                    // �ȍ~�ATrace�i�Ȃ�т�Log�N���X�j�ɂ��o�͂́A���̃��X�i�i�����O�t�@�C���j�ɂ��o�͂����B
                    Trace.Listeners.Add( new TraceLogListener( new StreamWriter( ���O�t�@�C����, false, Encoding.GetEncoding( "utf-8" ) ) ) );

                    Log.���݂̃X���b�h�ɖ��O������( "Form" );
                }
                //----------------
                #endregion

                #region " �^�C�g���A���쌠�A�V�X�e���������O�o�͂���B"
                //----------------
                Log.WriteLine( $"{Application.ProductName} Release {int.Parse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ) )}" );

                var copyrights = (AssemblyCopyrightAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
                Log.WriteLine( $"{copyrights[ 0 ].Copyright}" );
                Log.WriteLine( "" );

                Log.�V�X�e���������O�o�͂���();
                Log.WriteLine( "" );
                //----------------
                #endregion

                #region " �t�H���_�ϐ���ݒ肷��B"
                //----------------
                {
                    var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) ?? "";

                    Folder.�t�H���_�ϐ���ǉ��܂��͍X�V����( "Exe", exePath );
                    Folder.�t�H���_�ϐ���ǉ��܂��͍X�V����( "System", Path.Combine( exePath, "System" ) );
                    Folder.�t�H���_�ϐ���ǉ��܂��͍X�V����( "AppData", AppData�t�H���_�� );
                    Folder.�t�H���_�ϐ���ǉ��܂��͍X�V����( "UserProfile", Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) );
                }
                //----------------
                #endregion


                // �A�v���N��

                Application.SetHighDpiMode( HighDpiMode.SystemAware );
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new AppForm() );


                // �I��

                timeEndPeriod( 1 );

                Log.WriteLine( "" );
                Log.WriteLine( "�V��ł���Ă��肪�Ƃ��I" );
            }
#if !DEBUG
            // Release ���ɂ́A�������̗�O���L���b�`������_�C�A���O��\������B
            catch( Exception e )
            {
                MessageBox.Show(
                    $"�������̗�O���������܂����B\n\n" +
                    $"{e.Message}\n" +
                    $"{e.StackTrace}",
                    "Exception" );
            }
#else
            // Debug ���ɂ́A�������̗�O�����o����Ă������B�i�f�o�b�K�ŃL���b�`���邱�Ƃ�z��B�j
            finally
            {
            }
#endif
        }

        #region " Win32 "
        //----------------
        [System.Runtime.InteropServices.DllImport( "winmm.dll" )]
        static extern void timeBeginPeriod( uint x );

        [System.Runtime.InteropServices.DllImport( "winmm.dll" )]
        static extern void timeEndPeriod( uint x );
        //----------------
        #endregion
    }
}
