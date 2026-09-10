namespace Services.Constants
{
    public static class Permissions
    {
        public static class Parameter
        {
            public const string Read = "parameters.read";
            public const string Update = "parameters.update";
        }

        public static class User
        {
            public const string Read = "users.read";
            public const string Create = "users.create";
            public const string Update = "users.update";
            public const string Delete = "users.delete";
            public const string ResetPassword = "users.resetpassword";
        }

        public static class Role
        {
            public const string Read = "roles.read";
            public const string Create = "roles.create";
            public const string Update = "roles.update";
            public const string Delete = "roles.delete";
        }

    }
}
