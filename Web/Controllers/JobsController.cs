﻿using System;
using Microsoft.AspNetCore.Mvc;
using AppServices;
using AppServices.Services;
using Web.ViewModels;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class JobsController : BaseController
    {
        private IJobsService _jobsService;
        private ICategoriesService _categoriesService;
        private IHireTypesService _hiretypesService;
        private ITwitterService _twitterService;
        private readonly LegacyApiClient _apiClient;

        public JobsController(IJobsService jobsService, ICategoriesService categoriesService, IHireTypesService hiretypesService, ITwitterService twitterService, LegacyApiClient apiClient)
        {
            _jobsService = jobsService;
            _categoriesService = categoriesService;
            _hiretypesService = hiretypesService;
            _twitterService = twitterService;
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string keyword = "", bool isRemote = false)
        {
            var legacyJobs =  await _apiClient.GetJobsFromLegacy();
            var viewModel = new JobSeachViewModel
            {
                Keyword = keyword,
                IsRemote = isRemote,
                //Jobs = filteredJobs,
                JobCards = legacyJobs
            };
            return View(viewModel);
        }

       [Authorize]
        public IActionResult Wizard()
        {
            var model = new WizardViewModel
            {
                Categories = _categoriesService.GetAll(),
                JobTypes = _hiretypesService.GetAll()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Wizard(WizardViewModel model)
        {
            if (ModelState.IsValid)
            {
                var url = $"{this.Request.Scheme}://{this.Request.Host}/";
                //System.Threading.Tasks.Task.Run(()=>_twitterService.Tweet($"Se busca: {model.Title} para mas información, dirigirse a emplea.do {url} "));

                return RedirectToAction("", "");
            }
            return View(model);
        }


        public async Task<ActionResult> Details(string Id, bool isPreview)
        {

            if (String.IsNullOrEmpty(Id))
                return RedirectToAction(nameof(this.Index));

            //int jobId = this.GetJobIdFromTitle(Id);

            //if (jobId == 0)
            //    return RedirectToAction(nameof(this.Index));

            int jobId = Int32.Parse(Id);
            var job = await _apiClient.GetJobById(Id);//this._jobsService.GetDetails(jobId, isPreview);

            //Manage error message
            if (job == null)
                return RedirectToAction(nameof(this.Index));
            
            //If reach this line is because the job exists
            var viewModel = new JobDetailsViewModel
            {
                JobCard = job
            };

            if (isPreview)
            {
                viewModel.IsPreview = isPreview;
                return View(viewModel);
            }
            return View(viewModel);
        }


        private int GetJobIdFromTitle(string title)
        {
            var url = title.Split('-');
            if (String.IsNullOrEmpty(title) || title.Length == 0 || !int.TryParse(url[0], out int id))
                return 0;
            return id;
        }

    }
}
