﻿namespace Owin.Scim.Tests.Integration.Users.Create
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using Machine.Specifications;

    using Model;

    using Ploeh.AutoFixture;

    public class with_missing_enterprise_schema : using_a_scim_server
    {
        Establish context = () =>
        {
            var autoFixture = new Fixture();

            UserDto = new MyUser()
            {
                Schemas = new[] { @"urn:ietf:params:scim:schemas:core:2.0:User" },
                UserName = UserNameUtility.GenerateUserName(),
                Enterprise = autoFixture.Build<MyEnterprise>()
                    .With(x => x.Manager, autoFixture.Build<MyManager>()
                        .With(y => y.Ref, @"../Users/123")
                        .Create())
                    .Create()
            };
        };

        Because of = () =>
        {
            Response = Server
                .HttpClient
                .PostAsync("users", new ObjectContent<MyUser>(UserDto, new ScimJsonMediaTypeFormatter()))
                .Result;

            StatusCode = Response.StatusCode;

            Error = StatusCode != HttpStatusCode.BadRequest ? null : Response.Content
                .ReadAsAsync<IEnumerable<ScimError>>(ScimJsonMediaTypeFormatter.AsArray())
                .Result
                .Single();
        };

        It should_return_created = () => StatusCode.ShouldEqual(HttpStatusCode.Created);

        It should_ignore_enterprise_schema = () =>
        {
            var createdUser = Response.Content
                .ReadAsAsync<MyUser>(ScimJsonMediaTypeFormatter.AsArray())
                .Result;

            createdUser.Id.ShouldNotBeNull();
            createdUser.Enterprise.ShouldBeNull();
        };

        protected static MyUser UserDto;

        protected static HttpResponseMessage Response;

        protected static HttpStatusCode StatusCode;

        protected static ScimError Error;

        public class MyUser
        {
            public string[] Schemas { get; set; }
            public string UserName { get; set; }
            public string Id { get; set; }

            [Newtonsoft.Json.JsonProperty(PropertyName = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User")]
            public MyEnterprise Enterprise { get; set; }
        }

        public class MyEnterprise
        {
            public string EmployeeNumber { get; set; }
            public string CostCenter { get; set; }
            public string Organization { get; set; }
            public string Division { get; set; }
            public string Department { get; set; }
            public MyManager Manager { get; set; }
        }

        public class MyManager
        {
            public string Value { get; set; }
            [Newtonsoft.Json.JsonProperty(PropertyName = "$ref")]
            public string Ref { get; set; }
            public string DisplayName { get; set; }
        }
    }
}