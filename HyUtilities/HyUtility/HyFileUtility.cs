using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace HyUtilities
{
    public class HyFileUtility
    {
        public static string GetImageFileNameFromDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图片文件(*.jpg;*.jpeg;*.bmp;*.gif;*.png)|*.jpg;*.jpeg;*.bmp;*.gif;*.png|All files (*.*)|*.*";
            dlg.FilterIndex = 1;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return string.Empty;
        }

        public static string GetIconFileNameFromDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图标文件(*.ico)|*.ico";
            dlg.FilterIndex = 1;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return string.Empty;
        }

        public static string GetImageFileNameFromDialog(string InitPath)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (Directory.Exists(InitPath))
            {
                openFileDialog1.InitialDirectory = InitPath;
            }
            else
            {
                openFileDialog1.InitialDirectory = Application.StartupPath;
            }
            openFileDialog1.Filter = "图片文件(*.jpg;*.jpeg;*.bmp;*.gif;*.png)|*.jpg;*.jpeg;*.bmp;*.gif;*.png|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            return string.Empty;
        }

        public static string ReadNoCommentLine(StreamReader fs)
        {
            string line = null;
            try
            {
                do
                {
                    line = fs.ReadLine();
                } while (!(line != null && line.Trim().Length > 0 && !line.Trim().StartsWith("*")) && !fs.EndOfStream);

                if (line != null && line.Length > 0 && line[0] != '*')
                    //把全角逗号换成半角逗号
                    return line.Replace("，", ",");
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static string ReadNoCommentLine(StreamReader fs, ref int LineCount)
        {
            string line = null;
            try
            {
                do
                {
                    line = fs.ReadLine();
                    LineCount++;
                } while (!(line != null && line.Trim().Length > 0 && !line.Trim().StartsWith("*")) && !fs.EndOfStream);

                if (line != null && line.Length > 0 && line[0] != '*')
                    //把全角逗号换成半角逗号
                    return line.Replace("，", ",");
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static string ReadNoCommentLine(StreamReader fs, ref int LineCount, ref string contents)
        {
            string line = null;
            try
            {
                do
                {
                    line = fs.ReadLine();
                    LineCount++;
                    contents += line + "\r\n";
                } while (!(line != null && line.Trim().Length > 0 && !line.Trim().StartsWith("*")) && !fs.EndOfStream);

                if (line != null && line.Length > 0 && line[0] != '*')
                    //把全角逗号换成半角逗号
                    return line.Replace("，", ",");
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 安全打开文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static StreamReader OpenFileSafely(string fileName)
        {
            try
            {
                return new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read), Encoding.GetEncoding("gb2312"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("打开文件时发生错误：{0}", ex.ToString()));
                return null;
            }
        }

        /// <summary>
        /// 安全打开文件以写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static StreamWriter OpenWriteFileSafely(string fileName)
        {
            try
            {
                return new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write), Encoding.GetEncoding("gb2312"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("打开文件[{1}]时发生错误：{0}", ex.ToString(), fileName));
                return null;
            }
        }

        /// <summary>
        /// 安全打开文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static StreamReader OpenFileSafely(string fileName, Encoding encoding)
        {
            try
            {
                return new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read), encoding);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("打开文件时发生错误：{0}", ex.ToString()));
                return null;
            }
        }

        /// <summary>
        /// 安全打开文件以写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static StreamWriter OpenWriteFileSafely(string fileName, Encoding encoding)
        {
            try
            {
                return new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write), encoding);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("打开文件[{1}]时发生错误：{0}", ex.ToString(), fileName));
                return null;
            }
        }

        /// <summary>
        /// 显示打开文件对话框并返回文件名（不包含路径）
        /// </summary>
        /// <param name="InitPath"></param>
        /// <param name="Filter"></param>
        /// <param name="Title"></param>
        /// <returns>
        /// 文件名 or
        /// string.Empty</returns>
        public static string SelectTextFile(string InitPath, string Filter, string Title)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (Directory.Exists(InitPath))
            {
                openFileDialog1.InitialDirectory = InitPath;
            }
            else
            {
                openFileDialog1.InitialDirectory = Application.StartupPath;
            }
            openFileDialog1.Filter = Filter;
            openFileDialog1.Title = Title;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return Path.GetFileName(openFileDialog1.FileName);
            }
            return string.Empty;
        }

        /// <summary>
        /// 显示打开文件对话框并返回全路径
        /// </summary>
        /// <param name="InitPath"></param>
        /// <param name="Filter"></param>
        /// <param name="Title"></param>
        /// <returns>
        /// 全路径文件名 or
        /// string.Empty</returns>
        public static string SelectFile(string InitPath, string Filter, string Title)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (Directory.Exists(InitPath))
            {
                openFileDialog1.InitialDirectory = InitPath;
            }
            else
            {
                openFileDialog1.InitialDirectory = Application.StartupPath;
            }
            openFileDialog1.Filter = Filter;
            openFileDialog1.Title = Title;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            return string.Empty;
        }

        /// <summary>
        /// 检查文件名中是否全部是ASCII字符
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool CheckFileNameIsAscii(string filename)
        {
            for (int i = 0; i < filename.Length; i++)
            {
                char c = filename[i];
                if (c > 127) return false;
            }
            return true;
        }

        /// <summary>
        /// 以bytes返回指定目录下的所有文件(包含子目录)的总大小
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static long GetAllFileSize(string dir)
        {
            long size = 0;
            DirectoryInfo di = new DirectoryInfo(dir);

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            DirectoryInfo[] subdis = di.GetDirectories();
            foreach (DirectoryInfo subdi in subdis)
            {
                size += GetAllFileSize(Path.Combine(dir, subdi.Name));
            }

            return size;
        }

        /// <summary>
        /// 以";"隔开的字符串形式返回指定目录下的所有文件(包含子目录)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string GetAllFiles(string dir)
        {
            string fullfile = string.Empty;
            string[] files = Directory.GetFiles(dir);

            foreach (string file in files)
            {
                fullfile += file + ";";
            }

            string[] subdis = Directory.GetDirectories(dir);
            foreach (string subdi in subdis)
            {
                fullfile += GetAllFiles(subdi);
            }

            return fullfile;
        }

        /// <summary>
        /// 取得当前执行文件的目录
        /// </summary>
        /// <returns></returns>
        public static string GetExecutePath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }

        /// <summary>
        /// 检查是否绝对路径
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static bool IsAbsolutePath(string Path)
        {
            if (HyStringUtility.IsStringEmpty(Path)) return false;
            if (Path.Contains(@":\")) return true;
            return false;
        }

        public static string SelectFolder(string CurrentPath)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.RootFolder = Environment.SpecialFolder.Desktop;
            if (!HyStringUtility.IsStringEmpty(CurrentPath)) dlg.SelectedPath = CurrentPath;
            if (dlg.ShowDialog() == DialogResult.OK)
                return dlg.SelectedPath;
            else
                return null;
        }

        public static bool CreateDirectorySafely(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    MessageBox.Show(string.Format("创建目录【{0}】失败！", path));
                    return false;
                }
            }
            return true;
        }

        public static void PlaySoundFile(string soundfilename)
        {
            try
            {
                System.Media.SoundPlayer sndPlayer = new System.Media.SoundPlayer(soundfilename);
                sndPlayer.Play();
            }
            catch
            {
            }
        }
    }
}
