namespace System.IO.BACnet.Serialize.Decode
{
    public static class Result
    {
        public static Result<T> Create<T>(T value, int length)
            => new Result<T>(value, length);
    }

    public abstract class ResultBase
    {
        public int Length { get; protected set; }

        public abstract object ValueAsObject { get; }
    }

    public class Result<T> : ResultBase
    {
        public T Value { get; }
        public override object ValueAsObject
            => Value;

        public Result(T value, int length)
        {
            Value = value;
            Length = length;
        }

        public Result<T> AddLength(int length)
        {
            Length += length;
            return this;
        }
    }
}
