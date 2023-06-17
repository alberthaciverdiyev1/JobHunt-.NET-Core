﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProjectJobHunt.DAL;
using FinalProjectJobHunt.Models;
using FinalProjectJobHunt.ViewModels;
using FinalProjectJobHunt.Models.Job;
using FinalProjectJobHunt.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinalProjectJobHunt.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace FinalProjectJobHunt.Controllers
{
    public class JobController : Controller
    {
        private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;

		public JobController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
			_userManager = userManager;
		}
        public async Task<IActionResult> Index(int page)
        {
         
            ViewBag.Page = page;
            ViewBag.Total = Math.Ceiling((decimal)_context.PostJobs.Count() / 6);

            List<Category> categories = await _context.Categories.ToListAsync();
            List<Position> positions = await _context.Positions.ToListAsync();
            List<PostJob> postJobs = await _context.PostJobs
                .Include(x => x.JobType).Skip(page * 6).Take(6)
                .Include(x => x.City)
                .Include(x => x.Category).ToListAsync();

            JobVM jobVM = new JobVM
            {
                Category = categories,
                Positions = positions,
                PostJobs = postJobs

            };

            return View(jobVM);
        }


        public async Task<IActionResult> UserIndex(int page)
        {
            ViewBag.Page = page;
            ViewBag.Total = Math.Ceiling((decimal)_context.UserPostJobs.Count() / 6);

            List<Category> categories = await _context.Categories.ToListAsync();
            List<Position> positions = await _context.Positions.ToListAsync();
            List<UserPostJob> postJobs = await _context.UserPostJobs
                .Include(x => x.JobType)
                .Skip(page * 6).Take(6)
                .Include(x => x.City)
                .Include(x => x.Category).ToListAsync();
         

            UserPostJobVM jobVM = new UserPostJobVM
            {
                Category = categories,
                Positions = positions,
                UserPostJobs = postJobs,
                
            };

            return View(jobVM);
      }
        public async Task<IActionResult> Detail(int id)
        {

            UserPostJob singleJob = await _context.UserPostJobs
                .Include(x => x.Education)
                .Include(x => x.WorkExperience)
                .Include(x => x.Category)
                .Include(x => x.JobType)
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.id == id);
            AppUser appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == singleJob.AppUserId);

            DetailVM detail = new DetailVM
            {
                UserPostJob = singleJob,
                AppUser = appUser

            };
            return View(detail);
		}
		public async Task<IActionResult> CompanyJobDetail(int id)
		{

			PostJob singleJob = await _context.PostJobs
				.Include(x => x.Education)
				.Include(x => x.WorkExperience)
				.Include(x => x.Category)
				.Include(x => x.JobType)
				.Include(x => x.City)
				.FirstOrDefaultAsync(x => x.id == id);
			AppUser appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == singleJob.AppUserId);

			DetailVM detail = new DetailVM
			{
				PostJob = singleJob,
				AppUser = appUser

			};
			return View(detail);
		}
		[Authorize]

		public async Task<IActionResult> PostJob()
        {
            if (User.Identity.IsAuthenticated)
            {
                string role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "EMPLOYER")
                {
                    return RedirectToAction("UserPostJob");
                }
            }

            List<Category> categories = await _context.Categories.ToListAsync();
            List<City> cities = await _context.Cities.ToListAsync();
            List<Language> languages = await _context.Languages.ToListAsync();
            List<Education> education = await _context.Educations.ToListAsync();
            List<WorkExperience> workExperiences = _context.WorkExperiences.ToList();
            List<JobType> jobTypes = _context.JobTypes.ToList();
            JobVM jobVM = new JobVM
            {
                Category = categories,
                Languages = languages,
                WorkExperiences = workExperiences,
                JobTypes = jobTypes,
                Educations = education,
                Cities = cities,

            };

            //ViewBag.Category = await _context.Categories.ToListAsync();
            //ViewBag.Position = await _context.Positions.ToListAsync();
            //ViewBag.Language = await _context.Languages.ToListAsync();
            //ViewBag.Education = await _context.Educations.ToListAsync();
            //ViewBag.ExpeerienceYear = await _context.ExperienceYears.ToListAsync();
            //ViewBag.WorkExperience = await _context.WorkExperiences.ToListAsync();
            //ViewBag.City = await _context.Cities.ToListAsync();
            //ViewBag.JobType = await _context.JobTypes.ToListAsync();
            return View(jobVM);
        }
        [HttpPost]
        public async Task<IActionResult> PostJob(JobVM job)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Bu Model State Errorudur");
                return View();
            }
			var currentUser = await _userManager.GetUserAsync(User);

			PostJob postJob = new PostJob
            {
                PhoneNumber = job.PhoneNumber,
                LanguageId = job.LanguageId,
                CategoryId = job.CategoryId,
                MaxAge = job.MaxAge,
                MinAge = job.MinAge,
                EducationId = job.EducationId,
                Email = job.Email,
                Experience = job.Experience,
                WorkExperienceId = job.WorkExperienceId,
                Salary = job.Salary,
                CityId = job.CityId,
                Website = job.Website,
                JobTypeId = job.JobTypeId,
                Description = job.Description,
                Created = DateTime.Now,
                AppUserId=currentUser.Id
            };
            await _context.AddAsync(postJob);
            await _context.SaveChangesAsync();



            return RedirectToAction("Index", "Job");
        }


        public async Task<IActionResult> UserPostJob()
        {


            List<Category> categories = await _context.Categories.ToListAsync();
            List<City> cities = await _context.Cities.ToListAsync();
            List<Language> languages = await _context.Languages.ToListAsync();
            List<Education> education = await _context.Educations.ToListAsync();
            List<WorkExperience> workExperiences = _context.WorkExperiences.ToList();
            List<JobType> jobTypes = _context.JobTypes.ToList();
            UserPostJobVM jobVM = new UserPostJobVM
            {
                Category = categories,
                Languages = languages,
                WorkExperiences = workExperiences,
                JobTypes = jobTypes,
                Educations = education,
                Cities = cities,

            };

            return View(jobVM);

        }
        [HttpPost]
        public async Task<IActionResult> UserPostJob(UserPostJobVM job)
        {

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Bu Model State Errorudur");
				return View();
            }
			var currentUser = await _userManager.GetUserAsync(User);

			UserPostJob postJob = new UserPostJob
            {
                PhoneNumber = job.PhoneNumber,
                LanguageId = job.LanguageId,
                CategoryId = job.CategoryId,
                Age = job.Age,
                EducationId = job.EducationId,
                Email = job.Email,
                Experience = job.Experience,
                WorkExperienceId = job.WorkExperienceId,
                Salary = job.Salary,
                CityId = job.CityId,
                JobTypeId = job.JobTypeId,
                Description = job.Description,
                Created = DateTime.Now,
                AppUserId=currentUser.Id,   

            };
            await _context.AddAsync(postJob);
            await _context.SaveChangesAsync();



            return RedirectToAction("Index", "Job");




        }
    }


}
