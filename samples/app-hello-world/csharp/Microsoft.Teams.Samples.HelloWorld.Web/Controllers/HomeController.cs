using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
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

        [Route("viewmyreminders/{alias}")]
        public ActionResult ViewMyReminders()
        {
            List<DependencyReminder> myReminders = DependencyDataStore.RemindersListDataStore.Where(reminder => reminder.CreatedBy.Contains("pryada")).ToList<DependencyReminder>();
            return View(myReminders);
        }
    }
}