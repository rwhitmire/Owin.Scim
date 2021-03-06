namespace Owin.Scim.Tests.Integration.SchemaExtensions
{
    using System.Net;
    using System.Net.Http;
    using System.Text;

    using Machine.Specifications;

    using Model;
    using Model.Users;

    using Users;

    using v2.Model;

    public class with_patch_custom_user : using_a_scim_server
    {
        Establish context = () =>
        {
            var existingUser = new ScimUser2
            {
                UserName = UserNameUtility.GenerateUserName()
            };

            Response = Server
                .HttpClient
                .PostAsync("v2/users", new ScimObjectContent<ScimUser>(existingUser))
                .Result;

            UserDto = Response.Content.ScimReadAsAsync<ScimUser2>().Result;

           
            PatchContent = new StringContent(
                @"
                    {
                        ""schemas"": [""urn:ietf:params:scim:api:messages:2.0:PatchOp"",
                                      ""urn:scim:mycustom:schema:1.0:User""],
                        ""Operations"": [{
                            ""op"": ""add"",
                            ""path"": ""urn:scim:mycustom:schema:1.0:User:guid"",
                            ""value"": ""something new""
                        },
                        {
                            ""op"": ""add"",
                            ""path"": ""urn:scim:mycustom:schema:1.0:User:$ref"",
                            ""value"": ""http://localhost/users/12345""
                        },
                        {
                            ""op"": ""replace"",
                            ""path"": ""urn:scim:mycustom:schema:1.0:User:enablehelp"",
                            ""value"": false
                        },
                        {
                            ""op"": ""remove"",
                            ""path"": ""urn:scim:mycustom:schema:1.0:User:enddate""
                        },
                        {
                            ""op"": ""add"",
                            ""path"": ""urn:scim:mycustom:schema:1.0:User:complexdata.value"",
                            ""value"": ""its complicated""
                       }]
                    }",
                Encoding.UTF8,
                "application/json");
        };

        Because of = () =>
        {
            Response = Server
                .HttpClient
                .SendAsync(
                    new HttpRequestMessage(
                        new HttpMethod("PATCH"), "v2/users/" + UserDto.Id)
                    {
                        Content = PatchContent
                    })
                .Result;
            
            if (Response.StatusCode == HttpStatusCode.OK)
                UpdatedUser = Response.Content.ScimReadAsAsync<ScimUser2>().Result;
        };

        It should_return_ok = () => Response.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_return_new_version = () => UpdatedUser.Meta.Version.ShouldNotEqual(UserDto.Meta.Version);

        It should_return_guid = () =>
            UpdatedUser
                .Extension<MyUserSchema>()
                .Guid
                .ShouldEqual("something new");

        It should_replace_enablehelp = () =>
            UpdatedUser
                .Extension<MyUserSchema>()
                .EnableHelp
                .ShouldEqual(false);

        It should_delete_enddate = () =>
            UpdatedUser
                .Extension<MyUserSchema>()
                .EndDate
                .ShouldBeNull();

        It should_add_complexdata = () =>
            UpdatedUser
                .Extension<MyUserSchema>()
                .ComplexData
                .Value
                .ShouldEqual("its complicated");

        protected static ScimUser UserDto;

        protected static ScimUser UpdatedUser;

        protected static HttpContent PatchContent;

        protected static HttpResponseMessage Response;

        protected static ScimError Error;
    }
}