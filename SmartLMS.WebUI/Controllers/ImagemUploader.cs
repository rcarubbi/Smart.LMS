using System;
using System.IO;
using System.Web;

namespace SmartLMS.WebUI.Controllers
{
    internal class ImagemUploader
    {
      
        private string _pathString;


        public ImagemUploader()
        {
            _pathString = HttpContext.Current.Server.MapPath("~/Content/img/cursos");
        }
        

        internal dynamic Upload(HttpPostedFileBase file)
        {
            bool isSavedSuccessfully = true;
            string tempFileName = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(file.FileName), Guid.NewGuid(), Path.GetExtension(file.FileName));
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    
                    if (!Directory.Exists(_pathString))
                        Directory.CreateDirectory(_pathString);

                    var path = string.Format("{0}\\{1}", _pathString, tempFileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return new { Success = true, Message = tempFileName };
            }
            else
            {
                return new { Success = false, Message = "Erro ao salvar a imagem" };
            }
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