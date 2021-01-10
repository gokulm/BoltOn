namespace BoltOn.Samples.Application.Entities
{
	public class StudentType 
    {
		public int Id { get; set; }
		public string Description { get; set; }

        private StudentType()
        {
        }

        public StudentType(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}
