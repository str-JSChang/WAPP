using Microsoft.AspNetCore.Mvc;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student
{
    public class StudentDashboardModel : SessionPageModel
    {
        protected override bool MemberOnly => true;

        public void OnGet()
        {
        }
    }
}
