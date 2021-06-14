namespace Synchronization.Benchmark
{
    public interface IQueue<T>
    {
        T Dequeue();
        void Enqueue(T item);
        T Peek();
    }
}