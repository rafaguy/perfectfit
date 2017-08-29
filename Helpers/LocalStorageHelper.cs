
using System;
using System.Web;
using System.Web.Security;

namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    public  class LocalStorageHelper
    {

        public static void SetCookie(Core.Domain.Models.Registration.Login login, double expirateHour, bool rememberMe)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                           login.Username,
                           DateTime.Now,
                           DateTime.Now.AddHours(24),
                           rememberMe,
                           login.SiteID.ToString(),
                           FormsAuthentication.FormsCookiePath
                           );

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            //expiration
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            if (ticket.IsPersistent) {
                cookie.Expires = ticket.Expiration;
            }

            // Create the cookie.
            //response.Cookies.Add(cookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        
        public static FormsAuthenticationTicket getCookieValue()
        {
            /*https://msdn.microsoft.com/en-us/library/system.web.security.formsauthenticationticket.userdata(v=vs.110).aspx*/

            FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
            FormsAuthenticationTicket ticket = id.Ticket;
            return ticket;
        }
    }
}