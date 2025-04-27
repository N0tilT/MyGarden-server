namespace EntitiesLibrary.Gardens
{
    public class Bed
    {
        public int? Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int RotationAngle { get; set; }
        public string AdditionalInfo { get; set; } = string.Empty;
        public List<int> Plants { get; set; } = [];
    }
}