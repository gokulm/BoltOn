namespace BoltOn.Samples.Application.Entities
{
	public class StudentType 
    {
		public int StudentTypeId { get; set; }
		public string Description { get; set; }

        private StudentType()
        {
        }

        public StudentType(int id, string description)
        {
            StudentTypeId = id;
            Description = description;
        }
    }
}
