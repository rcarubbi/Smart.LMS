using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Repositories;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class SubjectController : BaseController
    {
        public SubjectController(IContext contexto)
            : base(contexto)
        {

        }

        [AllowAnonymous]
        public ActionResult Index(Guid id)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var activeKnowledgeAreas = knowledgeAreaRepository.ListActiveKnowledgeAreas();
            var selectedKnowledgeArea = activeKnowledgeAreas.Single(x => x.Id == id);
            if (selectedKnowledgeArea == null)
            {
                TempData["MessageType"] = "warning";
                TempData["MessageTitle"] = "Warning";
                TempData["Message"] = "This knowledge area is not available at this moment";
                return RedirectToAction("Index", "Home");
            }
            var viewModel = KnowledgeAreaViewModel.FromEntity(selectedKnowledgeArea, 2);
            ViewBag.OtherKnowledgeAreas = new SelectList(activeKnowledgeAreas.Except(new List<KnowledgeArea> { selectedKnowledgeArea }), "Id", "Name");
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult KnowledgeAreaPanel(Guid id)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var knowledgeArea = knowledgeAreaRepository.GetById(id);
            var viewModel = KnowledgeAreaViewModel.FromEntity(knowledgeArea, 2);
            return PartialView("_KnowledgeAreaPanel", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            var subjectRepository = new SubjectRepository(_context);
            ViewBag.SearchFields = new SelectList(new string[] { "Name", "Knowledge Area", "Id" });
            return View(SubjectViewModel.FromEntityList(subjectRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            var subjectRepository = new SubjectRepository(_context);
            return Json(SubjectViewModel.FromEntityList(subjectRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Excluir(string id)
        {
            var subjectRepository = new SubjectRepository(_context);
            subjectRepository.Delete(new Guid(id));
            _context.Save(_loggedUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var knowledgeAreas = knowledgeAreaRepository.ListActiveKnowledgeAreas();
            ViewBag.KnowledgeAreas = new SelectList(knowledgeAreas, "Id", "Name");
            return View();
        }


        // POST: teacher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(SubjectViewModel viewModel)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            if (ModelState.IsValid)
            {
                try
                {
                 
                    var knowledgeArea = knowledgeAreaRepository.GetById(viewModel.KnowledgeAreaId);
                    var subjectRepository = new SubjectRepository(_context);
                    subjectRepository.Create(SubjectViewModel.ToEntity(viewModel, knowledgeArea));
                    _context.Save(_loggedUser);

                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = "Subject created";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = ex.Message;
                }
            }

           
            var knowledgeAreas = knowledgeAreaRepository.ListActiveKnowledgeAreas();
            ViewBag.KnowledgeAreas = new SelectList(knowledgeAreas, "Id", "Name");

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Guid id)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);

            var knowledgeAreas = knowledgeAreaRepository.ListActiveKnowledgeAreas();
            ViewBag.KnowledgeAreas = new SelectList(knowledgeAreas, "Id", "Name");

            var subjectRepository = new SubjectRepository(_context);
            var subject = subjectRepository.GetById(id);
            return View(SubjectViewModel.FromEntity(subject, 0));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(Guid id, SubjectViewModel viewModel)
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var subjectRepository = new SubjectRepository(_context);
          

            if (ModelState.IsValid)
            {
                try
                {
                  
                    var knowledgeArea = knowledgeAreaRepository.GetById(id);
                    subjectRepository.Update(SubjectViewModel.ToEntity(viewModel, knowledgeArea));
                    _context.Save(_loggedUser);

                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = "Subject updated";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = ex.Message;
                }
            }

            var knowledgeAreas = knowledgeAreaRepository.ListActiveKnowledgeAreas();
            ViewBag.KnowledgeAreas = new SelectList(knowledgeAreas, "Id", "Name");
            return View(viewModel);
        }
    }
}
