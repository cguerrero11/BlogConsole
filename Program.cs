using System;
using NLog.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BlogsConsole
{
    class Program
    {
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {

                string choice;
                do
                {
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1) Display all blogs");
                    Console.WriteLine("2) Add Blog");
                    Console.WriteLine("3) Create Post");
                    Console.WriteLine("4) Display Posts");
                    Console.WriteLine("Enter q to quit");
                    choice = Console.ReadLine();
                    Console.Clear();

                    if (choice == "1")
                    {
                        // display blogs
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.Name);

                        Console.WriteLine($"{query.Count()} Blogs returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.Name);
                        }
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Enter a name for a new Blog: ");
                        var blog = new Blog { Name = Console.ReadLine() };

                        ValidationContext context = new ValidationContext(blog, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(blog, context, results, true);
                        if (isValid)
                        {
                            var db = new BloggingContext();
                            if (db.Blogs.Any(b => b.Name == blog.Name))
                            {
                                isValid = false;
                                results.Add(new ValidationResult("Blog name exists", new string[] { "Name" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db.AddBlog(blog);
                                logger.Info("Blog added - {name}", blog.Name);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.BlogId);

                        Console.WriteLine("Select the blog you would to post to:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.BlogId}) {item.Name}");
                        }
                        if (int.TryParse(Console.ReadLine(), out int BlogId))
                        {
                            if (db.Blogs.Any(b => b.BlogId == BlogId))
                            {
                                Post post = CreatePost(db);
                                if (post != null)
                                {
                                    post.BlogId = BlogId;
                                    db.AddPost(post);
                                    logger.Info("Post added - {title}", post.Title);
                                }
                            }
                            else
                            {
                                logger.Error("There are no Blogs saved with that Id");
                            }
                        }
                        else
                        {
                            logger.Error("Invalid Blog Id");
                        }
                    }
                    else if (choice == "4")
                    {
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.BlogId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.BlogId}) Posts from {item.Name}");
                        }

                        if (int.TryParse("0", out int BlogId))
                        {
                            IEnumerable<Post> Posts;
                            if (BlogId != 0 && db.Blogs.Count(b => b.BlogId == BlogId) == 0)
                            {
                                logger.Error("No matching Blogs");
                            }
                            else
                            {
                                Posts = db.Posts.OrderBy(p => p.Title);
                                if (BlogId == 0)
                                {
                                    Posts = db.Posts.OrderBy(p => p.Title);
                                }
                                else
                                {
                                    Posts = db.Posts.Where(p => p.BlogId == BlogId).OrderBy(p => p.Title);
                                }
                                Console.WriteLine($"{Posts.Count()} post(s) returned");
                                foreach (var item in Posts)
                                {
                                    Console.WriteLine($"Blog: {item.Blog.Name}\nTitle: {item.Title}\nContent: {item.Content}\n");
                                }
                            }
                        }
                        else
                        {
                            logger.Error("Invalid Blog Id");
                        }
                    }
                    Console.WriteLine();
                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Post CreatePost(BloggingContext db)
        {
            Post post = new Post();
            Console.WriteLine("Enter the Post title");
            post.Title = Console.ReadLine();
            Console.WriteLine("Enter the Post content");
            post.Content = Console.ReadLine();

            ValidationContext context = new ValidationContext(post, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(post, context, results, true);
            if (isValid)
            {
                return post;
            }
            else
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
            return null;
        }

        public static Post GetPost(BloggingContext db)
        {
            var blogs = db.Blogs.OrderBy(b => b.Name);
            foreach (Blog b in blogs)
            {
                Console.WriteLine(b.Name);
                if (b.Posts.Count() == 0)
                {
                    Console.WriteLine($"  <no posts>");
                }
                else
                {
                    foreach (Post p in b.Posts)
                    {
                        Console.WriteLine($"  {p.PostId}) {p.Title}");
                    }
                }
            }
            if (int.TryParse(Console.ReadLine(), out int PostId))
            {
                Post post = db.Posts.FirstOrDefault(p => p.PostId == PostId);
                if (post != null)
                {
                    return post;
                }
            }
            logger.Error("Invalid Post Id");
            return null;
        }
    }
}
