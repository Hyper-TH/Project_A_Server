namespace Project_A_Server.Utils
{
    public class ColorGenerator
    {
        public static string GenerateColor()
        {
            Random random = new Random();
            string color = $"#{random.Next(0x1000000):X6}";

            return color;
        }
    }
}
