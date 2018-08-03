using System;
using System.IO;
using System.Web;

namespace SmartLMS.WebUI.Controllers
{
    internal class ImageUploader
    {
      
        private readonly string _pathString;


        public ImageUploader()
        {
            _pathString = HttpContext.Current.Server.MapPath("~/Content/img/courses");
        }
        

        internal dynamic Upload(HttpPostedFileBase file)
        {
            var isSavedSuccessfully = true;
            var tempFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            try
            {
                if (file.ContentLength > 0)
                {
                    if (!Directory.Exists(_pathString))
                        Directory.CreateDirectory(_pathString);

                    var path = Path.Combine(_pathString, tempFileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }

            return isSavedSuccessfully 
                ? new { Success = true, Message = tempFileName } 
                : new { Success = false, Message = "Error saving image" };
        }

        internal byte[] GetFile(string filename)
        {
            var fullPath = Path.Combine(_pathString, filename);
            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            else
            {
                return null;
            }
        }

        internal FileInfo GetFileInfo(string filename)
        {
            if (filename == null)
                return null;

            var fullPath = Path.Combine(_pathString, filename);
            if (File.Exists(fullPath))
            {
                return new FileInfo(fullPath);
            }
            else
            {
                return null;
            }
        }

        internal void DeleteFile(string filename)
        {
            var fullPath = Path.Combine(_pathString, filename);
            File.Delete(fullPath);
        }
    }
}