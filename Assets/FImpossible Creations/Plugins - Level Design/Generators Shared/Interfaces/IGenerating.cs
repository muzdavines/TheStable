using UnityEngine;

namespace FIMSpace.Generating
{
    public interface IGenerating
    {
        //GameObject GetObject { get; }
        void Generate();
        /// <summary> Optional preview, can be empty </summary>
        void PreviewGenerate();
        /// <summary> (can be empty if not required) Extra object aligment operations which should be executed when scene objects are spawned by all other generators and physics can be simulated correctly now. </summary>
        void IG_CallAfterGenerated();
    }

}