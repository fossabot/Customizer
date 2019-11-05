namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct BoneWeight {
        public int bone1, bone2, bone3, bone4;
        public float weight1, weight2, weight3, weight4;
    }
}