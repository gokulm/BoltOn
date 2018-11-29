namespace BoltOn.Mediator
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
