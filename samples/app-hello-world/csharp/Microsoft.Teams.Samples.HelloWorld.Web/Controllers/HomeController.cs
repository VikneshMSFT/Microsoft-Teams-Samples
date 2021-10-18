using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRemainderRepository _repo;

        public HomeController(IRemainderRepository repository)
        {
            this._repo = repository;
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        [Route("first")]
        public ActionResult First()
        {
            return View();
        }

        [Route("second")]
        public ActionResult Second()
        {
            return View();
        }

        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }

        [Route("viewmyreminders")]
        public ActionResult ViewMyReminders()
        {
            List<DependencyReminder> myReminders = this._repo.GetAllDependencyRemainders().Where(reminder => reminder.CreatedBy.Contains("vimohan") || reminder.CreatedBy.Contains("Viknesh")).ToList<DependencyReminder>();
            return View(myReminders);
        }

        [Route("viewremindersassignedtome")]
        public ActionResult ViewRemindersAssignedToMe()
        {
            
            List<DependencyReminder> myReminders = this._repo.GetAllDependencyRemainders().Where(reminder => { return reminder.UsersAssigned.Select(remainder => remainder.Mentioned.Name.Contains("Viknesh")).Count() == 1; }).ToList<DependencyReminder>();
            return View(myReminders);
        }
    }
}