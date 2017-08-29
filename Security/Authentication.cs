using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Mars.PerfectFit.Presentation.Web.Security
{
    public  class Authentication
    {
        public static void SetCookie(Core.Domain.Models.Registration.User user, double expirateHour, bool rememberMe)
        {
            string userData =user.LoginField.ConsumerID + "#" + user.LoginField.Username+ "#" +user.ConsumerField.Firstname;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                           user.LoginField.Username,
                           DateTime.Now,
                           DateTime.Now.AddHours(24),
                           rememberMe,
                           userData,
                           FormsAuthentication.FormsCookiePath
                           );

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            //expiration
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            // Create the cookie.
            //response.Cookies.Add(cookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static bool isLogin()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public static void logOut()
        {
            FormsAuthentication.SignOut();
        }

        public static string GetUsername()
        {
            if (isLogin())
            {
                string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;

                string[] UserData = strUserData.Split('#');

                if (UserData.Length != 0)
                {
                    return UserData[1].ToString();
                }
                else
                {
                    return "";
                }
            }else
            {
                return "";
            }

        }


        public static string GetConsumerId()
        {
            if (isLogin())
            {
                string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;

                string[] UserData = strUserData.Split('#');

                if (UserData.Length != 0)
                {
                    return UserData[0].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

        }


        public static string GetUserFirstName()
        {
            if (isLogin())
            {
               // string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;


                FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                FormsAuthenticationTicket ticket = id.Ticket;

                string[] UserData = ticket.UserData.Split('#');

                if (UserData.Length != 0)
                {
                    return UserData[2].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

        }
    }
}