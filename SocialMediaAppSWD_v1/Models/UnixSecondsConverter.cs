using Google.Cloud.Firestore;

namespace SocialMediaAppSWD_v1.Models;

public class UnixSecondsConverter : IFirestoreConverter<DateTimeOffset>
{
    public object ToFirestore(DateTimeOffset value)
    {
        return value.ToUnixTimeSeconds();
    }

    public DateTimeOffset FromFirestore(object value)
    {
        if (value is not long seconds)
        {
            throw new ArgumentException("value is not a long");
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }
}