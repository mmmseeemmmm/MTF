namespace AutomotiveLighting.MTFCommon
{
    public enum ThreadSafeLevel
    {
        /// <summary>
        /// MTF will not call methods of class in parallel.
        /// </summary>
        No,
        /// <summary>
        /// MTF can call methods on diferent instances of class in parallel.
        /// </summary>
        Class,
        /// <summary>
        /// MTF can call methods on one instance in parallel.
        /// </summary>
        Instance
    }
}
