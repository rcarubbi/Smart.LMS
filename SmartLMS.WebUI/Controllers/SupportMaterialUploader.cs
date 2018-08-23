using System;
using System.IO;
using System.Web;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Controllers
{
    internal class SupportMaterialUploader
    {
        private readonly string _pathString;

        public SupportMaterialUploader(string classId)
        {
            _pathString = HttpContext.Current.Server.MapPath($"~/Content/Support/{classId}");
        }

        internal dynamic Upload(HttpPostedFileBase file)
        {
            var isSavedSuccessfully = true;

            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (!Directory.Exists(_pathString))
                        Directory.CreateDirectory(_pathString);

                    var path = $"{_pathString}\\{file.FileName}";
                    file.SaveAs(path);
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }

            return isSavedSuccessfully
                ? new {Success = true, Message = file?.FileName}
                : new {Success = false, Message = Resource.ErrorSavingImage};
        }

        internal void DeleteFile(string filename)
        {
            var fullPath = Path.Combine(_pathString, filename);
            File.Delete(fullPath);
        }

        internal FileInfo GetFileInfo(string filename)
        {
            var fullPath = Path.Combine(_pathString, filename);
            return new FileInfo(fullPath);
        }
    }
}