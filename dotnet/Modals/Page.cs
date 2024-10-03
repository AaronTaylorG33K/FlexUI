namespace FlexUI.Models
{
    public class Page
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty; // Initialize with default value
        public string slug { get; set; } = string.Empty; // Initialize with default value
        public string content { get; set; } = string.Empty; // Initialize with default value
        public List<Component> components { get; set; } = new List<Component>(); // Initialize with default value
    }
}