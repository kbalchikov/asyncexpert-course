using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipelines
{
    public class PipeLineCounter
    {
        private static int Count;

        public async Task<int> CountLines(Uri uri)
        {

            // Calculate how many lines (end of line characters `\n`) are in the network stream
            // To practice, use a pattern where you have the Pipe, Writer and Reader tasks
            // Read about SequenceReader<T>, https://docs.microsoft.com/en-us/dotnet/api/system.buffers.sequencereader-1?view=netcore-3.1
            // This struct h has a method that can be very useful for this scenario :)

            // Good luck and have fun with pipelines!

            var channel = Channel.CreateUnbounded<int>();

            await Task.WhenAll(
                ReadStreamAsync(uri, channel.Writer),
                HandleDataAsync(channel.Reader));

            return Count;
        }

        private async Task ReadStreamAsync(Uri uri, ChannelWriter<int> writer)
        {
            using var client = new HttpClient();
            await using var stream = await client.GetStreamAsync(uri);
            var reader = PipeReader.Create(stream);
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                int count = 0;
                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    count++;
                }

                await writer.WriteAsync(count);

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    writer.Complete();
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();
        }

        private async Task HandleDataAsync(ChannelReader<int> reader)
        {
            await foreach (int count in reader.ReadAllAsync())
            {
                Count += count;
            }
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }
    }
}
