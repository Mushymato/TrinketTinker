namespace TrinketTinker.Models.Mixin
{
    /// <summary>
    /// Interface for args which can change depending on the recipient of the data
    /// </summary>
    public interface IArgs
    {
        /// <summary>Checks if the given arguments are valid, potentially modify arguments to ensure they are valid</summary>
        /// <returns>true if valid</returns>
        public bool Validate();
    }

    /// <summary>
    /// No arguments are needed, Args will not be parsed.
    /// </summary>
    public class NoArgs : IArgs
    {
        /// <inheritdoc/>
        public bool Validate() => true;
    }
}