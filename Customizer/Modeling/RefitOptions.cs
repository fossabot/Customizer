namespace Customizer.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CA1815")]
    public struct RefitOptions {
        public float FarField { get; set; }

        public RefitOptions(float farField) {
            FarField = farField;
        }

        public static readonly RefitOptions Default = new RefitOptions(100.0f);
    }
}