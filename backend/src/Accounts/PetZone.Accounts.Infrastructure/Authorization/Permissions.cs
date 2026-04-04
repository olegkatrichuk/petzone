namespace PetZone.Accounts.Infrastructure.Authorization;

public static class Permissions
{
    public static class Volunteers
    {
        public const string Create = "volunteers.create";
        public const string Update = "volunteers.update";
        public const string Delete = "volunteers.delete";
        public const string Read = "volunteers.read";
    }

    public static class Pets
    {
        public const string Create = "pets.create";
        public const string Update = "pets.update";
        public const string Delete = "pets.delete";
        public const string Read = "pets.read";
        public const string UploadPhotos = "pets.upload-photos";
        public const string DeletePhotos = "pets.delete-photos";
        public const string SetMainPhoto = "pets.set-main-photo";
        public const string Move = "pets.move";
        public const string UpdateStatus = "pets.update-status";
    }

    public static class Species
    {
        public const string Read = "species.read";
        public const string Create = "species.create";
        public const string Delete = "species.delete";
    }

    public static class News
    {
        public const string Read = "news.read";
        public const string Create = "news.create";
        public const string Update = "news.update";
        public const string Delete = "news.delete";
    }
}