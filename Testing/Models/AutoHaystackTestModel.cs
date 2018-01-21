using IMS.Common.Needle.Attributes;

namespace IMS.Tests.Needle.Models
{
    public class AutoHaystackTestModel
    {
        public int MinLength;

        [Haystack(isVisible: "ExampleString.Length >= MinLength")]
        public string ExampleString { get; set; }

        [Haystack] public int ExampleInt { get; set; }

        public bool ShowHidden { get; set; }

        [Haystack(isVisible: "ShowHidden == true")]
        public string Hidden { get; set; }

        public bool ShowCustom { get; set; }

        [Haystack(isVisible: "ShowCustom == true", queryRender: "string.Concat(\"¶\",Custom,\"¶\")")]
        public string Custom { get; set; }
    }
}