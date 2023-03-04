namespace ThingsGateway.Core
{
    public interface ITree<T>
    {
        public List<T> Children { get; set; }
        public long Id { get; }

        public long ParentId { get; }
    }
}