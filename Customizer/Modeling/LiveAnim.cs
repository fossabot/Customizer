using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Stores animation data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public class LiveAnim {
        /// <summary>
        /// The unique name of the mesh originally authored against
        /// </summary>
        [CzSerialize(0)] public string SourceUniqueName;

        /// <summary>
        /// The bones modified in this animation
        /// </summary>
        [CzSerialize(1)] public Bone[] Bones;

        /// <summary>
        /// Animations per bone
        /// </summary>
        [CzSerialize(2)] public LiveSubAnim[] BoneSubAnims;
    }
}