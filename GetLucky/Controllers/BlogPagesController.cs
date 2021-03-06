﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using GetLucky.Models;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;

namespace GetLucky.Controllers
{

    public class BlogPagesController : BaseApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/BlogPages
        public IQueryable<BlogPage> GetBlogPages()
        {
            return db.BlogPages;
        }

        // GET: api/BlogPages/?offset=0&limit=4
        [HttpGet]
        [ResponseType(typeof(BlogPage))]
        public async Task<IHttpActionResult> GetBlogPages(int offset, int limit)
        {
            var userName = User.Identity.Name;
            var user = await AppUserManager.FindByNameAsync(userName);
            var role = await AppRoleManager.FindByIdAsync(user.Roles.ToList()[0].RoleId);
            IQueryable<BlogPage> blogPages;

            switch (role.Name)
            {
                case "Admin":
                    blogPages = db.BlogPages.OrderByDescending((s) => s.Id).Skip(offset).Take(limit);
                    break;
                case "PremiumUser":
                    blogPages = db.BlogPages.Where((item) => item.UserName != "Admin" ).OrderByDescending((s) => s.Id).Skip(offset).Take(limit);
                    break;
                case "User":
                    blogPages = db.BlogPages.Where((item) => item.UserName == userName).OrderByDescending((s) => s.Id).Skip(offset).Take(limit);
                    break;
                default:
                    return Ok();
            }

            //if (blogPages == null)
            //{
            //    return NotFound();
            //}

            return Ok(blogPages);
        }

        // GET: api/BlogPages/5
        [ResponseType(typeof(BlogPage))]
        public IHttpActionResult GetBlogPage(int id)
        {
            BlogPage blogPage = db.BlogPages.Find(id);
            if (blogPage == null)
            {
                return NotFound();
            }

            return Ok(blogPage);
        }

        // PUT: api/BlogPages/5
        [Authorize]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBlogPage([FromUri]int id, [FromBody]BlogPage blogPage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blogPage.Id)
            {
                return BadRequest();
            }

            db.Entry(blogPage).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }



        //POST: api/BlogPages

        [Authorize]
        public async Task<IHttpActionResult> PostFormData()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string imageFolder = HttpContext.Current.Server.MapPath("~/img/collage");
            var provider = new CustomMultipartFormDataStreamProvider(imageFolder);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                BlogPage bp = new BlogPage();
                bp.Title = provider.FormData.GetValues("title")[0];
                bp.Content = provider.FormData.GetValues("content")[0];
                bp.PageName = provider.FormData.GetValues("caption")[0];
                bp.Date = DateTimeOffset.Parse(provider.FormData.GetValues("date")[0]);
                bp.UserName = provider.FormData.GetValues("userName")[0];
                string name = provider.FileData[0].LocalFileName;
                
                bp.PicturePath = "../img/collage/" + provider.FileData[0].LocalFileName.Split(new char[] { '\\'}).Last();
                

                db.BlogPages.Add(bp);
                db.SaveChanges();
                return Ok();
            }
            catch (System.Exception e)
            {
                return InternalServerError(e);
            }
        }

        // DELETE: api/BlogPages/5
        [Authorize(Roles = "admin")]
        [ResponseType(typeof(BlogPage))]
        public IHttpActionResult DeleteBlogPage(int id)
        {
            BlogPage blogPage = db.BlogPages.Find(id);
            if (blogPage == null)
            {
                return NotFound();
            }

            db.BlogPages.Remove(blogPage);
            db.SaveChanges();

            return Ok(blogPage);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlogPageExists(int id)
        {
            return db.BlogPages.Count(e => e.Id == id) > 0;
        }
    }

    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProvider(string path) : base(path)
        { }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentType == null) return base.GetStream(parent, headers); 

            string[] allowExtensions = new[] { ".png", ".jpg", ".gif" };
            string extension = Path.GetExtension(headers.ContentDisposition.FileName.Replace("\"", string.Empty));
            
            return allowExtensions.Any(i => i.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                       ? base.GetStream(parent, headers)
                       : Stream.Null;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            
            string oldName = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            return Guid.NewGuid().ToString() + Path.GetExtension(oldName);
        }

    }
}