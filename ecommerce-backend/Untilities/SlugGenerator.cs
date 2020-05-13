using System.Text;
using System.Text.RegularExpressions;

namespace EcommerceApi.Untilities
{
    public class SlugGenerator
    {
        private const int MAX_LENGTH = 75;
        public static string ToSlug(string title)
        {
            var match = Regex.Match(title.ToLower(), "[\\w]+");
            StringBuilder result = new StringBuilder("");
            bool maxLengthHit = false;
            while (match.Success && !maxLengthHit)
            {
                if (result.Length + match.Value.Length <= MAX_LENGTH)
                {
                    result.Append(match.Value + "-");
                }
                else
                {
                    maxLengthHit = true;
                    // Handle a situation where there is only one word and it is greater than the max length.
                    if (result.Length == 0) result.Append(match.Value.Substring(0, MAX_LENGTH));
                }
                match = match.NextMatch();
            }
            // Remove trailing '-'
            if (result[result.Length - 1] == '-') result.Remove(result.Length - 1, 1);
            return result.ToString();
        }
    }
}
