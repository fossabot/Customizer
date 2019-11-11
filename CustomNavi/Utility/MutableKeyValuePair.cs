namespace CustomNavi.Utility {
    /// <summary>
    /// Mutable key-value pair for serialization
    /// </summary>
    /// <typeparam name="T1">Instance key type</typeparam>
    /// <typeparam name="T2">Instance value type</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct MutableKeyValuePair<T1, T2> {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public MutableKeyValuePair(T1 v1, T2 v2) {
            Item1 = v1;
            Item2 = v2;
        }

        public void Deconstruct(out T1 item1, out T2 item2) {
            item1 = Item1;
            item2 = Item2;
        }
    }
}