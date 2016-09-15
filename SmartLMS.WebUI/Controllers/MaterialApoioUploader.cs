using System;
using System.IO;
using System.Web;

namespace SmartLMS.WebUI.Controllers
{
    internal class MaterialApoioUploader
    {
        private string _idAula;
        private string _pathString;

        public MaterialApoioUploader(string idAula)
        {
            _idAula = idAula;
            _pathString = HttpContext.Current.Server.MapPath($"~/Content/apoio/{_idAula}");
        }

        internal dynamic Upload(HttpPostedFileBase file)
        {
            bool isSavedSuccessfully = true;
          
            try
            {
                if (file != null && file.ContentLength > 0)
                {

                    if (!Directory.Exists(_pathString))
                        Directory.CreateDirectory(_pathString);

                    var path = string.Format("{0}\\{1}", _pathString, file.FileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return new { Success = true, Message = file.FileName };
            }
            else
            {
                return new { Success = false, Message = "Erro ao salvar a imagem" };
            }
        }

        internal void DeleteFile(string nomeArquivo)
        {
            var fullPath = Path.Combine(_pathString, nomeArquivo);
            File.Delete(fullPath);
        }

        internal FileInfo GetFileInfo(string nomeArquivo)
        {
            var fullPath = Path.Combine(_pathString, nomeArquivo);
            return new FileInfo(fullPath);
        }
    }
}