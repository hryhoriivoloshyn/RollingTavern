﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Middleware
{
    public class UserDestroyerMiddleware
    {
        private readonly RequestDelegate _next;

        public UserDestroyerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            if (!string.IsNullOrEmpty(httpContext.User.Identity.Name))
            {
                var user = await userManager.FindByNameAsync(httpContext.User.Identity.Name);

                if (user.LockoutEnd > DateTimeOffset.Now)
                {
                    //Log the user out and redirect back to homepage
                    await signInManager.SignOutAsync();
                    httpContext.Response.Redirect("/");
                }
            }
            await _next(httpContext);
        }
    }
}
