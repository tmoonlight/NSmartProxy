using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSmartProxy.Infrastructure.Extensions
{
    public class PeekableBufferBlock<T>
    {
        private BufferBlock<T> innerBufferBlock;
        private Queue<T> innerQueue;

        public void Post(T item)
        {
            innerQueue.Enqueue(item);
            innerBufferBlock.Post(item);
        }

        public async Task<T> ReceiveAsync()
        {

            await innerBufferBlock.ReceiveAsync();
            return innerQueue.Dequeue();
        }

        public T Receive()
        {

            innerBufferBlock.Receive();
            return innerQueue.Dequeue();
        }

        public PeekableBufferBlock()
        {
            innerBufferBlock = new BufferBlock<T>();
            innerQueue = new Queue<T>();
        }

        public int Count => innerBufferBlock.Count;

        public T Peek()
        {
            if (innerQueue.Count == 0) return default(T);
            return innerQueue.Peek();
        }
    }
}
