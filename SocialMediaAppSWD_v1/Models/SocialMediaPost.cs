namespace SocialMediaAppSWD_v1.Models
{
    using Google.Cloud.Firestore;
    using System.ComponentModel.DataAnnotations;

    [FirestoreData]
    public class SocialMediaPost
    {
        [Required]
        [FirestoreProperty]
        public string PostId { get; set; }

        [FirestoreProperty]
        public string PostContent { get; set; }

        [FirestoreProperty]
        public string PostAuthor { get; set; }

        [FirestoreProperty]
        public string PostDate { get; set; }

        //TODO: Add image/video
    }
}
