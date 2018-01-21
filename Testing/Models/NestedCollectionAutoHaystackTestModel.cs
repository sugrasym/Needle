using System.Collections.Generic;
using IMS.Common.Needle.Attributes;

namespace IMS.Tests.Needle.Models
{
    public class NestedCollectionAutoHaystackTestModel
    {
        public NestedCollectionAutoHaystackTestModel()
        {
            Substrings = new List<string>();
            Submodels = new List<NestedAutoHaystackTestModel>();
            LinkedModels = new List<NestedCollectionAutoHaystackTestModel>();
        }

        [Haystack] public string TestString { get; set; }

        [Haystack] public List<string> Substrings { get; set; }

        [Haystack("TestModel.Hidden")] public List<NestedAutoHaystackTestModel> Submodels { get; set; }

        [Haystack("Submodels.TestModel.Custom")]
        public List<NestedCollectionAutoHaystackTestModel> LinkedModels { get; set; }
    }
}