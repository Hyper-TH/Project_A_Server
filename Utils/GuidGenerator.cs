namespace Project_A_Server.Utils
{
    public class GuidGenerator
    {
        /// Generates a new GUID as a string with no dashes.
        /// <returns>A new GUID as a string in "N" format.</returns>
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}