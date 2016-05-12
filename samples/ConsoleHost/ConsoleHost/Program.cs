﻿namespace ConsoleHost
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    using Nito.AsyncEx;

    using Owin.Scim.Model.Users;

    class Program
    {
        static void Main(string[] args)
        {
            using (Microsoft.Owin.Hosting.WebApp.Start<CompositionRoot>("http://+:8080"))
            {
                AsyncContext.Run(TestScimApi);
                Console.ReadLine();
            }
        }

        private static async Task TestScimApi()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/scim/")
            };

            Write("");
            Write("Creating user ...");
            var response = await client.PostAsync("users", new ObjectContent<User>(new User { UserName = "daniel" }, new JsonMediaTypeFormatter()));
            var user = await response.Content.ReadAsAsync<User>(new[] { new JsonMediaTypeFormatter() });
            Write(await response.Content.ReadAsStringAsync());
            Write("");


            Write("Getting user " + user.Id);
            var json = await client.GetStringAsync("users/" + user.Id);
            Write(json);
            Write("");



            Write("");
            Write("Creating custom resource type, tenant ...");
            response = await client.PostAsync("tenants", new ObjectContent<Tenant>(new Tenant { Name = "mytenant" }, new JsonMediaTypeFormatter()));
            var tenant = await response.Content.ReadAsAsync<Tenant>(new[] { new JsonMediaTypeFormatter() });
            Write(await response.Content.ReadAsStringAsync());
            Write("");


            Write("Getting custom resource type, tenant " + tenant.Id);
            json = await client.GetStringAsync("tenants/" + tenant.Id);
            Write(json);
            Write("");

        }

        private static void Write(string text)
        {
            Console.WriteLine(text);
        }
    }
}
