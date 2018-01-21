using IMS.Common.Needle.Attributes;

namespace IMS.Tests.Needle.Models
{
    public class NestedAutoHaystackTestModel
    {
        [Haystack("Hidden")] public AutoHaystackTestModel TestModel { get; set; }

        [Haystack("TestModel.Hidden")] public NestedAutoHaystackTestModel Deep { get; set; }
    }
}