namespace CustomNavi.Utility {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct MutableKeyValuePair<T1, T2> {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public MutableKeyValuePair(T1 v1, T2 v2) {
            Item1 = v1;
            Item2 = v2;
        }
    }
}