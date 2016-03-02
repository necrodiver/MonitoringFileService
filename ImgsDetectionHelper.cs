using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringFile
{
    public class ImgsDetectionHelper
    {
        private readonly static int minSize = Int32.Parse(MonitoringService.minSize) * 1024;
        private readonly static int maxSize = Int32.Parse(MonitoringService.maxSize) * 1024;
        private readonly static string suffix = ".JPG";
        public readonly static string path = MonitoringService.imagePath;
        private static bool isFirst = true;
        /// <summary>
        /// 首次获取所有图片路径
        /// </summary>
        private static List<string> firstPathList;
        /// <summary>
        /// 获取所有当前路径下图片
        /// </summary>
        private List<string> imgPathList;
        /// <summary>
        /// 新增加的路径
        /// </summary>
        private List<string> addPathList;

        public void Monitor()
        {
            imgPathList = new List<string>();
            addPathList = new List<string>();

            if (isFirst)
            {
                isFirst = false;
                FirstSearch();
            }
            else
            {
                imgPathList = GetImagesList(path);
                foreach (var childFPath in imgPathList)
                {
                    bool isPi = false;
                    foreach (var imgChildPath in firstPathList)
                    {
                        if (childFPath.Equals(imgChildPath, StringComparison.OrdinalIgnoreCase))
                            isPi = true;
                        continue;
                    }
                    if (!isPi)
                    {
                        addPathList.Add(childFPath);
                    }
                }
                DecideImageNorms();
            }
        }

        /// <summary>
        /// 首次获取所有图片路径
        /// </summary>
        private void FirstSearch()
        {
            firstPathList = new List<string>();
            firstPathList = GetImagesList(path);
        }

        /// <summary>
        /// 图片检测，大小和格式检测
        /// </summary>
        private void DecideImageNorms()
        {
            List<string> imagesList = addPathList;
            List<QuestionModel> notmeetImageList = new List<QuestionModel>();
            List<QuestionModel> imageContentList = new List<QuestionModel>();
            bool isAccordWithSize = true;
            if (imagesList.Count < 1)
            {
                return;
            }
            foreach (string imgFileName in imagesList)
            {
                QuestionModel QuestionPath = new QuestionModel();
                using (FileStream fs = new FileStream(imgFileName, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length < minSize || fs.Length > maxSize)
                    {
                        QuestionPath.error = "[图片大小 不符规范]";
                        QuestionPath.path = imgFileName;
                        QuestionPath.fileName = FieldInterception(imgFileName);
                        ErrorCollectHelper.ImgErrorLog(QuestionPath);
                        isAccordWithSize = false;
                    }
                    fs.Close();
                    fs.Dispose();
                }

                if (!isAccordWithSize)
                {
                    CopyFile(QuestionPath);
                    File.Delete(imgFileName);
                    isAccordWithSize = true;
                    continue;
                }

                string extension = Path.GetExtension(imgFileName);
                Image img = Image.FromFile(imgFileName);

                if (extension.ToUpper().Equals(".JPG"))
                {
                    if (img.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        firstPathList.Add(imgFileName);
                        continue;
                    }
                    else if (img.RawFormat.Equals(ImageFormat.Png))
                    {

                        QuestionPath.error = "[图片本质为PNG格式]";
                        QuestionPath.path = imgFileName;
                    }
                    else if (img.RawFormat.Equals(ImageFormat.Bmp))
                    {
                        QuestionPath.error = "[图片本质为BMP格式]";
                        QuestionPath.path = imgFileName;
                    }
                    else
                    {
                        QuestionPath.error = "[图片的本质格式未知]";
                        QuestionPath.path = imgFileName;
                    }
                }
                QuestionPath.fileName = FieldInterception(imgFileName);
                img.Dispose();
                CopyFile(QuestionPath);
                File.Delete(imgFileName);
                ErrorCollectHelper.ImgErrorLog(QuestionPath);
            }
        }

        /// <summary>
        /// 获取当前路径下所有的图片
        /// </summary>
        /// <param name="imgpath">根目录路径</param>
        /// <returns>List<string>图片路径集合</returns>
        private List<string> GetImagesList(string imgpath)
        {
            string[] paths = Directory.GetFiles(imgpath, "*" + suffix);
            for (int j = 0; j < paths.Length; j++)
            {
                imgPathList.Add(paths[j]);
            }
            DirectoryInfo di = new DirectoryInfo(imgpath);
            foreach (var item in di.GetDirectories())
            {
                if (!string.IsNullOrEmpty(item.FullName))
                    GetImagesList(item.FullName);
            };
            return imgPathList;
        }

        /// <summary>
        /// 字符串截取考场号
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        private string FieldInterception(string path)
        {
            string[] paths = path.Split('\\');
            return paths[paths.Length - 2];
        }


        private static void CopyFile(QuestionModel question)
        {
            string errorpath = ErrorCollectHelper.errorParent + question.fileName;
            string[] path1 = question.path.Split('\\');
            string path2 = path1[path1.Length - 1];
            string path3 = errorpath + @"\" + path2;
            if (!Directory.Exists(errorpath))
            {
                Directory.CreateDirectory(errorpath);
            }
            File.Copy(question.path, path3, true);
        }


    }
}
