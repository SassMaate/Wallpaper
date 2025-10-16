namespace Sucrose.XamlAnimatedGif.Decoding
{
    [Serializable]
    public abstract class GifDecoderException : Exception
    {
        protected GifDecoderException(string message) : base(message) { }
        protected GifDecoderException(string message, Exception inner) : base(message, inner) { }
    }
}