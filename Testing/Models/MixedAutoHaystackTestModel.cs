using System.ComponentModel.DataAnnotations;
using IMS.Common.Needle.Attributes;

namespace IMS.Tests.Needle.Models
{
    [MetadataType(typeof(MetaData))]
    public class MixedAutoHaystackTestModel
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

        [Haystack] public string Fourth { get; set; }

        internal class MetaData
        {
            public string First { get; set; }
            public string Second { get; set; }

            [Haystack] public string Third { get; set; }

            public string Fourth { get; set; }
        }
    }
}