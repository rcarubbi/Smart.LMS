using SmartLMS.Domain;
using SmartLMS.Domain.Repositories;
using SmartLMS.WebUI.Models;
using System;
using System.Net;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KnowledgeAreaController : BaseController
    {
        public KnowledgeAreaController(IContext context)
            : base(context)
        {

        }

        // GET: KnowledgeArea
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            ViewBag.SearchFields = new SelectList(new string[] { "Name", "Id" });
            return View(KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            return Json(KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            knowledgeAreaRepository.Delete(new Guid(id));
            _context.Save(_loggedUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Create()
        {
            return View();
        }


        // POST: teacher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KnowledgeAreaViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
                knowledgeAreaRepository.Create(KnowledgeAreaViewModel.ToEntity(viewModel));
                _context.Save(_loggedUser);
                TempData["MessageType"] = "success";
                TempData["MessageTitle"] = "Content management";
                TempData["Message"] = "Knowledge Area created";
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = "Content management";
                TempData["Message"] = ex.Message;
            }

            return View(viewModel);
        }

        public ActionResult Edit(Guid id)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var area = knowledgeAreaRepository.GetById(id);
            return View(KnowledgeAreaViewModel.FromEntity(area, 0));
        }

        [HttpPost]
        public ActionResult Edit(Guid id, KnowledgeAreaViewModel viewModel)
        {

            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            if (ModelState.IsValid)
            {
                try
                {
                    knowledgeAreaRepository.Update(KnowledgeAreaViewModel.ToEntity(viewModel));
                    _context.Save(_loggedUser);
                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = "Knowledge Area updated";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = ex.Message;
                }
            }


            return View(viewModel);

        }

    }
}