namespace Owin.Scim.Tests.Integration.Users.Update.add
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;

    using Machine.Specifications;

    using Model.Users;

    public class with_path_and_null_multivaluedattribute : when_updating_a_user
    {
        static with_path_and_null_multivaluedattribute()
        {
            UserToUpdate = new User
            {
                UserName = UserNameUtility.GenerateUserName(),
                Emails = null
            };
        }

        Establish context = () =>
        {
            PatchContent = new StringContent(
                @"
                    {
                        ""schemas"": [""urn:ietf:params:scim:api:messages:2.0:PatchOp""],
                        ""Operations"": [{
                            ""op"": ""add"",
                            ""path"": ""emails"",
                            ""value"": [{
                                ""value"": ""babs@jensen.org"",
                                ""type"": ""home""
                            }]
                        }]
                    }",
                Encoding.UTF8,
                "application/json");
        };

        It should_return_ok = () => PatchResponse.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_add_attribute_value = () => UpdatedUser
            .Emails
            .SingleOrDefault(e => e.Value.Equals("babs@jensen.org"))
            .ShouldNotBeNull();

        It should_only_have_the_one_value = () => UpdatedUser
            .Emails
            .Count()
            .ShouldEqual(1);
    }
}