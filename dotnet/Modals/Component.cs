namespace FlexUI.Models
{
    public class Component
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty; // Initialize with default value
        public string settings { get; set; } = string.Empty; // Initialize with default value
        public int ordinal { get; set; }
    }
}