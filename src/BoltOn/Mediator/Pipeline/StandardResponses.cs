namespace BoltOn.Mediator.Pipeline
{
    public class StandardBooleanResponse
    {
        public bool IsSuccessful { get; set; }
    }

	public class StandardIdResponse<TId>
	{
		public TId Id { get; set; }
	}
}
